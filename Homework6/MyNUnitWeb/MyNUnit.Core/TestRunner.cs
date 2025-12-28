//<copyright file="TestRunner.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit;

using System.Reflection;
using MyNUnit.Models;

/// <summary>
/// test runner.
/// </summary>
public class TestRunner
{
    private readonly string path;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestRunner"/> class.
    /// </summary>
    /// <param name="path">path to test project.</param>
    public TestRunner(string path)
    {
        this.path = path ?? throw new ArgumentNullException(nameof(path));
    }

    /// <summary>
    /// to run tests.
    /// </summary>
    /// <returns>return list with result of each test.</returns>
    public List<TestResult> RunTests()
    {
        var testResults = new List<TestResult>();

        var assemblies = this.FindAssemblies(this.path);

        Parallel.ForEach(assemblies, assemblyPath =>
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var testClasses = this.FindTestClasses(assembly);

                foreach (var testClass in testClasses)
                {
                    var testClassResults = this.RunTestsInClass(testClass);
                    lock (testResults)
                    {
                        testResults.AddRange(testClassResults);
                    }
                }
            }
            catch (Exception ex)
            {
                lock (testResults)
                {
                    testResults.Add(new TestResult
                    {
                        ClassName = "AssemblyLoader",
                        MethodName = Path.GetFileName(assemblyPath),
                        Status = TestStatus.Error,
                        ErrorMessage = $"Failed to load assembly: {ex.Message}",
                        Exception = ex,
                    });
                }
            }
        });

        return testResults;
    }

    /// <summary>
    /// to find all assemblies.
    /// </summary>
    /// <param name="searchPath">path to test project.</param>
    /// <returns>list with assemblies.</returns>
    private List<string> FindAssemblies(string searchPath)
    {
        var assemblies = new List<string>();

        if (File.Exists(searchPath) && searchPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            assemblies.Add(searchPath);
        }
        else if (Directory.Exists(searchPath))
        {
            assemblies.AddRange(Directory.GetFiles(searchPath, "*.dll", SearchOption.AllDirectories));
            assemblies.AddRange(Directory.GetFiles(searchPath, "*.exe", SearchOption.AllDirectories));
        }

        return assemblies.Where(a => !Path.GetFileName(a).StartsWith("MyNUnit")).ToList();
    }

    /// <summary>
    /// to find all test classes.
    /// </summary>
    /// <param name="assembly">current assembly.</param>
    /// <returns>list with all test classes.</returns>
    private List<Type> FindTestClasses(Assembly assembly)
    {
        var testClasses = new List<Type>();

        try
        {
            foreach (var type in assembly.GetTypes())
            {
                var methods = type.GetMethods();
                if (methods.Any(m => m.GetCustomAttribute<Attributes.TestAttribute>() != null))
                {
                    testClasses.Add(type);
                }
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            Console.WriteLine($"Failed to load types from assembly {assembly.FullName}: {ex.Message}");
        }

        return testClasses;
    }

    /// <summary>
    /// to run tests in current class.
    /// </summary>
    /// <param name="testClass">class with tests to run.</param>
    /// <returns>list with results of each test.</returns>
    private List<TestResult> RunTestsInClass(Type testClass)
    {
        var results = new List<TestResult>();

        try
        {
            AttributeFinder.ValidateTestClass(testClass);

            var beforeClassMethods = AttributeFinder.GetBeforeClassMethods(testClass);
            var afterClassMethods = AttributeFinder.GetAfterClassMethods(testClass);
            var beforeMethods = AttributeFinder.GetBeforeMethods(testClass);
            var afterMethods = AttributeFinder.GetAfterMethods(testClass);
            var testMethods = AttributeFinder.GetTestMethods(testClass);

            if (testMethods.Count == 0)
            {
                return results;
            }

            // Run BeforeClass methods
            foreach (var method in beforeClassMethods)
            {
                method.Invoke(null, null);
            }

            // Run tests in parallel, but create separate instance for each test
            Parallel.ForEach(testMethods, testMethod =>
            {
                var testResult = this.RunSingleTest(testClass, testMethod, beforeMethods, afterMethods);
                lock (results)
                {
                    results.Add(testResult);
                }
            });

            // Run AfterClass methods
            foreach (var method in afterClassMethods)
            {
                method.Invoke(null, null);
            }
        }
        catch (Exception ex)
        {
            results.Add(new TestResult
            {
                ClassName = testClass.FullName ?? testClass.Name,
                MethodName = "ClassInitialization",
                Status = TestStatus.Error,
                ErrorMessage = $"Failed to initialize test class: {ex.Message}",
                Exception = ex,
            });
        }

        return results;
    }

    /// <summary>
    /// Runs a single test with Before/After methods.
    /// </summary>
    private TestResult RunSingleTest(
        Type testClass,
        MethodInfo testMethod,
        List<MethodInfo> beforeMethods,
        List<MethodInfo> afterMethods)
    {
        var executor = new TestExecutor();

        try
        {
            var testInstance = Activator.CreateInstance(testClass);

            if (testInstance == null)
            {
                return new TestResult
                {
                    ClassName = testClass.FullName ?? testClass.Name,
                    MethodName = testMethod.Name,
                    Status = TestStatus.Error,
                    ErrorMessage = $"Failed to create instance of test class",
                    Exception = new InvalidOperationException($"Could not instantiate {testClass.Name}"),
                };
            }

            foreach (var method in beforeMethods)
            {
                method.Invoke(testInstance, null);
            }

            var result = executor.ExecuteTest(testInstance, testMethod);

            foreach (var method in afterMethods)
            {
                try
                {
                    method.Invoke(testInstance, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: After method '{method.Name}' failed: {ex.Message}");
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            return new TestResult
            {
                ClassName = testClass.FullName ?? testClass.Name,
                MethodName = testMethod.Name,
                Status = TestStatus.Error,
                ErrorMessage = $"Test setup failed: {ex.Message}",
                Exception = ex,
            };
        }
    }
}