// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using Maf.AgentSkills.Models;

namespace Maf.AgentSkills.Context;

/// <summary>
/// Provides system prompt templates for skills integration.
/// </summary>
public static class SkillsPromptTemplates
{
    /// <summary>
    /// The main skills system prompt template.
    /// Implements Anthropic's "Agent Skills" pattern with progressive disclosure.
    /// </summary>
    public const string SystemPromptTemplate = """
        ## Skills System

        You have access to a skills library that provides specialized capabilities and domain knowledge.

        {skills_locations}

        **Available Skills:**

        {skills_list}

        ---

        ### How to Use Skills (Progressive Disclosure) - CRITICAL

        Skills follow a **progressive disclosure** pattern - you know they exist (name + description above), 
        but you **MUST read the full instructions before using them**.

        **MANDATORY Workflow:**

        1. **Recognize when a skill applies**: Check if the user's task matches any skill's description above
        2. **Read the skill's full instructions FIRST**: Use `read_skill` tool to get the complete SKILL.md content
           - This tells you exactly what scripts exist, their parameters, and how to use them
           - **NEVER assume or guess script names, paths, or arguments**
        3. **Follow the skill's instructions precisely**: SKILL.md contains step-by-step workflows and examples
        4. **Execute scripts only after reading**: Use the exact script paths and argument formats from SKILL.md

        **IMPORTANT RULES:**

        ⚠️ **NEVER call `execute_skill_script` without first reading the skill with `read_skill`**
        - You do NOT know what scripts exist in a skill until you read it
        - You do NOT know the correct script arguments until you read the SKILL.md
        - Guessing script names will fail - always read first

        ✅ **Correct Workflow Example:**
        ```
        User: "Split this PDF into pages"
        1. Recognize: "split-pdf" skill matches this task
        2. Call: read_skill("split-pdf") → Get full instructions
        3. Learn: SKILL.md shows the actual script path and argument format
        4. Execute: Use the exact command format from SKILL.md
        ```

        ❌ **Wrong Workflow (DO NOT DO THIS):**
        ```
        User: "Split this PDF into pages"
        1. Recognize: "split-pdf" skill matches this task
        2. Guess: execute_skill_script("split-pdf", "split_pdf.py", ...) ← WRONG! Never guess!
        ```

        **Skills are Self-Documenting:**
        - Each SKILL.md tells you exactly what the skill does and how to use it
        - The skill may contain Python scripts, config files, or reference docs
        - Always use the exact paths and formats specified in SKILL.md

        Remember: **Read first, then execute.** This ensures you use skills correctly!
        """;

    /// <summary>
    /// Generates the skills section of the system prompt.
    /// </summary>
    /// <param name="state">The current skills state.</param>
    /// <param name="locationsDisplay">The skills locations display string.</param>
    /// <returns>The formatted skills system prompt section.</returns>
    public static string GenerateSkillsPrompt(SkillsState state, string locationsDisplay)
    {
        var skillsList = GenerateSkillsList(state);

        return SystemPromptTemplate
            .Replace("{skills_locations}", locationsDisplay)
            .Replace("{skills_list}", skillsList);
    }

    /// <summary>
    /// Generates a formatted list of available skills.
    /// </summary>
    /// <param name="state">The current skills state.</param>
    /// <returns>The formatted skills list.</returns>
    public static string GenerateSkillsList(SkillsState state)
    {
        var lines = new List<string>();

        // Group by source for clarity
        if (state.ProjectSkills.Count > 0)
        {
            lines.Add("*Project Skills:*");
            foreach (var skill in state.ProjectSkills)
            {
                lines.Add(skill.ToDisplayString());
            }
        }

        if (state.UserSkills.Count > 0)
        {
            // Filter out user skills that are overridden by project skills
            var projectSkillNames = state.ProjectSkills
                .Select(s => s.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var nonOverriddenUserSkills = state.UserSkills
                .Where(s => !projectSkillNames.Contains(s.Name))
                .ToList();

            if (nonOverriddenUserSkills.Count > 0)
            {
                if (lines.Count > 0)
                {
                    lines.Add("");
                }
                lines.Add("*User Skills:*");
                foreach (var skill in nonOverriddenUserSkills)
                {
                    lines.Add(skill.ToDisplayString());
                }
            }
        }

        if (lines.Count == 0)
        {
            return "*No skills available.*";
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Generates a minimal skills prompt for context-constrained situations.
    /// </summary>
    /// <param name="state">The current skills state.</param>
    /// <returns>A compact skills prompt.</returns>
    public static string GenerateCompactSkillsPrompt(SkillsState state)
    {
        var allSkills = state.AllSkills;
        if (allSkills.Count == 0)
        {
            return "";
        }

        var skillNames = string.Join(", ", allSkills.Select(s => s.Name));
        return $"Available skills: {skillNames}. Use `read_skill` to get instructions.";
    }
}
