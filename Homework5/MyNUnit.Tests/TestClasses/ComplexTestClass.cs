// <copyright file="ComplexTestClass.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit.Tests.TestClasses;

using MyNUnit.Attributes;

/// <summary>
/// Test class with various test configurations demonstrating different MyNUnit features.
/// </summary>
public class ComplexTestClass
{
    private string? state;

    /// <summary>
    /// Setup method that runs before each test to initialize state.
    /// </summary>
    [Before]
    public void Setup()
    {
        this.state = "Initialized";
    }

    /// <summary>
    /// Test method that verifies the state was properly initialized by the Before method.
    /// </summary>
    [Test]
    public void TestWithState()
    {
        if (this.state != "Initialized")
        {
            throw new InvalidOperationException($"State is {this.state}, expected 'Initialized'");
        }
    }

    /// <summary>
    /// Test marked as ignored with a reason - demonstrates test exclusion functionality.
    /// </summary>
    [Test(Ignore = true, IgnoreReason = "Deprecated test")]
    public void DeprecatedTest()
    {
        throw new Exception("Should not execute");
    }

    /// <summary>
    /// Test expecting a specific exception type - demonstrates Expected exception handling.
    /// </summary>
    [Test(Expected = typeof(NotImplementedException))]
    public void NotYetImplementedTest()
    {
        throw new NotImplementedException("Feature not implemented yet");
    }

    /// <summary>
    /// Cleanup method that runs after each test to reset state.
    /// </summary>
    [After]
    public void Cleanup()
    {
        this.state = null;
    }
}