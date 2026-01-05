// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Maf.AgentSkills.Configuration;
using Xunit;

namespace Maf.AgentSkills.Tests.Configuration;

public class SkillsSettingsTests
{
    [Fact]
    public void Constructor_WithValidAgentName_ShouldSucceed()
    {
        // Act
        var settings = new SkillsSettings("my-agent");

        // Assert
        settings.AgentName.Should().Be("my-agent");
        settings.ProjectRoot.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithProjectRoot_ShouldSucceed()
    {
        // Act
        var settings = new SkillsSettings("my-agent", "/project/root");

        // Assert
        settings.AgentName.Should().Be("my-agent");
        settings.ProjectRoot.Should().Be("/project/root");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidAgentName_ShouldThrow(string? agentName)
    {
        // Act
        var act = () => new SkillsSettings(agentName!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetUserSkillsDirectory_ShouldReturnCorrectPath()
    {
        // Arrange
        var settings = new SkillsSettings("test-agent");
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var expected = Path.Combine(homeDir, ".maf", "test-agent", "skills");

        // Act
        var result = settings.GetUserSkillsDirectory();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetProjectSkillsDirectory_WithProjectRoot_ShouldReturnCorrectPath()
    {
        // Arrange
        var projectRoot = Path.Combine(Path.GetTempPath(), "test-project");
        var settings = new SkillsSettings("test-agent", projectRoot);
        var expected = Path.Combine(projectRoot, ".maf", "skills");

        // Act
        var result = settings.GetProjectSkillsDirectory();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetProjectSkillsDirectory_WithoutProjectRoot_ShouldReturnNull()
    {
        // Arrange
        var settings = new SkillsSettings("test-agent");

        // Act
        var result = settings.GetProjectSkillsDirectory();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetUserSkillPath_ShouldReturnCorrectPath()
    {
        // Arrange
        var settings = new SkillsSettings("test-agent");
        var userSkillsDir = settings.GetUserSkillsDirectory();
        var expected = Path.Combine(userSkillsDir, "my-skill");

        // Act
        var result = settings.GetUserSkillPath("my-skill");

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetProjectSkillPath_WithProjectRoot_ShouldReturnCorrectPath()
    {
        // Arrange
        var projectRoot = Path.Combine(Path.GetTempPath(), "test-project");
        var settings = new SkillsSettings("test-agent", projectRoot);
        var expected = Path.Combine(projectRoot, ".maf", "skills", "my-skill");

        // Act
        var result = settings.GetProjectSkillPath("my-skill");

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetLocationsDisplayString_ShouldIncludeUserSkills()
    {
        // Arrange
        var settings = new SkillsSettings("test-agent");

        // Act
        var result = settings.GetLocationsDisplayString();

        // Assert
        result.Should().Contain("User Skills");
        result.Should().Contain(".maf");
    }

    [Fact]
    public void GetLocationsDisplayString_WithProjectRoot_ShouldIncludeBoth()
    {
        // Arrange
        var settings = new SkillsSettings("test-agent", "/project/root");

        // Act
        var result = settings.GetLocationsDisplayString();

        // Assert
        result.Should().Contain("User Skills");
        result.Should().Contain("Project Skills");
        result.Should().Contain("takes precedence");
    }
}
