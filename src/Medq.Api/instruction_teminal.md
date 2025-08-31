# Database Commands - SQLite

## Database Location

```
src/Medq.Api/medq.db
```

## Basic SQLite Commands

### 1. Connect to Database

```bash
# From root directory
sqlite3 src/Medq.Api/medq.db

# From API directory  
cd src/Medq.Api
sqlite3 medq.db
```

### 2. Show All Tables

```sql
.tables
```

### 3. Show Table Schema

```sql
.schema TableName
.schema Clinics
.schema Pharmacies
```

### 4. Show All Data

```sql
-- Show all clinics
SELECT * FROM Clinics;

-- Show all pharmacies
SELECT * FROM Pharmacies;

-- Show migration history
SELECT * FROM __EFMigrationsHistory;
```

### 5. Count Records

```sql
SELECT COUNT(*) FROM Clinics;
SELECT COUNT(*) FROM Pharmacies;
```

### 6. Filtered Queries

```sql
-- Pharmacies that are open now
SELECT * FROM Pharmacies WHERE OpenNow = 1;

-- Pharmacies that are closed
SELECT * FROM Pharmacies WHERE OpenNow = 0;

-- Clinics by name (partial match)
SELECT * FROM Clinics WHERE Name LIKE '%Health%';
```

### 7. Exit SQLite

```sql
.exit
```

## Quick Commands (One-liners)

### Show all tables

```bash
sqlite3 src/Medq.Api/medq.db ".tables"
```

### Show all clinics

```bash
sqlite3 src/Medq.Api/medq.db "SELECT * FROM Clinics;"
```

### Show all pharmacies

```bash
sqlite3 src/Medq.Api/medq.db "SELECT * FROM Pharmacies;"
```

### Show migration history

```bash
sqlite3 src/Medq.Api/medq.db "SELECT * FROM __EFMigrationsHistory;"
```

### Count records

```bash
sqlite3 src/Medq.Api/medq.db "SELECT COUNT(*) as Total_Clinics FROM Clinics;"
sqlite3 src/Medq.Api/medq.db "SELECT COUNT(*) as Total_Pharmacies FROM Pharmacies;"
```

## Database Management

### Backup Database

```bash
cp src/Medq.Api/medq.db backup_$(date +%Y%m%d).db
```

### Clear All Data (Keep Tables)

```bash
sqlite3 src/Medq.Api/medq.db "DELETE FROM Clinics; DELETE FROM Pharmacies;"
```

### Database Info

```bash
sqlite3 src/Medq.Api/medq.db ".dbinfo"
```

## EF Core Commands

### Apply Migrations

```bash
dotnet ef database update --project ../Medq.Infrastructure --startup-project .
```

### Create New Migration

```bash
dotnet ef migrations add MigrationName --project ../Medq.Infrastructure --startup-project . -o Data/Migrations
```

### Check Migration Status

```bash
dotnet ef migrations list --project ../Medq.Infrastructure --startup-project .
```
