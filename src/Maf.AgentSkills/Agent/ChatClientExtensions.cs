// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using Maf.AgentSkills.Context;
using Maf.AgentSkills.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Maf.AgentSkills.Agent;

/// <summary>
/// Extension methods for creating skills-enabled AI agents from ChatClient.
/// </summary>
public static class ChatClientExtensions
{
    /// <summary>
    /// Creates an AI Agent with skills support using the AIContextProviderFactory pattern.
    /// </summary>
    /// <param name="chatClient">The chat client.</param>
    /// <param name="configureSkills">Optional callback to configure skills options.</param>
    /// <param name="configureAgent">Optional callback to configure agent options.</param>
    /// <returns>An AI Agent with skills support enabled.</returns>
    /// <example>
    /// <code>
    /// var agent = chatClient.CreateSkillsAgent(
    ///     configureSkills: options =>
    ///     {
    ///         options.AgentName = "my-assistant";
    ///         options.ProjectRoot = Directory.GetCurrentDirectory();
    ///     },
    ///     configureAgent: options =>
    ///     {
    ///         options.ChatOptions = new() { Instructions = "You are a helpful assistant." };
    ///     });
    /// </code>
    /// </example>
    public static AIAgent CreateSkillsAgent(
        this IChatClient chatClient,
        Action<SkillsOptions>? configureSkills = null,
        Action<ChatClientAgentOptions>? configureAgent = null)
    {
        ArgumentNullException.ThrowIfNull(chatClient);

        var skillsOptions = new SkillsOptions();
        configureSkills?.Invoke(skillsOptions);

        var agentOptions = new ChatClientAgentOptions
        {
            AIContextProviderFactory = ctx =>
            {
                // Check if we're restoring from serialized state
                if (ctx.SerializedState.ValueKind != JsonValueKind.Undefined)
                {
                    return new SkillsContextProvider(
                        chatClient,
                        ctx.SerializedState,
                        ctx.JsonSerializerOptions);
                }

                // Create new instance
                return new SkillsContextProvider(
                    chatClient,
                    skillsOptions);
            }
        };

        configureAgent?.Invoke(agentOptions);

        return chatClient.CreateAIAgent(agentOptions);
    }
}
