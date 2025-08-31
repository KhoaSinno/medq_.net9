using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Medq.Api.Contracts.Pharmacies;
using Medq.Domain.Entities;
using Medq.Infrastructure.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
            }).WithName("ListPharmacies").WithTags("Pharmacies").WithSummary("List all pharmacies").WithDescription("Returns a list of all pharmacies.").Produces<IEnumerable<Pharmacy>>(StatusCodes.Status200OK).ProducesProblem(StatusCodes.Status404NotFound).WithOpenApi();

            // filter ?openNow=false | true
            group.MapGet("/filter", async (MedqDbContext db, bool? openNow, CancellationToken ct) =>
            {
                var p = db.Pharmacies.AsNoTracking().AsQueryable();

                if (openNow.HasValue)
                    p = p.Where(x => x.OpenNow == openNow.Value);

                return await p.ToListAsync(ct);
            }).WithName("FilterPharmacies").WithTags("Pharmacies").WithSummary("Filter pharmacies").WithDescription("Filters pharmacies by open status.").Produces<IEnumerable<Pharmacy>>(StatusCodes.Status200OK).ProducesProblem(StatusCodes.Status404NotFound).WithOpenApi();

            // get by id
            group.MapGet("/{id:int}", async (MedqDbContext db, int id, CancellationToken ct) =>
            {
                var pharmacy = await db.Pharmacies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
                return pharmacy is not null ? Results.Ok(pharmacy) : Results.NotFound();
            }).WithName("GetPharmacyById").WithTags("Pharmacies").WithSummary("Get pharmacy by id").Produces<Pharmacy>(StatusCodes.Status200OK).ProducesProblem(StatusCodes.Status404NotFound).WithOpenApi();

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
            }).WithName("CreatePharmacy").WithTags("Pharmacies").WithSummary("Create pharmacy").WithDescription("Creates a pharmacy and returns 201 with Location header.").Accepts<PharmacyCreateDto>("application/json").Produces<Domain.Entities.Pharmacy>(StatusCodes.Status201Created).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi();

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
            }).WithName("UpdatePharmacy").WithTags("Pharmacies").WithSummary("Update pharmacy").WithDescription("Updates a pharmacy and returns 200 OK.").Accepts<PharmacyUpdateDto>("application/json").Produces<Domain.Entities.Pharmacy>(StatusCodes.Status200OK).ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi();

            // Delete
            group.MapDelete("/{id:int}", async (MedqDbContext db, int id, CancellationToken ct) =>
            {
                var pharmacy = await db.Pharmacies.FindAsync(new object[] { id }, ct);
                if (pharmacy is null) return Results.NotFound();

                db.Pharmacies.Remove(pharmacy);
                await db.SaveChangesAsync(ct);
                return Results.NoContent();
            }).WithName("DeletePharmacy").WithTags("Pharmacies").WithSummary("Delete pharmacy").WithDescription("Deletes a pharmacy and returns 204 No Content.").ProducesProblem(StatusCodes.Status404NotFound).WithOpenApi();


            return app;
        }
    }
}