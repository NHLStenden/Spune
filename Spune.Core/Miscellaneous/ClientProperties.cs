//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Text.Json;
using Spune.Common.Miscellaneous;
using Spune.Core.Interfaces;

namespace Spune.Core.Miscellaneous;

/// <summary>
/// Represents client-specific properties and provides methods for loading, saving,
/// and retrieving configuration specific to the client application.
/// </summary>
public class ClientProperties : IClientProperties
{
    /// <summary>
    /// A static instance of the <see cref="ClientProperties" /> class,
    /// initialized asynchronously and shared across the application for
    /// managing client configuration settings.
    /// </summary>
    static ClientProperties? _instance;

    /// <inheritdoc />
    public string ApplicationUri { get; set; } = string.Empty;

    /// <inheritdoc />
    public string ChatServerUri { get; set; } = string.Empty;

    /// <inheritdoc />
    public string ChatServerModel { get; set; } = string.Empty;

    /// <inheritdoc />
    public string EmailFrom { get; set; } = string.Empty;

    /// <inheritdoc />
    public string EmailPassword { get; set; } = string.Empty;

    /// <inheritdoc />
    public int EmailPort { get; set; } = 587;

    /// <inheritdoc />
    public string EmailSmtpHost { get; set; } = string.Empty;

    /// <inheritdoc />
    public string EmailUserName { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool SplashScreen { get; set; }

    /// <inheritdoc />
    public string SpuneServerUri { get; set; } = string.Empty;

    /// <summary>
    /// Asynchronously gets an instance of the ClientProperties class.
    /// </summary>
    /// <returns>An instance of the ClientProperties class.</returns>
    public static async Task<ClientProperties> GetInstanceAsync()
    {
        if (_instance != null) return _instance;
        _instance = await LoadAsync();
        return _instance;
    }

    /// <inheritdoc />
    public async Task CommitAsync()
    {
        if (OperatingSystem.IsBrowser())
            return;
        var fileName = GetSettingsFileName();
        if (string.IsNullOrEmpty(fileName))
            return;

        var json = JsonSerializer.Serialize(this);
        await File.WriteAllLinesAsync(fileName, [json]);
    }

    /// <summary>
    /// Asynchronously loads the client properties from a configuration file or sets default values if the file is not
    /// found or invalid.
    /// </summary>
    /// <returns>An instance of <see cref="ClientProperties" /> populated with the loaded or default settings.</returns>
    static async Task<ClientProperties> LoadAsync()
    {
        var fileName = OperatingSystem.IsBrowser() ? "spune.json" : GetSettingsFileName();
        Stream stream;
        try
        {
            stream = await FileLoader.LoadFileAsync(fileName);
        }
        catch (FileNotFoundException)
        {
            stream = Stream.Null;
        }

        using var streamReader = new StreamReader(stream);
        var json = await streamReader.ReadToEndAsync();
        ClientProperties? result;
        try
        {
            result = JsonSerializer.Deserialize<ClientProperties>(json);
            if (result == null)
            {
                result = new ClientProperties();
                result.DefaultValues();
            }
        }
        catch (JsonException)
        {
            result = new ClientProperties();
            result.DefaultValues();
        }

        if (OperatingSystem.IsBrowser())
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length <= 1)
            {
                result = new ClientProperties();
                result.DefaultValues();
                return result;
            }

            // Contains something like https://localhost:5001/index.html
            var uri = args[1];
            var uriBuilder = new UriBuilder(uri) { Path = string.Empty };

            result.ApplicationUri = uriBuilder.Uri.ToString();
        }
        else
        {
            result.ApplicationUri = "http://localhost/";
        }

        await stream.DisposeAsync();
        return result;
    }

    /// <summary>
    /// Sets the default values for client properties.
    /// </summary>
    void DefaultValues()
    {
        ChatServerUri = "ollama/";
        ChatServerModel = "gemma2:27b";
        EmailFrom = string.Empty;
        EmailPassword = string.Empty;
        EmailPort = 587;
        EmailSmtpHost = string.Empty;
        EmailUserName = string.Empty;
        SplashScreen = true;
        SpuneServerUri = "spune_server";
    }

    /// <summary>
    /// Gets the settings file name.
    /// </summary>
    /// <returns>The settings file name or an empty string if unavailable.</returns>
    static string GetSettingsFileName()
    {
        var path = DotNetHelper.GetApplicationFileName();
        return !string.IsNullOrEmpty(path) ? Path.ChangeExtension(path, ".json") : "";
    }
}