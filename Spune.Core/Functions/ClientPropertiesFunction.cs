//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using Spune.Core.Interfaces;

namespace Spune.Core.Functions;

/// <summary>
/// This class contains client properties related functions.
/// </summary>
public static class ClientPropertiesFunction
{
    /// <summary>
    /// Gets the full URI of the Spune server. If the SpuneServerUri is not a complete URI, it combines it with the
    /// ApplicationUri to form a full URI.
    /// </summary>
    /// <param name="clientProperties">Client properties instance to get URI from.</param>
    /// <returns>The full Spune server URI as a string.</returns>
    public static string GetFullSpuneServerUri(IClientProperties clientProperties)
    {
        var spuneServerUri = clientProperties.SpuneServerUri;
        var applicationUri = clientProperties.ApplicationUri;
        if (spuneServerUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || spuneServerUri.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            return spuneServerUri;

        var uriBuilder = new UriBuilder(applicationUri) { Path = spuneServerUri };
        return uriBuilder.Uri.ToString();
    }

    /// <summary>
    /// Gets the validity of the chat server URI.
    /// </summary>
    /// <param name="clientProperties">Client properties instance to get URI from.</param>
    /// <returns>True if configured properly and false otherwise.</returns>
    public static bool ChatServerUriIsValid(IClientProperties clientProperties)
    {
        var chatServerUri = clientProperties.ChatServerUri;
        return !string.IsNullOrEmpty(chatServerUri);
    }

    /// <summary>
    /// Gets the full URI of the chat server. If the ChatServerUri is not a complete URI, it combines it with the
    /// ApplicationUri to form a full URI.
    /// </summary>
    /// <param name="clientProperties">Client properties instance to get URI from.</param>
    /// <returns>The full chat server URI as a string.</returns>
    public static string GetFullChatServerUri(IClientProperties clientProperties)
    {
        var chatServerUri = clientProperties.ChatServerUri;
        var applicationUri = clientProperties.ApplicationUri;
        if (chatServerUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || chatServerUri.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            return chatServerUri;

        var uriBuilder = new UriBuilder(applicationUri) { Path = chatServerUri };
        return uriBuilder.Uri.ToString();
    }
}