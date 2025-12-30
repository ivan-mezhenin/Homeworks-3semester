// <copyright file="Program.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using MyNUnit.Web.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowReactApp",
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

var uploadsPath = Path.Combine("wwwroot", "uploads");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

    try
    {
        var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
        if (pendingMigrations.Any())
        {
            Console.WriteLine($"Applying {pendingMigrations.Count} pending migrations...");
            dbContext.Database.Migrate();
            Console.WriteLine("Migrations applied successfully");
        }
        else
        {
            Console.WriteLine("No pending migrations");

            var tableExists = dbContext.Database.ExecuteSqlRaw(
                "SELECT 1 FROM TestRuns LIMIT 1") != -1;

            if (!tableExists)
            {
                Console.WriteLine("Database is empty, creating...");
                dbContext.Database.EnsureCreated();
            }
        }

        Console.WriteLine("Database ready");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database error: {ex.Message}");
        dbContext.Database.EnsureCreated();
        Console.WriteLine("Database created using EnsureCreated");
    }
}

app.Run();