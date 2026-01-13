// <copyright file="AfterAttribute.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit.Attributes;

/// <summary>
/// attribute for methods which should be run after each test.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AfterAttribute : Attribute
{
}