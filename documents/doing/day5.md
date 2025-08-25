# **Day 5** D5 — OpenAPI “đẹp” cho dev FE (45’)

---

# Mục tiêu Day 5

* Bật **built‑in OpenAPI** (.NET 9): `AddOpenApi()` + `MapOpenApi()` → máy phát JSON `/openapi/v1.json`.
* Thêm **UI** xem tài liệu:

  * **Scalar UI** (nhẹ, hiện đại): `Scalar.AspNetCore` → `/scalar`
  * (Tùy chọn) **Swagger UI** (nếu bạn/thầy quen): `Swashbuckle.AspNetCore` → `/swagger`
* Gắn metadata cho từng endpoint: `.WithOpenApi()`, `.WithTags()`, `.WithSummary()`, `.WithDescription()`, `.Produces<>()`.

---

# 1) Cài gói (nếu chưa có)

```bash
dotnet add package Scalar.AspNetCore
# (tuỳ chọn) nếu muốn Swagger UI
# dotnet add package Swashbuckle.AspNetCore
```

---

# 2) Program.cs – khối DI và middleware (copy‑paste)

> Hợp nhất với code Day 3/4 của bạn. Nếu đã có phần nào thì giữ nguyên, chỉ bổ sung chỗ thiếu.

```csharp
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore; // nếu dùng Scalar

var builder = WebApplication.CreateBuilder(args);

// === Built-in OpenAPI ===
builder.Services.AddOpenApi(); // máy phát JSON /openapi/v1.json

// (tuỳ chọn) Thêm info/title/version cho tài liệu
builder.Services.AddOpenApi(opt =>
{
    opt.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info = new()
        {
            Title = "MEDQ API",
            Version = "v1",
            Description = "API cho hệ thống MEDQ (clinics, pharmacies, queues)."
        };
        return Task.CompletedTask;
    });
});

// === Day 3: Options pattern ===
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("App"));

// === Day 4: Rate limit (trả 429) ===
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 60;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.OnRejected = (ctx, token) =>
        {
            ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            ctx.HttpContext.Response.Headers["Retry-After"] = ((int)opt.Window.TotalSeconds).ToString();
            return ValueTask.CompletedTask;
        };
    });
});

// (tuỳ chọn) Swagger UI nếu thích
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

var app = builder.Build();

if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseHttpsRedirection();
app.UseRateLimiter();

// === Phát JSON OpenAPI ===
app.MapOpenApi(); // -> /openapi/v1.json

// === Scalar UI ===
app.MapScalarApiReference(options =>
{
    options.Title = "MEDQ API";
    // options.Theme = ScalarTheme.Mars; // tuỳ chọn, giữ mặc định cũng đẹp
});
// => UI ở /scalar

// (tuỳ chọn) Swagger UI cổ điển
// app.UseSwagger();
// app.UseSwaggerUI(c =>
// {
//     c.SwaggerEndpoint("/openapi/v1.json", "MEDQ v1");
//     c.RoutePrefix = "swagger"; // /swagger
// });

// ==== Group version v1 ====
var api = app.MapGroup("/api/v1")
             .WithTags("v1")
             .RequireRateLimiting("fixed");

// ===== Mock data (Day 4) =====
public record Clinic(int Id, string Name, string Address);
public record Pharmacy(int Id, string Name, string Address, bool OpenNow);
public record QueueItem(int Id, int ClinicId, int TicketNumber, string Status);

var clinics = new List<Clinic>
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

static ProblemDetails P(string title, int status) => new() { Title = title, Status = status };

// ===== Endpoints có metadata OpenAPI “đẹp” =====

// Clinics (GET list có paging từ AppOptions)
api.MapGet("/clinics", (int? page, int? pageSize, IOptions<AppOptions> opt) =>
{
    var cfg = opt.Value;
    int size = pageSize ?? cfg.DefaultPageSize;
    if (size < 1 || size > 500) return Results.BadRequest(P("Invalid pageSize", 400));

    int p = Math.Max(page ?? 1, 1);
    int skip = (p - 1) * size;

    var items = clinics.Skip(skip).Take(size).ToList();
    return Results.Ok(new { page = p, pageSize = size, total = clinics.Count, items });
})
.WithName("ListClinics") // operationId trên UI
.WithTags("Clinics")
.WithSummary("List clinics with paging")
.WithDescription("Returns paginated clinics. Defaults come from configuration (AppOptions.DefaultPageSize).")
.Produces(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status400BadRequest)
.WithOpenApi();

// Clinics (GET by id)
api.MapGet("/clinics/{id:int}", (int id) =>
{
    var c = clinics.FirstOrDefault(x => x.Id == id);
    return c is null ? Results.NotFound(P("Clinic not found", 404)) : Results.Ok(c);
})
.WithName("GetClinicById")
.WithTags("Clinics")
.WithSummary("Get clinic by id")
.Produces<Clinic>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status404NotFound)
.WithOpenApi();

// Clinics (POST create)
api.MapPost("/clinics", (Clinic input) =>
{
    if (string.IsNullOrWhiteSpace(input.Name))
        return Results.BadRequest(P("Name is required", 400));

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

// Pharmacies (GET)
api.MapGet("/pharmacies", () => Results.Ok(pharmacies))
   .WithName("ListPharmacies")
   .WithTags("Pharmacies")
   .WithSummary("List all pharmacies")
   .Produces<IEnumerable<Pharmacy>>(StatusCodes.Status200OK)
   .WithOpenApi();

// Queues (GET by clinicId)
api.MapGet("/queues", (int? clinicId) =>
{
    if (clinicId is null) return Results.BadRequest(P("Missing clinicId", 400));
    if (!clinics.Any(c => c.Id == clinicId)) return Results.NotFound(P("Clinic not found", 404));
    var items = queues.Where(q => q.ClinicId == clinicId).ToList();
    return Results.Ok(items);
})
.WithName("ListQueuesByClinic")
.WithTags("Queues")
.WithSummary("List queue items by clinic")
.WithDescription("400 if clinicId missing; 404 if clinic not found.")
.Produces<IEnumerable<QueueItem>>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status404NotFound)
.WithOpenApi();

app.Run();

public sealed class AppOptions
{
    public int DefaultPageSize { get; set; } = 20;
    public int CacheTtlSeconds { get; set; } = 60;
}
```

