// <copyright file="LazySingleThread.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace Lazy;

/// <summary>
/// lazy calculation of type T for single thread.
/// </summary>
/// <typeparam name="T">The type of value to be calculated.</typeparam>
public class LazySingleThread<T> : ILazy<T>
{
    private Func<T>? supplier;
    private T? value;
    private bool isValueCalculated;

    /// <summary>
    /// Initializes a new instance of the <see cref="LazySingleThread{T}"/> class.
    /// </summary>
    /// <param name="supplier">the function that calculates the value.</param>
    public LazySingleThread(Func<T> supplier)
    {
        this.supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
        this.isValueCalculated = false;
    }

    /// <summary>
    /// creating an instance of the class LazySingleThread.
    /// </summary>
    /// <param name="supplier">the function that calculate the value.</param>
    /// <returns>new instance of current class.</returns>
    public static LazySingleThread<T> Create(Func<T> supplier) => new LazySingleThread<T>(supplier);

    /// <inheritdoc/>
    public T Get()
    {
        if (!this.isValueCalculated)
        {
            this.value = this.supplier!();
            this.supplier = null;
            this.isValueCalculated = true;
        }

        return this.value!;
    }
}