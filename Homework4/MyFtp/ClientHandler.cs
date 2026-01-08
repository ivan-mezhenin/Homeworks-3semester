// <copyright file="ClientHandler.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyFtp;

using System.Net.Sockets;
using System.Text;

/// <summary>
/// client request handler.
/// </summary>
public static class ClientHandler
{
    private static readonly string BaseDirectory = Directory.GetCurrentDirectory();

    /// <summary>
    /// Handle client connection.
    /// </summary>
    /// <param name="socket">Client socket.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the client handling operation.</returns>
    public static async Task HandleAsync(Socket socket, CancellationToken cancellationToken = default)
    {
        await using var stream = new NetworkStream(socket);
        await using var writer = new StreamWriter(stream);
        writer.AutoFlush = true;
        using var reader = new StreamReader(stream);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = await ReadLineWithCancellationAsync(reader, cancellationToken);
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
                    await HandleListAsync(filePath, writer, cancellationToken);
                    break;
                }

                case 2:
                {
                    await HandleGetAsync(filePath, writer, stream, cancellationToken);
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Запрос отменен - нормальное завершение при остановке сервера
        }
        finally
        {
            try
            {
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }
            catch
            {
                // Игнорируем ошибки при закрытии сокета
            }
        }
    }

    /// <summary>
    /// to handle Get request.
    /// </summary>
    /// <param name="filePath">file to get.</param>
    /// <param name="writer">stream writer.</param>
    /// <param name="stream">network stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    private static async Task HandleGetAsync(string filePath, StreamWriter writer, NetworkStream stream, CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(Path.Combine(BaseDirectory, filePath));

        if (!fullPath.StartsWith(BaseDirectory, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
        {
            await WriteLineWithCancellationAsync(writer, "-1", cancellationToken);
            await WriteLineWithCancellationAsync(writer, string.Empty, cancellationToken);
            return;
        }

        var fileInfo = new FileInfo(fullPath);
        var fileSize = fileInfo.Length;

        await WriteWithCancellationAsync(writer, fileSize + " ", cancellationToken);

        await using var fileStream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        await fileStream.CopyToAsync(stream, cancellationToken);
        await stream.WriteAsync(new[] { (byte)'\n' }, cancellationToken);
    }

    /// <summary>
    /// to handle list request.
    /// </summary>
    /// <param name="filePath">file to list.</param>
    /// <param name="writer">stream writer.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>task.</returns>
    private static async Task HandleListAsync(string filePath, StreamWriter writer, CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(Path.Combine(BaseDirectory, filePath));

        if (!fullPath.StartsWith(BaseDirectory, StringComparison.OrdinalIgnoreCase) || !Directory.Exists(fullPath))
        {
            await WriteLineWithCancellationAsync(writer, "-1", cancellationToken);
            await WriteLineWithCancellationAsync(writer, string.Empty, cancellationToken);
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var files = Directory.EnumerateFileSystemEntries(fullPath)
            .Select(file => new
            {
                name = Path.GetFileName(file),
                isDirectory = Directory.Exists(file),
            })
            .OrderBy(x => x.name)
            .ToArray();

        await WriteLineWithCancellationAsync(writer, files.Length.ToString(), cancellationToken);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await WriteLineWithCancellationAsync(writer, $"{file.name} {file.isDirectory.ToString().ToLower()}", cancellationToken);
        }

        await WriteLineWithCancellationAsync(writer, string.Empty, cancellationToken);
    }

    /// <summary>
    /// Reads a line with cancellation support.
    /// </summary>
    private static async Task<string?> ReadLineWithCancellationAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        var result = new StringBuilder();
        var buffer = new char[1];

        while (!cancellationToken.IsCancellationRequested)
        {
            var read = await reader.ReadAsync(buffer, 0, 1);
            if (read == 0)
            {
                return null;
            }

            if (buffer[0] == '\n')
            {
                break;
            }

            if (buffer[0] != '\r')
            {
                result.Append(buffer[0]);
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        return result.ToString();
    }

    /// <summary>
    /// Writes a line with cancellation support.
    /// </summary>
    private static async Task WriteLineWithCancellationAsync(StreamWriter writer, string value, CancellationToken cancellationToken)
    {
        await WriteWithCancellationAsync(writer, value + "\n", cancellationToken);
    }

    /// <summary>
    /// Writes text with cancellation support.
    /// </summary>
    private static async Task WriteWithCancellationAsync(StreamWriter writer, string value, CancellationToken cancellationToken)
    {
        foreach (var ch in value)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await writer.WriteAsync(ch);
        }

        await writer.FlushAsync(cancellationToken);
    }
}