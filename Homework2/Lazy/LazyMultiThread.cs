// <copyright file="LazyMultiThread.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace Lazy;

/// <summary>
/// lazy calculation of type T for multi thread.
/// </summary>
/// <typeparam name="T">The type of value to be calculated.</typeparam>
public class LazyMultiThread<T> : ILazy<T>
{
    private readonly object lockObject = new();

    private Func<T>? supplier;
    private T? value;
    private volatile bool isValueCalculated;

    /// <summary>
    /// Initializes a new instance of the <see cref="LazyMultiThread{T}"/> class.
    /// </summary>
    /// <param name="supplier">the function that calculates the value.</param>
    public LazyMultiThread(Func<T> supplier)
    {
        this.supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
        this.isValueCalculated = false;
    }

    /// <summary>
    /// creating an instance of the class LazyMultiThread.
    /// </summary>
    /// <param name="supplier">the function that calculate the value.</param>
    /// <returns>new instance of current class.</returns>
    public static LazyMultiThread<T> Create(Func<T> supplier) => new LazyMultiThread<T>(supplier);

    /// <inheritdoc/>
    public T Get()
    {
        if (!this.isValueCalculated)
        {
            lock (this.lockObject)
            {
                if (!this.isValueCalculated)
                {
                    this.value = this.supplier!();
                    this.supplier = null;
                    this.isValueCalculated = true;
                }
            }
        }

        return this.value!;
    }
}