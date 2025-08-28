using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Medq.Api.Contracts.Pharmacies;
using Medq.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Medq.Api.Features.Pharmacies
{
    public static class PharmaciesEndpoints
    {
        public static IEndpointRouteBuilder MapPharmaciesEndpoint(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/pharmacies").WithTags("Pharmacies");
            // Main

            group.MapGet("/", async (MedqDbContext db, CancellationToken ct) =>
            {
                var pharmacies = await db.Pharmacies.AsNoTracking().ToListAsync(ct);
                return Results.Ok(pharmacies);
            }).WithOpenApi();

            // filter ?openNow=false | true
            group.MapGet("/filter", async (MedqDbContext db, bool? openNow, CancellationToken ct) =>
            {
                var p = db.Pharmacies.AsNoTracking().AsQueryable();

                if (openNow.HasValue)
                    p = p.Where(x => x.OpenNow == openNow.Value);

                return await p.ToListAsync(ct);
            }).WithOpenApi();

            // get by id
            group.MapGet("/{id:int}", async (MedqDbContext db, int id, CancellationToken ct) =>
            {
                var pharmacy = await db.Pharmacies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
                return pharmacy is not null ? Results.Ok(pharmacy) : Results.NotFound();
            }).WithOpenApi();

            // Create
            group.MapPost("/", async (MedqDbContext db, PharmacyCreateDto dto, CancellationToken ct) =>
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        { nameof(dto.Name), new[] { "Name is required." } }
                    });
                }

                var entity = new Domain.Entities.Pharmacy
                {
                    Name = dto.Name.Trim(),
                    Address = dto.Address?.Trim(),
                    OpenNow = dto.OpenNow
                };
                db.Pharmacies.Add(entity);
                await db.SaveChangesAsync(ct);
                return Results.Created($"/api/v1/pharmacies/{entity.Id}", entity);
            }).WithOpenApi();

            // Update
            group.MapPut("/{id:int}", async (MedqDbContext db, int id, PharmacyUpdateDto dto, CancellationToken ct) =>
            {
                var pharmacy = await db.Pharmacies.FindAsync(new object[] { id }, ct);
                if (pharmacy is null) return Results.NotFound();

                pharmacy.Name = dto.Name.Trim();
                pharmacy.Address = dto.Address?.Trim();
                pharmacy.OpenNow = dto.OpenNow;

                await db.SaveChangesAsync(ct);
                return Results.Ok(pharmacy);
            }).WithOpenApi();

            // Delete
            group.MapDelete("/{id:int}", async (MedqDbContext db, int id, CancellationToken ct) =>
            {
                var pharmacy = await db.Pharmacies.FindAsync(new object[] { id }, ct);
                if (pharmacy is null) return Results.NotFound();

                db.Pharmacies.Remove(pharmacy);
                await db.SaveChangesAsync(ct);
                return Results.NoContent();
            }).WithOpenApi();

            
            return app;
        }
    }
}