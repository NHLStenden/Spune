//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Spune.Common.Interfaces;

namespace Spune.ServiceBase.Service;

/// <summary>
/// This class represents an email sender.
/// </summary>
public class EmailSender : IEmailSender
{
    /// <inheritdoc />
    public string SmtpHost { get; set; } = string.Empty;
    /// <inheritdoc />
    public int Port { get; set; }
    /// <inheritdoc />
    public string UserName { get; set; } = string.Empty;
    /// <inheritdoc />
    public string Password { get; set; } = string.Empty;
    /// <inheritdoc />
    public string From { get; set; } = string.Empty;
    /// <inheritdoc />
    public string To { get; set; } = string.Empty;
    /// <inheritdoc />
    public string Subject { get; set; } = string.Empty;
    /// <inheritdoc />
    public string Body { get; set; } = string.Empty;
    /// <inheritdoc />
    public string AttachmentFileName { get; set; } = string.Empty;
    /// <inheritdoc />
    public Stream? AttachmentStream { get; set; } = new MemoryStream();

    /// <inheritdoc />
    public async Task<string> SendAsync()
    {
        if (string.IsNullOrEmpty(From))
            return "The author (from) is empty";
        if (string.IsNullOrEmpty(SmtpHost))
            return "The SMTP host is empty";
        if (!string.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(Password))
            return "The password is not provided";

        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(From));
        email.To.Add(MailboxAddress.Parse(To));
        email.Subject = Subject;
        var bodyBuilder = new BodyBuilder { TextBody = Body };
        if (AttachmentStream != null)
        {
            var fileName = AttachmentFileName;
            await bodyBuilder.Attachments.AddAsync(fileName, AttachmentStream);
        }
        email.Body = bodyBuilder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(SmtpHost, Port >= 0 ? Port : 587, SecureSocketOptions.StartTls);
        if (!string.IsNullOrEmpty(UserName))
            await smtp.AuthenticateAsync(UserName, Password);
        var resultMessage = await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
        return resultMessage;
    }
}
