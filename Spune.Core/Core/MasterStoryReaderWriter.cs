//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Text.Json;
using Spune.Common.Miscellaneous;

namespace Spune.Core.Core;

/// <summary>
/// Provides functionality for reading and writing master story files in JSON format.
/// </summary>
/// <remarks>
/// This static class is responsible for handling the serialization and deserialization of master stories.
/// It includes methods for reading a master story from a file asynchronously and writing a master story to a file.
/// </remarks>
public static class MasterStoryReaderWriter
{
    /// <summary>
    /// Reads a master story from a specified file asynchronously, deserializes its contents,
    /// initializes it, and returns the resulting <see cref="MasterStory" /> object.
    /// </summary>
    /// <param name="filePath">The file path of the master story to be loaded.</param>
    /// <returns>The deserialized and initialized <see cref="MasterStory" /> object.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
    /// <exception cref="JsonException">Thrown if an error occurs during JSON deserialization.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if access to the file is denied.</exception>
    public static async Task<MasterStory> ReadMasterStoryAsync(string filePath)
    {
        await using var stream = await FileLoader.LoadFileAsync(filePath);
        using var streamReader = new StreamReader(stream);
        var json = await streamReader.ReadToEndAsync();
        var result = JsonSerializer.Deserialize<MasterStory>(json, MasterStorySerializerOptions.Options) ?? MasterStory.CreateInstance();
        result.Initialize();
        await result.StartAsync(filePath);
        return result;
    }

    /// <summary>
    /// Writes a master story object to a specified file path in JSON format.
    /// </summary>
    /// <param name="filePath">The file path where the master story will be written.</param>
    /// <param name="masterStory">The master story object to serialize and save to a file.</param>
    public static void WriteMasterStory(string filePath, MasterStory masterStory) => File.WriteAllText(filePath, (string?)JsonSerializer.Serialize(masterStory, MasterStorySerializerOptions.Options));

    /// <summary>
    /// Creates a master story.
    /// </summary>
    /// <param name="directory">Directory to save master story in.</param>
    /// <param name="title">Title of the master story.</param>
    /// <returns>File name of the created master story.</returns>
    public static string CreateMasterStory(string directory, string title)
    {
        using var masterStory = new MasterStory();
        masterStory.Text = title;
        var fileName = Path.Combine(directory, Invariant($"{masterStory.Text}.json"));
        WriteMasterStory(fileName, masterStory);
        return fileName;
    }
}