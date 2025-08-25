using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

// Mock data

List<Clinic> clinics = new List<Clinic>
{
    new(1, "Clinic A", "123 Lê Lợi"),
    new(2, "Clinic B", "456 Trần Hưng Đạo")
};

var pharmacies = new List<Pharmacy>
{
    new(1,"Pharmacy A","789 Hai Bà Trưng", true),
    new(2,"Pharmacy B","1011 Nguyễn Huệ", false)
};

var queues = new List<QueueItem>
{
    new(1, 1, 101, "waiting"),
    new(2, 1, 102, "serving"),
    new(3, 2, 201, "waiting")
};

var builder = WebApplication.CreateBuilder(args);

// --- REGISTER SERVICES ---
// Document API
builder.Services.AddOpenApi();
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
var app = builder.Build();
app.UseHttpsRedirection();
app.UseRateLimiter();

// Dev: Document API
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// --- GROUP API --- 
var api = app.MapGroup("/api/v1").WithTags("v1").WithOpenApi().RequireRateLimiting("fixed");

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
        cacheTtl = v.CacheTtlSeconds
    });
}).WithTags("Config")
  .WithOpenApi();

// --- Clinics ---
// GET /api/v1/clinics  -> 200
api.MapGet("/clinics", () =>
{
    return Results.Ok(clinics);
});
// GET /api/v1/clinics/{id}  -> 200 / 404
api.MapGet("/clinics/{id:int}", (int id) =>
{
    var c = clinics.FirstOrDefault(x => x.Id == id);

    return c is null ? Results.NotFound(new ProblemDetails { Title = "Clinic not found", Status = StatusCodes.Status404NotFound }) : Results.Ok(c);
}).WithTags("Get clinic by id")
  .WithOpenApi();
// POST /api/v1/clinics  -> 201 / 400 (demo 201)
api.MapPost("/clinics", (Clinic input) =>
{
    if (string.IsNullOrWhiteSpace(input.Name))
        return Results.Problem("Name is required", statusCode: 400);

    var nextId = clinics.Count == 0 ? 1 : clinics.Max(x => x.Id) + 1;
    var created = input with { Id = nextId };
    clinics.Add(created);

    return Results.Created($"/api/v1/clinics/{created.Id}", created);
})
.WithSummary("Create clinic")
.WithOpenApi();

// --- Pharmacies ---
// GET /api/v1/pharmacies  -> 200
api.MapGet("/pharmacies", () =>
{
    return Results.Ok(pharmacies);
}).WithTags("Pharmacies")
  .WithOpenApi();

// ----- Queues -----
// GET /api/v1/queues?clinicId=...  -> 200 / 400 / 404
api.MapGet("/queues", (int? clinicId) =>
{
    if (clinicId is null)
    {
        return Results.Problem("clinicId is required", statusCode: 400);
    }

    if (!clinics.Any(x => x.Id == clinicId))
    {
        return Results.Problem("Clinic not found", statusCode: 404);
    }
    var q = queues.Where(x => x.ClinicId == clinicId).ToList();
    if (q is null || q.Count == 0)
    {
        return Results.Problem("No queues found for the specified clinic", statusCode: 404);
    }
    return Results.Ok(q);

}).WithTags("List queue items of a clinic")
  .WithOpenApi();


// -- Main routes --

app.MapGet("/", () => Results.Redirect("/scalar"))
    .ExcludeFromDescription(); // Exclude from OpenAPI documentation
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));




app.Run();

// ===== Mock domain models & data =====
public record Clinic(int Id, string Name, string Address);
public record Pharmacy(int Id, string Name, string Address, bool OpenNow);
public record QueueItem(int Id, int ClinicId, int TicketNumber, string Status); // e.g. waiting, serving, done

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

// --- Option pattern ---
public sealed class AppOptions
{
    public int DefaultPageSize { get; set; } = 20;
    public int CacheTtlSeconds { get; set; } = 60;
}
