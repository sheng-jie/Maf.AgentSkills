// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Text.Json;
using Maf.AgentSkills.Loading;
using Maf.AgentSkills.Models;

namespace Maf.AgentSkills.Tools;

/// <summary>
/// Tool that lists files and directories within a skill's directory.
/// </summary>
public sealed class ListDirectoryTool
{
    private readonly SkillLoader _loader;
    private readonly Func<SkillsState> _stateProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListDirectoryTool"/> class.
    /// </summary>
    /// <param name="loader">The skill loader instance.</param>
    /// <param name="stateProvider">Function that returns the current skills state.</param>
    public ListDirectoryTool(SkillLoader loader, Func<SkillsState> stateProvider)
    {
        _loader = loader;
        _stateProvider = stateProvider;
    }

    /// <summary>
    /// Gets the tool name.
    /// </summary>
    public static string ToolName => "list_skill_directory";

    /// <summary>
    /// Lists the contents of a skill's directory.
    /// </summary>
    /// <param name="skillName">The name of the skill.</param>
    /// <param name="relativePath">Optional relative path within the skill directory.</param>
    /// <returns>List of files and directories.</returns>
    [Description("Lists files and directories within a skill's directory. Use this to discover what resources a skill provides.")]
    public string ListDirectory(
        [Description("The name of the skill")]
        string skillName,
        [Description("Optional relative path within the skill directory. Leave empty to list the root.")]
        string? relativePath = null)
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

        try
        {
            var entries = _loader.ListSkillDirectory(skill, relativePath).ToList();

            var result = entries.Select(e => new
            {
                name = e.Name,
                type = e.IsDirectory ? "directory" : "file",
                size = e.Size
            }).ToList();

            return JsonSerializer.Serialize(new
            {
                success = true,
                skill_name = skillName,
                path = string.IsNullOrEmpty(relativePath) ? "/" : relativePath,
                entries = result
            });
        }
        catch (UnauthorizedAccessException)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Invalid path: path traversal not allowed."
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = $"Failed to list directory: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Creates an <see cref="Microsoft.Extensions.AI.AIFunction"/> for this tool.
    /// </summary>
    public Microsoft.Extensions.AI.AIFunction ToAIFunction()
    {
        return Microsoft.Extensions.AI.AIFunctionFactory.Create(ListDirectory, ToolName);
    }
}
