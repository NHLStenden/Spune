//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Spune.UIShared.Core;

/// <summary>
/// Represents a collection of master stories.
/// </summary>
public class MasterStories : INotifyPropertyChanged
{
    /// <summary>
    /// File path member.
    /// </summary>
    string _filePath = string.Empty;

    /// <summary>
    /// File path (directory) that contains the master stories.
    /// </summary>
    public string FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Items (the master stories).
    /// </summary>
    public ObservableCollection<ShortMasterStory> Items { get; set; } = [];

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Loads the master stories from the given file path.
    /// </summary>
    /// <returns>Collection with master stories.</returns>
    public async Task LoadAsync()
    {
        Items = await LoadAsync(_filePath);
    }

    /// <summary>
    /// Loads the master stories from the given file path.
    /// </summary>
    /// <param name="filePath">File path to load from.</param>
    /// <returns>Collection with master stories.</returns>
    async Task<ObservableCollection<ShortMasterStory>> LoadAsync(string filePath)
    {
        var result = new ObservableCollection<ShortMasterStory>();
        var fileNames = Directory.EnumerateFiles(filePath, "*.json");
        foreach (var fileName in fileNames)
        {
            var json = string.Join(Environment.NewLine, await File.ReadAllLinesAsync(fileName));
            using var jsonDocument = JsonDocument.Parse(json);
            var text = jsonDocument.RootElement.GetProperty("Text").GetString() ?? "";

            var shortStory = new ShortMasterStory
                { BaseFilePath = filePath, FilePath = Path.GetRelativePath(filePath, fileName), Title = text };
            result.Add(shortStory);
        }

        result.CollectionChanged += (_, _) => NotifyPropertyChanged(nameof(Items));
        return result;
    }

    /// <summary>
    /// Notify that the property has changed.
    /// </summary>
    /// <param name="propertyName">Name of property that changed.</param>
    void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}