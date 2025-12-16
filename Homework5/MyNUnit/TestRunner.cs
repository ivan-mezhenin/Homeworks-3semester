// <copyright file="TestRunner.cs" company="ivan-mezhenin">
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

        foreach (var assemblyPath in assemblies)
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);

                var testClases = this.FindTestClasses(assembly);

                foreach (var testClass in testClases)
                {
                    var testClassResults = this.RunTestsInClass(testClass);
                    testResults.AddRange(testClassResults);
                }
            }
            catch (Exception ex)
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

        return testResults;
    }

    /// <summary>
    /// to find all assemblies.
    /// </summary>
    /// <param name="path">path to test project.</param>
    /// <returns>list with assemblies.</returns>
    private List<string> FindAssemblies(string path)
    {
        var assemblies = new List<string>();

        // TODO: Реализовать поиск DLL файлов
        // Если path - файл .dll -> добавить его
        // Если path - директория -> найти все .dll в ней и поддиректориях

        return assemblies;
    }

    /// <summary>
    /// to find all test classes.
    /// </summary>
    /// <param name="assembly">current assembly.</param>
    /// <returns>list with all test classes.</returns>
    private List<Type> FindTestClasses(Assembly assembly)
    {
        var testClasses = new List<Type>();

        // TODO: Реализовать поиск классов с методами [Test]

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

        // TODO: Реализовать запуск тестов в одном классе
        // 1. Проверить валидность класса (статичность BeforeClass/AfterClass)
        // 2. Запустить BeforeClass методы
        // 3. Для каждого теста:
        //    - Запустить Before методы
        //    - Запустить тест
        //    - Запустить After методы
        // 4. Запустить AfterClass методы

        return results;
    }
}