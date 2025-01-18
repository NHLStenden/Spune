//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Spune.Common.Miscellaneous;

namespace Spune.Core.Core;

/// <summary>
/// Represents a media entity that encapsulates both image and audio content functionalities.
/// </summary>
/// <remarks>
/// The Media class provides methods to manage media resources, including loading and retrieving image streams,
/// as well as disposing of unmanaged resources to ensure optimal memory usage.
/// </remarks>
public class Media : IDisposable, INotifyPropertyChanged
{
    /// <summary>
    /// The audio member.
    /// </summary>
    string _audioPath = string.Empty;

    /// <summary>
    /// Represents a stream to handle the content of an audio file.
    /// </summary>
    Stream? _audioStream;

    /// <summary>
    /// The image member.
    /// </summary>
    string _imagePath = string.Empty;

    /// <summary>
    /// Represents a stream to handle the content of an image file.
    /// </summary>
    Stream? _imageStream;

    /// <summary>
    /// Gets or sets the audio file path associated with this media instance.
    /// The property holds the relative or absolute path of the audio file
    /// to be used with the media element.
    /// </summary>
    public string AudioPath
    {
        get => _audioPath;
        set
        {
            _audioPath = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Represents the image file name or path associated with the media item.
    /// </summary>
    /// <remarks>
    /// This property stores the file name or path of the image associated with the media object. It can be used to
    /// reference
    /// the image when performing operations such as loading or processing the image.
    /// </remarks>
    /// <value>
    /// A string representing the file name or path of the image. Default is an empty string.
    /// </value>
    public string ImagePath
    {
        get => _imagePath;
        set
        {
            _imagePath = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Determines whether the media object has associated audio.
    /// </summary>
    /// <returns>True if the media object has audio; otherwise, false.</returns>
    public bool HasAudio() => !string.IsNullOrEmpty(AudioPath);

    /// <summary>
    /// Determines whether the media object has an associated image.
    /// </summary>
    /// <returns>True if the media object has a non-empty image; otherwise, false.</returns>
    public bool HasImage() => !string.IsNullOrEmpty(ImagePath);

    /// <summary>
    /// Asynchronously loads media resources such as images and audio from the specified file path.
    /// </summary>
    /// <param name="filePath">The file path where the media resources are located.</param>
    /// <returns>A task representing the asynchronous load operation.</returns>
    public async Task LoadAsync(string filePath)
    {
        if (HasAudio())
        {
            try
            {
                _audioStream = await FileLoader.LoadFileAsync(Path.Combine(filePath, AudioPath));
            }
            catch (FileNotFoundException)
            {
                _audioStream = Stream.Null;
            }
        }

        if (HasImage())
        {
            try
            {
                _imageStream = await FileLoader.LoadFileAsync(Path.Combine(filePath, ImagePath));
            }
            catch (FileNotFoundException)
            {
                _imageStream = Stream.Null;
            }
        }
    }

    /// <summary>
    /// Releases the unmanaged resources used by this class and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// True to release both managed and unmanaged resources; false to release only unmanaged
    /// resources.
    /// </param>
    void Dispose(bool disposing)
    {
        if (_audioStream == null && _imageStream == null) return;
        if (!disposing) return;
        _imageStream?.Dispose();
        _imageStream = null;
        _audioStream?.Dispose();
        _audioStream = null;
    }

    /// <summary>
    /// Asynchronously retrieves a stream of the audio data associated with this media instance.
    /// </summary>
    /// <returns>A <see cref="Stream" /> containing the audio data, positioned at the beginning of the stream.</returns>
    public async Task<Stream> GetAudioStreamAsync() => await CopyStreamAsync(_audioStream);

    /// <summary>
    /// Asynchronously retrieves a stream of the image data associated with this media instance.
    /// </summary>
    /// <returns>A <see cref="Stream" /> containing the image data, positioned at the beginning of the stream.</returns>
    public async Task<Stream> GetImageStreamAsync() => await CopyStreamAsync(_imageStream);

    /// <summary>
    /// Asynchronously retrieves a copy of a stream of data for the given stream.
    /// </summary>
    /// <param name="stream">Stream to copy.</param>
    /// <returns>A <see cref="Stream" /> containing the data, positioned at the beginning of the stream.</returns>
    static async Task<Stream> CopyStreamAsync(Stream? stream)
    {
        var memoryStream = new MemoryStream();
        if (stream != null)
        {
            var position = stream.Position;
            stream.Position = 0;
            await stream.CopyToAsync(memoryStream);
            stream.Position = position;
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <summary>
    /// Notify that the property has changed.
    /// </summary>
    /// <param name="propertyName">Name of property that changed.</param>
    void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}