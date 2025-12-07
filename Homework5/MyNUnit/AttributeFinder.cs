// <copyright file="AttributeFinder.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit;

using System.Reflection;
using MyNUnit.Attributes;

/// <summary>
/// class with methods to find methods with attributes.
/// </summary>
public class AttributeFinder
{
    /// <summary>
    /// to get methods with attribute "Test".
    /// </summary>
    /// <param name="type">type of current class with tests.</param>
    /// <returns>list with test methods.</returns>
    public static List<MethodInfo> GetTestMethods(Type type)
        => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttribute<TestAttribute>() != null)
            .ToList();

    /// <summary>
    /// to get methods with attribute "BeforeClass".
    /// </summary>
    /// <param name="type">type of current class with tests.</param>
    /// <returns>list with methods with attribute "BeforeClass".</returns>
    public static List<MethodInfo> GetBeforeClassMethods(Type type)
        => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttribute<BeforeClassAttribute>() != null)
            .ToList();

    /// <summary>
    /// to get methods with attribute "AfterClass".
    /// </summary>
    /// <param name="type">type of current class with tests.</param>
    /// <returns>list with methods with attribute "AfterClass".</returns>
    public static List<MethodInfo> GetAfterClassMethods(Type type)
        => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttribute<AfterClassAttribute>() != null)
            .ToList();

    /// <summary>
    /// to get methods with attribute "Before".
    /// </summary>
    /// <param name="type">type of current class with tests.</param>
    /// <returns>list with methods with attribute "Before".</returns>
    public static List<MethodInfo> GetBeforeMethods(Type type)
        => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttribute<BeforeAttribute>() != null)
            .ToList();

    /// <summary>
    /// to get methods with attribute "After".
    /// </summary>
    /// <param name="type">type of current class with tests.</param>
    /// <returns>list with methods with attribute "After".</returns>
    public static List<MethodInfo> GetAfterMethods(Type type)
        => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttribute<AfterAttribute>() != null)
            .ToList();

    /// <summary>
    /// to verify that classes with attributes "BeforeClass" and "AfterClasses" are static.
    /// </summary>
    /// <param name="type">type of current class with tests.</param>
    public static void ValidateTestClass(Type type)
    {
        var beforeClassMethods = GetBeforeClassMethods(type);
        foreach (var method in beforeClassMethods)
        {
            if (!method.IsStatic)
            {
                throw new InvalidOperationException(
                    $"Method '{method.Name}' with [BeforeClass] must be static");
            }
        }

        var afterClassMethods = GetAfterClassMethods(type);
        foreach (var method in afterClassMethods)
        {
            if (!method.IsStatic)
            {
                throw new InvalidOperationException(
                    $"Method '{method.Name}' with [AfterClass] must be static");
            }
        }
    }
}