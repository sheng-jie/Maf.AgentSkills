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
/// Tool that executes scripts from within skill directories.
/// DISABLED BY DEFAULT for security. Must be explicitly enabled.
/// </summary>
public sealed class ExecuteScriptTool
{
    private readonly Func<SkillsState> _stateProvider;
    private readonly SkillsToolsOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteScriptTool"/> class.
    /// </summary>
    /// <param name="stateProvider">Function that returns the current skills state.</param>
    /// <param name="options">Tool options including allowed extensions and timeout.</param>
    public ExecuteScriptTool(Func<SkillsState> stateProvider, SkillsToolsOptions options)
    {
        _stateProvider = stateProvider;
        _options = options;
    }

    /// <summary>
    /// Gets the tool name.
    /// </summary>
    public static string ToolName => "execute_skill_script";

    /// <summary>
    /// Executes a script from within a skill's directory.
    /// </summary>
    /// <param name="skillName">The name of the skill containing the script.</param>
    /// <param name="scriptPath">The relative path to the script within the skill directory.</param>
    /// <param name="arguments">Optional arguments to pass to the script.</param>
    /// <returns>The script output or an error message.</returns>
    [Description("Executes a script from within a skill's directory. Only enabled scripts with allowed extensions can be executed.")]
    public async Task<string> ExecuteScriptAsync(
        [Description("The name of the skill containing the script")]
        string skillName,
        [Description("The relative path to the script within the skill directory")]
        string scriptPath,
        [Description("Optional arguments to pass to the script")]
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

        // Validate and resolve the path safely
        var safePath = PathSecurity.ResolveSafePath(skill.Path, scriptPath);
        if (safePath is null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Invalid path: path traversal not allowed."
            });
        }

        if (!File.Exists(safePath))
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = $"Script not found: {scriptPath}"
            });
        }

        // Validate extension
        var extension = Path.GetExtension(safePath).ToLowerInvariant();
        if (!_options.AllowedScriptExtensions.Any(e => e.Equals(extension, StringComparison.OrdinalIgnoreCase)))
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = $"Script extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", _options.AllowedScriptExtensions)}"
            });
        }

        // Determine the interpreter
        var (interpreter, interpreterArgs) = GetInterpreter(extension);
        if (interpreter is null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = $"No interpreter configured for extension '{extension}'"
            });
        }

        try
        {
            var result = await ExecuteProcessAsync(
                interpreter,
                $"{interpreterArgs} \"{safePath}\" {arguments ?? ""}".Trim(),
                skill.Path,
                TimeSpan.FromSeconds(_options.ScriptTimeoutSeconds));

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
                error = $"Script execution timed out after {_options.ScriptTimeoutSeconds} seconds."
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = $"Failed to execute script: {ex.Message}"
            });
        }
    }

    private static (string? interpreter, string args) GetInterpreter(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".py" => ("python", ""),
            ".cs" => ("dotnet",""),
            ".ps1" => ("powershell", "-ExecutionPolicy Bypass -File"),
            ".sh" => OperatingSystem.IsWindows() ? ("bash", "") : ("/bin/bash", ""),
            _ => (null, "")
        };
    }

    private static async Task<ProcessResult> ExecuteProcessAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);

        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
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
    /// Tool description emphasizing progressive disclosure pattern.
    /// </summary>
    private const string ToolDescription = """
        Executes a script from within a skill's directory.
        
        ⚠️ IMPORTANT: You MUST call read_skill FIRST to get the exact script path and argument format.
        Never guess script names or arguments - they are defined in each skill's SKILL.md file.
        
        Workflow: 1) read_skill(skillName) → 2) Learn script path & args from SKILL.md → 3) execute_skill_script(...)
        """;

    /// <summary>
    /// Creates an <see cref="Microsoft.Extensions.AI.AIFunction"/> for this tool.
    /// </summary>
    public Microsoft.Extensions.AI.AIFunction ToAIFunction()
    {
        return Microsoft.Extensions.AI.AIFunctionFactory.Create(
            (string skillName, string scriptPath, string arguments) =>
                ExecuteScriptAsync(skillName, scriptPath, arguments),
            ToolName,
            ToolDescription);
    }

    private sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);
}
