# **Day 4**  **thiết kế route REST + versioning**

D4: Thiết kế route chuẩn REST (resources/verbs/status codes)
---

# Mục tiêu

* Prefix version: **`/api/v1`** (URL versioning đơn giản, rõ ràng).
* RESTful resources: **clinics, pharmacies, queues**.
* Trả mã đúng chuẩn: **200/201/400/404/429**.
* Có **rate limiting** để ra **429** khi cần.
* Mô tả đẹp trên OpenAPI UI (Scalar).

> Bạn đã có `/api/v1` group ở Day 2/3; nếu chưa có thì thêm ngay.

---

## 1) Route group theo version (nếu chưa có)

Trong `Program.cs` (sau khi `var app = builder.Build();`):

```csharp
var api = app.MapGroup("/api/v1")
             .WithTags("v1")
             .WithOpenApi();
```

---

## 2) Mock dữ liệu domain (đặt tạm trong Program.cs để Day 4 test)

Thêm trước phần `app.Run();`:

```csharp
// ===== Mock domain models & data =====
public record Clinic(int Id, string Name, string Address);
public record Pharmacy(int Id, string Name, string Address, bool OpenNow);
public record QueueItem(int Id, int ClinicId, int TicketNumber, string Status); // e.g. waiting, serving, done

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
```

---

## 3) Endpoints REST (GET + ví dụ POST để thấy 201)

Dán ngay **sau** `var api = app.MapGroup("/api/v1")...`:

```csharp
// ----- Clinics -----

// GET /api/v1/clinics  -> 200
api.MapGet("/clinics", () => Results.Ok(clinics))
   .WithSummary("List all clinics")
   .WithOpenApi();

// GET /api/v1/clinics/{id}  -> 200 / 404
api.MapGet("/clinics/{id:int}", (int id) =>
{
    var c = clinics.FirstOrDefault(x => x.Id == id);
    return c is null
        ? Results.NotFound(ProblemDetailsFactory("Clinic not found", 404))
        : Results.Ok(c);
})
.WithSummary("Get clinic by id")
.WithOpenApi();

// POST /api/v1/clinics  -> 201 / 400 (demo 201)
api.MapPost("/clinics", (Clinic input) =>
{
    if (string.IsNullOrWhiteSpace(input.Name))
        return Results.BadRequest(ProblemDetailsFactory("Name is required", 400));

    var nextId = clinics.Count == 0 ? 1 : clinics.Max(x => x.Id) + 1;
    var created = input with { Id = nextId };
    clinics.Add(created);

    return Results.Created($"/api/v1/clinics/{created.Id}", created);
})
.WithSummary("Create clinic")
.WithOpenApi();


// ----- Pharmacies -----

// GET /api/v1/pharmacies  -> 200
api.MapGet("/pharmacies", () => Results.Ok(pharmacies))
   .WithSummary("List all pharmacies")
   .WithOpenApi();


// ----- Queues -----

// GET /api/v1/queues?clinicId=...  -> 200 / 400 / 404
api.MapGet("/queues", (int? clinicId) =>
{
    // 400: thiếu clinicId
    if (clinicId is null)
        return Results.BadRequest(ProblemDetailsFactory("Missing clinicId", 400));

    // 404: clinic không tồn tại
    if (!clinics.Any(c => c.Id == clinicId))
        return Results.NotFound(ProblemDetailsFactory("Clinic not found", 404));

    var items = queues.Where(q => q.ClinicId == clinicId).ToList();
    return Results.Ok(items);
})
.WithSummary("List queue items of a clinic")
.WithOpenApi();
```

> Ở đây bạn thấy rõ:
>
> * **200** cho GET thành công.
> * **201** cho POST tạo mới.
> * **400** khi thiếu/invalid query.
> * **404** khi resource không tồn tại.

---

## 4) Rate limiting để có **429**

Nếu **chưa** thêm ở Day 1/2, bổ sung:

**Ở services (trước `builder.Build()`):**

```csharp
using System.Threading.RateLimiting;

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 60;                  // 60 req/phút
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});
```

**Sau khi build app:**

```csharp
app.UseRateLimiter();
```

**Áp dụng cho group v1 hoặc từng endpoint:**

```csharp
api.RequireRateLimiting("fixed");
```

> Khi vượt hạn mức, middleware sẽ trả **429 Too Many Requests** tự động.

---

## 5) Problem Details helper (trả lỗi chuẩn)

Đặt trước `app.Run();`:

```csharp
static Microsoft.AspNetCore.Mvc.ProblemDetails ProblemDetailsFactory(string title, int status)
    => new()
    {
        Title = title,
        Status = status,
        Type = "about:blank"
    };
```

Bạn có thể nâng cấp sau sang RFC 7807 đầy đủ (add `detail`, `instance`…).

---

## 6) OpenAPI mô tả đẹp (dev xem hợp đồng nhanh)

Bạn đã có `.WithOpenApi()` và Scalar rồi. Có thể thêm:

```csharp
.WithDescription("Returns queue items by clinicId. 400 if missing clinicId, 404 if clinic not found.")
```

vào endpoint `/queues` để FE dễ hiểu.

---

## 7) Quy ước nhanh (để nhất quán)

* **Collection**: `GET /api/v1/{resource}` (200)
* **Single**: `GET /api/v1/{resource}/{id}` (200/404)
* **Create**: `POST /api/v1/{resource}` (201 + `Location` header)
* **Update**: `PUT /api/v1/{resource}/{id}` (200/204/404)
* **Partial**: `PATCH /api/v1/{resource}/{id}` (200/204/404)
* **Delete**: `DELETE /api/v1/{resource}/{id}` (204/404)
* **Validation** → 400; **Not found** → 404; **Rate limit** → 429.
* **Version** sau này: thêm `app.MapGroup("/api/v2")` song song (không phá v1).

---

## 8) Test nhanh

```bash
# Clinics
curl -ks https://localhost:****/api/v1/clinics | jq
curl -ks https://localhost:****/api/v1/clinics/1 | jq
curl -ks -X POST https://localhost:****/api/v1/clinics -H "Content-Type: application/json" -d '{"id":0,"name":"Clinic C","address":"999 Phan Xích Long"}' | jq

# Pharmacies
curl -ks https://localhost:****/api/v1/pharmacies | jq

# Queues
curl -ks "https://localhost:****/api/v1/queues?clinicId=1" | jq
curl -ks "https://localhost:****/api/v1/queues" | jq   # -> 400
curl -ks "https://localhost:****/api/v1/queues?clinicId=999" | jq  # -> 404
```

---

### Xong Day 4 ✅

Bạn đã có **versioning bằng URL**, **route REST rõ ràng**, **status code chuẩn**, và **rate limit** cho **429**.
Nếu muốn, mình refactor endpoints sang file riêng (extension methods) để `Program.cs` gọn hẳn, hoặc thêm **pagination** (dùng `AppOptions.DefaultPageSize`) ngay trên `/clinics` cho Day 5.
