// <copyright file="MyThreadPoolTest.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace ThreadPool.Test;

using System.Collections.Concurrent;

/// <summary>
/// tests for my thread pool.
/// </summary>
public class MyThreadPoolTest
{
    /// <summary>
    /// test for correct completion one task.
    /// </summary>
    [Test]
    public void MyThreadPool_Submit_ValidTask_CompleteAndReturnsResult()
    {
        var pool = new MyThreadPool(2);
        int[] numbers = [1, 2, 3, 5, 14, 2, 21];
        var expectedResult = numbers.Sum(x => x * x);

        var task = pool.Submit(() => numbers.Sum(x => x * x));

        Assert.Multiple(() =>
        {
            Assert.That(task.Result, Is.EqualTo(expectedResult));
            Assert.That(task.IsCompleted, Is.True);
        });

        pool.Shutdown();
    }

    /// <summary>
    /// test for correct throwing aggregate exception without stopping the pool.
    /// </summary>
    [Test]
    public void MyThreadPool_Submit_TaskWithException_ThrowsAggregateExceptionButPoolContinues()
    {
        var pool = new MyThreadPool(2);

        var failingTask = () =>
        {
            int[] numbers = [];
            if (numbers.Length == 0)
            {
                throw new InvalidOperationException("Test exception");
            }

            return numbers.Sum(x => x * x);
        };

        var task = pool.Submit(failingTask);

        Assert.Throws<AggregateException>(() => _ = task.Result);

        try
        {
            _ = task.Result;
        }
        catch (AggregateException ex)
        {
            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(ex.InnerException!.Message, Is.EqualTo("Test exception"));
        }

        var anotherTask = pool.Submit(() => 42);
        Assert.Multiple(() =>
        {
            Assert.That(anotherTask.Result, Is.EqualTo(42));
            Assert.That(anotherTask.IsCompleted, Is.True);
        });

        pool.Shutdown();
    }

    /// <summary>
    /// test for correct completion of 30 tasks in 5 threads.
    /// </summary>
    [Test]
    public void MyThreadPool_Submit_MultipleTasks_AllTasksCompleteCorrectly()
    {
        var pool = new MyThreadPool(5);
        const int taskCount = 30;

        var tasks = new IMyTask<string>[taskCount];
        for (var i = 1; i <= taskCount; i++)
        {
            var id = i;
            tasks[id - 1] = pool.Submit(() => string.Concat(Enumerable.Repeat($"{id}", id)));
        }

        for (var i = 1; i <= taskCount; i++)
        {
            var expectedResult = string.Concat(Enumerable.Repeat($"{i}", i));
            Assert.Multiple(() =>
            {
                Assert.That(tasks[i - 1].Result, Is.EqualTo(expectedResult));
                Assert.That(tasks[i - 1].IsCompleted, Is.True);
            });
        }

        pool.Shutdown();
    }

    /// <summary>
    /// test for throwing argument out of range exception when passed 0 to constructor.
    /// </summary>
    [Test]
    public void MyThreadPool_Constructor_ZeroThreads_ThrowsArgumentOutOfRangeException()
     => Assert.Throws<ArgumentOutOfRangeException>(() => new MyThreadPool(0));

    /// <summary>
    /// test for correct throwing aggregate exception in continue with.
    /// </summary>
    [Test]
    public void MyThreadPool_ContinueWith_ContinuationThrowsException_ThrowsAggregateException()
    {
        var pool = new MyThreadPool(5);

        var initialTask = pool.Submit(() => 5);

        var continuationTask = initialTask.ContinueWith<int>(_ => throw new InvalidOperationException());
        Exception? caughtException = null;
        try
        {
            _ = continuationTask.Result;
        }
        catch (AggregateException ex)
        {
            caughtException = ex.InnerException;
        }

        Assert.That(caughtException, Is.TypeOf<InvalidOperationException>());
    }

