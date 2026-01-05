// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Maf.AgentSkills.Tools;
using Xunit;

namespace Maf.AgentSkills.Tests.Tools;

public class SkillsToolFactoryTests
{
    [Fact]
    public void AllToolNames_ShouldContainAllTools()
    {
        // Assert
        SkillsToolFactory.AllToolNames.Should().Contain("read_skill");
        SkillsToolFactory.AllToolNames.Should().Contain("read_skill_file");
        SkillsToolFactory.AllToolNames.Should().Contain("list_skill_directory");
        SkillsToolFactory.AllToolNames.Should().Contain("execute_skill_script");
        SkillsToolFactory.AllToolNames.Should().Contain("run_skill_command");
    }

    [Fact]
    public void DefaultEnabledToolNames_ShouldContainSafeToolsOnly()
    {
        // Assert
        SkillsToolFactory.DefaultEnabledToolNames.Should().Contain("read_skill");
        SkillsToolFactory.DefaultEnabledToolNames.Should().Contain("read_skill_file");
        SkillsToolFactory.DefaultEnabledToolNames.Should().Contain("list_skill_directory");
        
        SkillsToolFactory.DefaultEnabledToolNames.Should().NotContain("execute_skill_script");
        SkillsToolFactory.DefaultEnabledToolNames.Should().NotContain("run_skill_command");
    }

    [Fact]
    public void OptInToolNames_ShouldContainDangerousToolsOnly()
    {
        // Assert
        SkillsToolFactory.OptInToolNames.Should().Contain("execute_skill_script");
        SkillsToolFactory.OptInToolNames.Should().Contain("run_skill_command");
        
        SkillsToolFactory.OptInToolNames.Should().NotContain("read_skill");
        SkillsToolFactory.OptInToolNames.Should().NotContain("read_skill_file");
        SkillsToolFactory.OptInToolNames.Should().NotContain("list_skill_directory");
    }
}
