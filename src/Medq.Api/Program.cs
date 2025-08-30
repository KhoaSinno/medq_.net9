using System.Threading.RateLimiting;
using Medq.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

static ProblemDetails P(string title, int status) => new() { Title = title, Status = status };

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
var connSqlite = builder.Configuration.GetConnectionString("sqlite");

builder.Services.AddDbContext<MedqDbContext>(opt =>
    opt.UseSqlite(connSqlite, x => x.MigrationsAssembly("Medq.Infrastructure")));

// ProblemDetails 
builder.Services.AddProblemDetails();


var app = builder.Build();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseExceptionHandler();


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
api.MapGet("/clinics", (int? page, int? pageSize, IOptions<AppOptions> opt) =>
{
    var cfg = opt.Value;
    int size = pageSize ?? cfg.DefaultPageSize;
    int p = Math.Max(page ?? 1, 1);
    if (size < 1 || size > 500)
        return Results.BadRequest(P("PageSize must be between 1 and 500", 400));
    int skipItems = (p - 1) * size;

    var items = clinics.Skip(skipItems).Take(size).ToList();
    return Results.Ok(new { page = p, pageSize = size, total = clinics.Count, items });
}).WithName("ListClinics") // operationId trên UI
.WithTags("Clinics")
.WithSummary("List clinics with paging")
.WithDescription("Returns paginated clinics. Defaults come from configuration (AppOptions.DefaultPageSize).")
.Produces<IEnumerable<Clinic>>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status400BadRequest)
.WithOpenApi();

// GET /api/v1/clinics/{id}  -> 200 / 404
api.MapGet("/clinics/{id:int}", (int id) =>
{
    var c = clinics.FirstOrDefault(x => x.Id == id);

    return c is null ? Results.NotFound(P("Clinic not found", 404)) : Results.Ok(c);
}).WithName("GetClinicById")
.WithTags("Clinics")
.WithSummary("Get clinic by id")
.Produces<Clinic>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status404NotFound)
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
.WithName("CreateClinic")
.WithTags("Clinics")
.WithSummary("Create clinic")
.WithDescription("Creates a clinic and returns 201 with Location header.")
.Accepts<Clinic>("application/json")
.Produces<Clinic>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status400BadRequest)
.WithOpenApi();

// --- Pharmacies ---
// GET /api/v1/pharmacies  -> 200
api.MapGet("/pharmacies", (int? page, int? pageSize, IOptions<AppOptions> opt) =>
{
    var cfg = opt.Value;
    int size = pageSize ?? cfg.DefaultPageSize;
    int p = Math.Max(page ?? 1, 1);
    int skipItems = (p - 1) * size;
    var items = pharmacies.Skip(skipItems).Take(size).ToList();
    return Results.Ok(new { page = p, pageSize = size, total = pharmacies.Count, items });
}).WithTags("Pharmacies")
   .WithName("ListPharmacies")
   .WithSummary("List all pharmacies")
   .Produces<IEnumerable<Pharmacy>>(StatusCodes.Status200OK)
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

}).WithName("ListQueuesByClinic")
.WithTags("Queues")
.WithSummary("List queue items by clinic")
.WithDescription("400 if clinicId missing; 404 if clinic not found.")
.Produces<IEnumerable<QueueItem>>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status404NotFound)
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
    public int MaxPageSize { get; set; } = 100;
    public int CacheTtlSeconds { get; set; } = 60;
}
