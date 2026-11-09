using Heelnet.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddLogging(builder =>
    builder.AddDebug()
        .AddConsole()
        .SetMinimumLevel(LogLevel.Information));

// Prefer environment variable for connection string if present
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnenction")
    ?? builder.Configuration.GetConnectionString("DefaultConnenction");
Console.WriteLine($"Using connection string: {connectionString}");

builder.Services.AddDbContext <HeelnetDbContext> (options =>
  options.UseSqlServer(connectionString));


var app = builder.Build();

using (var scope = app.Services.CreateScope()) {
    // Bij het opstarten zal hij de database migraties uitvoeren.
    var db = scope.ServiceProvider.GetRequiredService <HeelnetDbContext> ();
    db.Database.Migrate();
}

// Forwarded headers are required for running behind a reverse proxy
app.UseForwardedHeaders(new ForwardedHeadersOptions {
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.MapScalarApiReference();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
