// <copyright file="TestExecutorTest.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit.Tests;

using MyNUnit;
using MyNUnit.Models;
using MyNUnit.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// tests for test executor.
/// </summary>
public class TestExecutorTest
{
    private readonly TestExecutor executor = new();

    /// <summary>
    /// Tests that a test method throwing DivideByZeroException returns Failed status.
    /// </summary>
    [Test]
    public void ExecuteTest_DivideByZeroException_ReturnsFailedStatus()
    {
        var instance = new ExceptionTestClass();
        var method = typeof(ExceptionTestClass).GetMethod(nameof(ExceptionTestClass.DivideByZeroExceptionTest))!;

        var result = this.executor.ExecuteTest(instance, method);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(TestStatus.Failed));
            Assert.That(result.Exception, Is.TypeOf<DivideByZeroException>());
        });
    }

    /// <summary>
    /// Tests that a test method throwing ArgumentException returns Failed status.
    /// </summary>
    [Test]
    public void ExecuteTest_ArgumentException_ReturnsFailedStatus()
    {
        var instance = new ExceptionTestClass();
        var method = typeof(ExceptionTestClass).GetMethod(nameof(ExceptionTestClass.ArgumentExceptionTest))!;

        var result = this.executor.ExecuteTest(instance, method);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(TestStatus.Failed));
            Assert.That(result.Exception, Is.TypeOf<ArgumentException>());
        });
    }

    /// <summary>
    /// Tests that a test method with Expected exception that matches thrown exception returns Passed status.
    /// </summary>
    [Test]
    public void ExecuteTest_ExpectedExceptionMatches_ReturnsPassedStatus()
    {
        var instance = new ExceptionTestClass();
        var method = typeof(ExceptionTestClass).GetMethod(nameof(ExceptionTestClass.ExpectedExceptionPassesTest))!;

        var result = this.executor.ExecuteTest(instance, method);

        Assert.That(result.Status, Is.EqualTo(TestStatus.Passed));
    }

    /// <summary>
    /// Tests that a test method with Expected exception but different thrown exception returns Failed status.
    /// </summary>
    [Test]
    public void ExecuteTest_WrongExceptionType_ReturnsFailedStatus()
    {
        var instance = new ExceptionTestClass();
        var method = typeof(ExceptionTestClass).GetMethod(nameof(ExceptionTestClass.WrongExceptionTypeTest))!;

        var result = this.executor.ExecuteTest(instance, method);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(TestStatus.Failed));
            Assert.That(result.ErrorMessage, Contains.Substring("Expected ArgumentException"));
            Assert.That(result.ErrorMessage, Contains.Substring("but got InvalidOperationException"));
        });
    }

    /// <summary>
    /// Tests that a deprecated test with Ignore attribute returns Ignored status.
    /// </summary>
    [Test]
    public void ExecuteTest_DeprecatedTest_ReturnsIgnoredStatus()
    {
        var instance = new ComplexTestClass();
        var method = typeof(ComplexTestClass).GetMethod(nameof(ComplexTestClass.DeprecatedTest))!;

        var result = this.executor.ExecuteTest(instance, method);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(TestStatus.Ignored));
            Assert.That(result.IgnoreReason, Is.EqualTo("Deprecated test"));
        });
    }
}