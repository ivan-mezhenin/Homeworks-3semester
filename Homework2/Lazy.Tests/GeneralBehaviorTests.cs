// <copyright file="GeneralBehaviorTests.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace Lazy.Tests;

/// <summary>
/// general behavior tests for single and multithread realization of Lazy.
/// </summary>
[TestFixture]
public abstract class GeneralBehaviorTests
{
    /// <summary>
    /// test for one call of supplier.
    /// </summary>
    [Test]
    public void Lazy_Get_SimpleValue_OneCallOfSupplier()
    {
        var calculationCount = 0;
        var lazy = this.CreateLazy<int>(() =>
            {
                calculationCount++;
                return 1;
            });

        var result1 = lazy.Get();
        var result2 = lazy.Get();

        Assert.That(calculationCount, Is.EqualTo(1));
    }

    /// <summary>
    /// test for correct work of supplier.
    /// </summary>
    [Test]
    public void Lazy_Get_SimpleSupplier_CorrectValue()
    {
        var lazy = this.CreateLazy<string>(() => "lazy");
        const string expected = "lazy";

        Assert.That(lazy.Get(), Is.EqualTo(expected));
    }

    /// <summary>
    /// test for Get() returns null if supplier returns null.
    /// </summary>
    [Test]
    public void Lazy_Get_NullSupplier_ReturnsNull()
    {
        var lazy = this.CreateLazy<string?>(() => null);
        Assert.That(lazy.Get(), Is.Null);
    }

    /// <summary>
    /// test for Get() throws exception if supplier throws.
    /// </summary>
    [Test]
    public void Lazy_Get_SupplierThrowsException_ThrowsException()
    {
        var lazy = this.CreateLazy<int>(() => throw new InvalidOperationException());
        Assert.Throws<InvalidOperationException>(() => lazy.Get());
    }

    /// <summary>
    /// test for throwing argument null exception when passing in constructor a null supplier.
    /// </summary>
    [Test]
    public void Lazy_Constructor_NullSupplier_ThrowsArgumentNullException()
       => Assert.Throws<ArgumentNullException>(() => this.CreateLazy<int>(null!));

    /// <summary>
    /// an abstract method for creating an instance of ILazy.
    /// </summary>
    /// <param name="supplier">the function that calculates the value.</param>
    /// <typeparam name="T">type of supplier value.</typeparam>
    /// <returns>new instance of ILazy.</returns>
    protected abstract ILazy<T> CreateLazy<T>(Func<T> supplier);
}