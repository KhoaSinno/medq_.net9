# **Day 3 – Options pattern**

**A) Options pattern** và **B) `AppOptions` (paging size, cache TTL)**.

---

# 1) Tạo model cấu hình `AppOptions`

Trong **`Program.cs`** (tạm để chung file cho nhanh), thêm cuối file:

```csharp
public sealed class AppOptions
{
    public int DefaultPageSize { get; set; } = 20;   // default nếu thiếu config
    public int CacheTtlSeconds { get; set; } = 60;   // default nếu thiếu config
}
```

> Có thể tách ra `Options/AppOptions.cs`.

---

## 2) Khai báo cấu hình trong `appsettings.json`

Mở file **`appsettings.json`** (ở cùng cấp `Medq.Api.csproj`) và thêm:

```json
{
  "App": { "DefaultPageSize": 10, "CacheTtlSeconds": 120 }
}
```

> Nếu đã có nội dung khác, chỉ cần **thêm** block `"App"` vào.

---

## 3) Đăng ký Options vào DI

Trong **`Program.cs`**, ngay **sau** `builder.Services.AddOpenApi();` thêm dòng **bind**:

```csharp
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("App"));
```

> Dòng này nói với .NET: “Lấy section `App` trong cấu hình, bind vào class `AppOptions` để dùng kiểu mạnh (strongly-typed).”

---

## 4) Tạo endpoint đọc cấu hình

Vẫn trong **`Program.cs`**, bạn đã có group `var api = app.MapGroup("/api/v1");` ở Day 2.
Nếu **chưa có**, thêm trước các endpoint:

```csharp
var api = app.MapGroup("/api/v1");
```

Thêm endpoint:

```csharp
using Microsoft.Extensions.Options;

api.MapGet("/app-settings", (IOptions<AppOptions> opt) =>
{
    var v = opt.Value; // lấy giá trị hiện tại
    return Results.Ok(new
    {
        pageSize = v.DefaultPageSize,
        cacheTtl = v.CacheTtlSeconds
    });
})
.WithTags("Config")
.WithOpenApi();
```

> `IOptions<T>`: lấy giá trị tại thời điểm startup.
> (Sau này nếu bạn muốn **hot-reload** khi file đổi, dùng `IOptionsMonitor<T>`.)

---

## 5) Chạy & test

```bash
dotnet watch
```

* Mở: `https://localhost:xxxx/api/v1/app-settings`
  → Kỳ vọng:

  ```json
  { "pageSize": 10, "cacheTtl": 120 }
  ```

* Bạn cũng có thể xem trên UI `/scalar` (nếu đã bật ở Dev).

---

## (Tuỳ chọn) Thêm validation cho Options

Nếu muốn ràng buộc hợp lệ ngay khi app khởi động:

```csharp
using System.ComponentModel.DataAnnotations;

public sealed class AppOptions
{
    [Range(1, 500)]
    public int DefaultPageSize { get; set; } = 20;

    [Range(5, 86400)]
    public int CacheTtlSeconds { get; set; } = 60;
}
```

Và thay đăng ký thành:

```csharp
builder.Services
    .AddOptions<AppOptions>()
    .Bind(builder.Configuration.GetSection("App"))
    .ValidateDataAnnotations()
    .Validate(o => o.DefaultPageSize % 5 == 0, "PageSize must be multiple of 5")
    .ValidateOnStart(); // báo lỗi ngay khi khởi động nếu cấu hình sai
```

---

## (Tuỳ chọn) Override theo môi trường

Trong `appsettings.Development.json`:

```json
{
  "App": { "DefaultPageSize": 5, "CacheTtlSeconds": 30 }
}
```

→ Khi chạy Dev, giá trị sẽ **ghi đè** cấu hình mặc định.

---

### Tóm tắt Day 3

1. Tạo `AppOptions`
2. Khai báo section `"App"` trong `appsettings.json`
3. `builder.Services.Configure<AppOptions>(...)`
4. Endpoint dùng `IOptions<AppOptions>` để trả config

Nếu bạn dán **`Program.cs` hiện tại** của bạn, mình sẽ chèn đúng các dòng cần thiết để bạn copy một phát chạy luôn.
