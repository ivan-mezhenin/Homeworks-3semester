// <copyright file="ClientHandler.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyFtp;

using System.Net.Sockets;

/// <summary>
/// client request handler.
/// </summary>
public class ClientHandler
{
    private readonly Socket socket;
    private readonly string baseDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientHandler"/> class.
    /// </summary>
    /// <param name="socket">socket.</param>
    public ClientHandler(Socket socket)
    {
        this.socket = socket;
        this.baseDirectory = Directory.GetCurrentDirectory();
    }

    public async Task HandleAsync()
    {
        
    }
}