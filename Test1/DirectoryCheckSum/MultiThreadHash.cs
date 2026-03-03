// <copyright file="MultiThreadHash.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace DirectoryChecksum;

using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Multithreaded hasher.
/// </summary>
public class MultiThreadHash
{
    private readonly ConcurrentDictionary<string, byte[]> hashCache = new();

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
        this.hashCache.Clear();

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
        if (this.hashCache.TryGetValue(directoryPath, out var cachedHash))
        {
            return cachedHash;
        }

        var directoryName = Path.GetFileName(directoryPath);
        var directoryNameBytes = Encoding.UTF8.GetBytes(directoryName);

        using var md5 = MD5.Create();
        md5.TransformBlock(directoryNameBytes, 0, directoryNameBytes.Length, null, 0);

        var subdirectories = Directory.GetDirectories(directoryPath)
            .OrderBy(d => d, StringComparer.Ordinal)
            .ToArray();

        var files = Directory.GetFiles(directoryPath)
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToArray();

        cancellationToken.ThrowIfCancellationRequested();

        var subdirectoryHashes = new byte[subdirectories.Length][];
        await Parallel.ForEachAsync(
            subdirectories.Select((sd, index) => (sd, index)),
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
            },
            async (item, ct) =>
            {
                subdirectoryHashes[item.index] = await this.ComputeDirectoryHashAsync(item.sd, ct);
            });

        foreach (var hash in subdirectoryHashes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            md5.TransformBlock(hash, 0, hash.Length, null, 0);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var fileHashes = new byte[files.Length][];
        await Parallel.ForEachAsync(
            files.Select((file, index) => (file, index)),
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
            },
            async (item, ct) =>
            {
                fileHashes[item.index] = await this.ComputeFileHashAsync(item.file, ct);
            });

        foreach (var hash in fileHashes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            md5.TransformBlock(hash, 0, hash.Length, null, 0);
        }

        md5.TransformFinalBlock([], 0, 0);
        var resultHash = md5.Hash!;

        this.hashCache.TryAdd(directoryPath, resultHash);
        return resultHash;
    }

    /// <summary>
    /// to compute hash of file.
    /// </summary>
    /// <param name="filePath">path to file.</param>
    /// <param name="cancellationToken">cancellation token.</param>
    /// <returns>task.</returns>
    private async Task<byte[]> ComputeFileHashAsync(string filePath, CancellationToken cancellationToken)
    {
        if (this.hashCache.TryGetValue(filePath, out var cachedHash))
        {
            return cachedHash;
        }

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
        var resultHash = md5.Hash!;

        this.hashCache.TryAdd(filePath, resultHash);
        return resultHash;
    }
}