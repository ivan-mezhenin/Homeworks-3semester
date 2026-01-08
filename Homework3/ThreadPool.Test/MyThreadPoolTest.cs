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
}