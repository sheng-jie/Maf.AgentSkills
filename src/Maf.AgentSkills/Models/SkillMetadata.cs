// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

namespace Maf.AgentSkills.Models;

/// <summary>
/// Represents the metadata of a skill parsed from SKILL.md YAML frontmatter.
/// Follows the Agent Skills specification: https://agentskills.io
/// </summary>
/// <param name="Name">
/// Required. Skill identifier (lowercase alphanumeric with hyphens, max 64 characters).
/// Must match the directory name.
/// </param>
/// <param name="Description">
/// Required. Brief description of the skill's purpose (max 1024 characters).
/// </param>
/// <param name="Path">
/// Absolute path to the skill directory containing SKILL.md.
/// </param>
/// <param name="Source">
/// Whether the skill is from user-level or project-level location.
/// </param>
/// <param name="License">
/// Optional. SPDX license identifier (e.g., "MIT", "Apache-2.0").
/// </param>
/// <param name="Compatibility">
/// Optional. Compatibility constraints (e.g., "vscode", "cursor", "any").
/// </param>
/// <param name="Metadata">
/// Optional. Additional key-value metadata pairs.
/// </param>
/// <param name="AllowedTools">
/// Optional. List of tools the skill is allowed to use.
/// </param>
/// <param name="References">
/// Optional. List of reference files available in the skill directory (e.g., "reference.md", "forms.md").
/// </param>
public sealed record SkillMetadata(
    string Name,
    string Description,
    string Path,
    SkillSource Source,
    string? License = null,
    string? Compatibility = null,
    IReadOnlyDictionary<string, string>? Metadata = null,
    IReadOnlyList<AllowedTool>? AllowedTools = null,
    IReadOnlyList<string>? References = null)
{
    /// <summary>
    /// Maximum allowed length for skill name.
    /// </summary>
    public const int MaxNameLength = 64;

    /// <summary>
    /// Maximum allowed length for skill description.
    /// </summary>
    public const int MaxDescriptionLength = 1024;

    /// <summary>
    /// Maximum file size for SKILL.md in bytes (10 MB).
    /// </summary>
    public const long MaxSkillFileSize = 10 * 1024 * 1024;

    /// <summary>
    /// The standard skill definition filename.
    /// </summary>
    public const string SkillFileName = "SKILL.md";

    /// <summary>
    /// Gets the full path to the SKILL.md file.
    /// </summary>
    public string SkillFilePath => System.IO.Path.Combine(Path, SkillFileName);

    /// <summary>
    /// Returns a display string for the skill suitable for system prompts.
    /// </summary>
    public string ToDisplayString() => $"- **{Name}**: {Description}";
}
