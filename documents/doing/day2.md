# **Day 2**: **A) DI trong .NET/ASP.NET Core**, **B) Tạo `IClock` + inject `ILogger<T>`**

1. Hiểu 3 lifetime cơ bản trong DI: `Singleton`, `Scoped`, `Transient`.
2. Tạo service `IClock` (đồng hồ hệ thống) và **đăng ký vào DI**.
3. **Inject `ILogger<T>`** vào endpoint để log có cấu trúc.
4. Test nhanh: gọi `/api/v1/now` thấy log và JSON thời gian.

---

DI (Dependency Injection) là một kỹ thuật lập trình giúp tự động cung cấp các phụ thuộc (dependency) cho một class hoặc hàm thay vì tự tạo ra chúng bên trong.

DI giúp bạn nhận được đối tượng cần thiết (IClock) một cách tự động, chỉ cần khai báo trong tham số hàm, không cần tự tạo bằng new.

Trong code này, IClock là một interface dùng để lấy thời gian hiện tại. Cách Dependency Injection (DI) hoạt động với IClock như sau:

1. Đăng ký dịch vụ:

Dòng này nói với ASP.NET Core rằng:

```bash
builder.Services.AddSingleton<IClock, SystemClock>();
```

- Khi cần một đối tượng kiểu IClock, hãy cung cấp một instance của SystemClock.
- Singleton nghĩa là chỉ tạo một instance duy nhất cho toàn bộ ứng dụng.

2. Sử dụng dịch vụ:

```bash
api.MapGet("/now", (IClock clock, ILogger<ClockEndpoint> logger) => { ... })
```

Khi endpoint /api/v1/now được gọi, ASP.NET Core sẽ:

- Tự động tạo (hoặc lấy lại) instance của SystemClock đã đăng ký.
- Truyền instance này vào tham số IClock clock của hàm xử lý.

3. Kết quả:

- Dùng clock.UtcNow để lấy thời gian hiện tại mà không cần tự khởi tạo SystemClock.
- Việc này giúp code dễ kiểm thử, dễ thay đổi cách lấy thời gian (chỉ cần thay đổi đăng ký dịch vụ)

---

## Bước 1 – Thêm service `IClock`

Mở `Program.cs` và thêm phần **hỗ trợ DI** ở cuối file (nếu bạn chưa có):

```csharp
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
```

> Dùng `DateTimeOffset` để chuẩn hóa timezone.

---

## Bước 2 – Đăng ký DI + giải thích lifetime

Trong phần cấu hình services (trước `var app = builder.Build();`), thêm:

```csharp
builder.Services.AddSingleton<IClock, SystemClock>();
```

- `AddSingleton` → tạo **một** instance dùng chung suốt vòng đời app (phù hợp cho clock, config, cache client…).
- `AddScoped` → **mỗi request một** instance (thường dùng cho repo/db context).
- `AddTransient` → **mỗi lần resolve** một instance (stateless, rẻ, không giữ tài nguyên).

> Bạn có thể đổi lifetime để thử, nhưng với `IClock` thì `Singleton` là ổn.

---

## Bước 3 – Tạo endpoint `/api/v1/now` và inject `ILogger<T>`

Vẫn trong `Program.cs`, giả sử bạn đã có group:

```csharp
var api = app.MapGroup("/api/v1");
```

Thêm endpoint và **inject** cả `IClock` lẫn `ILogger<T>`:

```csharp
// Tạo type "ảo" chỉ để đặt category log cho đẹp
sealed class ClockEndpoint { }

api.MapGet("/now", (IClock clock, ILogger<ClockEndpoint> logger) =>
{
    var now = clock.UtcNow;
    logger.LogInformation("Now endpoint called at {Now}", now);
    return Results.Ok(new { now });
})
.WithTags("Time")
.WithOpenApi();
```

- `ILogger<ClockEndpoint>` giúp log có **category** dễ lọc (bạn cũng có thể dùng `ILoggerFactory` hoặc `ILogger<Program>`).
- Tham số trong lambda của Minimal API **chính là các dependency** được resolve từ DI.

---

## Bước 4 – (Tuỳ chọn) Bật OpenAPI + UI nếu chưa có

Trong Dev bạn có thể đã bật ở Day 1. Nếu chưa:

```csharp
builder.Services.AddOpenApi();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();            // /openapi/v1.json
    app.MapScalarApiReference(); // /scalar
}
```

---

## Bước 5 – Test

1. Gọi API:

```bash
curl -ks https://localhost:****/api/v1/now | jq
```

Kết quả (ví dụ):

```json
{ "now": "2025-08-22T13:45:12.3456789+00:00" }
```

2. Xem console nơi `dotnet watch` đang chạy — bạn sẽ thấy log:

``` bash
info: ClockEndpoint[0]
      Now endpoint called at 2025-08-22T13:45:12.3456789+00:00
```

---

## (Tuỳ chọn) Điều chỉnh mức log & format

Thêm vào `appsettings.Development.json` để thấy log info rõ hơn:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## Tóm tắt ghi nhớ DI (theo Microsoft Learn)

- **Đăng ký** dịch vụ trong `builder.Services` → **Resolve** bằng cách khai báo tham số vào handler/constructor.
- **Lifetime**:

  - `Singleton`: 1 cho toàn app (config, clock, clients).
  - `Scoped`: 1 cho mỗi HTTP request (DbContext).
  - `Transient`: mỗi lần resolve tạo mới (dịch vụ nhẹ, stateless).
- `ILogger<T>` là **dịch vụ sẵn có** trong DI: chỉ cần khai báo tham số là dùng.

---

Bạn cứ copy phần code ở Bước 1–3 vào `Program.cs` là hoàn thành **D2**. Nếu muốn mình refactor sang **file riêng** (ví dụ `TimeEndpoints.cs` + extension `MapTimeEndpoints`) để Program.cs gọn hơn, mình viết sẵn cho bạn ngay.
