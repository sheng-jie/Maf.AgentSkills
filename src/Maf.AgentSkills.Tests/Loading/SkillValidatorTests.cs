// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Maf.AgentSkills.Loading;
using Xunit;

namespace Maf.AgentSkills.Tests.Loading;

public class SkillValidatorTests
{
    [Theory]
    [InlineData("web-research")]
    [InlineData("code-review")]
    [InlineData("arxiv-search")]
    [InlineData("a")]
    [InlineData("skill123")]
    [InlineData("my-awesome-skill")]
    public void ValidateName_WithValidName_ShouldSucceed(string name)
    {
        // Act
        var result = SkillValidator.ValidateName(name);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateName_WithNullOrEmpty_ShouldFail(string? name)
    {
        // Act
        var result = SkillValidator.ValidateName(name);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("null or empty");
    }

    [Theory]
    [InlineData("Web-Research")] // uppercase
    [InlineData("web_research")] // underscore
    [InlineData("-web-research")] // starts with hyphen
    [InlineData("web-research-")] // ends with hyphen
    [InlineData("web research")] // space
    [InlineData("web.research")] // dot
    public void ValidateName_WithInvalidFormat_ShouldFail(string name)
    {
        // Act
        var result = SkillValidator.ValidateName(name);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("lowercase");
    }

    [Fact]
    public void ValidateName_WithTooLongName_ShouldFail()
    {
        // Arrange
        var longName = new string('a', 65);

        // Act
        var result = SkillValidator.ValidateName(longName);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("64");
    }

    [Fact]
    public void ValidateName_WithMaxLengthName_ShouldSucceed()
    {
        // Arrange
        var maxName = new string('a', 64);

        // Act
        var result = SkillValidator.ValidateName(maxName);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateNameMatchesDirectory_WhenMatch_ShouldSucceed()
    {
        // Act
        var result = SkillValidator.ValidateNameMatchesDirectory("web-research", "web-research");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateNameMatchesDirectory_WhenCaseInsensitiveMatch_ShouldSucceed()
    {
        // Act
        var result = SkillValidator.ValidateNameMatchesDirectory("web-research", "Web-Research");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateNameMatchesDirectory_WhenNoMatch_ShouldFail()
    {
        // Act
        var result = SkillValidator.ValidateNameMatchesDirectory("web-research", "different-name");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("does not match");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateDescription_WithNullOrEmpty_ShouldFail(string? description)
    {
        // Act
        var result = SkillValidator.ValidateDescription(description);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("null or empty");
    }

    [Fact]
    public void ValidateDescription_WithValidDescription_ShouldSucceed()
    {
        // Act
        var result = SkillValidator.ValidateDescription("A skill for web research");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateDescription_WithTooLongDescription_ShouldFail()
    {
        // Arrange
        var longDescription = new string('a', 1025);

        // Act
        var result = SkillValidator.ValidateDescription(longDescription);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("1024");
    }
}
