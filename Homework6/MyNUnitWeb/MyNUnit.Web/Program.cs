// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using MyNUnit.Web.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddDbContext<TestDbContext>(options =>
    options.UseSqlite("Data Source=mynunit.db"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");
app.UseHttpsRedirection();

var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
}

var uploadsPath = Path.Combine(wwwrootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
        Console.WriteLine("Database created successfully");

        var tables = dbContext.Database.SqlQueryRaw<string>(
            "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'").ToList();

        Console.WriteLine($"Tables created: {string.Join(", ", tables)}");

        if (tables.Count == 0)
        {
            Console.WriteLine("Creating tables manually...");

            dbContext.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS TestRuns (
                    Id TEXT PRIMARY KEY,
                    RunTime TEXT NOT NULL,
                    TotalDurationMs INTEGER NOT NULL
                )");

            dbContext.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS TestResults (
                    Id TEXT PRIMARY KEY,
                    TestRunId TEXT NOT NULL,
                    ClassName TEXT NOT NULL,
                    MethodName TEXT NOT NULL,
                    Status INTEGER NOT NULL,
                    Duration TEXT NOT NULL,
                    ErrorMessage TEXT,
                    IgnoreReason TEXT,
                    FOREIGN KEY (TestRunId) REFERENCES TestRuns(Id)
                )");

            dbContext.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS UploadedAssemblies (
                    Id TEXT PRIMARY KEY,
                    FileName TEXT NOT NULL,
                    FilePath TEXT NOT NULL,
                    UploadTime TEXT NOT NULL,
                    TestRunId TEXT,
                    FOREIGN KEY (TestRunId) REFERENCES TestRuns(Id)
                )");

            Console.WriteLine("Tables created manually");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating database: {ex.Message}");
        Console.WriteLine($"StackTrace: {ex.StackTrace}");
    }
}

app.Run();