// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using Maf.AgentSkills.Tools;

namespace Maf.AgentSkills.Models;

/// <summary>
/// Configuration options for the Agent Skills integration.
/// </summary>
public sealed class SkillsOptions
{
    /// <summary>
    /// Gets or sets the agent name used for user-level skill directory.
    /// Default is "default".
    /// </summary>
    /// <remarks>
    /// Different agent names use different user-level skill directories: ~/.maf/{AgentName}/skills/
    /// This allows different agents to have different skill sets.
    /// </remarks>
    public string AgentName { get; set; } = "default";

    /// <summary>
    /// Gets or sets the project root directory for project-level skills.
    /// If null, project skills will not be loaded.
    /// </summary>
    public string? ProjectRoot { get; set; }

    /// <summary>
    /// Gets or sets the user-level skills directory.
    /// Overrides the default ~/.maf/{AgentName}/skills/.
    /// </summary>
    public string? UserSkillsDir { get; set; }

    /// <summary>
    /// Gets or sets the project-level skills directory.
    /// Overrides the default {ProjectRoot}/.maf/skills/.
    /// </summary>
    public string? ProjectSkillsDir { get; set; }

    /// <summary>
    /// Gets or sets whether to cache skills.
    /// Default is true.
    /// </summary>
    public bool CacheSkills { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate skills on startup.
    /// Default is true.
    /// </summary>
    public bool ValidateOnStartup { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable user-level skills.
    /// Default is true.
    /// </summary>
    public bool EnableUserSkills { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable project-level skills.
    /// Default is true.
    /// </summary>
    public bool EnableProjectSkills { get; set; } = true;

    /// <summary>
    /// Gets or sets the tools configuration options.
    /// </summary>
    public SkillsToolsOptions ToolsOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to enable the ReadSkill tool.
    /// Default is true.
    /// </summary>
    /// <remarks>This property delegates to <see cref="ToolsOptions"/>.</remarks>
    public bool EnableReadSkillTool
    {
        get => ToolsOptions.EnableReadSkillTool;
        set => ToolsOptions.EnableReadSkillTool = value;
    }

    /// <summary>
    /// Gets or sets whether to enable the ReadFile tool for reading files within skill directories.
    /// Default is true.
    /// </summary>
    /// <remarks>This property delegates to <see cref="ToolsOptions"/>.</remarks>
    public bool EnableReadFileTool
    {
        get => ToolsOptions.EnableReadFileTool;
        set => ToolsOptions.EnableReadFileTool = value;
    }

    /// <summary>
    /// Gets or sets whether to enable the ListDirectory tool.
    /// Default is true.
    /// </summary>
    /// <remarks>This property delegates to <see cref="ToolsOptions"/>.</remarks>
    public bool EnableListDirectoryTool
    {
        get => ToolsOptions.EnableListDirectoryTool;
        set => ToolsOptions.EnableListDirectoryTool = value;
    }

    /// <summary>
    /// Gets or sets whether to enable the ExecuteScript tool.
    /// Default is false (disabled for security).
    /// </summary>
    /// <remarks>This property delegates to <see cref="ToolsOptions"/>.</remarks>
    public bool EnableExecuteScriptTool
    {
        get => ToolsOptions.EnableExecuteScriptTool;
        set => ToolsOptions.EnableExecuteScriptTool = value;
    }

    /// <summary>
    /// Gets or sets the allowed script extensions when ExecuteScript is enabled.
    /// Default is [".py", ".ps1", ".sh"].
    /// </summary>
    /// <remarks>This property delegates to <see cref="ToolsOptions"/>.</remarks>
    public IList<string> AllowedScriptExtensions
    {
        get => ToolsOptions.AllowedScriptExtensions.ToList();
        set => ToolsOptions.AllowedScriptExtensions = value.ToList();
    }

    /// <summary>
    /// Gets or sets the script execution timeout in seconds.
    /// Default is 30 seconds.
    /// </summary>
    /// <remarks>This property delegates to <see cref="ToolsOptions"/>.</remarks>
    public int ScriptTimeoutSeconds
    {
        get => ToolsOptions.ScriptTimeoutSeconds;
        set => ToolsOptions.ScriptTimeoutSeconds = value;
    }

    /// <summary>
    /// Gets or sets whether to enable the RunCommand tool.
    /// Default is false (disabled for security).
    /// </summary>
    /// <remarks>This property delegates to <see cref="ToolsOptions"/>.</remarks>
    public bool EnableRunCommandTool
    {
        get => ToolsOptions.EnableRunCommandTool;
        set => ToolsOptions.EnableRunCommandTool = value;
    }

    /// <summary>
    /// Gets or sets the allowed commands when RunCommand is enabled.
    /// Only commands in this list can be executed.
    /// </summary>
    /// <remarks>This property delegates to <see cref="ToolsOptions"/>.</remarks>
    public IList<string> AllowedCommands
    {
        get => ToolsOptions.AllowedCommands.ToList();
        set => ToolsOptions.AllowedCommands = value.ToList();
    }

    /// <summary>
    /// Gets or sets the command execution timeout in seconds.
    /// Default is 30 seconds.
    /// </summary>
    /// <remarks>This property delegates to <see cref="ToolsOptions"/>.</remarks>
    public int CommandTimeoutSeconds
    {
        get => ToolsOptions.CommandTimeoutSeconds;
        set => ToolsOptions.CommandTimeoutSeconds = value;
    }

    /// <summary>
    /// Gets or sets the maximum output size in bytes for script/command execution.
    /// Default is 50 KB.
    /// </summary>
    /// <remarks>This property delegates to <see cref="ToolsOptions"/>.</remarks>
    public int MaxOutputSizeBytes
    {
        get => ToolsOptions.MaxOutputSizeBytes;
        set => ToolsOptions.MaxOutputSizeBytes = value;
    }

    /// <summary>
    /// Gets or sets whether to automatically refresh skills on each agent invocation.
    /// Default is false (manual refresh only).
    /// </summary>
    public bool AutoRefreshSkills { get; set; } = false;

    /// <summary>
    /// Gets or sets the cache duration for loaded skills in seconds.
    /// Default is 300 seconds (5 minutes).
    /// </summary>
    public int SkillsCacheDurationSeconds { get; set; } = 300;

    /// <summary>
    /// Enables script execution with the specified extensions.
    /// </summary>
    /// <param name="allowedExtensions">Allowed script file extensions.</param>
    /// <param name="timeoutSeconds">Execution timeout in seconds.</param>
    /// <returns>This options instance for chaining.</returns>
    public SkillsOptions EnableScriptExecution(
        IEnumerable<string>? allowedExtensions = null,
        int timeoutSeconds = 30)
    {
        ToolsOptions.EnableExecuteScriptTool = true;
        if (allowedExtensions is not null)
        {
            ToolsOptions.AllowedScriptExtensions = allowedExtensions.ToList();
        }
        ToolsOptions.ScriptTimeoutSeconds = timeoutSeconds;
        return this;
    }

    /// <summary>
    /// Enables command execution with the specified allowed commands.
    /// </summary>
    /// <param name="allowedCommands">Commands that are allowed to be executed.</param>
    /// <param name="timeoutSeconds">Execution timeout in seconds.</param>
    /// <returns>This options instance for chaining.</returns>
    public SkillsOptions EnableCommandExecution(
        IEnumerable<string> allowedCommands,
        int timeoutSeconds = 30)
    {
        ToolsOptions.EnableRunCommandTool = true;
        ToolsOptions.AllowedCommands = allowedCommands.ToList();
        ToolsOptions.CommandTimeoutSeconds = timeoutSeconds;
        return this;
    }
}
