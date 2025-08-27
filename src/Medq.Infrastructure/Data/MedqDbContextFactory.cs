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