    /// <summary>
    /// test for correct work of continueWith.
    /// </summary>
    [Test]
    public void MyThreadPool_ContinueWith()
    {
        var pool = new MyThreadPool(2);
        int[] numbers = [1, 2, 3, 5, 14, 2, 21];
        var task = pool.Submit(() => numbers.Sum(x => x * x));
        var expectedTaskResult = numbers.Sum(x => x * x);

        var continuationTask = task.ContinueWith<string>(x => x.ToString());
        var expectedContinuationTaskResult = expectedTaskResult.ToString();

        Assert.Multiple(() =>
        {
            Assert.That(continuationTask.Result, Is.EqualTo(expectedContinuationTaskResult));
            Assert.That(task.IsCompleted, Is.True);
            Assert.That(task.Result, Is.EqualTo(expectedTaskResult));
        });

        pool.Shutdown();
    }

    /// <summary>
    /// test that pool has at least N working threads.
    /// </summary>
    [Test]
    public void MyThreadPool_HasAtLeastNWorkingThreads()
    {
        const int n = 4;

        var pool = new MyThreadPool(n);

        var concurrentTasks = 0;
        var maxConcurrent = 0;
        var lockObj = new object();

        var startSignal = new ManualResetEvent(false);

        for (var i = 0; i < n * 2; i++)
        {
            pool.Submit(() =>
            {
                startSignal.WaitOne();

                lock (lockObj)
                {
                    concurrentTasks++;
                    if (concurrentTasks > maxConcurrent)
                    {
                        maxConcurrent = concurrentTasks;
                    }
                }

                Thread.Sleep(100);

                lock (lockObj)
                {
                    concurrentTasks--;
                }

                return 0;
            });
        }

        Thread.Sleep(100);

        startSignal.Set();

        Thread.Sleep(50);

        Assert.That(maxConcurrent, Is.GreaterThanOrEqualTo(n));

        pool.Shutdown();
    }

    /// <summary>
    /// test that one task can have multiple continuations.
    /// </summary>
    [Test]
    public void MyThreadPool_OneTask_MultipleContinuations()
    {
        var pool = new MyThreadPool(2);

        var initialTask = pool.Submit(() => 10);

        var continuation1 = initialTask.ContinueWith(x => x * 2);
        var continuation2 = initialTask.ContinueWith(x => x + 5);

        Assert.Multiple(() =>
        {
            Assert.That(initialTask.Result, Is.EqualTo(10));
            Assert.That(continuation1.Result, Is.EqualTo(20));
            Assert.That(continuation2.Result, Is.EqualTo(15));

            Assert.That(initialTask.IsCompleted, Is.True);
            Assert.That(continuation1.IsCompleted, Is.True);
            Assert.That(continuation2.IsCompleted, Is.True);
        });

        pool.Shutdown();
    }

    /// <summary>
    /// test chain of continuations.
    /// </summary>
    [Test]
    public void MyThreadPool_ChainOfContinuations()
    {
        var pool = new MyThreadPool(2);

        var task = pool.Submit(() => 5);
        var continuation1 = task.ContinueWith(x => x * 2);
        var continuation2 = continuation1.ContinueWith(x => x + 3);
        var continuation3 = continuation2.ContinueWith(x => x.ToString());

        Assert.Multiple(() =>
        {
            Assert.That(task.Result, Is.EqualTo(5));
            Assert.That(continuation1.Result, Is.EqualTo(10));
            Assert.That(continuation2.Result, Is.EqualTo(13));
            Assert.That(continuation3.Result, Is.EqualTo("13"));

            Assert.That(task.IsCompleted, Is.True);
            Assert.That(continuation1.IsCompleted, Is.True);
            Assert.That(continuation2.IsCompleted, Is.True);
            Assert.That(continuation3.IsCompleted, Is.True);
        });

        pool.Shutdown();
    }

