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

        // Seed data
        e.HasData(
            new Clinic { Id = 1, Name = "Downtown Health Clinic", Address = "123 Main St, Cityville" },
            new Clinic { Id = 2, Name = "Uptown Medical Center", Address = "456 Elm St, Townsville" },
            new Clinic { Id = 3, Name = "Suburban Family Clinic", Address = "789 Oak St, Suburbia" }
        );
    }

}
