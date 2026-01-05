// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Maf.AgentSkills.Loading;
using Maf.AgentSkills.Models;
using Xunit;

namespace Maf.AgentSkills.Tests.Loading;

public class SkillParserTests
{
    private readonly SkillParser _parser = new();

    [Fact]
    public void ParseContent_WithValidFrontmatter_ShouldParseCorrectly()
    {
        // Arrange
        var content = """
            ---
            name: web-research
            description: A skill for conducting web research
            license: MIT
            compatibility: any
            ---

            # Web Research Skill

            This skill helps you research topics on the web.
            """;

        // Act
        var result = _parser.ParseContent(content, "/skills/web-research", "web-research", SkillSource.User);

        // Assert
        result.Name.Should().Be("web-research");
        result.Description.Should().Be("A skill for conducting web research");
        result.License.Should().Be("MIT");
        result.Compatibility.Should().Be("any");
        result.Path.Should().Be("/skills/web-research");
        result.Source.Should().Be(SkillSource.User);
    }

    [Fact]
    public void ParseContent_WithAllowedTools_ShouldParseToolsList()
    {
        // Arrange
        var content = """
            ---
            name: web-research
            description: A skill for web research
            allowed-tools: read_file execute_* web_search
            ---

            Content here.
            """;

        // Act
        var result = _parser.ParseContent(content, "/skills/web-research", "web-research", SkillSource.User);

        // Assert
        result.AllowedTools.Should().NotBeNull();
        result.AllowedTools.Should().HaveCount(3);
        result.AllowedTools![0].Name.Should().Be("read_file");
        result.AllowedTools[0].IsPattern.Should().BeFalse();
        result.AllowedTools[1].Name.Should().Be("execute_*");
        result.AllowedTools[1].IsPattern.Should().BeTrue();
    }

    [Fact]
    public void ParseContent_WithMetadata_ShouldParseMetadataDict()
    {
        // Arrange
        var content = """
            ---
            name: web-research
            description: A skill for web research
            metadata:
              author: John Doe
              version: 1.0.0
            ---

            Content here.
            """;

        // Act
        var result = _parser.ParseContent(content, "/skills/web-research", "web-research", SkillSource.User);

        // Assert
        result.Metadata.Should().NotBeNull();
        result.Metadata.Should().ContainKey("author");
        result.Metadata!["author"].Should().Be("John Doe");
        result.Metadata.Should().ContainKey("version");
        result.Metadata["version"].Should().Be("1.0.0");
    }

    [Fact]
    public void ParseContent_WithoutFrontmatter_ShouldThrow()
    {
        // Arrange
        var content = """
            # Web Research Skill

            No frontmatter here.
            """;

        // Act
        var act = () => _parser.ParseContent(content, "/skills/web-research", "web-research", SkillSource.User);

        // Assert
        act.Should().Throw<SkillParseException>()
            .WithMessage("*frontmatter*");
    }

    [Fact]
    public void ParseContent_WithMissingName_ShouldThrow()
    {
        // Arrange
        var content = """
            ---
            description: A skill for web research
            ---

            Content here.
            """;

        // Act
        var act = () => _parser.ParseContent(content, "/skills/web-research", "web-research", SkillSource.User);

        // Assert
        act.Should().Throw<SkillParseException>()
            .WithMessage("*name*required*");
    }

    [Fact]
    public void ParseContent_WithMissingDescription_ShouldThrow()
    {
        // Arrange
        var content = """
            ---
            name: web-research
            ---

            Content here.
            """;

        // Act
        var act = () => _parser.ParseContent(content, "/skills/web-research", "web-research", SkillSource.User);

        // Assert
        act.Should().Throw<SkillParseException>()
            .WithMessage("*description*required*");
    }

    [Fact]
    public void ParseContent_WithNameMismatch_ShouldThrow()
    {
        // Arrange
        var content = """
            ---
            name: different-name
            description: A skill
            ---

            Content here.
            """;

        // Act
        var act = () => _parser.ParseContent(content, "/skills/web-research", "web-research", SkillSource.User);

        // Assert
        act.Should().Throw<SkillParseException>()
            .WithMessage("*does not match*");
    }

    [Fact]
    public void ParseContent_WithInvalidNameFormat_ShouldThrow()
    {
        // Arrange
        var content = """
            ---
            name: Invalid_Name
            description: A skill
            ---

            Content here.
            """;

        // Act
        var act = () => _parser.ParseContent(content, "/skills/Invalid_Name", "Invalid_Name", SkillSource.User);

        // Assert
        act.Should().Throw<SkillParseException>()
            .WithMessage("*lowercase*");
    }

    [Fact]
    public void ParseContent_WithUnclosedFrontmatter_ShouldThrow()
    {
        // Arrange
        var content = """
            ---
            name: web-research
            description: A skill
            
            No closing delimiter
            """;

        // Act
        var act = () => _parser.ParseContent(content, "/skills/web-research", "web-research", SkillSource.User);

        // Assert
        act.Should().Throw<SkillParseException>()
            .WithMessage("*frontmatter*");
    }
}
