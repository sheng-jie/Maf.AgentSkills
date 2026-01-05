// Dependency Injection example for Maf.AgentSkills with AIContextProviderFactory pattern
// This example demonstrates how to integrate skills with MAF Agent using DI.

using Maf.AgentSkills.Agent;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.ClientModel;
using OpenAI;
using Microsoft.Extensions.Logging;
using System.ClientModel.Primitives;

Console.WriteLine("=== Maf.AgentSkills DI Example (AIContextProviderFactory Pattern) ===");
Console.WriteLine();


// =============================================================================
// Build the host with DI
// =============================================================================

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(options => options.SetMinimumLevel(LogLevel.Trace).AddConsole());

// Register ChatClient
builder.Services.AddChatClient(sp =>
{
    var clientOptions = new OpenAIClientOptions();
    clientOptions.Endpoint = new Uri(Keys.QwenEndpoint);

    var loggingOptions = new ClientLoggingOptions();

    loggingOptions.LoggerFactory = sp.GetRequiredService<ILoggerFactory>();

    loggingOptions.EnableLogging = true;                    // 总开关：启用日志
    loggingOptions.EnableMessageLogging = true;             // 记录请求/响应的行与头
    loggingOptions.EnableMessageContentLogging = true;      // 记录请求/响应的完整内容
    loggingOptions.MessageContentSizeLimit = 64 * 1024;     // 增大到 64KB

    // 可选：白名单（避免默认打码影响诊断）
    loggingOptions.AllowedHeaderNames.Add("Content-Type");
    loggingOptions.AllowedHeaderNames.Add("Accept");
    loggingOptions.AllowedHeaderNames.Add("Content-Length");
    loggingOptions.AllowedQueryParameters.Add("api-version");

    clientOptions.ClientLoggingOptions = loggingOptions;

    var aiClient = new OpenAIClient(new ApiKeyCredential(Keys.QwenApiKey), clientOptions);
    var chatClient = aiClient.GetChatClient("qwen-max").AsIChatClient();
    return chatClient;
});

// Register skills-enabled AIAgent using the new factory pattern
builder.Services.AddSingleton<AIAgent>(sp =>
{
    var chatClient = sp.GetRequiredService<IChatClient>();

    return chatClient.CreateSkillsAgent(
        configureSkills: options =>
        {
            options.AgentName = "di-agent";
            options.ProjectRoot = Directory.GetCurrentDirectory();

            // Configure tools
            options.ToolsOptions.EnableReadSkillTool = true;
            options.ToolsOptions.EnableReadFileTool = true;
            options.ToolsOptions.EnableListDirectoryTool = true;
            options.ToolsOptions.EnableExecuteScriptTool = true;
        },
        configureAgent: options =>
        {
            options.ChatOptions = new()
            {
                Instructions = "You are a helpful assistant with access to specialized skills."
            };
        });
});

var host = builder.Build();

// =============================================================================
// Use the agent
// =============================================================================

using var scope = host.Services.CreateScope();
var agent = scope.ServiceProvider.GetRequiredService<AIAgent>();

var thread = agent.GetNewThread();

var path = "E:\\GitHub\\My\\dotnet-agent-skills\\NET+AI：技术栈全景解密.pdf";
var response = await agent.RunAsync($"请将指定目录：{path}的文件拆分前3页后，并总结第2页的内容。",
    thread);

Console.WriteLine($"Assistant: {response.Text}");
Console.WriteLine();
Console.WriteLine("=== Demo Complete ===");
