// <copyright file="TestRun.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit.Core.Models;

/// <summary>
/// Represents a single test run execution containing test results and metadata.
/// A test run is created each time the test suite is executed.
/// </summary>
public class TestRun
{
    /// <summary>
    /// Gets or sets the unique identifier for the test run.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the date and time when the test run started.
    /// Stored in UTC format.
    /// </summary>
    public DateTime RunTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the total duration of the test run in milliseconds.
    /// This includes the execution time of all tests in the run.
    /// </summary>
    public long TotalDurationMs { get; set; }

    /// <summary>
    /// Gets or sets the collection of individual test results in this run.
    /// Each entry represents the outcome of a single test method.
    /// </summary>
    public List<TestResult> TestResults { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of assemblies that were tested in this run.
    /// Contains metadata about the uploaded DLL files.
    /// </summary>
    public List<UploadedAssembly> Assemblies { get; set; } = [];

    /// <summary>
    /// Gets the total number of tests executed in this run.
    /// </summary>
    public int TotalTests => this.TestResults.Count;

    /// <summary>
    /// Gets the number of tests that passed successfully.
    /// </summary>
    public int Passed => this.TestResults.Count(r => r.Status == TestStatus.Passed);

    /// <summary>
    /// Gets the number of tests that failed (assertion failures).
    /// </summary>
    public int Failed => this.TestResults.Count(r => r.Status == TestStatus.Failed);

    /// <summary>
    /// Gets the number of tests that were ignored/skipped.
    /// </summary>
    public int Ignored => this.TestResults.Count(r => r.Status == TestStatus.Ignored);

    /// <summary>
    /// Gets the number of tests that encountered errors during execution
    /// (exceptions in test setup or infrastructure).
    /// </summary>
    public int Error => this.TestResults.Count(r => r.Status == TestStatus.Error);
}