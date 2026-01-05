// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Maf.AgentSkills.Models;
using Xunit;

namespace Maf.AgentSkills.Tests.Models;

public class AllowedToolTests
{
    [Theory]
    [InlineData("read_file", "read_file", true)]
    [InlineData("read_file", "READ_FILE", true)]
    [InlineData("read_file", "write_file", false)]
    [InlineData("execute_*", "execute_script", true)]
    [InlineData("execute_*", "execute_command", true)]
    [InlineData("execute_*", "read_file", false)]
    [InlineData("*_file", "read_file", true)]
    [InlineData("*_file", "write_file", true)]
    [InlineData("*_file", "execute_script", false)]
    [InlineData("*", "anything", true)]
    public void Matches_ShouldReturnExpectedResult(string pattern, string toolName, bool expected)
    {
        // Arrange
        var isPattern = pattern.Contains('*');
        var allowedTool = new AllowedTool(pattern, isPattern);

        // Act
        var result = allowedTool.Matches(toolName);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Parse_WithNullOrEmpty_ShouldReturnEmptyList()
    {
        // Act
        var result1 = AllowedTool.Parse(null);
        var result2 = AllowedTool.Parse("");
        var result3 = AllowedTool.Parse("   ");

        // Assert
        result1.Should().BeEmpty();
        result2.Should().BeEmpty();
        result3.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WithSingleTool_ShouldReturnSingleItem()
    {
        // Act
        var result = AllowedTool.Parse("read_file");

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("read_file");
        result[0].IsPattern.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithMultipleTools_ShouldReturnAllItems()
    {
        // Act
        var result = AllowedTool.Parse("read_file write_file execute_*");

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("read_file");
        result[0].IsPattern.Should().BeFalse();
        result[1].Name.Should().Be("write_file");
        result[1].IsPattern.Should().BeFalse();
        result[2].Name.Should().Be("execute_*");
        result[2].IsPattern.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithExtraSpaces_ShouldTrimAndParse()
    {
        // Act
        var result = AllowedTool.Parse("  read_file   write_file  ");

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("read_file");
        result[1].Name.Should().Be("write_file");
    }
}
