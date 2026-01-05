// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using FluentAssertions;
using Maf.AgentSkills.Agent;
using Maf.AgentSkills.Context;
using Maf.AgentSkills.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Moq;
using Xunit;

namespace Maf.AgentSkills.Tests.Agent;

/// <summary>
/// Tests for the <see cref="ChatClientExtensions"/> class.
/// </summary>
public class ChatClientExtensionsTests
{
    [Fact]
    public void CreateSkillsAgent_WithValidChatClient_CreatesAgent()
    {
        // Arrange
        var chatClient = new TestChatClient();

        // Act
        var agent = chatClient.CreateSkillsAgent(
            configureSkills: options =>
            {
                options.AgentName = "test-agent";
                options.EnableUserSkills = false;
                options.EnableProjectSkills = false;
            });

        // Assert
        agent.Should().NotBeNull();
    }

    [Fact]
    public void CreateSkillsAgent_WithNullChatClient_ThrowsArgumentNullException()
    {
        // Arrange
        IChatClient chatClient = null!;

        // Act & Assert
        var act = () => chatClient.CreateSkillsAgent();
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("chatClient");
    }

    [Fact]
    public void CreateSkillsAgent_WithOptions_ConfiguresCorrectly()
    {
        // Arrange
        var chatClient = new TestChatClient();
        var agentName = "custom-agent";
        var projectRoot = "/test/path";

        // Act
        var agent = chatClient.CreateSkillsAgent(
            configureSkills: options =>
            {
                options.AgentName = agentName;
                options.ProjectRoot = projectRoot;
                options.EnableUserSkills = true;
                options.EnableProjectSkills = true;
            });

        // Assert
        agent.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateSkillsAgent_RunAsync_ExecutesSuccessfully()
    {
        // Arrange
        var chatClient = new TestChatClient();
        var agent = chatClient.CreateSkillsAgent(
            configureSkills: options =>
            {
                options.AgentName = "test-agent";
                options.EnableUserSkills = false;
                options.EnableProjectSkills = false;
            });

        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "Hello")
        };

        // Act
        var response = await agent.RunAsync(messages);

        // Assert
        response.Should().NotBeNull();
        response.Messages.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateSkillsAgent_WithAgentOptions_ConfiguresAgent()
    {
        // Arrange
        var chatClient = new TestChatClient();
        var instructions = "You are a helpful assistant.";

        // Act
        var agent = chatClient.CreateSkillsAgent(
            configureSkills: options =>
            {
                options.EnableUserSkills = false;
                options.EnableProjectSkills = false;
            },
            configureAgent: options =>
            {
                options.ChatOptions = new() { Instructions = instructions };
            });

        // Assert
        agent.Should().NotBeNull();
    }

    [Fact]
    public void CreateSkillsAgent_WithToolsOptions_ConfiguresTools()
    {
        // Arrange
        var chatClient = new TestChatClient();

        // Act
        var agent = chatClient.CreateSkillsAgent(
            configureSkills: options =>
            {
                options.ToolsOptions.EnableReadSkillTool = true;
                options.ToolsOptions.EnableReadFileTool = true;
                options.ToolsOptions.EnableListDirectoryTool = false;
            });

        // Assert
        agent.Should().NotBeNull();
    }

    /// <summary>
    /// A simple test implementation of IChatClient for testing purposes.
    /// </summary>
    private class TestChatClient : IChatClient
    {
        public ChatClientMetadata Metadata => new("test-client");

        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> chatMessages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var response = new ChatResponse([
                new ChatMessage(ChatRole.Assistant, "Test response")
            ]);
            return Task.FromResult(response);
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> chatMessages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return AsyncEnumerable.Empty<ChatResponseUpdate>();
        }

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            return null;
        }

        public void Dispose() { }
    }
}