    /// <summary>
    /// test multiple continuations and chains.
    /// </summary>
    [Test]
    public void MyThreadPool_ComplexContinuationStructure()
    {
        var pool = new MyThreadPool(4);

        var root = pool.Submit(() => 100);

        var branch1 = root.ContinueWith(x => x / 2);
        var branch2 = root.ContinueWith(x => x * 2);

        var branch1Chain1 = branch1.ContinueWith(x => x + 10);
        var branch1Chain2 = branch1.ContinueWith(x => x - 10);

        var branch2Chain1 = branch2.ContinueWith(x => x.ToString());
        var branch2Chain2 = branch2.ContinueWith(x => x / 10);

        var longChain = branch1Chain1
            .ContinueWith(x => x * 3)
            .ContinueWith(x => x.ToString() + "!");

        Assert.Multiple(() =>
        {
            Assert.That(root.Result, Is.EqualTo(100));

            Assert.That(branch1.Result, Is.EqualTo(50));
            Assert.That(branch2.Result, Is.EqualTo(200));

            Assert.That(branch1Chain1.Result, Is.EqualTo(60));
            Assert.That(branch1Chain2.Result, Is.EqualTo(40));
            Assert.That(branch2Chain1.Result, Is.EqualTo("200"));
            Assert.That(branch2Chain2.Result, Is.EqualTo(20));

            Assert.That(longChain.Result, Is.EqualTo("180!"));

            Assert.That(root.IsCompleted, Is.True);
            Assert.That(branch1.IsCompleted, Is.True);
            Assert.That(branch2.IsCompleted, Is.True);
            Assert.That(branch1Chain1.IsCompleted, Is.True);
            Assert.That(branch1Chain2.IsCompleted, Is.True);
            Assert.That(branch2Chain1.IsCompleted, Is.True);
            Assert.That(branch2Chain2.IsCompleted, Is.True);
            Assert.That(longChain.IsCompleted, Is.True);
        });

        pool.Shutdown();
    }

    /// <summary>
    /// Test concurrent submission of many tasks to the pool.
    /// </summary>
    [Test]
    public void MyThreadPool_ConcurrentSubmit_ManyTasks()
    {
        const int threadCount = 4;
        const int tasksPerThread = 250;

        var pool = new MyThreadPool(threadCount);
        var tasks = new List<IMyTask<int>>();
        var lockObj = new object();
        var exceptions = new ConcurrentBag<Exception>();

        var threads = new Thread[threadCount];
        for (var i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (var j = 0; j < tasksPerThread; j++)
                {
                    try
                    {
                        var task = pool.Submit(() => Thread.CurrentThread.ManagedThreadId);
                        lock (lockObj)
                        {
                            tasks.Add(task);
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.Multiple(() =>
        {
            Assert.That(exceptions, Is.Empty, "No exceptions should be thrown during concurrent Submit");
            Assert.That(tasks, Has.Count.EqualTo(threadCount * tasksPerThread));
        });

        foreach (var task in tasks)
        {
            Assert.DoesNotThrow(() => _ = task.Result);
            Assert.That(task.IsCompleted, Is.True);
        }

        pool.Shutdown();
    }

    /// <summary>
    /// Test Shutdown while continuations are being created.
    /// </summary>
    [Test]
    public void MyThreadPool_ShutdownDuringContinueWith()
    {
        const int iterations = 50;

        for (var i = 0; i < iterations; i++)
        {
            var pool = new MyThreadPool(2);
            var rootTask = pool.Submit(() => 100);

            var continuations = new List<IMyTask<int>>();
            var continuationLock = new object();

            var continuationThread = new Thread(() =>
            {
                for (var iteration = 0; iteration < 20; iteration++)
                {
                    var currentIteration = iteration;
                    try
                    {
                        var continuation = rootTask.ContinueWith(x => x + currentIteration);
                        lock (continuationLock)
                        {
                            continuations.Add(continuation);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            });

            var shutdownThread = new Thread(() =>
            {
                Thread.Sleep(GetRandomDelay(0, 15));
                pool.Shutdown();
            });

            continuationThread.Start();
            shutdownThread.Start();

            continuationThread.Join();
            shutdownThread.Join();

            Assert.That(rootTask.Result, Is.EqualTo(100));

            foreach (var continuation in continuations)
            {
                if (continuation.IsCompleted)
                {
                    Assert.DoesNotThrow(() => _ = continuation.Result);
                }
            }
        }
    }

    private static int GetRandomDelay(int min, int max)
    {
        var random = new Random(Guid.NewGuid().GetHashCode());
        return random.Next(min, max);
    }
}