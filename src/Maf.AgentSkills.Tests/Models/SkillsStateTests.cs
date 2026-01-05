// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Maf.AgentSkills.Models;
using Xunit;

namespace Maf.AgentSkills.Tests.Models;

public class SkillsStateTests
{
    [Fact]
    public void AllSkills_WithNoOverlap_ShouldReturnAllSkills()
    {
        // Arrange
        var userSkills = new List<SkillMetadata>
        {
            CreateSkill("skill-a", SkillSource.User),
            CreateSkill("skill-b", SkillSource.User)
        };
        var projectSkills = new List<SkillMetadata>
        {
            CreateSkill("skill-c", SkillSource.Project)
        };

        var state = new SkillsState
        {
            UserSkills = userSkills,
            ProjectSkills = projectSkills
        };

        // Act
        var allSkills = state.AllSkills;

        // Assert
        allSkills.Should().HaveCount(3);
        allSkills.Select(s => s.Name).Should().Contain(["skill-a", "skill-b", "skill-c"]);
    }

    [Fact]
    public void AllSkills_WithOverlap_ProjectSkillsShouldTakePrecedence()
    {
        // Arrange
        var userSkills = new List<SkillMetadata>
        {
            CreateSkill("shared-skill", SkillSource.User, "User version"),
            CreateSkill("user-only", SkillSource.User)
        };
        var projectSkills = new List<SkillMetadata>
        {
            CreateSkill("shared-skill", SkillSource.Project, "Project version")
        };

        var state = new SkillsState
        {
            UserSkills = userSkills,
            ProjectSkills = projectSkills
        };

        // Act
        var allSkills = state.AllSkills;

        // Assert
        allSkills.Should().HaveCount(2);
        
        var sharedSkill = allSkills.First(s => s.Name == "shared-skill");
        sharedSkill.Source.Should().Be(SkillSource.Project);
        sharedSkill.Description.Should().Be("Project version");
    }

    [Fact]
    public void GetSkill_ShouldReturnProjectSkillFirst()
    {
        // Arrange
        var state = new SkillsState
        {
            UserSkills = [CreateSkill("test-skill", SkillSource.User, "User")],
            ProjectSkills = [CreateSkill("test-skill", SkillSource.Project, "Project")]
        };

        // Act
        var skill = state.GetSkill("test-skill");

        // Assert
        skill.Should().NotBeNull();
        skill!.Source.Should().Be(SkillSource.Project);
        skill.Description.Should().Be("Project");
    }

    [Fact]
    public void GetSkill_ShouldBeCaseInsensitive()
    {
        // Arrange
        var state = new SkillsState
        {
            UserSkills = [CreateSkill("my-skill", SkillSource.User)]
        };

        // Act
        var skill = state.GetSkill("MY-SKILL");

        // Assert
        skill.Should().NotBeNull();
        skill!.Name.Should().Be("my-skill");
    }

    [Fact]
    public void GetSkill_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        var state = new SkillsState();

        // Act
        var skill = state.GetSkill("nonexistent");

        // Assert
        skill.Should().BeNull();
    }

    [Fact]
    public void GetSkillsBySource_ShouldFilterCorrectly()
    {
        // Arrange
        var state = new SkillsState
        {
            UserSkills = [CreateSkill("user-skill", SkillSource.User)],
            ProjectSkills = [CreateSkill("project-skill", SkillSource.Project)]
        };

        // Act
        var userSkills = state.GetSkillsBySource(SkillSource.User);
        var projectSkills = state.GetSkillsBySource(SkillSource.Project);

        // Assert
        userSkills.Should().HaveCount(1);
        userSkills[0].Name.Should().Be("user-skill");
        
        projectSkills.Should().HaveCount(1);
        projectSkills[0].Name.Should().Be("project-skill");
    }

    private static SkillMetadata CreateSkill(string name, SkillSource source, string? description = null)
    {
        return new SkillMetadata(
            Name: name,
            Description: description ?? $"Description for {name}",
            Path: $"/path/to/{name}",
            Source: source
        );
    }
}
