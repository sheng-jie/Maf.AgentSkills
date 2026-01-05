// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Maf.AgentSkills.Context;
using Maf.AgentSkills.Models;
using Xunit;

namespace Maf.AgentSkills.Tests.Context;

public class SkillsPromptTemplatesTests
{
    [Fact]
    public void GenerateSkillsList_WithNoSkills_ShouldReturnNoSkillsMessage()
    {
        // Arrange
        var state = new SkillsState();

        // Act
        var result = SkillsPromptTemplates.GenerateSkillsList(state);

        // Assert
        result.Should().Contain("No skills available");
    }

    [Fact]
    public void GenerateSkillsList_WithUserSkillsOnly_ShouldListUserSkills()
    {
        // Arrange
        var state = new SkillsState
        {
            UserSkills =
            [
                new SkillMetadata("web-research", "Research topics on the web", "/path", SkillSource.User),
                new SkillMetadata("code-review", "Review code for quality", "/path", SkillSource.User)
            ]
        };

        // Act
        var result = SkillsPromptTemplates.GenerateSkillsList(state);

        // Assert
        result.Should().Contain("User Skills");
        result.Should().Contain("web-research");
        result.Should().Contain("code-review");
        result.Should().NotContain("Project Skills");
    }

    [Fact]
    public void GenerateSkillsList_WithProjectSkillsOnly_ShouldListProjectSkills()
    {
        // Arrange
        var state = new SkillsState
        {
            ProjectSkills =
            [
                new SkillMetadata("my-skill", "A project skill", "/path", SkillSource.Project)
            ]
        };

        // Act
        var result = SkillsPromptTemplates.GenerateSkillsList(state);

        // Assert
        result.Should().Contain("Project Skills");
        result.Should().Contain("my-skill");
        result.Should().NotContain("User Skills");
    }

    [Fact]
    public void GenerateSkillsList_WithBothSources_ShouldListBoth()
    {
        // Arrange
        var state = new SkillsState
        {
            UserSkills = [new SkillMetadata("user-skill", "User skill", "/path", SkillSource.User)],
            ProjectSkills = [new SkillMetadata("project-skill", "Project skill", "/path", SkillSource.Project)]
        };

        // Act
        var result = SkillsPromptTemplates.GenerateSkillsList(state);

        // Assert
        result.Should().Contain("User Skills");
        result.Should().Contain("Project Skills");
        result.Should().Contain("user-skill");
        result.Should().Contain("project-skill");
    }

    [Fact]
    public void GenerateSkillsList_WithOverlap_ShouldExcludeOverriddenUserSkills()
    {
        // Arrange
        var state = new SkillsState
        {
            UserSkills =
            [
                new SkillMetadata("shared-skill", "User version", "/path", SkillSource.User),
                new SkillMetadata("user-only", "User only", "/path", SkillSource.User)
            ],
            ProjectSkills = [new SkillMetadata("shared-skill", "Project version", "/path", SkillSource.Project)]
        };

        // Act
        var result = SkillsPromptTemplates.GenerateSkillsList(state);

        // Assert
        result.Should().Contain("Project Skills");
        result.Should().Contain("shared-skill");
        result.Should().Contain("User Skills");
        result.Should().Contain("user-only");
        
        // Count occurrences of shared-skill - should only appear once (in project section)
        var occurrences = result.Split("shared-skill").Length - 1;
        occurrences.Should().Be(1);
    }

    [Fact]
    public void GenerateCompactSkillsPrompt_WithNoSkills_ShouldReturnEmpty()
    {
        // Arrange
        var state = new SkillsState();

        // Act
        var result = SkillsPromptTemplates.GenerateCompactSkillsPrompt(state);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GenerateCompactSkillsPrompt_WithSkills_ShouldReturnCompactFormat()
    {
        // Arrange
        var state = new SkillsState
        {
            UserSkills =
            [
                new SkillMetadata("web-research", "Research", "/path", SkillSource.User),
                new SkillMetadata("code-review", "Review", "/path", SkillSource.User)
            ]
        };

        // Act
        var result = SkillsPromptTemplates.GenerateCompactSkillsPrompt(state);

        // Assert
        result.Should().Contain("Available skills:");
        result.Should().Contain("web-research");
        result.Should().Contain("code-review");
        result.Should().Contain("read_skill");
    }

    [Fact]
    public void GenerateSkillsPrompt_ShouldIncludeAllSections()
    {
        // Arrange
        var state = new SkillsState
        {
            UserSkills = [new SkillMetadata("test-skill", "Test description", "/path", SkillSource.User)]
        };
        var locations = "- **User Skills**: `/home/.agentskills/default/skills`";

        // Act
        var result = SkillsPromptTemplates.GenerateSkillsPrompt(state, locations);

        // Assert
        result.Should().Contain("Skills System");
        result.Should().Contain("User Skills");
        result.Should().Contain("test-skill");
        result.Should().Contain("Progressive Disclosure");
        result.Should().Contain("read_skill");
    }
}
