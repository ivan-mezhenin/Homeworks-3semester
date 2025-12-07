// <copyright file="TestExecutor.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit;

using System.Diagnostics;
using System.Reflection;
using MyNUnit.Attributes;
using MyNUnit.Models;

/// <summary>
/// Executor of current test.
/// </summary>
public class TestExecutor
{
    /// <summary>
    /// to execute test.
    /// </summary>
    /// <param name="testInstance"> an instance of the class with tests.</param>
    /// <param name="testMethod">test method.</param>
    /// <returns>test result.</returns>
    public TestResult ExecuteTest(object testInstance, MethodInfo testMethod)
    {
        var testAttribute = testMethod.GetCustomAttribute<TestAttribute>();
        var result = new TestResult
        {
            ClassName = testMethod.DeclaringType?.FullName ?? string.Empty,
            MethodName = testMethod.Name,
        };

        if (testAttribute?.Ignore == true)
        {
            result.Status = TestStatus.Ignored;
            result.IgnoreReason = testAttribute.IgnoreReason;
            return result;
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (testAttribute?.Expected != null)
            {
                this.ExecuteTestWithExpectedException(testInstance, testMethod, testAttribute.Expected, result);
            }
            else
            {
                this.ExecuteTestMethod(testInstance, testMethod, result);
            }
        }
        catch (Exception ex)
        {
            result.Status = TestStatus.Error;
            result.ErrorMessage = $"Test error: {ex.Message}";
            result.Exception = ex;
        }
        finally
        {
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
        }

        return result;
    }

    /// <summary>
    /// to execute test with expected exception.
    /// </summary>
    /// <param name="testInstance">an instance of the class with tests.</param>
    /// <param name="testMethod">test method.</param>
    /// <param name="expectedExceptionType">expected exception type.</param>
    /// <param name="result">test result.</param>
    private void ExecuteTestWithExpectedException(
        object testInstance,
        MethodInfo testMethod,
        Type expectedExceptionType,
        TestResult result)
    {
        try
        {
            testMethod.Invoke(testInstance, null);

            result.Status = TestStatus.Failed;
            result.ErrorMessage = $"Expected exception {expectedExceptionType.Name} was not thrown";
        }
        catch (TargetInvocationException ex)
        {
            var actualException = ex.InnerException;

            if (expectedExceptionType.IsInstanceOfType(actualException))
            {
                result.Status = TestStatus.Passed;
            }
            else
            {
                result.Status = TestStatus.Failed;
                result.ErrorMessage =
                    $"Expected {expectedExceptionType.Name}, but got {actualException?.GetType().Name}";
                result.Exception = actualException;
            }
        }
    }

    /// <summary>
    /// to execute test.
    /// </summary>
    /// <param name="testInstance">an instance of the class with tests.</param>
    /// <param name="testMethod">test method.</param>
    /// <param name="result">test result.</param>
    private void ExecuteTestMethod(object testInstance, MethodInfo testMethod, TestResult result)
    {
        try
        {
            testMethod.Invoke(testInstance, null);
            result.Status = TestStatus.Passed;
        }
        catch (TargetInvocationException ex)
        {
            result.Status = TestStatus.Failed;
            result.ErrorMessage = ex.InnerException?.Message ?? ex.Message;
            result.Exception = ex.InnerException ?? ex;
        }
    }
}
