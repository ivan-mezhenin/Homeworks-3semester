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
        this.server.StartAsync().Wait();
    }

    /// <summary>
    /// method to be called immediately after each test is run.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        if (this.server != null)
        {
            this.server.Stop();
            this.server.Dispose();
        }
    }

    /// <summary>
    /// test for correct listing directory with test files.
    /// </summary>
    /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task List_Directory()
    {
        using var client = new Client("127.0.0.1", Port);
        await client.ConnectAsync(CancellationToken.None);

        const int expectedSize = 2;
        var expectedItems = new List<(string, bool)> { ("TestFile1.txt", false), ("TestFile2.txt", false) };

        var (error, size, items) = await client.ListRequestAsync("TestFiles", CancellationToken.None);

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
        await client.ConnectAsync(CancellationToken.None);

        var (error, size, items) = await client.ListRequestAsync("Non-existingFile.txt", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(error, Is.EqualTo("Directory not found"));
            Assert.That(size, Is.EqualTo(-1));
            Assert.That(items, Is.Empty);
        });
    }

    /// <summary>
    /// test for correct getting testFile1 using MemoryStream.
    /// </summary>
    /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Get_TestFile1_WithMemoryStream()
    {
        using var client = new Client("127.0.0.1", Port);
        await client.ConnectAsync(CancellationToken.None);

        var testFile = Path.Combine(AppContext.BaseDirectory, "TestFiles", "TestFile1.txt");
        var expectedBytes = await File.ReadAllBytesAsync(testFile);

        using var memoryStream = new MemoryStream();
        var (error, size) = await client.GetRequestAsync("TestFiles/TestFile1.txt", memoryStream, CancellationToken.None);
        var content = memoryStream.ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(error, Is.Null);
            Assert.That(size, Is.EqualTo(expectedBytes.Length));
            Assert.That(content, Is.EqualTo(expectedBytes));
        });
    }

    /// <summary>
    /// test for correct getting testFile1 by ten clients using MemoryStream.
    /// </summary>
    /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Get_10Clients_TestFile1_Concurrently_WithMemoryStream()
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
                await client.ConnectAsync(CancellationToken.None);

                using var memoryStream = new MemoryStream();
                var (error, size) = await client.GetRequestAsync("TestFiles/TestFile1.txt", memoryStream, CancellationToken.None);
                results[index] = (error, size, memoryStream.ToArray());
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
            await client.ConnectAsync(CancellationToken.None);
            var (error, size, items) = await client.ListRequestAsync("TestFiles", CancellationToken.None);
            return (error, size, items);
        });

        var getTask = Task.Run(async () =>
        {
            using var client = new Client("127.0.0.1", Port);
            await client.ConnectAsync(CancellationToken.None);
            var expectedBytes = await File.ReadAllBytesAsync(testFile);

            using var memoryStream = new MemoryStream();
            var (error, size) = await client.GetRequestAsync("TestFiles/TestFile2.txt", memoryStream, CancellationToken.None);
            return (error, size, memoryStream.ToArray(), expectedBytes);
        });

        await Task.WhenAll(listTask, getTask);

        var listResult = listTask.Result;
        var getResult = getTask.Result;

        Assert.Multiple(() =>
        {
            Assert.That(listResult.error, Is.Null);
            Assert.That(listResult.size, Is.GreaterThan(0));

            Assert.That(getResult.error, Is.Null);
            Assert.That(getResult.size, Is.EqualTo(getResult.expectedBytes.Length));
            Assert.That(getResult.Item3, Is.EqualTo(getResult.expectedBytes));
        });
    }

    /// <summary>
    /// test for calling request without connection.
    /// </summary>
    [Test]
    public void Request_WithoutConnection_ShouldThrow()
    {
        using var client = new Client("127.0.0.1", Port);

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.ListRequestAsync("TestFiles", CancellationToken.None));
    }

    /// <summary>
    /// test for multiple connect calls.
    /// </summary>
    /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Connect_MultipleTimes_ShouldNotFail()
    {
        using var client = new Client("127.0.0.1", Port);

        await client.ConnectAsync(CancellationToken.None);
        await client.ConnectAsync(CancellationToken.None);

        Assert.That(client.IsConnected(), Is.True);
    }

    /// <summary>
    /// test for cancellation during Get request.
    /// </summary>
    /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task GetRequest_Cancellation_ShouldReturnCancelled()
    {
        using var client = new Client("127.0.0.1", Port);
        await client.ConnectAsync(CancellationToken.None);

        using var cts = new CancellationTokenSource();

        await cts.CancelAsync();

        using var memoryStream = new MemoryStream();
        var (error, size) = await client.GetRequestAsync("TestFiles/TestFile1.txt", memoryStream, cts.Token);

        Assert.Multiple(() =>
        {
            Assert.That(error, Is.EqualTo("Request cancelled"));
            Assert.That(size, Is.EqualTo(-1));
        });
    }

    /// <summary>
    /// test for cancellation during List request.
    /// </summary>
    /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ListRequest_Cancellation_ShouldReturnCancelled()
    {
        using var client = new Client("127.0.0.1", Port);
        await client.ConnectAsync(CancellationToken.None);

        using var cts = new CancellationTokenSource();

        await cts.CancelAsync();

        var (error, size, items) = await client.ListRequestAsync("TestFiles", cts.Token);

        Assert.Multiple(() =>
        {
            Assert.That(error, Is.EqualTo("Request cancelled"));
            Assert.That(size, Is.EqualTo(-1));
            Assert.That(items, Is.Empty);
        });
    }
}