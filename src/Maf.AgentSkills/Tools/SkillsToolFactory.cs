// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using Maf.AgentSkills.Loading;
using Maf.AgentSkills.Models;
using Microsoft.Extensions.AI;

namespace Maf.AgentSkills.Tools;

/// <summary>
/// Factory for creating skills-related tools based on configuration.
/// </summary>
public sealed class SkillsToolFactory
{
    private readonly SkillLoader _loader;
    private readonly Func<SkillsState> _stateProvider;
    private readonly SkillsToolsOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SkillsToolFactory"/> class.
    /// </summary>
    /// <param name="loader">The skill loader instance.</param>
    /// <param name="stateProvider">Function that returns the current skills state.</param>
    /// <param name="options">Tool configuration options.</param>
    public SkillsToolFactory(
        SkillLoader loader,
        Func<SkillsState> stateProvider,
        SkillsToolsOptions options)
    {
        _loader = loader;
        _stateProvider = stateProvider;
        _options = options;
    }

    /// <summary>
    /// Creates all enabled tools based on configuration.
    /// </summary>
    /// <returns>Collection of enabled AI tools.</returns>
    public IReadOnlyList<AITool> CreateTools()
    {
        var tools = new List<AITool>();

        if (_options.EnableReadSkillTool)
        {
            var readSkillTool = new ReadSkillTool(_loader, _stateProvider);
            tools.Add(readSkillTool.ToAIFunction());
        }

        if (_options.EnableReadFileTool)
        {
            var readFileTool = new ReadFileTool(_stateProvider);
            tools.Add(readFileTool.ToAIFunction());
        }

        if (_options.EnableListDirectoryTool)
        {
            var listDirTool = new ListDirectoryTool(_loader, _stateProvider);
            tools.Add(listDirTool.ToAIFunction());
        }

        if (_options.EnableExecuteScriptTool)
        {
            var executeScriptTool = new ExecuteScriptTool(_stateProvider, _options);
            tools.Add(executeScriptTool.ToAIFunction());
        }

        if (_options.EnableRunCommandTool && _options.AllowedCommands.Count > 0)
        {
            var runCommandTool = new RunCommandTool(_stateProvider, _options);
            tools.Add(runCommandTool.ToAIFunction());
        }

        return tools;
    }

    /// <summary>
    /// Creates tools using the specified skills state directly.
    /// </summary>
    /// <param name="loader">The skill loader.</param>
    /// <param name="state">The current skills state.</param>
    /// <param name="options">Tool options.</param>
    /// <returns>Collection of enabled AI tools.</returns>
    public static IReadOnlyList<AITool> CreateTools(
        SkillLoader loader,
        SkillsState state,
        SkillsToolsOptions options)
    {
        var factory = new SkillsToolFactory(loader, () => state, options);
        return factory.CreateTools();
    }

    /// <summary>
    /// Gets the names of all potentially available tools.
    /// </summary>
    public static IReadOnlyList<string> AllToolNames =>
    [
        ReadSkillTool.ToolName,
        ReadFileTool.ToolName,
        ListDirectoryTool.ToolName,
        ExecuteScriptTool.ToolName,
        RunCommandTool.ToolName
    ];

    /// <summary>
    /// Gets the names of tools that are enabled by default (safe tools).
    /// </summary>
    public static IReadOnlyList<string> DefaultEnabledToolNames =>
    [
        ReadSkillTool.ToolName,
        ReadFileTool.ToolName,
        ListDirectoryTool.ToolName
    ];

    /// <summary>
    /// Gets the names of tools that require explicit opt-in (potentially dangerous).
    /// </summary>
    public static IReadOnlyList<string> OptInToolNames =>
    [
        ExecuteScriptTool.ToolName,
        RunCommandTool.ToolName
    ];
}
