// <copyright file="UploadedAssembly.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit.Core.Models;

/// <summary>
/// Represents an uploaded assembly file that contains test classes.
/// This entity tracks DLL files uploaded by users for testing.
/// </summary>
public class UploadedAssembly
{
    /// <summary>
    /// Gets or sets the unique identifier for the uploaded assembly.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the original file name of the uploaded assembly.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full path where the assembly is stored on the server.
    /// This is the physical location of the uploaded file.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the assembly was uploaded.
    /// Stored in UTC format.
    /// </summary>
    public DateTime UploadTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the identifier of the test run that used this assembly.
    /// Null if the assembly hasn't been used in a test run yet.
    /// </summary>
    public Guid? TestRunId { get; set; } // Изменено на nullable

    /// <summary>
    /// Gets or sets the navigation property to the associated test run.
    /// This is the test run where this assembly was executed.
    /// </summary>
    public TestRun? TestRun { get; set; }
}