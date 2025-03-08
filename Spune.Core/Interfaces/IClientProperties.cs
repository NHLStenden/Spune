//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

namespace Spune.Core.Interfaces;

/// <summary>
/// Represents an interface of client-specific properties and provides methods for loading, saving,
/// and retrieving configuration specific to the client application.
/// </summary>
public interface IClientProperties
{
    /// <summary>
    /// Gets or sets the base URI of the application.
    /// This property is primarily used to construct full URIs
    /// for other components, ensuring they are relative to the
    /// base application URI.
    /// </summary>
    string ApplicationUri { get; set; }

    /// <summary>
    /// Gets or sets the URI for the chat server.
    /// </summary>
    string ChatServerUri { get; set; }

    /// <summary>
    /// Represents the from field for the e-mail.
    /// </summary>
    string EmailFrom { get; set; }

    /// <summary>
    /// Represents the password for the e-mail.
    /// </summary>
    string EmailPassword { get; set; }

    /// <summary>
    /// Represents the port for the e-mail.
    /// </summary>
    int EmailPort { get; set; }

    /// <summary>
    /// Represents the SMTP host for the e-mail.
    /// </summary>
    string EmailSmtpHost { get; set; }

    /// <summary>
    /// Represents the username for the e-mail.
    /// </summary>
    string EmailUserName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the splash screen should be displayed at application startup.
    /// </summary>
    bool SplashScreen { get; set; }

    /// <summary>
    /// Gets or sets the URI for the Spune server.
    /// </summary>
    string SpuneServerUri { get; set; }
    /// <summary>
    /// Commits the current state of the client properties asynchronously to a file.
    /// </summary>
    /// <returns>A task that represents the asynchronous commit operation.</returns>
    Task CommitAsync();
}
