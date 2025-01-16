//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

namespace Spune.Common.Miscellaneous;

/// <summary>
/// Provides an interface for loading files asynchronously.
/// </summary>
public interface IFileLoader
{
    /// <summary>
    /// Asynchronously loads a file and returns its content as a stream.
    /// </summary>
    /// <param name="path">The file path or URI of the file to load.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a stream of the file's content.</returns>
    Task<Stream> LoadFileAsync(string path);
}

/// <summary>
/// Provides a mechanism for loading files asynchronously in a desktop environment.
/// </summary>
/// <remarks>
/// DesktopFileLoader is an implementation of the IFileLoader interface, designed to load files
/// from the local filesystem asynchronously. It utilizes a FileStream to facilitate reading files
/// with asynchronous support, enabling efficient I/O operations.
/// </remarks>
public class DesktopFileLoader : IFileLoader
{
    /// <summary>
    /// Asynchronously loads a file as a <see cref="Stream" /> based on the provided file path.
    /// </summary>
    /// <param name="path">The path to the file that needs to be loaded.</param>
    /// <returns>
    /// A <see cref="Task" /> representing the asynchronous operation.
    /// The task result contains the loaded file as a <see cref="Stream" />.
    /// </returns>
    public async Task<Stream> LoadFileAsync(string path)
    {
        var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        return await Task.FromResult(fileStream);
    }
}

/// <summary>
/// Provides functionality to load files in a WebAssembly environment.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IFileLoader" /> interface to handle asynchronous loading of files
/// over HTTP. It uses the command-line arguments to determine the base URI for the file requests.
/// </remarks>
public class WebAssemblyFileLoader : IFileLoader
{
    /// <summary>
    /// Represents the shared instance of <see cref="System.Net.Http.HttpClient" /> used for making HTTP requests
    /// in the <see cref="WebAssemblyFileLoader" /> class. This instance is utilized
    /// to retrieve files from specified URIs in a WebAssembly environment.
    /// </summary>
    static readonly HttpClient HttpClient = new();

    /// <summary>
    /// Asynchronously loads a file from the specified path and returns its content as a stream.
    /// The method adapts to the platform to provide an appropriate file loading mechanism.
    /// </summary>
    /// <param name="path">The file path or URL of the file to load.</param>
    /// <returns>A task representing the asynchronous operation, containing a <see cref="Stream" /> of the file's content.</returns>
    public async Task<Stream> LoadFileAsync(string path)
    {
        var args = Environment.GetCommandLineArgs();
        if (args.Length <= 1) return Stream.Null;
        // Contains something like https://localhost:5000/index.html
        var uri = args[1];
        var uriBuilder = new UriBuilder(uri) { Path = path };

        var response = await HttpClient.GetAsync(uriBuilder.Uri);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync();
    }
}

/// <summary>
/// Provides a factory for creating instances of IFileLoader based on the runtime environment.
/// </summary>
/// <remarks>
/// Depending on the runtime environment, either a WebAssemblyFileLoader or DesktopFileLoader is created.
/// The choice is determined dynamically at runtime.
/// </remarks>
/// <threadsafety>
/// This class maintains a static, thread-safe singleton of the IFileLoader instance
/// to minimize overhead associated with loader creation.
/// </threadsafety>
/// <seealso cref="IFileLoader" />
/// <seealso cref="DesktopFileLoader" />
/// <seealso cref="WebAssemblyFileLoader" />
public static class FileLoaderFactory
{
    /// <summary>
    /// Represents a static instance of an <see cref="IFileLoader" /> implementation,
    /// used to load files either for desktop or WebAssembly environments based
    /// on the execution platform.
    /// </summary>
    static IFileLoader? _fileLoader;

    /// <summary>
    /// Creates an instance of an appropriate implementation of the IFileLoader interface
    /// based on the current operating system.
    /// If the application is running in a browser, a WebAssemblyFileLoader is created.
    /// Otherwise, a DesktopFileLoader is created. Subsequent calls will return the same instance.
    /// <returns>An initialized instance of IFileLoader.</returns>
    /// </summary>
    public static IFileLoader CreateFileLoader()
    {
        if (_fileLoader != null)
            return _fileLoader;
        if (OperatingSystem.IsBrowser())
            _fileLoader = new WebAssemblyFileLoader();
        else
            _fileLoader = new DesktopFileLoader();
        return _fileLoader;
    }
}

/// <summary>
/// Provides functionality for loading files asynchronously.
/// </summary>
public static class FileLoader
{
    /// <summary>
    /// Asynchronously loads a file from the specified path and returns a stream for reading the file's content.
    /// </summary>
    /// <param name="path">The path of the file to be loaded.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a stream for the file's content.</returns>
    public static async Task<Stream> LoadFileAsync(string path)
    {
        var fileLoader = FileLoaderFactory.CreateFileLoader();
        return await fileLoader.LoadFileAsync(path);
    }
}