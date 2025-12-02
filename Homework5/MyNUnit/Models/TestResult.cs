// <copyright file="TestResult.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit.Models;

/// <summary>
/// result of test.
/// </summary>
public class TestResult
{
    /// <summary>
    /// Gets or sets name of tested class.
    /// </summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets name of nmethod.
    /// </summary>
    public string MethodName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets test status.
    /// </summary>
    public TestStatus Status { get; set; }

    /// <summary>
    /// Gets or sets duration of test.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets error message.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets exception while test.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets reason of ignoring test.
    /// </summary>
    public string? IgnoreReason { get; set; }

    /// <summary>
    /// Gets full name of method.
    /// </summary>
    public string FullName => $"{this.ClassName}.{this.MethodName}";
}