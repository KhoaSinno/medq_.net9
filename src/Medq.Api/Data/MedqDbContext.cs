using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Medq.Api.Data
{
    public class MedqDbContext : DbContext
    {
        public MedqDbContext(DbContextOptions<MedqDbContext> options) : base(options)
        {
        }

        public DbSet<Clinic> Clinics => Set<Clinic>();
        public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Clinic>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).HasMaxLength(200).IsRequired();
                e.Property(x => x.Address).HasMaxLength(300);
            });

            b.Entity<Pharmacy>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).HasMaxLength(200).IsRequired();
                e.Property(x => x.Address).HasMaxLength(300);
            });
        }
    }


    public sealed class Clinic
    {
        public int Id { get; set; }
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
}

