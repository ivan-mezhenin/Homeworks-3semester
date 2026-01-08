// <copyright file="Server.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyFtp;

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// server.
/// </summary>
public class Server : IDisposable
{
    private readonly int port;
    private readonly ConcurrentDictionary<Task, bool> clientTasks = new();
    private readonly CancellationTokenSource serverCts = new();
    private TcpListener? listener;
    private bool isDisposed;

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
    /// <returns>Task representing the server operation.</returns>
    public Task StartAsync()
    {
        try
        {
            this.listener = new TcpListener(IPAddress.Any, this.port);
            this.listener.Start();
        }
        catch (SocketException ex)
        {
            throw new InvalidOperationException("Could not start server", ex);
        }

        _ = Task.Run(this.AcceptClientsLoopAsync);
        return Task.CompletedTask;
    }

    /// <summary>
    /// to stop listen server immediately (without waiting for clients).
    /// </summary>
    public void Stop()
    {
        this.serverCts.Cancel();
        this.listener?.Stop();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Dispose()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.Stop();
        this.serverCts.Dispose();

        this.isDisposed = true;
    }

    /// <summary>
    /// Main loop for accepting clients.
    /// </summary>
    private async Task AcceptClientsLoopAsync()
    {
        while (!this.serverCts.IsCancellationRequested)
        {
            try
            {
                var socket = await this.listener!.AcceptSocketAsync(this.serverCts.Token);

                var clientTask = Task.Run(
                    async () =>
                {
                        await ClientHandler.HandleAsync(socket, this.serverCts.Token);
                },
                    this.serverCts.Token);

                this.clientTasks.TryAdd(clientTask, true);

                _ = clientTask.ContinueWith(
                    t =>
                {
                    this.clientTasks.TryRemove(t, out _);
                },
                    TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
            }
            catch (OperationCanceledException) when (this.serverCts.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception)
            {
                if (!this.serverCts.IsCancellationRequested)
                {
                    await Task.Delay(1000);
                }
            }
        }
    }
}