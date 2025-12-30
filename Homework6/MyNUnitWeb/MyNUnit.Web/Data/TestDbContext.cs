// <copyright file="TestDbContext.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit.Web.Data;

using Microsoft.EntityFrameworkCore;
using MyNUnit.Core.Models;

/// <summary>
/// Database context for the MyNUnit testing framework.
/// Manages the database connection and entity configurations for test runs,
/// test results, and uploaded assemblies.
/// </summary>
public class TestDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the DbSet for TestRun entities.
    /// Represents a collection of all test run executions in the database.
    /// </summary>
    public DbSet<TestRun> TestRuns => this.Set<TestRun>();

    /// <summary>
    /// Gets the DbSet for TestResult entities.
    /// Represents a collection of all individual test results in the database.
    /// </summary>
    public DbSet<TestResult> TestResults => this.Set<TestResult>();

    /// <summary>
    /// Gets the DbSet for UploadedAssembly entities.
    /// Represents a collection of all uploaded DLL assemblies in the database.
    /// </summary>
    public DbSet<UploadedAssembly> UploadedAssemblies => this.Set<UploadedAssembly>();

    /// <summary>
    /// Configures the database model and relationships between entities.
    /// This method is called when the model for a derived context is being created.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestRun>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RunTime).IsRequired();
            entity.Property(e => e.TotalDurationMs).IsRequired();

            entity.HasMany(e => e.TestResults)
                  .WithOne()
                  .HasForeignKey(e => e.TestRunId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Assemblies)
                  .WithOne(e => e.TestRun)
                  .HasForeignKey(e => e.TestRunId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClassName).IsRequired();
            entity.Property(e => e.MethodName).IsRequired();
            entity.Property(e => e.Status).IsRequired();

            entity.Property(e => e.Duration).IsRequired()
                  .HasConversion(
                      v => v.ToString("c"),
                      v => TimeSpan.Parse(v));
        });

        modelBuilder.Entity<UploadedAssembly>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired();
            entity.Property(e => e.FilePath).IsRequired();
            entity.Property(e => e.UploadTime).IsRequired();
            entity.Property(e => e.TestRunId).IsRequired(false);
        });
    }

    /// <summary>
    /// Configures the database provider and connection string.
    /// This method is called to configure the DbContext before it is used.
    /// </summary>
    /// <param name="optionsBuilder">A builder used to create or modify options for this context.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=mynunit.db");
        }
    }
}