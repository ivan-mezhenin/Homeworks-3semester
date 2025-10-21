// <copyright file="Server.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyFtp;

using System.Net;
using System.Net.Sockets;

/// <summary>
/// server.
/// </summary>
public class Server
{
    private readonly int port;
    private TcpListener? listener;

    /// <summary>
    /// Initializes a new instance of the <see cref="Server"/> class.
    /// </summary>
    /// <param name="port">the port on which the server will be running.</param>
    public Server(int port)
    {
        if (port is < 1 or > 65535)
        {
            throw new ArgumentException("Port must be between 1 and 65535");
        }

        this.port = port;
    }

    /// <summary>
    /// to start server.
    /// </summary>
    /// <returns>nothing.</returns>
    public async Task StartAsync()
    {
        try
        {
            this.listener = new TcpListener(IPAddress.Any, this.port);

            try
            {
                this.listener.Start();
            }
            catch (SocketException ex)
            {
                throw new InvalidOperationException("Could not start server", ex);
            }

            while (true)
            {
                try
                {
                    var socket = await this.listener.AcceptSocketAsync();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }
        finally
        {
            this.listener?.Stop();
        }
    }

    /// <summary>
    /// to stop listen server.
    /// </summary>
    public void Stop()
    {
        this.listener?.Stop();
    }
}