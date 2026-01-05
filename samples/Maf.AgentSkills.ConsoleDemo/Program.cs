// Basic usage example of Maf.AgentSkills with the new AIContextProviderFactory pattern
// This example demonstrates how to create a skills-enabled agent using the MAF pattern.

using Maf.AgentSkills.Agent;
using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel.Primitives;

Console.WriteLine("=== Maf.AgentSkills Basic Usage Example (AIContextProviderFactory Pattern) ===");
Console.WriteLine();

// Get API credentials
var apiKey = Keys.QwenApiKey;
if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("Error: API key not set in Keys.cs");
    Console.WriteLine("Please set your API key to run this example.");
    return;
}

// Create ChatClient
var clientOptions = new OpenAIClientOptions();
clientOptions.Endpoint = new Uri(Keys.QwenEndpoint);

var aiClient = new OpenAIClient(new ApiKeyCredential(apiKey), clientOptions);
var chatClient = aiClient.GetChatClient("qwen-max").AsIChatClient();

Console.WriteLine("Creating skills-enabled agent using AIContextProviderFactory pattern...");
Console.WriteLine();

// Create skills-enabled agent using the new factory pattern
var agent = chatClient.CreateSkillsAgent(
    configureSkills: options =>
    {
        options.AgentName = "my-assistant";
        options.ProjectRoot = Directory.GetCurrentDirectory();
        
        // Enable tools
        options.ToolsOptions.EnableReadSkillTool = true;
        options.ToolsOptions.EnableReadFileTool = true;
        options.ToolsOptions.EnableListDirectoryTool = true;
        options.ToolsOptions.EnableRunCommandTool = true;
    },
    configureAgent: options =>
    {
        options.ChatOptions = new() 
        { 
            Instructions = "You are a helpful assistant with access to specialized skills." 
        };
    });

// Create a conversation thread
var thread = agent.GetNewThread();

// Example 1: Ask about available skills
Console.WriteLine("User: What skills do you have available?");
Console.WriteLine();

var response = await agent.RunAsync(
    "What skills do you have available? Please list them.",
    thread);

Console.WriteLine($"Assistant: {response.Text}");
Console.WriteLine();

// Example 2: Ask to use a specific skill
Console.WriteLine("User: Can you read the web-research skill and tell me what it does?");
Console.WriteLine();

response = await agent.RunAsync(
    "Can you read the web-research skill and tell me what it does?",
    thread);

Console.WriteLine($"Assistant: {response.Text}");
Console.WriteLine();

// Demonstrate thread serialization
Console.WriteLine("--- Thread Serialization Demo ---");
Console.WriteLine();

var serializedThread = thread.Serialize();
Console.WriteLine($"Thread serialized successfully");
Console.WriteLine($"Serialized data length: {serializedThread.ToString().Length} characters");
Console.WriteLine();

// Deserialize and continue conversation
var restoredThread = agent.DeserializeThread(serializedThread);

var path = "E:\\GitHub\\My\\dotnet-agent-skills\\NET+AI：技术栈全景解密.pdf";
response = await agent.RunAsync($"请将指定目录：{path}的文件拆分前3页",
    restoredThread);

Console.WriteLine($"Assistant: {response.Text}");
Console.WriteLine();
Console.WriteLine("=== Demo Complete ===");
