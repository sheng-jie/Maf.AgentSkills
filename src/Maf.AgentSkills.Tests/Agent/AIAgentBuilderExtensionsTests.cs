// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using FluentAssertions;
using Maf.AgentSkills.Agent;
using Maf.AgentSkills.Context;
using Maf.AgentSkills.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Xunit;

namespace Maf.AgentSkills.Tests.Agent;

/// <summary>
/// Tests for the <see cref="SkillsContextProvider"/> serialization and deserialization.
/// </summary>
public class SkillsContextProviderTests
{
    [Fact]
    public void Constructor_WithValidChatClient_CreatesInstance()
    {
        // Arrange
        var chatClient = new TestChatClient();
        var options = new SkillsOptions
        {
            AgentName = "test-agent",
            EnableUserSkills = false,
            EnableProjectSkills = false
        };

        // Act
        var provider = new SkillsContextProvider(chatClient, options);

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullChatClient_ThrowsArgumentNullException()
    {
        // Arrange
        IChatClient chatClient = null!;

        // Act & Assert
        var act = () => new SkillsContextProvider(chatClient, null);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("chatClient");
    }

    [Fact]
    public void Constructor_WithDefaultOptions_CreatesInstance()
    {
        // Arrange
        var chatClient = new TestChatClient();

        // Act
        var provider = new SkillsContextProvider(chatClient);

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void Serialize_ReturnsValidJsonElement()
    {
        // Arrange
        var chatClient = new TestChatClient();
        var options = new SkillsOptions
        {
            AgentName = "test-agent",
            EnableUserSkills = false,
            EnableProjectSkills = false
        };
        var provider = new SkillsContextProvider(chatClient, options);

        // Act
        var serialized = provider.Serialize();

        // Assert
        serialized.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    public void Constructor_WithSerializedState_RestoresProvider()
    {
        // Arrange
        var chatClient = new TestChatClient();
        var originalOptions = new SkillsOptions
        {
            AgentName = "serialized-agent",
            EnableUserSkills = false,
            EnableProjectSkills = false
        };
        var originalProvider = new SkillsContextProvider(chatClient, originalOptions);
        var serializedState = originalProvider.Serialize();

        // Act
        var restoredProvider = new SkillsContextProvider(
            chatClient,
            serializedState);

        // Assert
        restoredProvider.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithOptionsAndNoSkills_CreatesValidProvider()
    {
        // Arrange
        var chatClient = new TestChatClient();
        var options = new SkillsOptions
        {
            AgentName = "test-agent",
            EnableUserSkills = false,
            EnableProjectSkills = false
        };

        // Act
        var provider = new SkillsContextProvider(chatClient, options);

        // Assert
        provider.Should().NotBeNull();
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
