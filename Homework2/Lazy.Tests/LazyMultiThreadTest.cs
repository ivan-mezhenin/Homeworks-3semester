// <copyright file="LazyMultiThreadTest.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace Lazy.Tests;

/// <summary>
/// tests for lazy using multi threading.
/// </summary>
[TestFixture]
public class LazyMultiThreadTest : GeneralBehaviorTests
{
    /// <summary>
    /// test for absence of data races while using Get() multithreading.
    /// </summary>
    [Test]
    public void Lazy_Get_MultiThreads_NoDataRaces()
    {
        var callCount = 0;
        var lazy = this.CreateLazy<int>(() =>
        {
            Interlocked.Increment(ref callCount);
            return 44;
        });
        const int expected = 44;

        const int threadsCount = 100;
        var threads = new Thread[threadsCount];
        var results = new int[threadsCount];
        for (var i = 0; i < threadsCount; i++)
        {
            var index = i;
            threads[i] = new Thread(() =>
            {
                results[index] = lazy.Get();
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
            Assert.That(results, Has.All.EqualTo(expected));
            Assert.That(callCount, Is.EqualTo(1));
        });
    }

    /// <summary>
    /// test for Get() returns null if supplier returns null.
    /// </summary>
    [Test]
    public void Lazy_Get_MultiThreads_NullSupplier_ReturnsNull()
    {
        var callCount = 0;
        var lazy = this.CreateLazy<string?>(() =>
        {
            Interlocked.Increment(ref callCount);
            return null;
        });

        const int threadsCount = 100;
        var threads = new Thread[threadsCount];
        var results = new string?[threadsCount];
        for (var i = 0; i < threadsCount; i++)
        {
            var index = i;
            threads[i] = new Thread(() =>
            {
                results[index] = lazy.Get();
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
            Assert.That(results, Has.All.Null);
            Assert.That(callCount, Is.EqualTo(1));
        });
    }

    /// <summary>
    /// test for Get() throws exception if supplier throws.
    /// </summary>
    [Test]
    public void Lazy_Get_MultiThreads_SupplierThrowsException_ThrowsException()
    {
        var lazy = this.CreateLazy<string?>(() => throw new InvalidOperationException());

        const int threadsCount = 100;
        var threads = new Thread[threadsCount];
        var exceptions = new Exception[threadsCount];
        for (var i = 0; i < threadsCount; i++)
        {
            var index = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    lazy.Get();
                }
                catch (Exception e)
                {
                    exceptions[index] = e;
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

        Assert.That(exceptions, Has.All.InstanceOf<InvalidOperationException>());
    }

    /// <summary>
    /// The CreateLazy implementation for creating an instance of LazyMultiThread.
    /// </summary>
    /// <param name="supplier">the function that calculates the value.</param>
    /// <typeparam name="T">type of supplier value.</typeparam>
    /// <returns>new instance of ILazy.</returns>
    protected override ILazy<T> CreateLazy<T>(Func<T> supplier)
    {
        return LazyMultiThread<T>.Create(supplier);
    }
}