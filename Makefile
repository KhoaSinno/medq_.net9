migrate:
	dotnet ef migrations add InitialCreate -p src/Medq.Infrastructure -s src/Medq.Api -o Data/Migrations

db-update:
	dotnet ef database update -p src/Medq.Infrastructure -s src/Medq.Api

migrate-update: migrate db-update

# Seed database
migrate-seed-v2:
	dotnet ef migrations add SeedV2 -p src/Medq.Infrastructure -s src/Medq.Api -o Data/Migrations

seed-update: migrate-seed-v2 db-update