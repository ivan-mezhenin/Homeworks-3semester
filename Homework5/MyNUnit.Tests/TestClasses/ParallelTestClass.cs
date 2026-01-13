// <copyright file="ParallelTestClass.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit.Tests.TestClasses;

using MyNUnit.Attributes;

/// <summary>
/// Test class demonstrating parallel test execution scenarios.
/// </summary>
public class ParallelTestClass
{
    /// <summary>
    /// Static counter that is shared across all instances of the test class.
    /// </summary>
    private static int sharedCounter = 0;

    /// <summary>
    /// Instance counter that is unique to each test instance.
    /// </summary>
    private int instanceCounter = 0;

    /// <summary>
    /// Static method marked with [BeforeClass] attribute.
    /// </summary>
    [BeforeClass]
    public static void ResetSharedCounter()
    {
        sharedCounter = 0;
    }

    /// <summary>
    /// Instance method marked with [Before] attribute.
    /// </summary>
    [Before]
    public void ResetInstanceCounter()
    {
        this.instanceCounter = 0;
    }

    /// <summary>
    /// First parallel test method.
    /// </summary>
    [Test]
    public void ParallelTest1()
    {
        this.instanceCounter++;
        sharedCounter++;
    }

    /// <summary>
    /// Second parallel test method.
    /// </summary>
    [Test]
    public void ParallelTest2()
    {
        this.instanceCounter += 2;
        sharedCounter += 2;
    }

    /// <summary>
    /// Third parallel test method.
    /// </summary>
    [Test]
    public void ParallelTest3()
    {
        this.instanceCounter += 3;
        sharedCounter += 3;
    }
}