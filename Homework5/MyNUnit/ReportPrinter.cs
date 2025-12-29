// <copyright file="ReportPrinter.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit;

using MyNUnit.Models;

/// <summary>
/// Prints test reports.
/// </summary>
public class ReportPrinter
{
    /// <summary>
    /// Prints test results to console.
    /// </summary>
    /// <param name="results">Test results to print.</param>
    public void PrintReport(List<TestResult> results)
    {
        Console.WriteLine("=== MyNUnit Test Report ===\n");

        var groupedResults = results
            .Where(r => r.ClassName != "AssemblyLoader")
            .GroupBy(r => r.ClassName)
            .ToList();

        // Print assembly loading errors first
        var assemblyErrors = results
            .Where(r => r.ClassName == "AssemblyLoader")
            .ToList();

        if (assemblyErrors.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Assembly Loading Errors:");
            Console.ResetColor();
            foreach (var error in assemblyErrors)
            {
                Console.WriteLine($"  {error.MethodName}: {error.ErrorMessage}");
            }

            Console.WriteLine();
        }

        // Print test results by class
        foreach (var classGroup in groupedResults)
        {
            Console.WriteLine($"Class: {classGroup.Key}");
            Console.WriteLine(new string('-', 80));

            var classResults = classGroup.ToList();
            var passed = classResults.Count(r => r.Status == TestStatus.Passed);
            var failed = classResults.Count(r => r.Status == TestStatus.Failed);
            var ignored = classResults.Count(r => r.Status == TestStatus.Ignored);
            var errors = classResults.Count(r => r.Status == TestStatus.Error);
            var totalDuration = classResults.Sum(r => r.Duration.TotalMilliseconds);

            Console.WriteLine($"Total: {classResults.Count}, Passed: {passed}, Failed: {failed}, " +
                            $"Ignored: {ignored}, Errors: {errors}, Duration: {totalDuration:F2}ms\n");

            // Print each test result
            foreach (var result in classResults.OrderBy(r => r.MethodName))
            {
                this.PrintTestResult(result);
            }

            Console.WriteLine();
        }

        // Print summary
        this.PrintSummary(results);
    }

    /// <summary>
    /// Prints a single test result.
    /// </summary>
    private void PrintTestResult(TestResult result)
    {
        var statusChar = result.Status switch
        {
            TestStatus.Passed => '✓',
            TestStatus.Failed => '✗',
            TestStatus.Ignored => '~',
            TestStatus.Error => '!',
            _ => '?',
        };

        var statusColor = result.Status switch
        {
            TestStatus.Passed => ConsoleColor.Green,
            TestStatus.Failed => ConsoleColor.Red,
            TestStatus.Ignored => ConsoleColor.Yellow,
            TestStatus.Error => ConsoleColor.DarkRed,
            _ => ConsoleColor.Gray,
        };

        Console.ForegroundColor = statusColor;
        Console.Write($"[{statusChar}] ");
        Console.ResetColor();

        Console.Write($"{result.MethodName} ");
        Console.Write($"({result.Duration.TotalMilliseconds:F2}ms)");

        if (result.Status == TestStatus.Ignored && !string.IsNullOrEmpty(result.IgnoreReason))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($" - Ignored: {result.IgnoreReason}");
            Console.ResetColor();
        }
        else if (result.Status == TestStatus.Failed && !string.IsNullOrEmpty(result.ErrorMessage))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($" - Failed: {result.ErrorMessage}");
            Console.ResetColor();
        }
        else if (result.Status == TestStatus.Error && !string.IsNullOrEmpty(result.ErrorMessage))
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write($" - Error: {result.ErrorMessage}");
            Console.ResetColor();
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Prints summary of all tests.
    /// </summary>
    private void PrintSummary(List<TestResult> results)
    {
        var validTests = results.Where(r => r.ClassName != "AssemblyLoader").ToList();

        if (validTests.Count == 0)
        {
            Console.WriteLine("No tests found.");
            return;
        }

        var totalTests = validTests.Count;
        var passed = validTests.Count(r => r.Status == TestStatus.Passed);
        var failed = validTests.Count(r => r.Status == TestStatus.Failed);
        var ignored = validTests.Count(r => r.Status == TestStatus.Ignored);
        var errors = validTests.Count(r => r.Status == TestStatus.Error);
        var totalDuration = validTests.Sum(r => r.Duration.TotalMilliseconds);

        Console.WriteLine("=== Summary ===");
        Console.WriteLine($"Total tests: {totalTests}");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Passed: {passed}");
        Console.ResetColor();

        if (failed > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }

        Console.WriteLine($"Failed: {failed}");
        Console.ResetColor();

        if (ignored > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
        }

        Console.WriteLine($"Ignored: {ignored}");
        Console.ResetColor();

        if (errors > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
        }

        Console.WriteLine($"Errors: {errors}");
        Console.ResetColor();

        Console.WriteLine($"Total duration: {totalDuration:F2}ms");

        if (passed == totalTests)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n All tests passed!");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n {failed + errors} test(s) failed or had errors.");
            Console.ResetColor();
        }
    }
}