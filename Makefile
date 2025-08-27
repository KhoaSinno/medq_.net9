migrate:
    dotnet ef migrations add InitialCreate -p src/Medq.Infrastructure -s src/Medq.Api -o Data/Migrations

db-update:
    dotnet ef database update -p src/Medq.Infrastructure -s src/Medq.Api

all: migrate db-update