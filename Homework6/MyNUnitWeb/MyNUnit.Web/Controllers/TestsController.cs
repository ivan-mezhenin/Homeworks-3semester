// <copyright file="TestsController.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace MyNUnit.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyNUnit.Core;
using MyNUnit.Core.Models;
using MyNUnit.Web.Data;

/// <summary>
/// test controller.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestsController(
    TestDbContext context,
    IWebHostEnvironment environment,
    ILogger<TestsController> logger)
    : ControllerBase
{
    /// <summary>
    /// Uploads DLL files containing tests.
    /// </summary>
    /// <param name="files">Files to upload.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFiles(IFormFileCollection files)
    {
        try
        {
            if (files.Count == 0)
            {
                return this.BadRequest(new { error = "No files uploaded" });
            }

            var uploadsPath = Path.Combine(environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var uploadedAssemblies = new List<UploadedAssembly>();

            foreach (var file in files)
            {
                if (!Path.GetExtension(file.FileName).Equals(".dll", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsPath, fileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                uploadedAssemblies.Add(new UploadedAssembly
                {
                    FileName = file.FileName,
                    FilePath = filePath,
                    UploadTime = DateTime.UtcNow,
                });
            }

            await context.UploadedAssemblies.AddRangeAsync(uploadedAssemblies);
            await context.SaveChangesAsync();

            return this.Ok(new
            {
                message = $"Uploaded {uploadedAssemblies.Count} file(s)",
                files = uploadedAssemblies.Select(a => new
                {
                    a.Id,
                    a.FileName,
                }),
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading files");
            return this.StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Runs tests from uploaded assemblies.
    /// </summary>
    /// <param name="request">request to db.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpPost("run")]
    public async Task<IActionResult> RunTests([FromBody] RunTestsRequest request)
    {
        try
        {
            if (request.AssemblyIds == null || request.AssemblyIds.Count == 0)
            {
                var allAssemblies = await context.UploadedAssemblies.ToListAsync();
                request = new RunTestsRequest
                {
                    AssemblyIds = allAssemblies.Select(a => a.Id).ToList(),
                };
            }

            var assemblies = await context.UploadedAssemblies
                .Where(a => request.AssemblyIds.Contains(a.Id))
                .ToListAsync();

            if (assemblies.Count == 0)
            {
                return this.BadRequest(new { error = "No assemblies found" });
            }

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                foreach (var assembly in assemblies)
                {
                    var destPath = Path.Combine(tempDir, Path.GetFileName(assembly.FilePath));
                    System.IO.File.Copy(assembly.FilePath, destPath, true);
                }

                var runner = new TestRunner(tempDir);
                var results = runner.RunTests();

                var testRun = new TestRun
                {
                    Id = Guid.NewGuid(),
                    RunTime = DateTime.UtcNow,
                    TotalDurationMs = (long)results.Sum(r => r.Duration.TotalMilliseconds),
                    TestResults = results,
                };

                foreach (var result in results)
                {
                    result.TestRunId = testRun.Id;
                }

                foreach (var assembly in assemblies)
                {
                    assembly.TestRunId = testRun.Id;
                }

                await context.TestRuns.AddAsync(testRun);

                testRun.Assemblies = assemblies;

                await context.SaveChangesAsync();

                return this.Ok(new
                {
                    runId = testRun.Id,
                    message = $"Tests completed: {results.Count} tests executed",
                    summary = new
                    {
                        total = testRun.TotalTests,
                        passed = testRun.Passed,
                        failed = testRun.Failed,
                        ignored = testRun.Ignored,
                        errors = testRun.Error,
                        duration = testRun.TotalDurationMs,
                    },
                });
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to cleanup temp directory");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running tests");
            return this.StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all test runs.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet("runs")]
    public async Task<IActionResult> GetAllTestRuns()
    {
        var runs = await context.TestRuns
            .Include(r => r.Assemblies)
            .Include(r => r.TestResults)
            .OrderByDescending(r => r.RunTime)
            .Select(r => new
            {
                r.Id,
                r.RunTime,
                r.TotalTests,
                r.Passed,
                r.Failed,
                r.Ignored,
                r.Error,
                r.TotalDurationMs,
                assemblies = r.Assemblies.Select(a => new { a.Id, a.FileName }),
                testResults = r.TestResults.Select(tr => new
                {
                    tr.Id,
                    tr.ClassName,
                    tr.MethodName,
                    tr.Status,
                    tr.Duration,
                    tr.ErrorMessage,
                    tr.IgnoreReason,
                }),
            })
            .ToListAsync();

        return this.Ok(runs);
    }

    /// <summary>
    /// Gets a specific test run with detailed results.
    /// </summary>
    /// <param name="id">id of test run.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet("runs/{id}")]
    public async Task<IActionResult> GetTestRun(Guid id)
    {
        var run = await context.TestRuns
            .Include(r => r.TestResults)
            .Include(r => r.Assemblies)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (run == null)
        {
            return this.NotFound(new { error = "Test run not found" });
        }

        return this.Ok(new
        {
            run.Id,
            run.RunTime,
            run.TotalTests,
            run.Passed,
            run.Failed,
            run.Ignored,
            run.Error,
            run.TotalDurationMs,
            assemblies = run.Assemblies.Select(a => new { a.Id, a.FileName }),
            testResults = run.TestResults.Select(tr => new
            {
                tr.ClassName,
                tr.MethodName,
                tr.Status,
                tr.Duration,
                tr.ErrorMessage,
                tr.IgnoreReason,
            }),
        });
    }

    /// <summary>
    /// Gets uploaded assemblies.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [HttpGet("assemblies")]
    public async Task<IActionResult> GetAssemblies()
    {
        var assemblies = await context.UploadedAssemblies
            .OrderByDescending(a => a.UploadTime)
            .Select(a => new
            {
                a.Id,
                a.FileName,
                a.UploadTime,
                a.TestRunId,
            })
            .ToListAsync();

        return this.Ok(assemblies);
    }
}