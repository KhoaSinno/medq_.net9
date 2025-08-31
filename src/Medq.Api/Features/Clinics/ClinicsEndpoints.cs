using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Medq.Api.Contracts.Clinics;
using Medq.Api.Contracts.Common;
using Medq.Api.Options;
using Medq.Domain.Entities;
using Medq.Infrastructure.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Medq.Api.Features.Clinics
{
    public static class ClinicsEndpoints
    {
        public static IEndpointRouteBuilder MapClinicsEndpoint(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/clinics").WithTags("Clinics");

            // Get all (simple)
            group.MapGet("/", (async (MedqDbContext db, CancellationToken ct) =>
            {
                var clinics = await db.Clinics.AsNoTracking().ToListAsync(ct);
                return Results.Ok(clinics);
            })).WithName("ListClinics").WithTags("Clinics").WithSummary("List all clinics").WithDescription("Returns a list of all clinics.").Produces<IEnumerable<Clinic>>(StatusCodes.Status200OK).ProducesProblem(StatusCodes.Status404NotFound).WithOpenApi();

            // Search
            group.MapGet("/search", async ([AsParameters] PagingQuery q, IOptions<AppOptions> opt, MedqDbContext db, CancellationToken ct) =>
            {
                var page = q.Page < 1 ? 1 : q.Page;
                var pageSize = q.PageSize > 0 ? Math.Min(q.PageSize, opt.Value.MaxPageSize) : opt.Value.DefaultPageSize;

                var query = db.Clinics.AsNoTracking();

                query = q.Sort?.ToLowerInvariant() switch
                {
                    "name" => query.OrderBy(x => x.Name),
                    "-name" => query.OrderByDescending(x => x.Name),
                    "-id" => query.OrderByDescending(x => x.Id),
                    _ => query.OrderBy(x => x.Id)
                };

                var total = await query.CountAsync(ct);
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

                return Results.Ok(new { total, page, pageSize, items });
            }).WithName("SearchClinics").WithTags("Clinics").WithDescription("Search clinics with pagination and sorting.").WithSummary("Search clinics").Produces<IEnumerable<Clinic>>(StatusCodes.Status200OK).ProducesProblem(StatusCodes.Status404NotFound).WithOpenApi();

            // Get by ID
            group.MapGet("/{id}", (async (MedqDbContext db, int id, CancellationToken ct) =>
            {
                var clinic = await db.Clinics.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
                return clinic is not null ? Results.Ok(clinic) : Results.NotFound();
            })).WithName("GetClinicById").WithTags("Clinics").WithSummary("Get clinic by id").Produces<Clinic>(StatusCodes.Status200OK).ProducesProblem(StatusCodes.Status404NotFound).WithOpenApi();

            // Post 
            group.MapPost("/", async Task<Results<Created<Domain.Entities.Clinic>, ValidationProblem>> (MedqDbContext db, ClinicCreateDto dto, CancellationToken ct) =>
            {
                var name = dto.Name.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                    {
                        { nameof(dto.Name), new[] { "Name is required." } }
                    });
                }

                var clinic = new Domain.Entities.Clinic { Name = name, Address = dto.Address?.Trim() };
                db.Clinics.Add(clinic);
                await db.SaveChangesAsync(ct);
                return TypedResults.Created($"/api/v1/clinics/{clinic.Id}", clinic);
            }).WithName("CreateClinic").WithTags("Clinics").WithSummary("Create clinic").WithDescription("Creates a clinic and returns 201 with Location header.").Accepts<ClinicCreateDto>("application/json").Produces<Domain.Entities.Clinic>(StatusCodes.Status201Created).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi();

            // PUT
            group.MapPut("/{id:int}", async Task<Results<Ok<Domain.Entities.Clinic>, NotFound, ValidationProblem>> (int id, ClinicUpdateDto dto, MedqDbContext db, CancellationToken ct) =>
            {
                var entity = await db.Clinics.FirstOrDefaultAsync(x => x.Id == id, ct);
                if (entity is null) return TypedResults.NotFound();

                var name = dto.Name?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["name"] = new[] { "Name is required." } });

                entity.Name = name!;
                entity.Address = dto.Address?.Trim();
                await db.SaveChangesAsync(ct);
                return TypedResults.Ok(entity);
            }).WithName("UpdateClinic").WithTags("Clinics").WithSummary("Update clinic").WithDescription("Updates a clinic and returns 200 OK.").Accepts<ClinicUpdateDto>("application/json").Produces<Domain.Entities.Clinic>(StatusCodes.Status200OK).ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi();

            // DELETE
            group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (int id, MedqDbContext db, CancellationToken ct) =>
            {
                var entity = await db.Clinics.FirstOrDefaultAsync(x => x.Id == id, ct);
                if (entity is null) return TypedResults.NotFound();

                db.Clinics.Remove(entity);
                await db.SaveChangesAsync(ct);
                return TypedResults.NoContent();
            }).WithName("DeleteClinic").WithTags("Clinics").WithSummary("Delete clinic").WithDescription("Deletes a clinic and returns 204 No Content.").ProducesProblem(StatusCodes.Status404NotFound).WithOpenApi();

            return app;
        }
    }
}