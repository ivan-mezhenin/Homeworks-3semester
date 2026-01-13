// <copyright file="ExceptionTestClass.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit.Tests.TestClasses;

using MyNUnit.Attributes;

/// <summary>
/// Test class with methods that throw various exceptions.
/// </summary>
public class ExceptionTestClass
{
    /// <summary>
    /// Test method that triggers a DivideByZeroException at runtime.
    /// This demonstrates how runtime exceptions are handled by the test framework.
    /// The division by zero operation will cause the CLR to throw a DivideByZeroException.
    /// </summary>
    [Test]
    public void DivideByZeroExceptionTest()
    {
        var x = 0;

        var y = 1;
        _ = y / x;
    }

    /// <summary>
    /// Test method that explicitly throws an ArgumentException with a custom message.
    /// </summary>
    [Test]
    public void ArgumentExceptionTest()
    {
        throw new ArgumentException("Invalid argument");
    }

    /// <summary>
    /// Test method annotated with Expected property that matches the thrown exception.
    /// </summary>
    [Test(Expected = typeof(InvalidOperationException))]
    public void ExpectedExceptionPassesTest()
    {
        throw new InvalidOperationException("Expected exception");
    }

    /// <summary>
    /// Test method where the thrown exception type does not match the expected type.
    /// </summary>
    [Test(Expected = typeof(ArgumentException))]
    public void WrongExceptionTypeTest()
    {
        throw new InvalidOperationException("Wrong exception type");
    }
}