migrate:
	dotnet ef migrations add InitialCreate -p src/Medq.Infrastructure -s src/Medq.Api -o Data/Migrations

db-update:
	dotnet ef database update -p src/Medq.Infrastructure -s src/Medq.Api

migrate-seed-v1:
	dotnet ef migrations add SeedV1 -p src/Medq.Infrastructure -s src/Medq.Api -o Data/Migrations

all: migrate db-update