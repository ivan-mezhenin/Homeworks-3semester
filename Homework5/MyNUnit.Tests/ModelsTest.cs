// <copyright file="ModelsTest.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit.Tests;

using MyNUnit.Models;
using NUnit.Framework;

/// <summary>
/// test for models.
/// </summary>
public class ModelsTest
{
    /// <summary>
    /// Tests that TestResult.FullName property returns correct concatenated name.
    /// </summary>
    [Test]
    public void TestResult_FullName_ReturnsCorrectFormat()
    {
        var result = new TestResult
        {
            ClassName = "Namespace.TestClass",
            MethodName = "TestMethod",
        };

        Assert.That(result.FullName, Is.EqualTo("Namespace.TestClass.TestMethod"));
    }

    /// <summary>
    /// Tests that TestResult with null class name returns empty string for FullName.
    /// </summary>
    [Test]
    public void TestResult_FullName_WithEmptyClassName_ReturnsMethodNameOnly()
    {
        var result = new TestResult
        {
            ClassName = string.Empty,
            MethodName = "TestMethod",
        };

        Assert.That(result.FullName, Is.EqualTo(".TestMethod"));
    }

    /// <summary>
    /// Tests that TestClassResult calculates correct statistics.
    /// </summary>
    [Test]
    public void TestClassResult_CalculatesStatisticsCorrectly()
    {
        var classResult = new TestClassResult
        {
            ClassName = "TestClass",
            TestResults =
            [
                new TestResult { Status = TestStatus.Passed, Duration = TimeSpan.FromMilliseconds(100) },
                new TestResult { Status = TestStatus.Passed, Duration = TimeSpan.FromMilliseconds(200) },
                new TestResult { Status = TestStatus.Failed, Duration = TimeSpan.FromMilliseconds(150) },
                new TestResult { Status = TestStatus.Ignored, Duration = TimeSpan.FromMilliseconds(0) },
                new TestResult { Status = TestStatus.Error, Duration = TimeSpan.FromMilliseconds(300) }
            ],
        };

        Assert.Multiple(() =>
        {
            Assert.That(classResult.TotalTests, Is.EqualTo(5));
            Assert.That(classResult.Passed, Is.EqualTo(2));
            Assert.That(classResult.Failed, Is.EqualTo(2));
            Assert.That(classResult.Ignored, Is.EqualTo(1));
        });
    }

    /// <summary>
    /// Tests that TestClassResult with empty test results returns zero statistics.
    /// </summary>
    [Test]
    public void TestClassResult_EmptyResults_ReturnsZeroStatistics()
    {
        var classResult = new TestClassResult
        {
            ClassName = "EmptyTestClass",
            TestResults = new List<TestResult>(),
        };

        Assert.Multiple(() =>
        {
            Assert.That(classResult.TotalTests, Is.EqualTo(0));
            Assert.That(classResult.Passed, Is.EqualTo(0));
            Assert.That(classResult.Failed, Is.EqualTo(0));
            Assert.That(classResult.Ignored, Is.EqualTo(0));
            Assert.That(classResult.TotalDuration, Is.EqualTo(TimeSpan.Zero));
        });
    }

    /// <summary>
    /// Tests that TestResult properties can be set and retrieved correctly.
    /// </summary>
    [Test]
    public void TestResult_Properties_SetAndGetCorrectly()
    {
        var exception = new InvalidOperationException("Test exception");
        var duration = TimeSpan.FromMilliseconds(123.45);

        var result = new TestResult
        {
            ClassName = "TestClass",
            MethodName = "TestMethod",
            Status = TestStatus.Failed,
            Duration = duration,
            ErrorMessage = "Something went wrong",
            Exception = exception,
            IgnoreReason = "Not applicable",
        };

        Assert.Multiple(() =>
        {
            Assert.That(result.ClassName, Is.EqualTo("TestClass"));
            Assert.That(result.MethodName, Is.EqualTo("TestMethod"));
            Assert.That(result.Status, Is.EqualTo(TestStatus.Failed));
            Assert.That(result.Duration, Is.EqualTo(duration));
            Assert.That(result.ErrorMessage, Is.EqualTo("Something went wrong"));
            Assert.That(result.Exception, Is.EqualTo(exception));
            Assert.That(result.IgnoreReason, Is.EqualTo("Not applicable"));
        });
    }
}