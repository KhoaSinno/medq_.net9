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

        // Seed data
        e.HasData(
            new Pharmacy { Id = 1, Name = "City Pharmacy", Address = "100 Health St, Cityville", OpenNow = true },
            new Pharmacy { Id = 2, Name = "Town Drugstore", Address = "200 Wellness Ave, Townsville", OpenNow = false },
            new Pharmacy { Id = 3, Name = "Suburbia Meds", Address = "300 Care Blvd, Suburbia", OpenNow = true }
        );
    }
}
