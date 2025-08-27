# Restructure folder

`Medq.Domain`, `Medq.Infrastructure`, `Medq.Api` là **3 project** riêng trong **cùng 1 solution `Medq.sln`**. Đây là bộ lệnh “tạo từ trắng” + “nâng cấp repo đang có `Medq.Api` sẵn”.

---

# A) Tạo mới từ trắng (khuyến nghị)

```bash
# 0) Tạo solution + thư mục chuẩn
mkdir Medq && cd Medq
dotnet new sln -n Medq
mkdir src && cd src

# 1) Tạo 3 project
dotnet new classlib -n Medq.Domain        -f net9.0
dotnet new classlib -n Medq.Infrastructure -f net9.0
dotnet new webapi   -n Medq.Api            -f net9.0

# 2) Thêm vào solution
cd ..
dotnet sln Medq.sln add \
  src/Medq.Domain/Medq.Domain.csproj \
  src/Medq.Infrastructure/Medq.Infrastructure.csproj \
  src/Medq.Api/Medq.Api.csproj

# 3) Thêm project reference
dotnet add src/Medq.Infrastructure/Medq.Infrastructure.csproj reference src/Medq.Domain/Medq.Domain.csproj
dotnet add src/Medq.Api/Medq.Api.csproj          reference src/Medq.Infrastructure/Medq.Infrastructure.csproj
dotnet add src/Medq.Api/Medq.Api.csproj          reference src/Medq.Domain/Medq.Domain.csproj

# 4) Cài package
dotnet add src/Medq.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add src/Medq.Infrastructure package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/Medq.Infrastructure package Microsoft.EntityFrameworkCore.Design

# (tuỳ) OpenAPI + Scalar UI nếu bạn dùng
dotnet add src/Medq.Api package Microsoft.AspNetCore.OpenApi
dotnet add src/Medq.Api package Scalar.AspNetCore

# 5) EF CLI
dotnet tool install -g dotnet-ef
```

## Bố cục file tối thiểu

```
src/
  Medq.Domain/
    Entities/
      Clinic.cs
      Pharmacy.cs
  Medq.Infrastructure/
    Data/
      MedqDbContext.cs
      MedqDbContextFactory.cs
      Configurations/
        ClinicConfig.cs
        PharmacyConfig.cs
  Medq.Api/
    Program.cs
    appsettings.Development.json
```

### Mẫu mã nhanh

**`src/Medq.Domain/Entities/Clinic.cs`**

```csharp
namespace Medq.Domain.Entities;
public sealed class Clinic
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
}
```

**`src/Medq.Domain/Entities/Pharmacy.cs`**

```csharp
namespace Medq.Domain.Entities;
public sealed class Pharmacy
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
    public bool OpenNow { get; set; }
}
```

**`src/Medq.Infrastructure/Data/MedqDbContext.cs`**

```csharp
using Medq.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Medq.Infrastructure.Data;

public sealed class MedqDbContext : DbContext
{
    public MedqDbContext(DbContextOptions<MedqDbContext> options) : base(options) {}
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();

    protected override void OnModelCreating(ModelBuilder b)
        => b.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
}
```

**`src/Medq.Infrastructure/Data/Configurations/ClinicConfig.cs`**

```csharp
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
    }
}
```

**`src/Medq.Infrastructure/Data/Configurations/PharmacyConfig.cs`**

```csharp
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
    }
}
```

**`src/Medq.Infrastructure/Data/MedqDbContextFactory.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Medq.Infrastructure.Data;

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

**`src/Medq.Api/appsettings.Development.json`**

```json
{
  "Database": { "Provider": "sqlite" },
  "ConnectionStrings": {
    "sqlite": "Data Source=medq.db"
  }
}
```

**`src/Medq.Api/Program.cs`**

```csharp
using Medq.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var provider = builder.Configuration["Database:Provider"] ?? "sqlite";
var connSqlite = builder.Configuration.GetConnectionString("sqlite");

builder.Services.AddDbContext<MedqDbContext>(opt =>
    opt.UseSqlite(connSqlite, x => x.MigrationsAssembly("Medq.Infrastructure")));

builder.Services.AddOpenApi(); // nếu đã cài package
var app = builder.Build();

app.MapOpenApi(); // nếu dùng OpenAPI
app.MapGet("/", () => "OK");
app.Run();
```

## Tạo migration & DB (migrations đặt trong Infrastructure)

```bash
dotnet ef migrations add InitialCreate \
  -p src/Medq.Infrastructure -s src/Medq.Api \
  -o Data/Migrations

dotnet ef database update \
  -p src/Medq.Infrastructure -s src/Medq.Api
```

---

# B) Nếu bạn đã có `src/Medq.Api` rồi

```bash
# 1) Thêm 2 project mới
cd src
dotnet new classlib -n Medq.Domain -f net9.0
dotnet new classlib -n Medq.Infrastructure -f net9.0
cd ..

# 2) Thêm vào solution
dotnet sln Medq.sln add \
  src/Medq.Domain/Medq.Domain.csproj \
  src/Medq.Infrastructure/Medq.Infrastructure.csproj

# 3) Thêm reference dây chuyền
dotnet add src/Medq.Infrastructure/Medq.Infrastructure.csproj reference src/Medq.Domain/Medq.Domain.csproj
dotnet add src/Medq.Api/Medq.Api.csproj          reference src/Medq.Infrastructure/Medq.Infrastructure.csproj
dotnet add src/Medq.Api/Medq.Api.csproj          reference src/Medq.Domain/Medq.Domain.csproj

# 4) Cài EF packages
dotnet add src/Medq.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add src/Medq.Infrastructure package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/Medq.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet tool install -g dotnet-ef
```

Sau đó chép các file mẫu ở trên vào đúng chỗ, cập nhật `Program.cs` và `appsettings.Development.json`, rồi chạy migrations như mục A.

---

## Ghi chú đổi DB provider sau này

* Giữ **Domain thuần** (không attribute EF).
* Mọi mapping đặt trong `Medq.Infrastructure` (Fluent API).
* Khi đổi provider (Postgres/SQL Server), chỉ đổi `UseNpgsql/UseSqlServer` + connection string, và **tạo lại migrations** cho provider mới:

  ```bash
  dotnet ef migrations add InitialCreate_Postgres \
    -p src/Medq.Infrastructure -s src/Medq.Api -o Data/Migrations/Postgres
  ```

  (Nếu không cần giữ dữ liệu cũ, xoá migrations cũ và tạo mới cho provider mới cho gọn.)

Bạn cứ chạy theo block lệnh trên, nếu vướng ở bước nào (build fail, không tìm thấy DbContext, mismatch version EF) nói mình biết — mình chỉ chỗ fix ngay.
