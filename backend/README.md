# PMSS Backend - .NET Web API

## Overview

ASP.NET Core 10 Web API built with Clean Architecture principles for managing academic projects with GitHub and Jira integration.

## Tech Stack

- **.NET 10** - Framework
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **Swagger/OpenAPI** - API Documentation

## Architecture

```
backend/
├── PMSS.Domain/          # Entities, Enums
├── PMSS.Application/     # DTOs, Interfaces, Business Logic
├── PMSS.Infrastructure/  # Data Access, Services, Repositories
└── PMSS.API/            # Controllers, Middleware
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (Express/Developer Edition)
- [Visual Studio 2025](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [SQL Server Management Studio](https://docs.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms) (Optional)

## Quick Start

### 1. Install .NET SDK

```bash
# Verify installation
dotnet --version
```

### 2. Navigate to Backend Directory

```bash
cd backend
```

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Configure Database

Create `appsettings.Development.json` in `PMSS.API/` directory:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PMSS_DB;Trusted_Connection=true;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

**For SQL Server Express:**
```
Server=localhost\\SQLEXPRESS;Database=PMSS_DB;Trusted_Connection=true;TrustServerCertificate=True
```

**For SQL Server with credentials:**
```
Server=localhost;Database=PMSS_DB;User Id=your_username;Password=your_password;TrustServerCertificate=True
```

### 5. Apply Database Migrations

```bash
cd PMSS.API

# Create migration (if needed)
dotnet ef migrations add InitialCreate --project ../PMSS.Infrastructure

# Apply migration
dotnet ef database update --project ../PMSS.Infrastructure
```

### 6. Run the Application

```bash
dotnet run --project PMSS.API
```

### 7. Access Swagger UI

Open browser and navigate to:
- **HTTPS**: https://localhost:5001/swagger
- **HTTP**: http://localhost:5000/swagger

## Development

### Project Structure

- **PMSS.Domain** - Core business entities and enums
- **PMSS.Application** - DTOs, interfaces, business logic contracts
- **PMSS.Infrastructure** - Database context, repositories, services
- **PMSS.API** - REST API controllers, middleware

### Adding a New Feature

1. **Define Entity** in `PMSS.Domain/Entities/`
2. **Create DTOs** in `PMSS.Application/DTOs/`
3. **Define Interfaces** in `PMSS.Application/Interfaces/`
4. **Implement Repository** in `PMSS.Infrastructure/Repositories/`
5. **Implement Service** in `PMSS.Infrastructure/Services/`
6. **Create Controller** in `PMSS.API/Controllers/`
7. **Register Services** in `ServiceCollectionExtensions.cs`

### Database Commands

```bash
# Create new migration
dotnet ef migrations add MigrationName --project PMSS.Infrastructure --startup-project PMSS.API

# Update database
dotnet ef database update --project PMSS.Infrastructure --startup-project PMSS.API

# Rollback migration
dotnet ef database update PreviousMigrationName --project PMSS.Infrastructure --startup-project PMSS.API

# Remove last migration (if not applied)
dotnet ef migrations remove --project PMSS.Infrastructure --startup-project PMSS.API

# Drop database
dotnet ef database drop --force --project PMSS.Infrastructure --startup-project PMSS.API
```

## API Endpoints

### Core Resources

- **Semesters** - `/api/semesters`
- **Users** - `/api/users`
- **Courses** - `/api/courses`
- **Projects** - `/api/projects`

All endpoints support:
- ✅ Pagination (`pageNumber`, `pageSize`)
- ✅ Sorting (`sortBy`, `sortDescending`)
- ✅ Filtering (resource-specific)
- ✅ Search (`searchTerm`)

### Example Request

```http
GET /api/projects?pageNumber=1&pageSize=10&sortBy=name&courseId=5
```

### Response Format

```json
{
  "success": true,
  "message": "Projects retrieved successfully",
  "data": {
    "items": [...],
    "totalCount": 50,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

## Testing

```bash
# Run all tests (when implemented)
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## Troubleshooting

### Port Already in Use

```bash
# Windows
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

### Database Connection Issues

1. Verify SQL Server is running
2. Check connection string in `appsettings.Development.json`
3. Test connection using SSMS
4. Check Windows Authentication vs SQL Authentication

### Migration Errors

```bash
# Reset migrations
dotnet ef database drop --force
dotnet ef migrations remove
dotnet ef migrations add InitialCreate --project ../PMSS.Infrastructure
dotnet ef database update --project ../PMSS.Infrastructure
```

## Environment Variables

See `.env.example` for configuration options.

## Security Notes

⚠️ **Never commit:**
- `appsettings.Development.json`
- `.env` files with real credentials
- Database connection strings with passwords

✅ **Always:**
- Use environment variables for sensitive data
- Keep `appsettings.json` with placeholder values
- Use User Secrets for local development

## CI/CD

GitHub Actions workflow located in `.github/workflows/`

## Contributing

See [CONTRIBUTING.md](../CONTRIBUTING.md) in the root directory.

## License

[Your License]
