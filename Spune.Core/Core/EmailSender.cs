//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using Google.Protobuf;
using Grpc.Core;
using Spune.Common.Grpc;
using Spune.Common.Interfaces;
using Spune.Core.Common;
using Spune.Core.Functions;
using Spune.Core.Miscellaneous;

namespace Spune.Core.Core;

/// <summary>
/// This class represents an email sender on the client side of the application.
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
    public Stream? AttachmentStream { get; set; }
    /// <inheritdoc />
    public async Task<string> SendAsync()
    {
        var clientProperties = await ClientProperties.GetInstanceAsync();
        var uri = new Uri(ClientPropertiesFunction.GetFullSpuneServerUri(clientProperties));
        var channelManager = new ChannelManager(uri.AbsoluteUri);
        var client = new EmailService.EmailServiceClient(channelManager.Channel);
        try
        {
            var response = await client.SendEmailAsync(new SendEmailRequest
            {
                SmtpHost = SmtpHost,
                Port = Port,
                UserName = UserName,
                Password = Password,
                From = From,
                To = To,
                Subject = Subject,
                AttachmentFileName = AttachmentFileName,
                Attachment = await ByteString.FromStreamAsync(AttachmentStream),
                Body = Body
            }, cancellationToken: CancellationToken.None);
            return response.Message;
        }
        catch (RpcException)
        {
            // Eat it!
            return "A remote procedure call exception occurred.";
        }
    }
}