---

# 3) Chạy và xem

```bash
dotnet watch run --launch-profile https
```

* JSON OpenAPI: `https://localhost:7262/openapi/v1.json`
* **Scalar UI**: `https://localhost:7262/scalar`
* (Nếu bật Swagger UI) `https://localhost:7262/swagger`

> Nếu HTTPS không vào được, nhớ `dotnet dev-certs https --trust` và dùng đúng port từ log “Now listening on”.

---

# 4) Mẹo làm tài liệu “đẹp” hơn (5 phút là xịn)

* **Tags** rõ ràng theo domain: `"Clinics"`, `"Pharmacies"`, `"Queues"`.
* **OperationId** (`.WithName("...")`) giúp FE sinh SDK/clients chuẩn.
* **Summary/Description** ngắn gọn nhưng chỉ đúng case 400/404/429.
* **Produces / ProducesProblem** để UI hiện status & schema rõ ràng.
* **Examples** (tuỳ chọn nâng cao): bạn có thể thêm ví dụ mẫu bằng document/operation transformer khi cần.

---

# 5) Checklist Day 5 ✅

* [x] `AddOpenApi()` + `MapOpenApi()` → `/openapi/v1.json`.
* [x] UI: `MapScalarApiReference()` → `/scalar` (hoặc Swagger UI nếu thích).
* [x] Mọi endpoint đều có `.WithOpenApi()` + `.WithTags(...)` + Summary/Description.
* [x] Khai báo `Produces/ProducesProblem` cho status chính (200/201/400/404/429).
* [x] Dùng `AppOptions.DefaultPageSize` để FE thấy behavior từ config.

Bạn cứ dán đoạn trên là có ngay OpenAPI “đẹp” cho Day 5. Nếu muốn, mình làm tiếp **Day 6**: tách endpoint theo file/extension + chuẩn hoá response wrapper (envelope) và thêm **OpenAPI examples** cho từng route.
