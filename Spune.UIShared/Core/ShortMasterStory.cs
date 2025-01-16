//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Spune.UIShared.Core;

/// <summary>
/// Represents a master story at its basic level.
/// </summary>
public class ShortMasterStory : INotifyPropertyChanged
{
    /// <summary>
    /// Base file path member.
    /// </summary>
    string _baseFilePath = string.Empty;

    /// <summary>
    /// File path member.
    /// </summary>
    string _filePath = string.Empty;

    /// <summary>
    /// Title member.
    /// </summary>
    string _title = string.Empty;

    /// <summary>
    /// Base file path of the master story.
    /// </summary>
    public string BaseFilePath
    {
        get => _baseFilePath;
        set
        {
            _baseFilePath = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// File path of the master story.
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
    /// Title of the master story.
    /// </summary>
    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            NotifyPropertyChanged();
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the combined file name.
    /// </summary>
    /// <returns>The combined file name.</returns>
    public string GetCombinedFileName() => Path.Combine(_baseFilePath, _filePath);

    /// <summary>
    /// Notify that the property has changed.
    /// </summary>
    /// <param name="propertyName">Name of property that changed.</param>
    void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}