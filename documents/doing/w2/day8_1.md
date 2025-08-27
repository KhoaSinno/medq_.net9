# Setup DB provider (SQLite → SQL Server/Postgres…)

# 1) `OnModelCreating` là gì? “Kiểu” tham số là gì?

* Ký hiệu đầy đủ: `protected override void OnModelCreating(ModelBuilder modelBuilder)`.
* Tham số `modelBuilder` (bạn hay đặt `b`) là **ModelBuilder** của EF Core. Nó cung cấp **Fluent API** để cấu hình mapping: khóa chính, độ dài, quan hệ, chỉ mục, seed, chuyển kiểu, v.v.
  Ví dụ:

  ```csharp
  protected override void OnModelCreating(ModelBuilder b)
  {
      b.Entity<Clinic>(e =>
      {
          e.HasKey(x => x.Id);
          e.Property(x => x.Id).ValueGeneratedOnAdd();
          e.Property(x => x.Name).HasMaxLength(200).IsRequired();
          e.Property(x => x.Address).HasMaxLength(300);
      });
  }
  ```

# 2) Data Annotations vs Fluent API (trong `OnModelCreating`)

**Hai cách cấu hình mapping**:

* **Data Annotations**: gắn attribute ngay trên entity class/properties.

  ```csharp
  public sealed class Clinic
  {
      public int Id { get; set; }

      [Required, MaxLength(200)]
      public string Name { get; set; } = default!;

      [MaxLength(300)]
      public string? Address { get; set; }
  }
  ```

* **Fluent API**: viết trong `OnModelCreating` (hoặc tách ra `IEntityTypeConfiguration<T>` – bên dưới mình khuyên dùng cách này).

**So sánh nhanh**

* Dễ & nhanh:

  * Annotations rất nhanh cho các ràng buộc **đơn giản** (Required/MaxLength/Column/Index…).
  * Fluent API “dài hơi” hơn nhưng **đầy đủ & mạnh** (composite key, quan hệ phức tạp, owned types, shadow props, value converter, global query filter, seed…).
* Tách biệt mối quan tâm:

  * Annotations **làm bẩn domain** (entity biết về EF).
  * Fluent API cho phép **domain thuần POCO** (sạch), mọi chi tiết persistence để riêng ở “Infrastructure”.
* Khả năng mở rộng/đa provider:

  * Annotations có giới hạn biểu đạt, khó xử lý khác biệt giữa các provider.
  * Fluent API dễ “điều chỉnh theo provider”, thêm converter, và gom cấu hình theo assembly.

**Khuyến nghị thực chiến**

* Với dự án học + muốn “sạch” và **dễ đổi DB provider**:
  → Dùng **Fluent API** (tách qua `IEntityTypeConfiguration<T>`).
* Annotations chỉ dùng **tối thiểu** (nếu thích) cho case cực đơn giản hoặc **trên DTO** để tận dụng validate (không khuyến khích gắn nhiều vào entity domain).

# 3) Tách class/đặt tên/đặt ở đâu cho “pro”

Giữ **Domain** tách khỏi EF, và đưa EF vào **Infrastructure**. Một cấu trúc gọn trong 1 solution:

```
src/
  Medq.Api/                      # Minimal API host
    Program.cs
    Contracts/                   # DTOs (request/response)
      Clinics/
        ClinicCreateDto.cs
        ClinicUpdateDto.cs
    Features/                    # (tùy) nhóm endpoints theo feature
      Clinics/
        ClinicsEndpoints.cs
  Medq.Domain/                   # THUẦN business/domain (KHÔNG EF)
    Entities/
      Clinic.cs
      Pharmacy.cs
  Medq.Infrastructure/           # EF Core, DbContext, migrations
    Data/
      MedqDbContext.cs
      Configurations/
        ClinicConfig.cs
        PharmacyConfig.cs
      Migrations/                # migrations nằm ở đây (SQLite)
```

### Domain entities (POCO, sạch)

