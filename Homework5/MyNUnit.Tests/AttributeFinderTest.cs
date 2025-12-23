// <copyright file="AttributeFinderTest.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit.Tests;

using MyNUnit;
using MyNUnit.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// tests for AttributeFinder.cs.
/// </summary>
[TestFixture]
public class AttributeFinderTest
{
    /// <summary>
    /// Tests that the GetTestMethods method correctly identifies all methods
    /// marked with the custom [Test] attribute, including those that are ignored.
    /// </summary>
    [Test]
    public void GetTestMethods_FindsAllTestMethods()
    {
        var methods = AttributeFinder.GetTestMethods(typeof(TestClassForAttributeFinder));

        Assert.That(methods, Has.Count.EqualTo(2));
        Assert.That(
            methods.Select(m => m.Name),
            Contains.Item(nameof(TestClassForAttributeFinder.TestMethod)));
        Assert.That(
            methods.Select(m => m.Name),
            Contains.Item(nameof(TestClassForAttributeFinder.IgnoredTestMethod)));
    }

    /// <summary>
    /// Tests that the GetBeforeClassMethods method correctly identifies static methods
    /// marked with the custom [BeforeClass] attribute.
    /// </summary>
    [Test]
    public void GetBeforeClassMethods_FindsStaticBeforeClassMethod()
    {
        var methods = AttributeFinder.GetBeforeClassMethods(typeof(TestClassForAttributeFinder));

        Assert.Multiple(() =>
        {
            Assert.That(methods, Has.Count.EqualTo(1));
            Assert.That(methods[0].Name, Is.EqualTo(nameof(TestClassForAttributeFinder.BeforeClassMethod)));
            Assert.That(methods[0].IsStatic, Is.True);
        });
    }

    /// <summary>
    /// Tests that the GetAfterClassMethods method correctly identifies static methods
    /// marked with the custom [AfterClass] attribute.
    /// </summary>
    [Test]
    public void GetAfterClassMethods_FindsStaticAfterClassMethod()
    {
        var methods = AttributeFinder.GetAfterClassMethods(typeof(TestClassForAttributeFinder));

        Assert.Multiple(() =>
        {
            Assert.That(methods, Has.Count.EqualTo(1));
            Assert.That(methods[0].Name, Is.EqualTo(nameof(TestClassForAttributeFinder.AfterClassMethod)));
            Assert.That(methods[0].IsStatic, Is.True);
        });
    }

    /// <summary>
    /// Tests that the GetBeforeMethods method correctly identifies instance methods
    /// marked with the custom [Before] attribute.
    /// </summary>
    [Test]
    public void GetBeforeMethods_FindsInstanceBeforeMethods()
    {
        var methods = AttributeFinder.GetBeforeMethods(typeof(TestClassForAttributeFinder));

        Assert.Multiple(() =>
        {
            Assert.That(methods, Has.Count.EqualTo(1));
            Assert.That(methods[0].Name, Is.EqualTo(nameof(TestClassForAttributeFinder.BeforeMethod)));
            Assert.That(methods[0].IsStatic, Is.False);
        });
    }

    /// <summary>
    /// Tests that the GetAfterMethods method correctly identifies instance methods
    /// marked with the custom [After] attribute.
    /// </summary>
    [Test]
    public void GetAfterMethods_FindsInstanceAfterMethods()
    {
        var methods = AttributeFinder.GetAfterMethods(typeof(TestClassForAttributeFinder));

        Assert.Multiple(() =>
        {
            Assert.That(methods, Has.Count.EqualTo(1));
            Assert.That(methods[0].Name, Is.EqualTo(nameof(TestClassForAttributeFinder.AfterMethod)));
            Assert.That(methods[0].IsStatic, Is.False);
        });
    }

    /// <summary>
    /// Tests that the ValidateTestClass method does not throw an exception
    /// when validating a test class with properly defined static BeforeClass and AfterClass methods.
    /// </summary>
    [Test]
    public void ValidateTestClass_ValidStaticMethods_DoesNotThrow()
    {
        Assert.DoesNotThrow(() =>
            AttributeFinder.ValidateTestClass(typeof(TestClassForAttributeFinder)));
    }
}