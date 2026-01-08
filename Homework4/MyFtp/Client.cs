// <copyright file="Client.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyFtp;

using System.Net.Sockets;

/// <summary>
/// Ftp client.
/// </summary>
public class Client : IDisposable
{
    private readonly string baseDirectory;
    private TcpClient? client;
    private NetworkStream? stream;
    private StreamWriter? writer;
    private StreamReader? reader;
    private bool isDisposed;
    private bool isConnected;

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

        this.baseDirectory = Directory.GetCurrentDirectory();
        this.Host = host;
        this.Port = port;
        this.isConnected = false;
        this.isDisposed = false;
    }

    /// <summary>
    /// Gets the host to connect to.
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// Gets the port to connect to.
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// Connects to the server asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (this.isConnected)
        {
            return;
        }

        this.client = new TcpClient();
        await this.client.ConnectAsync(this.Host, this.Port, cancellationToken);
        this.stream = this.client.GetStream();
        this.writer = new StreamWriter(this.stream);
        this.writer.AutoFlush = true;
        this.reader = new StreamReader(this.stream);
        this.isConnected = true;
    }

    /// <summary>
    /// list request.
    /// </summary>
    /// <param name="filePath">file to list.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>task.</returns>
    public async Task<(string? Error, int Size, List<(string Name, bool IsDirectory)> Data)> ListRequestAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        this.EnsureConnected();

        try
        {
            var fullPath = Path.GetFullPath(Path.Combine(this.baseDirectory, filePath));

            await this.writer!.WriteLineAsync($"1 {fullPath}");

            var size = await this.reader!.ReadLineAsync(cancellationToken);

            if (size == "-1")
            {
                await this.reader.ReadLineAsync(cancellationToken);
                return ("Directory not found", -1, []);
            }

            if (!int.TryParse(size, out var lineCount))
            {
                return ("Invalid size format", -1, []);
            }

            var files = new List<(string Name, bool IsDirectory)>();

            for (var i = 0; i < lineCount; i++)
            {
                var line = await this.reader.ReadLineAsync(cancellationToken);
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

            return (null, lineCount, files);
        }
        catch (OperationCanceledException)
        {
            return ("Request cancelled", -1, []);
        }
        catch (Exception ex)
        {
            return (ex.Message, -1, []);
        }
    }

    /// <summary>
    /// Get request.
    /// </summary>
    /// <param name="filePath">file to get content.</param>
    /// <param name="outputStream">output stream to write content to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>size of file.</returns>
    public async Task<(string? Error, long Size)> GetRequestAsync(
        string filePath,
        Stream outputStream,
        CancellationToken cancellationToken = default)
    {
        this.EnsureConnected();

        try
        {
            var fullPath = Path.GetFullPath(Path.Combine(this.baseDirectory, filePath));

            if (this.writer == null)
            {
                this.writer = new StreamWriter(fullPath);
            }

            await this.writer.WriteLineAsync($"2 {fullPath}");
            await this.writer.FlushAsync(cancellationToken);

            var sizeBuilder = new System.Text.StringBuilder();
            int currentByte;

            while ((currentByte = await this.ReadByteAsync(cancellationToken)) != -1)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return ("Request cancelled", -1);
                }

                if (currentByte == ' ')
                {
                    break;
                }

                switch (currentByte)
                {
                    case '\n':
                        return ("Empty request", -1);
                    case '\r':
                        continue;
                }

                sizeBuilder.Append((char)currentByte);
            }

            if (sizeBuilder.Length == 0)
            {
                return ("No size received", -1);
            }

            var sizeString = sizeBuilder.ToString();

            if (sizeString == "-1")
            {
                return ("File not found", -1);
            }

            if (!long.TryParse(sizeString, out var fileSize) || fileSize < 0)
            {
                return ("Invalid file size", -1);
            }

            var buffer = new byte[81920];
            long totalRead = 0;

            while (totalRead < fileSize)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return ("Request cancelled", -1);
                }

                var toRead = (int)Math.Min(buffer.Length, fileSize - totalRead);
                var read = await this.stream!.ReadAsync(buffer.AsMemory(0, toRead), cancellationToken);

                if (read < 0)
                {
                    return ("Connection lost during download", -1);
                }

                if (read == 0)
                {
                    break;
                }

                var bytesToTake = Math.Min(read, (int)(fileSize - totalRead));
                await outputStream.WriteAsync(buffer.AsMemory(0, bytesToTake), cancellationToken);
                totalRead += bytesToTake;
            }

            var finalByte = await this.ReadByteAsync(cancellationToken);
            if (finalByte != '\n')
            {
                return ("Invalid response format", -1);
            }

            return (null, fileSize);
        }
        catch (OperationCanceledException)
        {
            return ("Request cancelled", -1);
        }
        catch (Exception ex)
        {
            return (ex.Message, -1);
        }
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

        this.client?.Dispose();
        this.stream?.Dispose();
        this.writer?.Dispose();
        this.reader?.Dispose();
        this.isConnected = false;
        this.isDisposed = true;
    }

    /// <summary>
    /// Checks if the client is connected to the server.
    /// </summary>
    /// <returns>True if connected, false otherwise.</returns>
    public bool IsConnected() => this.isConnected && this.client?.Connected == true;

    /// <summary>
    /// to read one byte from stream.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>read byte.</returns>
    private async Task<byte> ReadByteAsync(CancellationToken cancellationToken = default)
    {
        var buffer = new byte[1];

        await this.stream!.ReadExactlyAsync(buffer, 0, 1, cancellationToken);

        return buffer[0];
    }

    /// <summary>
    /// Ensures that the client is connected before performing operations.
    /// </summary>
    private void EnsureConnected()
    {
        if (!this.IsConnected())
        {
            throw new InvalidOperationException("Client is not connected to the server. Call ConnectAsync first.");
        }
    }
}