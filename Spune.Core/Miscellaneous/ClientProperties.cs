//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Text.Json;
using Spune.Common.Miscellaneous;

namespace Spune.Core.Miscellaneous;

/// <summary>
/// Represents client-specific properties and provides methods for loading, saving,
/// and retrieving configuration specific to the client application.
/// </summary>
public class ClientProperties
{
    /// <summary>
    /// A static instance of the <see cref="ClientProperties" /> class,
    /// initialized asynchronously and shared across the application for
    /// managing client configuration settings.
    /// </summary>
    static ClientProperties? _instance;

    /// <summary>
    /// Gets or sets the base URI of the application.
    /// This property is primarily used to construct full URIs
    /// for other components, ensuring they are relative to the
    /// base application URI.
    /// </summary>
    public string ApplicationUri { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URI for the chat server.
    /// </summary>
    public string ChatServerUri { get; set; } = string.Empty;

    /// <summary>
    /// Represents the model used for the chat server in the application.
    /// It defines the specific AI model that should be utilized by the chat server implementation.
    /// </summary>
    public string ChatServerModel { get; set; } = string.Empty;

    /// <summary>
    /// Represents the from field for the e-mail.
    /// </summary>
    public string EmailFrom { get; set; } = string.Empty;

    /// <summary>
    /// Represents the password for the e-mail.
    /// </summary>
    public string EmailPassword { get; set; } = string.Empty;

    /// <summary>
    /// Represents the port for the e-mail.
    /// </summary>
    public int EmailPort { get; set; } = 587;

    /// <summary>
    /// Represents the SMTP host for the e-mail.
    /// </summary>
    public string EmailSmtpHost { get; set; } = string.Empty;

    /// <summary>
    /// Represents the user name for the e-mail.
    /// </summary>
    public string EmailUserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the splash screen should be displayed at application startup.
    /// </summary>
    public bool SplashScreen { get; set; }

    /// <summary>
    /// Gets or sets the URI for the Spune server.
    /// </summary>
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

    /// <summary>
    /// Commits the current state of the client properties asynchronously to a file.
    /// </summary>
    /// <returns>A task that represents the asynchronous commit operation.</returns>
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
    /// Gets the full URI of the Spune server. If the SpuneServerUri is not a complete URI, it combines it with the
    /// ApplicationUri to form a full URI.
    /// </summary>
    /// <returns>The full Spune server URI as a string.</returns>
    public string GetFullSpuneServerUri()
    {
        if (SpuneServerUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || SpuneServerUri.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            return SpuneServerUri;

        var uriBuilder = new UriBuilder(ApplicationUri) { Path = SpuneServerUri };
        return uriBuilder.Uri.ToString();
    }

    /// <summary>
    /// Gets the full URI of the chat server. If the ChatServerUri is not a complete URI, it combines it with the
    /// ApplicationUri to form a full URI.
    /// </summary>
    /// <returns>The full chat server URI as a string.</returns>
    public string GetFullChatServerUri()
    {
        if (ChatServerUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || ChatServerUri.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            return ChatServerUri;

        var uriBuilder = new UriBuilder(ApplicationUri) { Path = ChatServerUri };
        return uriBuilder.Uri.ToString();
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