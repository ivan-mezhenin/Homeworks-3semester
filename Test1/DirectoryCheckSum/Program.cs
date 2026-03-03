// <copyright file="Program.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

using DirectoryChecksum;

Console.WriteLine("Usage: dotnet run -- <path>");

var path = args[0];

if (!File.Exists(path) && !Directory.Exists(path))
{
    Console.WriteLine($"Error: Path not found - {path}");
    return;
}

try
{
    using var cts = new CancellationTokenSource();
    var hash = await new SingleThreadHash().ComputeHashAsync(path, cts.Token);
    var hexString = Convert.ToHexStringLower(hash);
    Console.WriteLine($"MD5 Checksum: {hexString}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation was cancelled");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}