```csharp
// src/Medq.Domain/Entities/Clinic.cs
namespace Medq.Domain.Entities;

public sealed class Clinic
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
}
```

```csharp
// src/Medq.Domain/Entities/Pharmacy.cs
namespace Medq.Domain.Entities;

public sealed class Pharmacy
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
    public bool OpenNow { get; set; }
}
```

### DbContext + auto-áp cấu hình

```csharp
// src/Medq.Infrastructure/Data/MedqDbContext.cs
using Medq.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Medq.Infrastructure.Data;

public sealed class MedqDbContext : DbContext
{
    public MedqDbContext(DbContextOptions<MedqDbContext> options) : base(options) { }

    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Tự động áp toàn bộ IEntityTypeConfiguration<> trong assembly này
        b.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

### Cấu hình mỗi entity (IEntityTypeConfiguration)

```csharp
// src/Medq.Infrastructure/Data/Configurations/ClinicConfig.cs
using Medq.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medq.Infrastructure.Data.Configurations;

public sealed class ClinicConfig : IEntityTypeConfiguration<Clinic>
{
    public void Configure(EntityTypeBuilder<Clinic> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedOnAdd();
        e.Property(x => x.Name).HasMaxLength(200).IsRequired();
        e.Property(x => x.Address).HasMaxLength(300);

        // (tuỳ chọn) seed
        // e.HasData(new Clinic { Id = 1, Name = "Clinic A", Address = "123 Lê Lợi" });
    }
}
```

```csharp
// src/Medq.Infrastructure/Data/Configurations/PharmacyConfig.cs
using Medq.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medq.Infrastructure.Data.Configurations;

public sealed class PharmacyConfig : IEntityTypeConfiguration<Pharmacy>
{
    public void Configure(EntityTypeBuilder<Pharmacy> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedOnAdd();
        e.Property(x => x.Name).HasMaxLength(200).IsRequired();
        e.Property(x => x.Address).HasMaxLength(300);
        e.Property(x => x.OpenNow).IsRequired();

        // Seed mẫu (tuỳ)
        // e.HasData(
        //   new Pharmacy { Id = 1, Name="Pharmacy A", Address="789 Hai Bà Trưng", OpenNow=true }
        // );
    }
}
```

### Wire up ở `Medq.Api`

```csharp
// src/Medq.Api/Program.cs
using Medq.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var provider = builder.Configuration["Database:Provider"] ?? "sqlite";
var connSqlite   = builder.Configuration.GetConnectionString("sqlite");
var connPostgres = builder.Configuration.GetConnectionString("postgres");
var connSqlServer= builder.Configuration.GetConnectionString("sqlserver");

// Chọn provider qua cấu hình (xem phần 4)
builder.Services.AddDbContext<MedqDbContext>(opt =>
{
    switch (provider.ToLowerInvariant())
    {
        case "postgres":
        case "npgsql":
            opt.UseNpgsql(connPostgres,
                x => x.MigrationsAssembly("Medq.Infrastructure")); // xem ghi chú migrations
            break;
        case "sqlserver":
            opt.UseSqlServer(connSqlServer,
                x => x.MigrationsAssembly("Medq.Infrastructure"));
            break;
        default:
            opt.UseSqlite(connSqlite,
                x => x.MigrationsAssembly("Medq.Infrastructure"));
            break;
    }
});

builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();

