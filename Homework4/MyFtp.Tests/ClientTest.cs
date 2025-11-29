// <copyright file="ClientTest.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyFtp.Tests;

/// <summary>
/// tests for client.
/// </summary>
public class ClientTest
{
    private const int Port = 8888;
    private Server? server;

    /// <summary>
    /// method to be called immediately before each test is run.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        this.server = new Server(Port);
        _ = Task.Run(() => this.server.StartAsync());
    }

    /// <summary>
    /// method to be called immediately after each test is run.
    /// </summary>
    [TearDown]
    public void TearDown() => this.server?.Stop();

    /// <summary>
    /// test for correct listing directory with test files.
    /// </summary>
    /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task List_Directory()
    {
        using var client = new Client("127.0.0.1", Port);
        const int expectedSize = 2;
        var expectedItems = new List<(string, bool)> { ("TestFile1.txt", false), ("TestFile2.txt", false) };

        var (error, size, items) = await client.ListRequestAsync("TestFiles");

        Assert.Multiple(() =>
        {
            Assert.That(error, Is.Null);
            Assert.That(size, Is.EqualTo(expectedSize));
            Assert.That(items, Is.EqualTo(expectedItems));
        });
    }

    /// <summary>
    /// test for throwing error while listing non-existing directory.
    /// </summary>
    /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task List_NonExistingDirectory()
    {
        using var client = new Client("127.0.0.1", Port);

        var (error, size, items) = await client.ListRequestAsync("Non-existingFile.txt");

        Assert.Multiple(() =>
        {
            Assert.That(error, Is.EqualTo("Directory not found"));
            Assert.That(size, Is.EqualTo(-1));
            Assert.That(items, Is.Empty);
        });
    }

    /// <summary>
    /// test for correct getting testFile1.
    /// </summary>
    /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Get_TestFile1()
    {
        using var client = new Client("127.0.0.1", Port);
        var testFile = Path.Combine(AppContext.BaseDirectory, "TestFiles", "TestFile1.txt");

        var expectedBytes = await File.ReadAllBytesAsync(testFile);

        var (error, size, items) = await client.GetRequestAsync("TestFiles/TestFile1.txt");

        Assert.Multiple(() =>
        {
            Assert.That(error, Is.Null);
            Assert.That(size, Is.EqualTo(expectedBytes.Length));
            Assert.That(items, Is.EqualTo(expectedBytes));
        });
    }

    /// <summary>
    /// test for correct getting testFile1 by ten clients.
    /// </summary>
    /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Get_10Clients_TestFile1_Concurrently()
    {
        const int clientCount = 10;
        var testFilePath = Path.Combine(AppContext.BaseDirectory, "TestFiles", "TestFile1.txt");
        var expectedBytes = await File.ReadAllBytesAsync(testFilePath);

        var tasks = new List<Task>();
        var results = new (string? Error, long Size, byte[] Content)[clientCount];

        for (var i = 0; i < clientCount; i++)
        {
            var index = i;
            tasks.Add(
                Task.Run(async () =>
            {
                using var client = new Client("127.0.0.1", Port);
                results[index] = await client.GetRequestAsync("TestFiles/TestFile1.txt");
            }));
        }

        await Task.WhenAll(tasks);

        Assert.Multiple(() =>
        {
            foreach (var result in results)
            {
                Assert.That(result.Error, Is.Null);
                Assert.That(result.Size, Is.EqualTo(expectedBytes.Length));
                Assert.That(result.Content, Is.EqualTo(expectedBytes));
            }
        });
    }

    /// <summary>
    /// test for correct get and list requests.
    /// </summary>
    /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task MixedRequests_ConcurrentListAndGet_Successful()
    {
        var testFile = Path.Combine(AppContext.BaseDirectory, "TestFiles", "TestFile2.txt");

        var listTask = Task.Run(async () =>
        {
            using var client = new Client("127.0.0.1", Port);
            var (error, size, items) = await client.ListRequestAsync("TestFiles");
            return (error, size, items);
        });

        var getTask = Task.Run(async () =>
        {
            using var client = new Client("127.0.0.1", Port);
            var expectedBytes = await File.ReadAllBytesAsync(testFile);
            var result = await client.GetRequestAsync("TestFiles/TestFile2.txt");
            return (result.Error, result.Size, result.Content, expectedBytes);
        });

        await Task.WhenAll(listTask, getTask);

        var listResult = listTask.Result;
        var getResult = getTask.Result;

        Assert.Multiple(() =>
        {
            Assert.That(listResult.error, Is.Null);
            Assert.That(listResult.size, Is.GreaterThan(0));

            Assert.That(getResult.Error, Is.Null);
            Assert.That(getResult.Content, Is.EqualTo(getResult.expectedBytes));
        });
    }
}