using System.Threading.RateLimiting;
using Medq.Api.Features.Clinics;
using Medq.Api.Features.Pharmacies;
using Medq.Api.Options;
using Medq.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// --- REGISTER SERVICES ---
// Document API
builder.Services.AddOpenApi(opt =>
{
    opt.AddDocumentTransformer(
        (doc, context, ct) =>
        {
            doc.Info = new()
            {
                Title = "Sinoo: MedQ API",
                Version = "v1",
                Description = "API for MedQ application"
            };
            return Task.CompletedTask;
        }
    );
});
// Clock services
builder.Services.AddSingleton<IClock, SystemClock>();
// Options
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("App"));
// Rate limited
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 60;                  // Cho phép 60 request
        opt.Window = TimeSpan.FromMinutes(1);  // Trong 1 phút
        opt.QueueLimit = 0;                    // Không xếp hàng, quá thì chặn luôn
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;

    });
});
// SQLite
var provider = builder.Configuration["Database:Provider"] ?? "sqlite";
var connSqlite = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<MedqDbContext>(opt =>
    opt.UseSqlite(connSqlite, x => x.MigrationsAssembly("Medq.Infrastructure")));

// ProblemDetails 
builder.Services.AddProblemDetails();


var app = builder.Build();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseExceptionHandler();   // để ProblemDetails trả RFC7807 cho 5xx
app.UseStatusCodePages();    // dev-friendly cho 404 text
//// Use map API endpoint
app.MapClinicsEndpoint();
app.MapPharmaciesEndpoint();
// Dev: Document API
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


// // --- GROUP API --- 
var api = app.MapGroup("/api/v1").WithTags("v1").WithOpenApi().RequireRateLimiting("fixed");

// Register DI manual
api.MapGet("/now", (IClock clock, ILogger<ClockEndpoint> logger) =>
{
    // Use the injected IClock service
    logger.LogInformation("Now endpoint called at {Now}", clock.UtcNow);
    return Results.Ok(new { utcNow = clock.UtcNow });
})
.WithTags("Time")
.WithOpenApi();

api.MapGet("/app-settings", (IOptions<AppOptions> opt) =>
{
    var v = opt.Value;

    return Results.Ok(new
    {
        pageSize = v.DefaultPageSize,
        maxPageSize = v.MaxPageSize
    });
}).WithTags("Config")
  .WithOpenApi();

// -- Main routes --
app.MapGet("/", () => Results.Redirect("/scalar"))
    .ExcludeFromDescription();

app.Run();

// ===== Mock domain models & data =====

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
