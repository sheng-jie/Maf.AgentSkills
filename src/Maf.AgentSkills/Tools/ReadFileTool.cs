// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Text.Json;
using Maf.AgentSkills.Loading;
using Maf.AgentSkills.Models;

namespace Maf.AgentSkills.Tools;

/// <summary>
/// Tool that reads files from within skill directories.
/// </summary>
public sealed class ReadFileTool
{
    private readonly Func<SkillsState> _stateProvider;
    private readonly int _maxFileSizeBytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadFileTool"/> class.
    /// </summary>
    /// <param name="stateProvider">Function that returns the current skills state.</param>
    /// <param name="maxFileSizeBytes">Maximum file size to read in bytes. Default is 1 MB.</param>
    public ReadFileTool(Func<SkillsState> stateProvider, int maxFileSizeBytes = 1024 * 1024)
    {
        _stateProvider = stateProvider;
        _maxFileSizeBytes = maxFileSizeBytes;
    }

    /// <summary>
    /// Gets the tool name.
    /// </summary>
    public static string ToolName => "read_skill_file";

    /// <summary>
    /// Reads a file from within a skill's directory.
    /// </summary>
    /// <param name="skillName">The name of the skill containing the file.</param>
    /// <param name="relativePath">The relative path to the file within the skill directory.</param>
    /// <returns>The file content or an error message.</returns>
    [Description("Reads a file from within a skill's directory. Use this to access templates, scripts, or other resources that a skill provides.")]
    public string ReadFile(
        [Description("The name of the skill containing the file")]
        string skillName,
        [Description("The relative path to the file within the skill directory (e.g., 'templates/checklist.md', 'search.py')")]
        string relativePath)
    {
        var state = _stateProvider();
        var skill = state.GetSkill(skillName);

        if (skill is null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = $"Skill '{skillName}' not found."
            });
        }

        // Validate and resolve the path safely
        var safePath = PathSecurity.ResolveSafePath(skill.Path, relativePath);
        if (safePath is null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Invalid path: path traversal not allowed."
            });
        }

        if (!File.Exists(safePath))
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = $"File not found: {relativePath}"
            });
        }

        try
        {
            var fileInfo = new FileInfo(safePath);
            if (fileInfo.Length > _maxFileSizeBytes)
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = $"File too large. Maximum size is {_maxFileSizeBytes / 1024} KB."
                });
            }

            var content = File.ReadAllText(safePath);
            return JsonSerializer.Serialize(new
            {
                success = true,
                skill_name = skillName,
                file_path = relativePath,
                size_bytes = fileInfo.Length,
                content
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = $"Failed to read file: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Creates an <see cref="Microsoft.Extensions.AI.AIFunction"/> for this tool.
    /// </summary>
    public Microsoft.Extensions.AI.AIFunction ToAIFunction()
    {
        return Microsoft.Extensions.AI.AIFunctionFactory.Create(ReadFile, ToolName);
    }
}
