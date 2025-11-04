// <copyright file="Client.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Sockets;

namespace MyFtp;

public class Client : IDisposable
{
    private readonly TcpClient client;
    private readonly NetworkStream stream;
    private readonly StreamWriter writer;
    private readonly StreamReader reader;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="host">host to connect.</param>
    /// <param name="port">the port on which the server will be running.</param>
    public Client(string host, int port)
    {
        if (port is < 1 or > 65535)
        {
            throw new ArgumentException("Port must be between 1 and 65535");
        }

        this.client = new TcpClient(host, port);
        this.stream = this.client.GetStream();
        this.writer = new StreamWriter(this.stream);
        this.writer.AutoFlush = true;
        this.reader = new StreamReader(this.stream);
        this.isDisposed = false;
    }

    public async Task<(string? Error, int Size, List<(string Name, bool IsDirectory)> Data)> ListRequestAsync(string filePath)
    {
        try
        {
            await this.writer.WriteLineAsync($"1 filePath");

            var size = await this.reader.ReadLineAsync();

            if (size == "-1")
            {
                await this.reader.ReadLineAsync();
                return ("Directory not found", -1, []);
            }

            if (!int.TryParse(size, out var lineCount))
            {
                return ("Invalid size format", -1, []);
            }

            var files = new List<(string Name, bool IsDirectory)>();

            for (var i = 0; i < lineCount; i++)
            {
                var line = await this.reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line))
                {
                    return ("Invalid request format", -1, []);
                }

                var lineParts = line.Split(' ', 2);
                if (lineParts.Length == 2 && bool.TryParse(lineParts[1], out var isDirectory))
                {
                    files.Add((lineParts[0], isDirectory));
                }
            }

            return (string.Empty, lineCount, files);
        }
        catch (Exception ex)
        {
            return (ex.Message, -1, []);
        }
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Dispose()
    {
        if (!this.isDisposed)
        {
            return;
        }

        this.client.Dispose();
        this.stream.Dispose();
        this.writer.Dispose();
        this.reader.Dispose();
        this.isDisposed = true;
    }
}