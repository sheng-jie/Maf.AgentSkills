// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

namespace Maf.AgentSkills.Configuration;

/// <summary>
/// Manages skill directory paths based on agent name and project root.
/// Follows the Agent Skills specification directory structure.
/// </summary>
public sealed class SkillsSettings
{
    private const string MafDirectoryName = ".maf";
    private const string SkillsSubdirectoryName = "skills";

    /// <summary>
    /// Gets the agent name for user-level skill paths.
    /// </summary>
    public string AgentName { get; }

    /// <summary>
    /// Gets the project root directory for project-level skills.
    /// </summary>
    public string? ProjectRoot { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SkillsSettings"/> class.
    /// </summary>
    /// <param name="agentName">The agent name for user-level skill paths.</param>
    /// <param name="projectRoot">The project root directory for project-level skills.</param>
    public SkillsSettings(string agentName, string? projectRoot = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentName);

        AgentName = agentName;
        ProjectRoot = projectRoot;
    }

    /// <summary>
    /// Gets the user-level skills directory path.
    /// Path: ~/.maf/{agent-name}/skills/
    /// </summary>
    /// <returns>The absolute path to the user skills directory.</returns>
    public string GetUserSkillsDirectory()
    {
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(homeDirectory, MafDirectoryName, AgentName, SkillsSubdirectoryName);
    }

    /// <summary>
    /// Gets the project-level skills directory path.
    /// Path: {project-root}/.maf/skills/
    /// </summary>
    /// <returns>The absolute path to the project skills directory, or null if no project root is set.</returns>
    public string? GetProjectSkillsDirectory()
    {
        if (string.IsNullOrEmpty(ProjectRoot))
        {
            return null;
        }

        return Path.Combine(ProjectRoot, MafDirectoryName, SkillsSubdirectoryName);
    }

    /// <summary>
    /// Gets the path to a specific user-level skill.
    /// </summary>
    /// <param name="skillName">The skill name.</param>
    /// <returns>The absolute path to the skill directory.</returns>
    public string GetUserSkillPath(string skillName)
    {
        return Path.Combine(GetUserSkillsDirectory(), skillName);
    }

    /// <summary>
    /// Gets the path to a specific project-level skill.
    /// </summary>
    /// <param name="skillName">The skill name.</param>
    /// <returns>The absolute path to the skill directory, or null if no project root is set.</returns>
    public string? GetProjectSkillPath(string skillName)
    {
        var projectSkillsDir = GetProjectSkillsDirectory();
        if (projectSkillsDir is null)
        {
            return null;
        }

        return Path.Combine(projectSkillsDir, skillName);
    }

    /// <summary>
    /// Checks if the user skills directory exists.
    /// </summary>
    /// <returns>True if the directory exists; otherwise, false.</returns>
    public bool UserSkillsDirectoryExists()
    {
        return Directory.Exists(GetUserSkillsDirectory());
    }

    /// <summary>
    /// Checks if the project skills directory exists.
    /// </summary>
    /// <returns>True if the directory exists; otherwise, false.</returns>
    public bool ProjectSkillsDirectoryExists()
    {
        var projectSkillsDir = GetProjectSkillsDirectory();
        return projectSkillsDir is not null && Directory.Exists(projectSkillsDir);
    }

    /// <summary>
    /// Creates a display string showing the configured skill locations.
    /// </summary>
    /// <returns>A formatted string describing skill locations.</returns>
    public string GetLocationsDisplayString()
    {
        var locations = new List<string>();

        var userSkillsDir = GetUserSkillsDirectory();
        locations.Add($"- **User Skills**: `{userSkillsDir}`");

        var projectSkillsDir = GetProjectSkillsDirectory();
        if (projectSkillsDir is not null)
        {
            locations.Add($"- **Project Skills**: `{projectSkillsDir}` (takes precedence)");
        }

        return string.Join(Environment.NewLine, locations);
    }
}
