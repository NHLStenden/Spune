//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using Grpc.Core;
using Spune.Common.Grpc;
using Spune.ServiceBase.Service;

namespace Spune.Server.Services;

/// <summary>
/// Provides a gRPC service for Spune.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="EmailService" /> class.
/// </remarks>
/// <param name="logger">The logger.</param>
public class EmailService(ILogger<EmailService> logger) : Common.Grpc.EmailService.EmailServiceBase
{
    /// <summary>
    /// The logger.
    /// </summary>
    readonly ILogger<EmailService> _logger = logger;

    /// <summary>
    /// Sends an e-mail.
    /// </summary>
    /// <param name="request">The email request parameter.</param>
    /// <param name="context">The server call context.</param>
    /// <returns>The email response.</returns>
    public override async Task<SendEmailResponse> SendEmail(SendEmailRequest request, ServerCallContext context)
    {
        await using var memoryStream = new MemoryStream();
        request.Attachment.WriteTo(memoryStream);
        memoryStream.Position = 0;
        var emailSender = new EmailSender
        {
            SmtpHost = request.SmtpHost,
            Port = request.Port,
            UserName = request.UserName,
            Password = request.Password,
            From = request.From,
            To = request.To,
            Subject = request.Subject,
            Body = request.Body,
            AttachmentFileName = request.AttachmentFileName,
            AttachmentStream = memoryStream
        };
        var resultMessage = await emailSender.SendAsync();
        _logger.LogInformation("{ResultMessage}", resultMessage);
        return new SendEmailResponse { Success = true, Message = resultMessage };
    }
}