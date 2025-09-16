// <copyright file="LazySingleThread.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace Lazy;

using System.Diagnostics.CodeAnalysis;

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