// <copyright file="TestClassForAttributeFinder.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit.Tests.TestClasses;

using MyNUnit.Attributes;

/// <summary>
/// Test class containing various methods annotated with custom MyNUnit attributes.
/// Used for testing the AttributeFinder functionality.
/// This class includes examples of all supported attribute types.
/// </summary>
public class TestClassForAttributeFinder
{
    /// <summary>
    /// Static method marked with [BeforeClass] attribute.
    /// Should be executed once before all tests in the class.
    /// </summary>
    [BeforeClass]
    public static void BeforeClassMethod()
    {
    }

    /// <summary>
    /// Static method marked with [AfterClass] attribute.
    /// Should be executed once after all tests in the class.
    /// </summary>
    [AfterClass]
    public static void AfterClassMethod()
    {
    }

    /// <summary>
    /// Instance method marked with [Before] attribute.
    /// Should be executed before each test method.
    /// </summary>
    [Before]
    public void BeforeMethod()
    {
    }

    /// <summary>
    /// Regular test method marked with [Test] attribute.
    /// Represents a standard test case.
    /// </summary>
    [Test]
    public void TestMethod()
    {
    }

    /// <summary>
    /// Test method marked with [Test] attribute and Ignore property set to true.
    /// This test should be skipped during execution with the specified reason.
    /// </summary>
    [Test(Ignore = true, IgnoreReason = "Not ready")]
    public void IgnoredTestMethod()
    {
    }

    /// <summary>
    /// Instance method marked with [After] attribute.
    /// Should be executed after each test method.
    /// </summary>
    [After]
    public void AfterMethod()
    {
    }

    /// <summary>
    /// Regular method without any custom attributes.
    /// Should not be recognized as a test or lifecycle method.
    /// </summary>
    public void RegularMethod()
    {
    }
}