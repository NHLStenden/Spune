//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

namespace Spune.Common.Interfaces;

/// <summary>
/// Interface for sending emails.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Gets or sets the SMTP host.
    /// </summary>
    string SmtpHost { get; set; }

    /// <summary>
    /// Gets or sets the port number.
    /// </summary>
    int Port { get; set; }

    /// <summary>
    /// Gets or sets the user name for authentication.
    /// </summary>
    string UserName { get; set; }

    /// <summary>
    /// Gets or sets the password for authentication.
    /// </summary>
    string Password { get; set; }

    /// <summary>
    /// Gets or sets the sender's email address.
    /// </summary>
    string From { get; set; }

    /// <summary>
    /// Gets or sets the recipient's email address.
    /// </summary>
    string To { get; set; }

    /// <summary>
    /// Gets or sets the email subject.
    /// </summary>
    string Subject { get; set; }

    /// <summary>
    /// Gets or sets the email body.
    /// </summary>
    string Body { get; set; }

    /// <summary>
    /// Gets or sets the attachment file name.
    /// </summary>
    string AttachmentFileName { get; set; }

    /// <summary>
    /// Gets or sets the attachment stream.
    /// </summary>
    Stream? AttachmentStream { get; set; }

    /// <summary>
    /// Sends the email asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous send operation. The task result contains the response from the email server.</returns>
    Task<string> SendAsync();
}
