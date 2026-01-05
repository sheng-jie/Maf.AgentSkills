// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

namespace Maf.AgentSkills.Loading;

/// <summary>
/// Provides path security utilities to prevent directory traversal attacks.
/// </summary>
public static class PathSecurity
{
    /// <summary>
    /// Validates that a path is safely contained within an allowed base directory.
    /// Prevents directory traversal attacks using "..", symlinks, etc.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <param name="allowedBaseDirectory">The base directory the path must be contained within.</param>
    /// <returns>True if the path is safe and contained within the base directory; otherwise, false.</returns>
    public static bool IsPathSafe(string path, string allowedBaseDirectory)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(allowedBaseDirectory))
        {
            return false;
        }

        try
        {
            // Get the full, normalized paths
            var normalizedPath = Path.GetFullPath(path);
            var normalizedBase = Path.GetFullPath(allowedBaseDirectory);

            // Ensure base path ends with directory separator for proper prefix checking
            if (!normalizedBase.EndsWith(Path.DirectorySeparatorChar))
            {
                normalizedBase += Path.DirectorySeparatorChar;
            }

            // Check if the path starts with the base directory
            return normalizedPath.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            // Any exception during path resolution means the path is not safe
            return false;
        }
    }

    /// <summary>
    /// Validates that a path is safely contained within any of the allowed base directories.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <param name="allowedBaseDirectories">The base directories the path may be contained within.</param>
    /// <returns>True if the path is safe and contained within any allowed directory; otherwise, false.</returns>
    public static bool IsPathSafe(string path, IEnumerable<string> allowedBaseDirectories)
    {
        return allowedBaseDirectories.Any(baseDir => IsPathSafe(path, baseDir));
    }

    /// <summary>
    /// Resolves a relative path within a skill directory safely.
    /// </summary>
    /// <param name="skillDirectory">The skill directory (absolute path).</param>
    /// <param name="relativePath">The relative path within the skill.</param>
    /// <returns>The resolved absolute path if safe; otherwise, null.</returns>
    public static string? ResolveSafePath(string skillDirectory, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(skillDirectory) || string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        try
        {
            var combinedPath = Path.Combine(skillDirectory, relativePath);
            var resolvedPath = Path.GetFullPath(combinedPath);

            if (IsPathSafe(resolvedPath, skillDirectory))
            {
                return resolvedPath;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks if a file or directory is a symbolic link.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path is a symbolic link; otherwise, false.</returns>
    public static bool IsSymbolicLink(string path)
    {
        try
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                return fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
            }

            var dirInfo = new DirectoryInfo(path);
            if (dirInfo.Exists)
            {
                return dirInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the real path by resolving any symbolic links.
    /// </summary>
    /// <param name="path">The path to resolve.</param>
    /// <returns>The resolved real path, or null if resolution fails.</returns>
    public static string? GetRealPath(string path)
    {
        try
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                return fileInfo.ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? fileInfo.FullName;
            }

            var dirInfo = new DirectoryInfo(path);
            if (dirInfo.Exists)
            {
                return dirInfo.ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? dirInfo.FullName;
            }

            return Path.GetFullPath(path);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Validates a path is safe, resolving any symbolic links first.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <param name="allowedBaseDirectory">The base directory the resolved path must be contained within.</param>
    /// <returns>True if the resolved path is safe; otherwise, false.</returns>
    public static bool IsPathSafeWithSymlinkResolution(string path, string allowedBaseDirectory)
    {
        var realPath = GetRealPath(path);
        if (realPath is null)
        {
            return false;
        }

        return IsPathSafe(realPath, allowedBaseDirectory);
    }
}
