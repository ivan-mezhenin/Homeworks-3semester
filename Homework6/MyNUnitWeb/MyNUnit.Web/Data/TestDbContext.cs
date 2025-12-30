// <copyright file="TestDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MyNUnit.Web.Data;

using Microsoft.EntityFrameworkCore;
using MyNUnit.Core.Models;

/// <summary>
/// Database context for MyNUnit test results.
/// </summary>
public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the collection of test runs in the database.
    /// </summary>
    public DbSet<TestRun> TestRuns => this.Set<TestRun>();

    /// <summary>
    /// Gets the collection of individual test results in the database.
    /// </summary>
    public DbSet<TestResult> TestResults => this.Set<TestResult>();

    /// <summary>
    /// Gets the collection of uploaded assemblies in the database.
    /// </summary>
    public DbSet<UploadedAssembly> UploadedAssemblies => this.Set<UploadedAssembly>();

    /// <summary>
    /// Configures the model relationships and constraints.
    /// </summary>
    /// <param name="modelBuilder">Model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestRun>()
            .HasMany(tr => tr.TestResults)
            .WithOne()
            .HasForeignKey(tr => tr.TestRunId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TestRun>()
            .HasMany(tr => tr.Assemblies)
            .WithOne(a => a.TestRun)
            .HasForeignKey(a => a.TestRunId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ClassName).IsRequired();
            entity.Property(e => e.MethodName).IsRequired();
            entity.Property(e => e.Duration).HasConversion(
                v => v.ToString(),
                v => TimeSpan.Parse(v));
        });

        modelBuilder.Entity<TestRun>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.RunTime).IsRequired();
        });

        modelBuilder.Entity<UploadedAssembly>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.FileName).IsRequired();
            entity.Property(e => e.FilePath).IsRequired();
            entity.Property(e => e.UploadTime).IsRequired();
            entity.Property(e => e.TestRunId).IsRequired(false);
        });
    }
}