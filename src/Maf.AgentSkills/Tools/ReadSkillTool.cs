// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Text.Json;
using Maf.AgentSkills.Loading;
using Maf.AgentSkills.Models;
using Microsoft.Extensions.AI;

namespace Maf.AgentSkills.Tools;

/// <summary>
/// Tool that reads the full content of a SKILL.md file.
/// Enables progressive disclosure of skill instructions.
/// </summary>
public sealed class ReadSkillTool
{
    private readonly SkillLoader _loader;
    private readonly Func<SkillsState> _stateProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadSkillTool"/> class.
    /// </summary>
    /// <param name="loader">The skill loader instance.</param>
    /// <param name="stateProvider">Function that returns the current skills state.</param>
    public ReadSkillTool(SkillLoader loader, Func<SkillsState> stateProvider)
    {
        _loader = loader;
        _stateProvider = stateProvider;
    }

    /// <summary>
    /// Gets the tool name.
    /// </summary>
    public static string ToolName => "read_skill";

    /// <summary>
    /// Reads the full content of a skill's SKILL.md file.
    /// </summary>
    /// <param name="skillName">The name of the skill to read.</param>
    /// <returns>The full content of the SKILL.md file.</returns>
    [Description("Reads the full content of a skill's SKILL.md file to get detailed instructions. Use this when you need to understand how to apply a skill.")]
    public string ReadSkill(
        [Description("The name of the skill to read (e.g., 'web-research', 'code-review')")]
        string skillName)
    {
        var state = _stateProvider();
        var skill = state.GetSkill(skillName);

        if (skill is null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = $"Skill '{skillName}' not found. Use the skills list in the system prompt to see available skills."
            });
        }

        try
        {
            var content = _loader.ReadSkillContent(skill);
            return JsonSerializer.Serialize(new
            {
                success = true,
                skill_name = skill.Name,
                source = skill.Source.ToString().ToLowerInvariant(),
                path = skill.Path,
                content
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = $"Failed to read skill '{skillName}': {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Creates an <see cref="AIFunction"/> for this tool.
    /// </summary>
    public AIFunction ToAIFunction()
    {
        return AIFunctionFactory.Create(ReadSkill, ToolName);
    }
}
