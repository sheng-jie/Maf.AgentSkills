// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

namespace Maf.AgentSkills.Tools;

/// <summary>
/// Configuration options for skills-related tools.
/// </summary>
public sealed class SkillsToolsOptions
{
    /// <summary>
    /// Gets or sets whether the ReadSkill tool is enabled.
    /// Default is true.
    /// </summary>
    public bool EnableReadSkillTool { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the ReadFile tool is enabled.
    /// Default is true.
    /// </summary>
    public bool EnableReadFileTool { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the ListDirectory tool is enabled.
    /// Default is true.
    /// </summary>
    public bool EnableListDirectoryTool { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the ExecuteScript tool is enabled.
    /// Default is false (disabled for security).
    /// </summary>
    public bool EnableExecuteScriptTool { get; set; } = false;

    /// <summary>
    /// Gets or sets the allowed script extensions when ExecuteScript is enabled.
    /// </summary>
    public IList<string> AllowedScriptExtensions { get; set; } = [".py", ".ps1", ".sh", ".cs"];

    /// <summary>
    /// Gets or sets the script execution timeout in seconds.
    /// </summary>
    public int ScriptTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets whether the RunCommand tool is enabled.
    /// Default is false (disabled for security).
    /// </summary>
    public bool EnableRunCommandTool { get; set; } = false;

    /// <summary>
    /// Gets or sets the allowed commands when RunCommand is enabled.
    /// </summary>
    public IList<string> AllowedCommands { get; set; } = [];

    /// <summary>
    /// Gets or sets the command execution timeout in seconds.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum output size in bytes for script/command execution.
    /// Default is 50 KB.
    /// </summary>
    public int MaxOutputSizeBytes { get; set; } = 50 * 1024;

    /// <summary>
    /// Creates options from a <see cref="Models.SkillsOptions"/> instance.
    /// </summary>
    public static SkillsToolsOptions FromSkillsOptions(Models.SkillsOptions options)
    {
        // Return the existing ToolsOptions reference
        return options.ToolsOptions;
    }
}
