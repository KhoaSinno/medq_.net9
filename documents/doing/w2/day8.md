Tuyệt vời, mình đọc kỹ roadmap + hướng dẫn tuần 2 của bạn rồi. D8 là “gắn EF Core + SQLite, thêm DbContext” đúng như bạn ghi trong README (Tuần 2 – D8) . Dưới đây là checklist làm được ngay + các lỗi hay gặp và cách fix, để bạn đi hết D8 trơn tru.

# D8 — Kết nối EF Core + SQLite (kèm fix lỗi hay gặp)

## 1) Cài package đúng phiên bản

```bash
cd src/Medq.Api
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet tool install --global dotnet-ef
```

Mẹo kiểm tra:

* `dotnet --list-sdks` và `dotnet ef --version` để chắc tools/SDK khớp major (ví dụ 9.x ↔ 9.x).
* Nếu từng cài dotnet-ef cũ, chạy `dotnet tool update -g dotnet-ef`.

**Lỗi hay gặp & cách xử lý**

* “The EF Core tools version ... older/newer than runtime”: cập nhật gói EF Core trong project cho khớp major với tools/SDK, hoặc `dotnet tool update -g dotnet-ef`.

## 2) appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=medq.db"
  }
}
```

Tip: khi chạy `dotnet ef` từ thư mục solution, Tools mặc định dùng cấu hình Development nếu bạn đang ở môi trường dev. Nếu nghi ngờ, có thể set `ASPNETCORE_ENVIRONMENT=Development` tạm thời trước lệnh.

## 3) DbContext + DI trong Program.cs

```csharp
// Program.cs
using Medq.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MedqDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddProblemDetails(); // tuần này bạn sẽ bật ProblemDetails ở D14
// ... các service khác

var app = builder.Build();
app.UseExceptionHandler(); // để ProblemDetails hoạt động (D14)
```

## 4) Khai báo DbContext + (Khuyến nghị) đổi entity từ record sang class

> Bạn đang dùng `record` dạng positional: `public sealed record Clinic(int Id, ...)`. Cách này **thường lỗi** khi insert vì khóa tự tăng cần EF set lại `Id` sau `SaveChanges`, mà property `init`/positional không có setter để EF cập nhật. Giải pháp an toàn là dùng `class` có setter.

```csharp
// src/Medq.Api/Data/MedqDbContext.cs
using Microsoft.EntityFrameworkCore;

namespace Medq.Api.Data;

public sealed class MedqDbContext : DbContext
{
    public MedqDbContext(DbContextOptions<MedqDbContext> options) : base(options) { }

    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Clinic>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd(); // SQLite INTEGER PRIMARY KEY AUTOINCREMENT
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Address).HasMaxLength(300);
        });

        b.Entity<Pharmacy>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Address).HasMaxLength(300);
            e.Property(x => x.OpenNow).IsRequired();
        });
    }
}

// Entities: dùng class thay vì positional record để tránh lỗi Id không set được
public sealed class Clinic
{
    public int Id { get; set; }                  // cần setter để EF set giá trị sau insert
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
}

public sealed class Pharmacy
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
    public bool OpenNow { get; set; }
}
```

**Lỗi hay gặp & cách xử lý**

* “The property ‘Clinic.Id’ is part of a key and so cannot be modified” / POST xong `Id` vẫn 0: nguyên nhân thường do `record` với `init` không có setter. Chuyển sang `class` như trên là hết.
* “No provider found for ‘Microsoft.EntityFrameworkCore.Sqlite’”: thiếu gói `Microsoft.EntityFrameworkCore.Sqlite` hoặc version mismatch → cài/upgrade gói.

## 5) (Tuỳ chọn) Design-time factory nếu `dotnet ef` không tìm thấy DbContext

Một số dự án Minimal API/kịch bản cấu hình phức tạp khiến Tools không tạo được host. Bạn thêm factory:

```csharp
// src/Medq.Api/Data/MedqDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Medq.Api.Data;

public sealed class MedqDbContextFactory : IDesignTimeDbContextFactory<MedqDbContext>
{
    public MedqDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<MedqDbContext>()
            .UseSqlite("Data Source=medq.db")
            .Options;

        return new MedqDbContext(opts);
    }
}
```

**Lỗi hay gặp & cách xử lý**

* “Unable to create an object of type ‘MedqDbContext’.” → thêm factory trên là chạy được ngay.

## 6) Tạo migration & tạo DB (chuẩn bị cho D9)

Bạn đã lên kế hoạch D9 chạy migrations (README: D9) . Có thể chạy ngay cuối D8 để chốt luồng:

```bash
# từ root solution (có thư mục src/Medq.Api)
dotnet ef migrations add InitialCreate -p src/Medq.Api -s src/Medq.Api
dotnet ef database update -p src/Medq.Api -s src/Medq.Api
```

**Lỗi hay gặp & cách xử lý**

* “No DbContext was found.” → kiểm tra namespace/assembly đúng, có AddDbContext trong Program.cs, hoặc dùng factory ở bước 5.
* “Build failed.” → fix compile trước (thường do thiếu `using Medq.Api.Data;` trong Program.cs).

## 7) Smoke test nhanh (kết thúc D8)

* Kiểm tra file `medq.db` đã tạo.
* `dotnet run` rồi gọi `GET /api/v1/clinics` (tạm thời sẽ rỗng nếu chưa seed; D10 bạn mới seed).
* Thử `POST /api/v1/clinics` (sau khi làm CRUD ở D11) xem `Id` tự tăng OK.

---

# Bonus: Vì sao roadmap đề xuất AsNoTracking/Paging sau này?

Trong README của bạn, tuần 2 còn nhấn mạnh AsNoTracking, paging Skip/Take, v.v. ở D11 (truy vấn) và D13 (paging) để tối ưu đọc và tránh tracking thừa (đã được note ở phần cảnh báo) . Cứ giữ thói quen:

* `GET` → `AsNoTracking()`
* `POST/PUT/DELETE` → tracking mặc định
* Paging + Sort sẽ implement ở D13.

---

## Tóm tắt những “điểm gãy” phổ biến bạn có thể đang gặp ở D8 (và cách gỡ):

1. **dotnet-ef không chạy/khác version** → cập nhật tool & packages cho khớp major.
2. **Không tìm được MedqDbContext** → thêm `IDesignTimeDbContextFactory`.
3. **POST xong Id = 0 hoặc lỗi set key** → đổi entity từ positional `record` sang `class` có `set;`.
4. **Provider SQLite không nạp được** → cài `Microsoft.EntityFrameworkCore.Sqlite`, kiểm tra `UseSqlite(...)` trong Program.cs.
5. **Không load được connection string** → kiểm tra `appsettings.Development.json` + biến môi trường.

---

Nếu bạn muốn, mình gói **Tuần 2** thành một file hướng dẫn Markdown + patch code đầy đủ (seed, CRUD, paging, ProblemDetails) để bạn commit một phát là xong — nói mình biết nhé. Còn bây giờ, cứ đi đúng checklist trên là bạn vượt qua D8 chuẩn, trùng khớp mục tiêu tuần 2 trong README của bạn .
