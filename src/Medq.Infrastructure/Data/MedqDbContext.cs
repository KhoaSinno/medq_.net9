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
