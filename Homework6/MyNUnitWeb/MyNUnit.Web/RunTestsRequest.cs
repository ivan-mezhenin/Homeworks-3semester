// <copyright file="RunTestsRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MyNUnit.Web;

/// <summary>
/// Request model for running tests.
/// </summary>
public class RunTestsRequest
{
    /// <summary>
    /// Gets or sets the assembly IDs to run tests from.
    /// If empty, runs tests from all uploaded assemblies.
    /// </summary>
    public List<Guid>? AssemblyIds { get; set; }
}