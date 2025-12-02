// <copyright file="TestClassResult.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit.Models;

/// <summary>
/// results of test class.
/// </summary>
public class TestClassResult
{
    /// <summary>
    /// Gets or sets name of test class.
    /// </summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets list of test results.
    /// </summary>
    public List<TestResult> TestResults { get; set; } = [];

    /// <summary>
    /// Gets amount of tests.
    /// </summary>
    public int TotalTests => this.TestResults.Count;

    /// <summary>
    /// Gets amount of passed tests.
    /// </summary>
    public int Passed => this.TestResults.Count(r => r.Status == TestStatus.Passed);

    /// <summary>
    /// Gets amount of failed tests.
    /// </summary>
    public int Failed => this.TestResults.Count(r => r.Status is TestStatus.Failed or TestStatus.Error);

    /// <summary>
    /// Gets amount of ignored tests.
    /// </summary>
    public int Ignored => this.TestResults.Count(r => r.Status == TestStatus.Ignored);

    /// <summary>
    /// Gets or sets total tests duration.
    /// </summary>
    public TimeSpan TotalDuration { get; set; }
}