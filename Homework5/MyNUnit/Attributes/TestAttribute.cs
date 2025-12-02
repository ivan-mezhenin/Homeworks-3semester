// <copyright file="TestAttribute.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit.Attributes;

/// <summary>
/// attribute for test class.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class TestAttribute
{
    /// <summary>
    /// Gets or sets expected type of exception.
    /// </summary>
    public Type? Expected { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether test should be ignored.
    /// </summary>
    public bool Ignore { get; set; }

    /// <summary>
    /// Gets or sets ignore reason.
    /// </summary>
    public string? IgnoreReason { get; set; }
}