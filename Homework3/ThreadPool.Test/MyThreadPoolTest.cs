// <copyright file="MyThreadPoolTest.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace ThreadPool.Test;

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

        var task = pool.Submit(() => numbers.Sum(x => x * x)); // Задача: сумма квадратов чисел

        Assert.Multiple(() =>
        {
            Assert.That(task.Result, Is.EqualTo(expectedResult));
            Assert.That(task.IsCompleted, Is.True);
            Assert.That(pool.PoolException, Is.Null);
        });

        pool.Shutdown();
    }

    /// <summary>
    /// test for correct throwing aggregate exception and ending of the threads.
    /// </summary>
    [Test]
    public void MyThreadPool_Submit_TaskWithException_ThrowsAggregateExceptionAndShutsDownPool()
    {
        var pool = new MyThreadPool(2);
        var failingTask = () =>
        {
            int[] numbers = [];
            if (numbers.Length == 0)
            {
                throw new InvalidOperationException();
            }

            return numbers.Sum(x => x * x);
        };

        var task = pool.Submit(failingTask);
        Exception? caughtException = null;
        try
        {
             _ = task.Result;
        }
        catch (AggregateException ex)
        {
            caughtException = ex.InnerException;
        }

        Assert.Multiple(() =>
        {
            Assert.That(caughtException, Is.TypeOf<InvalidOperationException>());
            Assert.That(pool.PoolException, Is.Not.Null);
            Assert.That(() => pool.Submit(() => 0), Throws.InvalidOperationException);
        });
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

        Assert.That(pool.PoolException, Is.Null);

        pool.Shutdown();
    }

    /// <summary>
    /// test for creation at least N threads when initializing thread pool.
    /// </summary>
    [Test]
    public void MyThreadPool_Constructor_CreatesAtLeastNThreads()
    {
        const int threadCount = 10;

        var pool = new MyThreadPool(threadCount);
        var threads = pool.GetType().GetField("threads", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(pool) as Thread[];

        Assert.That(threads, Is.Not.Null);
        Assert.That(threads, Has.Length.EqualTo(threadCount));
        Assert.That(threads, Has.All.Property("IsAlive").True);

        pool.Shutdown();
    }
}