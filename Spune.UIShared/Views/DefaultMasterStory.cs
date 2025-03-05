//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

namespace Spune.UIShared.Views;

/// <summary>
/// Represents default master story properties.
/// </summary>
public static class DefaultMasterStory
{
    /// <summary>
    /// Represents a file path for the default master story.
    /// </summary>
    /// <returns>The file path.</returns>
    public static string GetFilePath() => GetDirectory() + "/MasterStory.json";

    /// <summary>
    /// Gets the directory for the default master story.
    /// </summary>
    /// <returns>The file path of the directory.</returns>
    public static string GetDirectory() => !string.IsNullOrEmpty(CustomDirectory) ? CustomDirectory : Directory;

    /// <summary>
    /// Gets the custom directory.
    /// </summary>
    /// <returns>The file path of the custom directory or an empty string otherwise.</returns>
    static string GetCustomDirectory()
    {
        var args = Environment.GetCommandLineArgs();
        if (args.Length > 2 && string.Equals(args[1], "open", StringComparison.Ordinal))
        {
            return args[2];
        }
        return string.Empty;
    }

    /// <summary>
    /// Represents a file path to the directory for the default master story.
    /// </summary>
    static string Directory => "MasterStories";

    /// <summary>
    /// Represents a file path to the custom directory for the default master story.
    /// </summary>
    static readonly string CustomDirectory = GetCustomDirectory();
}