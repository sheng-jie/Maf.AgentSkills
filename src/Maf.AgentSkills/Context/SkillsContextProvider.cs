// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Maf.AgentSkills.Configuration;
using Maf.AgentSkills.Loading;
using Maf.AgentSkills.Models;
using Maf.AgentSkills.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Maf.AgentSkills.Context;

/// <summary>
/// Provides skills context to AI agents using the AIContextProvider pattern.
/// Implements the MAF recommended AIContextProviderFactory approach.
/// </summary>
/// <remarks>
/// This provider follows the Agent Skills specification (https://agentskills.io)
/// and supports thread serialization/deserialization for durable conversations.
/// </remarks>
public sealed class SkillsContextProvider : AIContextProvider
{
    private readonly IChatClient _chatClient;
    private readonly SkillLoader _skillLoader;
    private readonly SkillsOptions _options;
    private SkillsState _state;

    /// <summary>
    /// Creates a new skills context provider instance.
    /// Used when creating a new agent or thread.
    /// </summary>
    /// <param name="chatClient">The chat client.</param>
    /// <param name="options">Optional skills configuration options.</param>
    public SkillsContextProvider(IChatClient chatClient, SkillsOptions? options = null)
    {
        _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
        _options = options ?? new SkillsOptions();
        
        var settings = new SkillsSettings(_options.AgentName, _options.ProjectRoot);
        _skillLoader = new SkillLoader();
        _state = new SkillsState();

        // Auto-load skills on startup (always load initially)
        LoadSkills(settings);
    }

    /// <summary>
    /// Restores a skills context provider from serialized state.
    /// Used when deserializing a thread for conversation continuity.
    /// </summary>
    /// <param name="chatClient">The chat client.</param>
    /// <param name="serializedState">The serialized state from a previous session.</param>
    /// <param name="jsonSerializerOptions">Optional JSON serialization options.</param>
    public SkillsContextProvider(
        IChatClient chatClient,
        JsonElement serializedState,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));

        // Deserialize state
        if (serializedState.ValueKind == JsonValueKind.Object)
        {
            _options = serializedState.TryGetProperty("Options", out var optionsElement)
                ? optionsElement.Deserialize<SkillsOptions>(jsonSerializerOptions) ?? new SkillsOptions()
                : new SkillsOptions();

            _state = serializedState.TryGetProperty("State", out var stateElement)
                ? stateElement.Deserialize<SkillsState>(jsonSerializerOptions) ?? new SkillsState()
                : new SkillsState();
        }
        else
        {
            _options = new SkillsOptions();
            _state = new SkillsState();
        }

        var settings = new SkillsSettings(_options.AgentName, _options.ProjectRoot);
        _skillLoader = new SkillLoader();
    }

    /// <summary>
    /// Called before agent invocation to provide skills context.
    /// </summary>
    public override ValueTask<AIContext> InvokingAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        // Refresh skills if needed or if this is the first load
        if (_state.AllSkills.Count == 0)
        {
            var settings = new SkillsSettings(_options.AgentName, _options.ProjectRoot);
            LoadSkills(settings);
        }

        // Generate skills system prompt
        var instructions = GenerateSkillsPrompt(_state.AllSkills);

        // Create skill tools
        var tools = CreateSkillsTools(_state);

        return ValueTask.FromResult(new AIContext
        {
            Instructions = instructions,
            Tools = tools
        });
    }

    /// <summary>
    /// Serializes the provider state for thread persistence.
    /// </summary>
    public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        var state = new
        {
            Options = _options,
            State = _state
        };

        return JsonSerializer.SerializeToElement(state, jsonSerializerOptions);
    }

    /// <summary>
    /// Loads skills from configured directories.
    /// </summary>
    private void LoadSkills(SkillsSettings settings)
    {
        var skills = new Dictionary<string, SkillMetadata>(StringComparer.OrdinalIgnoreCase);

        // Load user-level skills
        var userDir = settings.GetUserSkillsDirectory();
        if (Directory.Exists(userDir))
        {
            foreach (var skill in _skillLoader.LoadSkillsFromDirectory(userDir, SkillSource.User))
            {
                skills[skill.Name] = skill;
            }
        }

        // Load project-level skills (overrides user-level with same name)
        var projectDir = settings.GetProjectSkillsDirectory();
        if (projectDir != null && Directory.Exists(projectDir))
        {
            foreach (var skill in _skillLoader.LoadSkillsFromDirectory(projectDir, SkillSource.Project))
            {
                skills[skill.Name] = skill;
            }
        }

        _state = new SkillsState
        {
            UserSkills = skills.Values.Where(s => s.Source == SkillSource.User).ToList(),
            ProjectSkills = skills.Values.Where(s => s.Source == SkillSource.Project).ToList(),
            LastRefreshed = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Generates the skills system prompt using the progressive disclosure pattern.
    /// </summary>
    private string GenerateSkillsPrompt(IReadOnlyList<SkillMetadata> skills)
    {
        if (skills.Count == 0)
        {
            return string.Empty;
        }

        // Generate locations display
        var settings = new SkillsSettings(_options.AgentName, _options.ProjectRoot);
        var locationsDisplay = GenerateLocationsDisplay(settings);

        // Use the centralized prompt template
        return SkillsPromptTemplates.GenerateSkillsPrompt(_state, locationsDisplay);
    }

    /// <summary>
    /// Generates the skills locations display string.
    /// </summary>
    private static string GenerateLocationsDisplay(SkillsSettings settings)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"**User Skills**: `{settings.GetUserSkillsDirectory()}`");

        var projectDir = settings.GetProjectSkillsDirectory();
        if (projectDir != null)
        {
            sb.AppendLine($"**Project Skills**: `{projectDir}` (takes precedence over user skills)");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Creates skill-related tools.
    /// </summary>
    private List<AITool> CreateSkillsTools(SkillsState state)
    {
        var factory = new SkillsToolFactory(_skillLoader, () => state, _options.ToolsOptions);
        return factory.CreateTools().ToList();
    }
}
