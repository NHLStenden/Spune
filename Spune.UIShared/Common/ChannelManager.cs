//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Net;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Spune.Common.Miscellaneous;

namespace Spune.UIShared.Common;

/// <summary>
/// Manages the creation and disposal of gRPC channels.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ChannelManager" /> class with the specified address.
/// </remarks>
/// <param name="address">The address of the gRPC channel.</param>
public class ChannelManager(string address) : IDisposable
{
    /// <summary>
    /// The dictionary with the channels and references.
    /// </summary>
    static readonly Dictionary<string, (GrpcChannel, int)> Dictionary = [];

    /// <summary>
    /// The lock object.
    /// </summary>
    static readonly Lock LockObject = new();

    /// <summary>
    /// The disposed value.
    /// </summary>
    bool _disposedValue;

    /// <summary>
    /// Gets the gRPC channel managed by this instance.
    /// </summary>
    public GrpcChannel Channel { get; } = GetChannel(address);

    /// <summary>
    /// Disposes the <see cref="ChannelManager" /> instance and its associated gRPC channel.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets or creates a gRPC channel for the specified address.
    /// </summary>
    /// <param name="address">The address of the gRPC channel.</param>
    /// <returns>The gRPC channel associated with the specified address.</returns>
    static GrpcChannel GetChannel(string address)
    {
        lock (LockObject)
        {
            if (Dictionary.TryGetValue(address, out var v))
            {
                Dictionary[address] = (v.Item1, v.Item2 + 1);
                return v.Item1;
            }

            var uri = new Uri(address);
            var handler = new SubdirectoryHandler(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()), uri.LocalPath);
            var httpClient = new HttpClient(handler);
            var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
            {
                HttpClient = httpClient,
                HttpVersion = HttpVersion.Version11
            });
            Dictionary[address] = (channel, 1);
            return channel;
        }
    }

    /// <summary>
    /// Disposes the specified gRPC channel and removes it from the internal dictionary if it exists.
    /// </summary>
    /// <param name="channel">The gRPC channel to dispose.</param>
    static void Dispose(GrpcChannel channel)
    {
        lock (LockObject)
        {
            foreach (var item in Dictionary)
            {
                if (item.Value.Item1 != channel)
                    continue;
                var count = item.Value.Item2 - 1;
                if (count > 0)
                {
                    Dictionary[item.Key] = (channel, count);
                }
                else
                {
                    channel.Dispose();
                    Dictionary.Remove(item.Key);
                }

                return;
            }
        }
    }

    /// <summary>
    /// Releases the unmanaged resources used by the ChannelManager and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// True to release both managed and unmanaged resources; false to release only unmanaged
    /// resources.
    /// </param>
    void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        if (disposing) Dispose(Channel);

        _disposedValue = true;
    }
}