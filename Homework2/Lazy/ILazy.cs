// <copyright file="ILazy.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace Lazy;

/// <summary>
/// Interface for lazy calculation of type T.
/// </summary>
/// <typeparam name="T">The type of value to be calculated.</typeparam>
public interface ILazy<T>
{
    /// <summary>
    /// Gets the calculated value. On the first call, the calculation is performed,
    /// subsequent calls return the cached value.
    /// </summary>
    /// <returns>Calculated value of the type T.</returns>
    T Get();
}