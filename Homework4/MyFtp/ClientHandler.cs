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

    /// <summary>
    /// client handler.
    /// </summary>
    /// <returns>task.</returns>
    public async Task HandleAsync()
    {
        await using var stream = new NetworkStream(this.socket);
        await using var writer = new StreamWriter(stream);
        writer.AutoFlush = true;
        using var reader = new StreamReader(stream);

        try
        {
            var request = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(request))
            {
                return;
            }

            var requestParts = request.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (requestParts.Length < 2)
            {
                return;
            }

            var command = int.Parse(requestParts[0]);
            var filePath = requestParts[1];

            switch (command)
            {
                case 1:
                {
                    await this.HandleListAsync(filePath, writer);
                    break;
                }

                case 2:
                {
                    await this.HandleGetAsync(filePath, writer, stream);
                    break;
                }
            }
        }
        finally
        {
            this.socket.Close();
        }
    }

    /// <summary>
    /// to handle Get request.
    /// </summary>
    /// <param name="filePath">file to get.</param>
    /// <param name="writer">stream writer.</param>
    /// <param name="stream">network stream.</param>
    private async Task HandleGetAsync(string filePath, StreamWriter writer, NetworkStream stream)
    {
        var fullPath = Path.GetFullPath(Path.Combine(this.baseDirectory, filePath));

        if (!fullPath.StartsWith(this.baseDirectory, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
        {
            await writer.WriteLineAsync("-1");
            await writer.WriteLineAsync();
            return;
        }

        var content = await File.ReadAllBytesAsync(fullPath);
        await writer.WriteAsync(content.Length + " ");

        await stream.WriteAsync(content);

        await stream.WriteAsync(new[] { (byte)'\n' });
    }

    /// <summary>
    /// to handle list request.
    /// </summary>
    /// <param name="filePath">file to list.</param>
    /// <param name="writer">stream writer.</param>
    /// <returns>task.</returns>
    private async Task HandleListAsync(string filePath, StreamWriter writer)
    {
        var fullPath = Path.GetFullPath(Path.Combine(this.baseDirectory, filePath));

        if (!fullPath.StartsWith(this.baseDirectory, StringComparison.OrdinalIgnoreCase) || !Directory.Exists(fullPath))
        {
            await writer.WriteLineAsync("-1");
            await writer.WriteLineAsync();
            return;
        }

        var files = Directory.GetFileSystemEntries(fullPath)
            .Select(file => new
            {
                name = Path.GetFileName(file),
                isDirectory = Directory.Exists(file),
            })
            .OrderBy(x => x.name)
            .ToArray();

        await writer.WriteLineAsync(files.Length.ToString());

        foreach (var file in files)
        {
            await writer.WriteLineAsync($"{file.name} {file.isDirectory.ToString().ToLower()}");
        }

        await writer.WriteLineAsync();
    }
}