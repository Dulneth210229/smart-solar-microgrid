/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: Program.cs
 * Description: Configures and starts the ASP.NET Core Web API.
 */

using SmartSolar.API.Services;
using SmartSolar.API.Settings;

var builder = WebApplication.CreateBuilder(args);

// Register controller support for the Web API.
builder.Services.AddControllers();

// Register API endpoint discovery required by Swagger.
builder.Services.AddEndpointsApiExplorer();

// Register Swagger documentation generation.
builder.Services.AddSwaggerGen();

// Load MongoDB settings from appsettings.json.
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings")
);

// Register a single MongoDB service for the lifetime of the application.
builder.Services.AddSingleton<MongoDbService>();

var app = builder.Build();

// Enable Swagger only while the application is running in development mode.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirect HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Enable authorization middleware.
app.UseAuthorization();

// Map controller routes such as /api/test.
app.MapControllers();

// Start the Web API.
app.Run();