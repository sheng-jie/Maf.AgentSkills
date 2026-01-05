// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Maf.AgentSkills.Loading;
using Maf.AgentSkills.Models;

namespace Maf.AgentSkills.Tools;

/// <summary>
/// Tool that executes allowed terminal commands within skill directories.
/// DISABLED BY DEFAULT for security. Must be explicitly enabled with a whitelist.
/// </summary>
public sealed class RunCommandTool
{
    private readonly Func<SkillsState> _stateProvider;
    private readonly SkillsToolsOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="RunCommandTool"/> class.
    /// </summary>
    /// <param name="stateProvider">Function that returns the current skills state.</param>
    /// <param name="options">Tool options including allowed commands and timeout.</param>
    public RunCommandTool(Func<SkillsState> stateProvider, SkillsToolsOptions options)
    {
        _stateProvider = stateProvider;
        _options = options;
    }

    /// <summary>
    /// Gets the tool name.
    /// </summary>
    public static string ToolName => "run_skill_command";

    /// <summary>
    /// Runs an allowed command within a skill's directory context.
    /// </summary>
    /// <param name="skillName">The name of the skill for working directory context.</param>
    /// <param name="command">The command to run (must be in the allowed list).</param>
    /// <param name="arguments">Arguments to pass to the command.</param>
    /// <returns>The command output or an error message.</returns>
    [Description("Runs an allowed terminal command within a skill's directory context. Only whitelisted commands can be executed.")]
    public async Task<string> RunCommandAsync(
        [Description("The name of the skill to use as working directory context")]
        string skillName,
        [Description("The command to run (must be in the allowed commands list)")]
        string command,
        [Description("Arguments to pass to the command")]
        string? arguments = null)
    {
        var state = _stateProvider();
        var skill = state.GetSkill(skillName);

        if (skill is null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = $"Skill '{skillName}' not found."
            });
        }

        // Validate command is in the allowed list
        var commandName = command.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? command;
        if (!_options.AllowedCommands.Any(c => c.Equals(commandName, StringComparison.OrdinalIgnoreCase)))
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = $"Command '{commandName}' is not allowed. Allowed commands: {string.Join(", ", _options.AllowedCommands)}"
            });
        }

        // Sanitize arguments to prevent injection
        var sanitizedArgs = SanitizeArguments(arguments);

        try
        {
            var result = await ExecuteCommandAsync(
                commandName,
                sanitizedArgs,
                skill.Path,
                TimeSpan.FromSeconds(_options.CommandTimeoutSeconds));

            // Truncate output if too large
            var stdout = TruncateOutput(result.StandardOutput);
            var stderr = TruncateOutput(result.StandardError);

            return JsonSerializer.Serialize(new
            {
                success = result.ExitCode == 0,
                exit_code = result.ExitCode,
                stdout,
                stderr,
                truncated = stdout.Length < result.StandardOutput.Length || stderr.Length < result.StandardError.Length
            });
        }
        catch (OperationCanceledException)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = $"Command execution timed out after {_options.CommandTimeoutSeconds} seconds."
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = $"Failed to execute command: {ex.Message}"
            });
        }
    }

    private static string? SanitizeArguments(string? arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return null;
        }

        // Remove potential command injection characters
        // This is a basic sanitization - for production, consider more robust approaches
        var dangerous = new[] { ';', '|', '&', '$', '`', '>', '<', '\n', '\r' };
        var sanitized = arguments;

        foreach (var c in dangerous)
        {
            sanitized = sanitized.Replace(c.ToString(), "");
        }

        return sanitized;
    }

    private static async Task<ProcessResult> ExecuteCommandAsync(
        string command,
        string? arguments,
        string workingDirectory,
        TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);

        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments ?? "",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };

        var stdoutBuilder = new StringBuilder();
        var stderrBuilder = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                stdoutBuilder.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                stderrBuilder.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cts.Token);

        return new ProcessResult(
            process.ExitCode,
            stdoutBuilder.ToString(),
            stderrBuilder.ToString());
    }

    private string TruncateOutput(string output)
    {
        if (output.Length <= _options.MaxOutputSizeBytes)
        {
            return output;
        }

        return output[.._options.MaxOutputSizeBytes] + "\n\n[Output truncated]";
    }

    /// <summary>
    /// Creates an <see cref="Microsoft.Extensions.AI.AIFunction"/> for this tool.
    /// </summary>
    public Microsoft.Extensions.AI.AIFunction ToAIFunction()
    {
        return Microsoft.Extensions.AI.AIFunctionFactory.Create(
            (string skillName, string command, string? arguments) =>
                RunCommandAsync(skillName, command, arguments),
            ToolName);
    }

    private sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);
}