app.MapGet("/", () => "OK");
app.Run();
```

### `appsettings.Development.json` (ví dụ)

```json
{
  "Database": { "Provider": "sqlite" },
  "ConnectionStrings": {
    "sqlite":   "Data Source=medq.db",
    "postgres": "Host=localhost;Database=medq;Username=postgres;Password=postgres",
    "sqlserver":"Server=localhost,1433;Database=medq;User Id=sa;Password=Your_password123;TrustServerCertificate=True"
  }
}
```

> Với cấu trúc này, **Domain** không hề biết EF; EF gói ở **Infrastructure** (DbContext + Config). API chỉ `AddDbContext` + gọi CRUD.

# 4) Làm sao để **đổi DB provider** tương lai cho dễ?

## a) Chọn provider qua config (runtime)

Như trên, chỉ cần đổi `"Database:Provider"` và connection string. **Code CRUD, DbContext, Config** giữ nguyên (trừ khi bạn đã dùng tính năng quá đặc thù một provider).

## b) Migrations & chiến lược đa provider

* **Sự thật quan trọng**: migrations có **dấu ấn provider** (SQLite/SQL Server/Postgres tạo SQL khác nhau).
* Nếu bạn **chỉ cần đổi provider khi triển khai mới** (không migrate dữ liệu cũ):
  → đơn giản là **xoá migration cũ**, chạy `add` lại cho provider mới, `database update` là xong.
* Nếu bạn muốn **duy trì song song nhiều provider** (ít gặp ở dự án sinh viên, nhưng nói để biết):

  * Dùng **separate migrations** theo provider (thậm chí separate assembly).
  * Ví dụ: xuất migrations vào thư mục con:

    ```bash
    # SQLite
    dotnet ef migrations add InitialCreate_Sqlite \
      -p src/Medq.Infrastructure -s src/Medq.Api \
      -o Data/Migrations/Sqlite

    # Postgres
    dotnet ef migrations add InitialCreate_Postgres \
      -p src/Medq.Infrastructure -s src/Medq.Api \
      -o Data/Migrations/Postgres
    ```

  * Khi cấu hình provider, chỉ định `MigrationsAssembly` (hoặc để chung 1 assembly nhưng bạn **chỉ dùng** đúng set migrations tương ứng môi trường đó).
  * Thực tế, để “đồng hành đa provider” mượt, người ta tạo **2 project Infrastructure** (ví dụ `Medq.Infrastructure.Sqlite` và `Medq.Infrastructure.Postgres`) mỗi project chứa migrations riêng, cùng tham chiếu `Medq.Domain`. Với student project, **không cần phức tạp hóa** trừ khi thật sự cần.

## c) Những **khác biệt provider** bạn nên lưu ý để code “trung tính”

* **`decimal` (tiền tệ)**: SQLite không có `decimal` thực sự → EF thường mapping kiểu TEXT/REAL. Nếu bạn tính toán tiền, với SQLite cân nhắc **long số tiền theo cents** + `ValueConverter`. SQL Server/Postgres thì `decimal(18,2)` ổn.
* **`DateOnly/TimeOnly`**: hỗ trợ phụ thuộc provider/version EF. Nếu muốn an toàn tuyệt đối giữa nhiều DB, ưu tiên `DateTime` (+ `Kind` rõ).
* **Auto-increment/Identity**: dùng `.ValueGeneratedOnAdd()` (trung tính). Tránh gọi API đặc thù như `UseIdentityColumn()` (SQL Server) nếu muốn multi-provider.
* **Computed/Index có filter/JSON column**: nhiều cái là **đặc thù** (SQL Server/Postgres mạnh hơn SQLite). Cố gắng tránh tính năng độc quyền nếu muốn “đổi provider không đau”.
* **Raw SQL**: hạn chế `FromSqlRaw` trừ khi cần; nếu dùng thì viết SQL **chuẩn ANSI** nhất có thể.

# 5) Kết luận & gợi ý thực thi

* **Dùng Fluent API tách file** (IEntityTypeConfiguration) ⇒ domain sạch, cấu hình mạnh, sẵn sàng đa provider.
* **Folder/project tách bạch** như cây ở trên.
* **Chọn provider** qua cấu hình; migrations xử lý theo nhu cầu (đổi provider 1 lần → regenerate; chạy đa provider song song → tách migrations).
* **Kiểu dữ liệu**: chọn “trung tính” (int/string/bool/DateTime), cẩn thận với `decimal` ở SQLite.
