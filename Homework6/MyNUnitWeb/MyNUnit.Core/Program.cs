// <copyright file="Program.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit.Core;

/// <summary>
/// Main program.
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: MyNUnit.Core <path-to-tests>");
            Console.WriteLine("  path-to-tests: Path to DLL file or directory containing test assemblies");
            return;
        }

        var path = args[0];

        if (!File.Exists(path) && !Directory.Exists(path))
        {
            Console.WriteLine($"Error: Path '{path}' does not exist.");
            return;
        }

        try
        {
            Console.WriteLine($"Running tests from: {path}");
            Console.WriteLine();

            var runner = new TestRunner(path);
            var results = runner.RunTests();

            var printer = new ReportPrinter();
            printer.PrintReport(results);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}