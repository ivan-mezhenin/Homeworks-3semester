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
    private readonly TcpClient client;
    private readonly NetworkStream stream;
    private readonly StreamWriter writer;
    private readonly StreamReader reader;
    private readonly string baseDirectory;
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

        this.baseDirectory = Directory.GetCurrentDirectory();
        this.client = new TcpClient(host, port);
        this.stream = this.client.GetStream();
        this.writer = new StreamWriter(this.stream);
        this.writer.AutoFlush = true;
        this.reader = new StreamReader(this.stream);
        this.isDisposed = false;
    }

    /// <summary>
    /// list request.
    /// </summary>
    /// <param name="filePath">file to list.</param>
    /// <returns>task.</returns>
    public async Task<(string? Error, int Size, List<(string Name, bool IsDirectory)> Data)> ListRequestAsync(string filePath)
    {
        try
        {
            var fullPath = Path.GetFullPath(Path.Combine(this.baseDirectory, filePath));

            await this.writer.WriteLineAsync($"1 {fullPath}");

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

            return (null, lineCount, files);
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
    /// <returns>size and content of file.</returns>
    public async Task<(string? Error, long Size, byte[] Content)> GetRequestAsync(string filePath)
    {
        try
        {
            var fullPath = Path.GetFullPath(Path.Combine(this.baseDirectory, filePath));

            await this.writer.WriteLineAsync($"2 {fullPath}");
            await this.writer.FlushAsync();

            var sizeBuilder = new System.Text.StringBuilder();
            int currentByte;

            while ((currentByte = await this.ReadByteAsync()) != -1)
            {
                if (currentByte == ' ')
                {
                    break;
                }

                switch (currentByte)
                {
                    case '\n':
                        return ("Empty request", -1, []);
                    case '\r':
                        continue;
                }

                sizeBuilder.Append((char)currentByte);
            }

            if (sizeBuilder.Length == 0)
            {
                return ("No size received", -1, []);
            }

            var sizeString = sizeBuilder.ToString();

            if (sizeString == "-1")
            {
                return ("File not found", -1, []);
            }

            if (!long.TryParse(sizeString, out var fileSize) || fileSize < 0)
            {
                return ("Invalid file size", -1, []);
            }

            using var ms = new MemoryStream();
            var buffer = new byte[81920];
            long totalRead = 0;

            while (totalRead < fileSize)
            {
                var toRead = (int)Math.Min(buffer.Length, fileSize - totalRead);
                var read = await this.stream.ReadAsync(buffer.AsMemory(0, toRead));

                if (read < 0)
                {
                    return ("Connection lost during download", -1, []);
                }

                if (read == 0)
                {
                    break;
                }

                var bytesToTake = Math.Min(read, (int)(fileSize - totalRead));
                ms.Write(buffer, 0, bytesToTake);
                totalRead += bytesToTake;
            }

            var resultBytes = ms.ToArray();
            if (resultBytes.Length > 0 && resultBytes[^1] == 10)
            {
                resultBytes = resultBytes[..^1];
            }

            return (null, fileSize, resultBytes);
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
        if (this.isDisposed)
        {
            return;
        }

        this.client.Dispose();
        this.stream.Dispose();
        this.writer.Dispose();
        this.reader.Dispose();
        this.isDisposed = true;
    }

    /// <summary>
    /// to read one byte from stream.
    /// </summary>
    /// <returns>read byte.</returns>
    private async Task<byte> ReadByteAsync()
    {
        var buffer = new byte[1];

        await this.stream.ReadExactlyAsync(buffer, 0, 1);

        return buffer[0];
    }
}
