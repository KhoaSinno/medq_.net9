# Day 1:

 **Minimal API** trong .NET nghĩa là bạn **không cần tạo Controller + class phức tạp** như MVC nữa, mà chỉ viết trực tiếp các route (endpoint) ngay trong `Program.cs`. Tức là thay vì:

```csharp
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok" });
}
```

👉 Thì Minimal API chỉ cần một dòng trong `Program.cs`:

```csharp
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
```

---

## 🟢 Bước 1: Cài môi trường

1. **Cài .NET 9 SDK** từ [trang chính Microsoft](https://dotnet.microsoft.com/en-us/download).
   Sau đó kiểm tra:

   ```bash
   dotnet --info
   dotnet --list-sdks
   ```

2. **Cài VS Code + extension**:

   * [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
   * [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)

---

## 🟢 Bước 2: Tạo skeleton dự án Minimal API

Mở terminal trong thư mục bạn muốn, rồi chạy:

```bash
mkdir medq && cd medq
dotnet new sln -n Medq
mkdir src && cd src
dotnet new web -n Medq.Api
dotnet sln ../Medq.sln add Medq.Api/Medq.Api.csproj
cd Medq.Api
dotnet run
```

> Lúc này bạn có một API trống chạy ở `https://localhost:xxxx`.

---

## 🟢 Bước 3: Thêm endpoint đầu tiên `/health`

Mở file `Program.cs`, sửa thành:

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
```

Chạy lại:

```bash
dotnet watch
```

Mở trình duyệt:
👉 `https://localhost:xxxx/health` sẽ thấy:

```json
{ "status": "ok" }
```

---

## Kết luận

* **Minimal API = code gọn hơn, trực tiếp hơn** (không controller).
* Bước đầu tiên bạn phải làm: **cài .NET 9 SDK + tạo project Minimal API** như trên.
* Sau khi chạy được `/health`, thì mới tiếp tục sang DI, Options, OpenAPI.

---

👉 Bạn có muốn mình vẽ sơ đồ **so sánh Minimal API vs Controller API** để bạn nhìn trực quan dễ hiểu hơn không?
