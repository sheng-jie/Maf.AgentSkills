// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Maf.AgentSkills.Models;
using Maf.AgentSkills.Tools;
using Xunit;

namespace Maf.AgentSkills.Tests.Tools;

public class SkillsToolsOptionsTests
{
    [Fact]
    public void DefaultOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange
        var options = new SkillsToolsOptions();

        // Assert - Safe tools enabled by default
        options.EnableReadSkillTool.Should().BeTrue();
        options.EnableReadFileTool.Should().BeTrue();
        options.EnableListDirectoryTool.Should().BeTrue();

        // Assert - Dangerous tools disabled by default
        options.EnableExecuteScriptTool.Should().BeFalse();
        options.EnableRunCommandTool.Should().BeFalse();

        // Assert - Sensible defaults for other options
        options.ScriptTimeoutSeconds.Should().Be(30);
        options.CommandTimeoutSeconds.Should().Be(30);
        options.MaxOutputSizeBytes.Should().Be(50 * 1024);
        options.AllowedScriptExtensions.Should().Contain(".py");
        options.AllowedScriptExtensions.Should().Contain(".ps1");
        options.AllowedScriptExtensions.Should().Contain(".sh");
        options.AllowedCommands.Should().BeEmpty();
    }

    [Fact]
    public void FromSkillsOptions_ShouldMapCorrectly()
    {
        // Arrange
        var skillsOptions = new SkillsOptions
        {
            EnableReadSkillTool = true,
            EnableReadFileTool = false,
            EnableListDirectoryTool = true,
            EnableExecuteScriptTool = true,
            AllowedScriptExtensions = [".py"],
            ScriptTimeoutSeconds = 60,
            EnableRunCommandTool = true,
            AllowedCommands = ["git", "npm"],
            CommandTimeoutSeconds = 45,
            MaxOutputSizeBytes = 100000
        };

        // Act
        var toolsOptions = SkillsToolsOptions.FromSkillsOptions(skillsOptions);

        // Assert
        toolsOptions.EnableReadSkillTool.Should().BeTrue();
        toolsOptions.EnableReadFileTool.Should().BeFalse();
        toolsOptions.EnableListDirectoryTool.Should().BeTrue();
        toolsOptions.EnableExecuteScriptTool.Should().BeTrue();
        toolsOptions.AllowedScriptExtensions.Should().ContainSingle().Which.Should().Be(".py");
        toolsOptions.ScriptTimeoutSeconds.Should().Be(60);
        toolsOptions.EnableRunCommandTool.Should().BeTrue();
        toolsOptions.AllowedCommands.Should().Contain("git");
        toolsOptions.AllowedCommands.Should().Contain("npm");
        toolsOptions.CommandTimeoutSeconds.Should().Be(45);
        toolsOptions.MaxOutputSizeBytes.Should().Be(100000);
    }
}
