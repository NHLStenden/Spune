//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

namespace Spune.Common.Miscellaneous;

/// <summary>
/// A handler that appends a specified subdirectory to the request URI.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SubdirectoryHandler" /> class.
/// </remarks>
/// <param name="innerHandler">The inner handler which processes the HTTP response messages.</param>
/// <param name="subdirectory">The subdirectory to append to the request URI.</param>
public class SubdirectoryHandler(HttpMessageHandler innerHandler, string subdirectory) : DelegatingHandler(innerHandler)
{
	/// <summary>
	/// The subdirectory member.
	/// </summary>
	readonly string _subdirectory = subdirectory;

    /// <summary>
    /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var url = Invariant(
            $"{request.RequestUri?.Scheme ?? ""}://{request.RequestUri?.Host ?? ""}{_subdirectory}{request.RequestUri?.AbsolutePath ?? ""}");
        request.RequestUri = new Uri(url, UriKind.Absolute);
        return base.SendAsync(request, cancellationToken);
    }
}