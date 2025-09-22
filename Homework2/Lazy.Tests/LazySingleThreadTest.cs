// <copyright file="LazySingleThreadTest.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace Lazy.Tests;

/// <summary>
/// tests for lazy in single thread.
/// </summary>
[TestFixture]
public class LazySingleThreadTest : GeneralBehaviorTests
{
    /// <summary>
    /// test for correct work Get() with double.
    /// </summary>
    [Test]
    public void LazySingleThread_Get_ValueType_ReturnsCorrectValue()
    {
        var callCount = 0;
        var lazy = this.CreateLazy<double>(() =>
            {
                callCount++;
                return 3.145;
            });
        const double expected = 3.145;

        var result1 = lazy.Get();
        var result2 = lazy.Get();

        Assert.Multiple(() =>
        {
            Assert.That(result1, Is.EqualTo(expected));
            Assert.That(result2, Is.EqualTo(expected));
            Assert.That(callCount, Is.EqualTo(1));
        });
    }

    /// <summary>
    /// The CreateLazy implementation for creating an instance of LazySingleThread.
    /// </summary>
    /// <param name="supplier">the function that calculates the value.</param>
    /// <typeparam name="T">type of supplier value.</typeparam>
    /// <returns>new instance of ILazy.</returns>
    protected override ILazy<T> CreateLazy<T>(Func<T> supplier)
    {
        return LazySingleThread<T>.Create(supplier);
    }
}