using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Clock services
builder.Services.AddSingleton<IClock, SystemClock>();

var app = builder.Build();
app.UseHttpsRedirection();

// Dev: Document API
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

var api = app.MapGroup("/api/v1");

api.MapGet("/now", (IClock clock, ILogger<ClockEndpoint> logger) =>
{
    // Use the injected IClock service
    logger.LogInformation("Now endpoint called at {Now}", clock.UtcNow);
    return Results.Ok(new { utcNow = clock.UtcNow });
})
.WithTags("Time")
.WithOpenApi();

// -- Main routes --

app.MapGet("/", () => Results.Redirect("/scalar"))
    .ExcludeFromDescription(); // Exclude from OpenAPI documentation
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();


// --- Interface ---
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

sealed class ClockEndpoint { }
