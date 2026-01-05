// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

namespace Maf.AgentSkills.Models;

/// <summary>
/// Indicates the source location of a skill.
/// </summary>
public enum SkillSource
{
    /// <summary>
    /// User-level skill stored in user's home directory.
    /// Path: ~/.agentskills/{agent-name}/skills/{skill-name}/
    /// </summary>
    User,

    /// <summary>
    /// Project-level skill stored in the project's .agentskills directory.
    /// Path: {project-root}/.agentskills/skills/{skill-name}/
    /// </summary>
    Project
}
