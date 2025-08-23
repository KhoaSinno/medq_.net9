Bạn tiếp tục hướng dẫn tôi day 3, tôi làm gì đây:
D3 — Options pattern (45’)

/// Đây là yêu cầu: D3: A) Options pattern. B) AppOptions (paging size, cache TTL).

/// Đây là hướng dẫn cơ bản:

Thêm appsettings.json:

{
  "App": { "DefaultPageSize": 10, "CacheTtlSeconds": 120 }
}

Inject IOptions<AppOptions> vào endpoint để đọc cấu hình:

using Microsoft.Extensions.Options;

app.MapGet("/api/v1/app-settings", (IOptions<AppOptions> opt) => opt.Value)
   .WithOpenApi();

Options pattern cung cấp truy cập cấu hình strongly-typed, khuyến nghị cho cấu hình phân nhóm.
Microsoft Learn: <https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-9.0&utm_source=chatgpt.com>
