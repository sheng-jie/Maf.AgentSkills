// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Maf.AgentSkills.Loading;
using Xunit;

namespace Maf.AgentSkills.Tests.Loading;

public class PathSecurityTests
{
    [Fact]
    public void IsPathSafe_WithPathInsideBase_ShouldReturnTrue()
    {
        // Arrange
        var baseDir = Path.GetTempPath();
        var safePath = Path.Combine(baseDir, "subdir", "file.txt");

        // Act
        var result = PathSecurity.IsPathSafe(safePath, baseDir);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPathSafe_WithPathOutsideBase_ShouldReturnFalse()
    {
        // Arrange
        var baseDir = Path.Combine(Path.GetTempPath(), "restricted");
        var unsafePath = Path.Combine(Path.GetTempPath(), "other", "file.txt");

        // Act
        var result = PathSecurity.IsPathSafe(unsafePath, baseDir);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsPathSafe_WithTraversalAttempt_ShouldReturnFalse()
    {
        // Arrange
        var baseDir = Path.Combine(Path.GetTempPath(), "skills", "my-skill");
        var traversalPath = Path.Combine(baseDir, "..", "..", "secrets", "file.txt");

        // Act
        var result = PathSecurity.IsPathSafe(traversalPath, baseDir);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null, "/base")]
    [InlineData("/path", null)]
    [InlineData("", "/base")]
    [InlineData("/path", "")]
    public void IsPathSafe_WithNullOrEmpty_ShouldReturnFalse(string? path, string? baseDir)
    {
        // Act
        var result = PathSecurity.IsPathSafe(path!, baseDir!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsPathSafe_WithMultipleBases_ShouldCheckAll()
    {
        // Arrange
        var bases = new[] 
        { 
            Path.Combine(Path.GetTempPath(), "dir1"),
            Path.Combine(Path.GetTempPath(), "dir2")
        };
        var pathInDir2 = Path.Combine(bases[1], "file.txt");

        // Act
        var result = PathSecurity.IsPathSafe(pathInDir2, bases);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ResolveSafePath_WithValidRelativePath_ShouldResolve()
    {
        // Arrange
        var skillDir = Path.Combine(Path.GetTempPath(), "skill");
        var relativePath = "templates/checklist.md";

        // Act
        var result = PathSecurity.ResolveSafePath(skillDir, relativePath);

        // Assert
        result.Should().NotBeNull();
        result.Should().StartWith(skillDir);
        result.Should().EndWith("checklist.md");
    }

    [Fact]
    public void ResolveSafePath_WithTraversalAttempt_ShouldReturnNull()
    {
        // Arrange
        var skillDir = Path.Combine(Path.GetTempPath(), "skill");
        var maliciousPath = "../../../etc/passwd";

        // Act
        var result = PathSecurity.ResolveSafePath(skillDir, maliciousPath);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null, "relative")]
    [InlineData("/skill", null)]
    [InlineData("", "relative")]
    [InlineData("/skill", "")]
    public void ResolveSafePath_WithNullOrEmpty_ShouldReturnNull(string? skillDir, string? relativePath)
    {
        // Act
        var result = PathSecurity.ResolveSafePath(skillDir!, relativePath!);

        // Assert
        result.Should().BeNull();
    }
}
