# Maf.AgentSkills

[![NuGet](https://img.shields.io/nuget/v/Maf.AgentSkills.svg)](https://www.nuget.org/packages/Maf.AgentSkills)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

Agent Skills integration for Microsoft Agent Framework (MAF). Enables AI agents to leverage reusable skills following the [Agent Skills specification](https://agentskills.io).

## Features

- 🎯 **Progressive Disclosure**: Skills are listed by metadata, with full instructions loaded on-demand
- 🔧 **Built-in Tools**: ReadSkill, ReadFile, ListDirectory, ExecuteScript, RunCommand
- 🔒 **Security First**: Script and command execution disabled by default
- 📦 **MAF Native**: Uses AIContextProviderFactory pattern for seamless integration
- 🔄 **Thread Serialization**: Full support for durable conversations
- 💉 **DI Friendly**: Easy dependency injection with Microsoft.Extensions.AI
- ✅ **Validated**: Automatic skill validation following Agent Skills specification
- ⚡ **Fluent API**: Chainable configuration methods for script and command execution

## Installation

```bash
dotnet add package Maf.AgentSkills
```

> **Note**: Requires .NET 10.0 or later

## Quick Start

### Basic Usage

```csharp
using Maf.AgentSkills.Agent;
using Microsoft.Extensions.AI;
using OpenAI;

// Create ChatClient
var chatClient = new OpenAIClient(apiKey)
    .GetChatClient("gpt-4")
    .AsIChatClient();

// Create skills-enabled agent
var agent = chatClient.CreateSkillsAgent(
    configureSkills: options =>
    {
        options.AgentName = "my-assistant";
        options.ProjectRoot = Directory.GetCurrentDirectory();
    },
    configureAgent: options =>
    {
        options.ChatOptions = new() 
        { 
            Instructions = "You are a helpful assistant." 
        };
    });

// Use the agent
var thread = agent.GetNewThread();
var response = await agent.RunAsync("What skills do you have?", thread);
Console.WriteLine(response.Text);
```

### Thread Serialization

```csharp
// Serialize thread for persistence
var serializedThread = thread.Serialize();

// Save to database, file, etc.
await SaveThreadAsync(userId, serializedThread);

// Later, restore and continue conversation
var restoredThread = agent.DeserializeThread(serializedThread);
var response = await agent.RunAsync("Continue our chat", restoredThread);
```

### With Dependency Injection

```csharp
using Maf.AgentSkills.Agent;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI;

var builder = Host.CreateApplicationBuilder(args);

// Register ChatClient
builder.Services.AddChatClient(sp =>
{
    return new OpenAIClient(apiKey)
        .GetChatClient("gpt-4")
        .AsIChatClient();
});

// Register skills-enabled agent
builder.Services.AddSingleton<AIAgent>(sp =>
{
    var chatClient = sp.GetRequiredService<IChatClient>();
    
    return chatClient.CreateSkillsAgent(
        configureSkills: options =>
        {
            options.AgentName = "my-assistant";
            options.ProjectRoot = Directory.GetCurrentDirectory();
        });
});

var host = builder.Build();

// Use from DI
var agent = host.Services.GetRequiredService<AIAgent>();
var thread = agent.GetNewThread();
var response = await agent.RunAsync("Hello!", thread);
```

## Creating Skills

Skills are defined in SKILL.md files with YAML frontmatter:

```markdown
---
name: web-research
description: A skill for conducting comprehensive web research
license: MIT
allowed-tools: web_search fetch_url
---

# Web Research Skill

## When to Use
Use this skill when researching topics...

## Instructions
1. Clarify the research scope
2. Search strategically
3. Synthesize information
...
```

### Skill Location

Skills are loaded from two locations:

- **User Skills**: `~/.maf/{agent-name}/skills/`
- **Project Skills**: `{project-root}/.maf/skills/`

Project skills take precedence over user skills when names conflict.

## Configuration Options

```csharp
var agent = chatClient.CreateSkillsAgent(
    configureSkills: options =>
    {
        // Basic configuration
        options.AgentName = "my-assistant";           // Agent name for user skills path
        options.ProjectRoot = "/path/to/project";     // Project root for project skills
        
        // Skill sources
        options.EnableUserSkills = true;              // Enable ~/.maf/{agent}/skills/
        options.EnableProjectSkills = true;           // Enable {project}/.maf/skills/
        options.UserSkillsDir = null;                 // Override user skills path
        options.ProjectSkillsDir = null;              // Override project skills path
        
        // Caching & validation
        options.CacheSkills = true;                   // Cache loaded skills (default: true)
        options.ValidateOnStartup = true;             // Validate skills on load (default: true)
        options.AutoRefreshSkills = false;            // Auto-refresh on each run (default: false)
        options.SkillsCacheDurationSeconds = 300;     // Cache duration (default: 5 min)
        
        // Tool configuration
        options.EnableReadSkillTool = true;           // Read SKILL.md content
        options.EnableReadFileTool = true;            // Read files in skill directories
        options.EnableListDirectoryTool = true;       // List skill directory contents
        
        // Script execution (disabled by default) - Fluent API
        options.EnableScriptExecution(
            allowedExtensions: [".py", ".ps1", ".sh", ".cs"],
            timeoutSeconds: 30);
        
        // Or configure manually
        options.EnableExecuteScriptTool = true;
        options.AllowedScriptExtensions = [".py", ".ps1"];
        options.ScriptTimeoutSeconds = 30;
        
        // Command execution (disabled by default) - Fluent API
        options.EnableCommandExecution(
            allowedCommands: ["git", "npm", "dotnet"],
            timeoutSeconds: 30);
        
        // Or configure manually
        options.EnableRunCommandTool = true;
        options.AllowedCommands = ["git", "npm", "dotnet"];
        options.CommandTimeoutSeconds = 30;
        
        // Output limits
        options.MaxOutputSizeBytes = 50 * 1024;       // Max output size (default: 50KB)
    },
    configureAgent: options =>
    {
        // Standard MAF agent configuration
        options.ChatOptions = new()
        {
            Instructions = "You are a helpful assistant.",
        };
    });
```

## Built-in Tools

| Tool | Description | Default |
|------|-------------|---------|
| `read_skill` | Reads full SKILL.md content | ✅ Enabled |
| `read_skill_file` | Reads files within skill directories | ✅ Enabled |
| `list_skill_directory` | Lists skill directory contents | ✅ Enabled |
| `execute_skill_script` | Executes scripts (.py, .ps1, .sh, .cs) | ❌ Disabled |
| `run_skill_command` | Runs whitelisted commands | ❌ Disabled |

## Security

- **Path Traversal Protection**: All file operations validate paths stay within skill directories
- **Script Execution**: Disabled by default, requires explicit opt-in with extension whitelist
- **Command Execution**: Disabled by default, requires explicit whitelist of allowed commands
- **Symlink Protection**: Symbolic links are validated to prevent escape attacks
- **Output Truncation**: Script/command output is truncated to prevent context overflow

## Examples

See the [samples](samples/) directory for:

- [Console Demo](samples/Maf.AgentSkills.ConsoleDemo/) - Basic console application
- [Web Demo](samples/Maf.AgentSkills.WebDemo/) - ASP.NET Core with dependency injection

## Dependencies

- [Microsoft.Agents.AI](https://github.com/microsoft/agent-framework) - Microsoft Agent Framework
- [Microsoft.Extensions.AI](https://github.com/dotnet/extensions) - AI abstractions
- [YamlDotNet](https://github.com/aaubry/YamlDotNet) - YAML frontmatter parsing

## Contributing

Contributions are welcome! Please read our contributing guidelines before submitting PRs.

## License

MIT License - see [LICENSE](LICENSE) for details.

## Migration from v1.x

If you're upgrading from v1.x (decorator pattern), here are the key changes:

### Old API (v1.x)
```csharp
var baseAgent = chatClient.CreateAIAgent(...);
var skillsAgent = baseAgent.AsBuilder()
    .UseSkills(options => { ... })
    .Build();
```

### New API (v2.x)
```csharp
var agent = chatClient.CreateSkillsAgent(
    configureSkills: options => { ... },
    configureAgent: options => { ... });
```

**Key Changes:**
- ✅ Use `CreateSkillsAgent` instead of `AsBuilder().UseSkills()`
- ✅ Directory changed from `.agentskills` to `.maf`
- ✅ Built-in support for thread serialization
- ✅ Simplified configuration with `SkillsOptions`

## Related Projects

- [Agent Skills Spec](https://agentskills.io)
- [Microsoft Agent Framework](https://github.com/microsoft/agent-framework)
- [Microsoft.Extensions.AI](https://github.com/dotnet/extensions)
- [DeepAgents](https://github.com/deepagents/deepagents)
