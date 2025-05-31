//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Text.Json;
using System.Text.Json.Serialization;
using Spune.Core.Resolvers;

namespace Spune.Core.Core;

/// <summary>
/// Provides configuration options for JSON serialization used in story-related operations.
/// </summary>
public static class MasterStorySerializerOptions
{
    /// <summary>
    /// Configuration options for <see cref="JsonSerializer" /> related operations.
    /// </summary>
    public static JsonSerializerOptions Options { get; private set; }

    /// <summary>
    /// Static constructor of class StoryReaderWriter.
    /// </summary>
    static MasterStorySerializerOptions()
    {
        Options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            TypeInfoResolver = new CustomTypeInfoResolver(),
            WriteIndented = true
        };
        Options.Converters.Add(new JsonStringEnumConverter());
    }
}
