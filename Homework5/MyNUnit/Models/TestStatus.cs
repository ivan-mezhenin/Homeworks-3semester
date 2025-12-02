namespace MyNUnit.Models;

/// <summary>
/// status of test.
/// </summary>
public enum TestStatus
{
    /// <summary>
    /// Test is passed.
    /// </summary>
    Passed,

    /// <summary>
    /// test is failed.
    /// </summary>
    Failed,

    /// <summary>
    /// test is ignored.
    /// </summary>
    Ignored,

    /// <summary>
    /// Error while passing test.
    /// </summary>
    Error,
}