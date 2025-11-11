// <copyright file="SingleThreadHash.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace DirectoryChecksum;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Single thread hasher.
/// </summary>
public class SingleThreadHash
{
    /// <summary>
    /// to compute hash of file or directory.
    /// </summary>
    /// <param name="path">path to file.</param>
    /// <param name="cancellationToken">cancellation token.</param>
    /// <returns>task.</returns>
    public async Task<byte[]> ComputeHashAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            throw new FileNotFoundException($"Path not found: {path}");
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (File.Exists(path))
        {
            return await this.ComputeFileHashAsync(path, cancellationToken);
        }

        return await this.ComputeDirectoryHashAsync(path, cancellationToken);
    }

    /// <summary>
    /// to compute hash of directory.
    /// </summary>
    /// <param name="directoryPath">path to directory.</param>
    /// <param name="cancellationToken">cancellation token.</param>
    /// <returns>task.</returns>
    private async Task<byte[]> ComputeDirectoryHashAsync(string directoryPath, CancellationToken cancellationToken)
    {
        var directoryName = Path.GetFileName(directoryPath);
        var directoryNameBytes = Encoding.UTF8.GetBytes(directoryName);

        using var md5 = MD5.Create();
        md5.TransformBlock(directoryNameBytes, 0, directoryNameBytes.Length, null, 0);

        var subdirectories = Directory.GetDirectories(directoryPath)
            .OrderBy(d => d, StringComparer.Ordinal);

        foreach (var subdirectory in subdirectories)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var subdirectoryHash = await this.ComputeDirectoryHashAsync(subdirectory, cancellationToken);
            md5.TransformBlock(subdirectoryHash, 0, subdirectoryHash.Length, null, 0);
        }

        var files = Directory.GetFiles(directoryPath)
            .OrderBy(f => f, StringComparer.Ordinal);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fileHash = await this.ComputeFileHashAsync(file, cancellationToken);
            md5.TransformBlock(fileHash, 0, fileHash.Length, null, 0);
        }

        md5.TransformFinalBlock([], 0, 0);
        return md5.Hash!;
    }

    /// <summary>
    /// to compute hash of file.
    /// </summary>
    /// <param name="filePath">path to file.</param>
    /// <param name="cancellationToken">cancellation token.</param>
    /// <returns>task.</returns>
    private async Task<byte[]> ComputeFileHashAsync(string filePath, CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileName(filePath);
        var fileNameBytes = Encoding.UTF8.GetBytes(fileName);

        using var md5 = MD5.Create();

        md5.TransformBlock(fileNameBytes, 0, fileNameBytes.Length, null, 0);

        await using var fileStream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 8192,
            useAsync: true);

        var buffer = new byte[8192];
        int bytesRead;

        while ((bytesRead = await fileStream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            md5.TransformBlock(buffer, 0, bytesRead, null, 0);
        }

        md5.TransformFinalBlock([], 0, 0);
        return md5.Hash!;
    }
}