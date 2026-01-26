# Project Management Support System (PMSS)

## Overview

A comprehensive ASP.NET Core 10 Web API for managing academic projects with GitHub and Jira integration. Built following Clean Architecture principles and SOLID design patterns.

## Architecture

The solution follows Clean Architecture with clear separation of concerns:

```
PMSS/
??? PMSS.Domain/          # Enterprise business rules
?   ??? Entities/         # Domain entities
?   ??? Enums/           # Domain enumerations
?
??? PMSS.Application/     # Application business rules
?   ??? DTOs/            # Data Transfer Objects
?   ?   ??? Common/      # ApiResponse, PagedResult, PaginationParams
?   ?   ??? Semester/
?   ?   ??? User/
?   ?   ??? Course/
?   ?   ??? Project/
?   ?   ??? ... (other DTOs)
?   ??? Interfaces/
?       ??? Repositories/ # Repository interfaces
?       ??? Services/     # Service interfaces
?
??? PMSS.Infrastructure/  # External concerns
?   ??? Data/
?   ?   ??? ApplicationDbContext.cs
?   ?   ??? Configurations/ # EF Core configurations
?   ??? Repositories/    # Repository implementations
?   ??? Services/        # Service implementations
?   ??? Middleware/      # Exception handling
?   ??? Utilities/       # Password hashing, etc.
?   ??? Extensions/      # Query extensions
?   ??? DependencyInjection/
?
??? PMSS.API/            # Presentation layer
    ??? Controllers/     # API controllers
    ??? Program.cs       # Application entry point
    ??? appsettings.json # Configuration
```

## Core Features

### 1. **Academic Structure Management**
- Semesters
- Courses with teacher assignments
- Student enrollments

### 2. **Project Management**
- Projects linked to courses
- Project members/teams
- Multi-repository support per project

### 3. **GitHub Integration**
- Repository tracking
- Contributor management
- Commit and contribution statistics
- Access request workflow for private repositories

### 4. **Jira Integration**
- Jira configuration per project
- Active/inactive configuration management
- API token encryption support

### 5. **Access Control**
- Role-based system (Student, Teacher, Admin)
- Access request workflow
- Teacher monitoring capabilities

## Key Design Patterns

### SOLID Principles

1. **Single Responsibility**: Each class has one reason to change
   - Controllers handle HTTP concerns
   - Services contain business logic
   - Repositories handle data access

2. **Open/Closed**: Extensible without modification
   - Generic repository pattern
   - Interface-based design
   - Extension methods

3. **Liskov Substitution**: Interfaces enable substitutability
   - `IGenericRepository<T>` base for all repos
   - Service interfaces for dependency injection

4. **Interface Segregation**: Focused interfaces
   - Separate repository interfaces per entity
   - Specific service contracts

5. **Dependency Inversion**: Depend on abstractions
   - Controllers depend on service interfaces
   - Services depend on repository interfaces
   - Infrastructure layer implements abstractions

### Clean Architecture Benefits

- **Independence**: Core business logic independent of frameworks
- **Testability**: Easy to unit test without UI or database
- **Maintainability**: Changes isolated to specific layers
- **Flexibility**: Easy to swap implementations

## API Features

### Pagination with Filter and Sort

All list endpoints support:
- **Filtering**: Entity-specific filters
- **Sorting**: By any property, ascending/descending
- **Pagination**: Page number and page size
- **Search**: Full-text search across relevant fields

**Flow**: Filter ? Sort ? Paginate

Example request:
```
GET /api/projects?pageNumber=1&pageSize=10&sortBy=name&sortDescending=false&courseId=5&searchTerm=web
```

### Standardized API Response

All endpoints return:
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { ... },
  "errors": []
}
```

### Pagination Response

```json
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

## Database Schema

### Key Entities

1. **Semesters** - Academic periods
2. **Users** - Students, teachers, admins
3. **Courses** - Linked to semesters and teachers
4. **CourseEnrollments** - Student enrollments
5. **Projects** - Course projects
6. **ProjectMembers** - Team membership
7. **GithubRepos** - Repository tracking
8. **RepoContributors** - Contributors per repo
9. **JiraConfigs** - Jira integration settings
10. **AccessRequests** - Permission workflow

### Relationships

- One Semester ? Many Courses
- One Teacher ? Many Courses
- One Course ? Many Projects
- One Project ? Many GithubRepos
- One Project ? One Active JiraConfig
- Many-to-Many: Courses ? Students (via CourseEnrollments)
- Many-to-Many: Projects ? Users (via ProjectMembers)

## Setup Instructions

### Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB or Express)
- Visual Studio 2025 or VS Code
- Git

### Installation

1. **Clone the repository**
```bash
git clone <repository-url>
cd PMSS
```

2. **Restore NuGet packages**
```bash
dotnet restore
```

3. **Update connection string**

Edit `PMSS.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PMSS_DB;Trusted_Connection=true"
  }
}
```

4. **Create database migration**
```bash
cd PMSS.API
dotnet ef migrations add InitialCreate --project ../PMSS.Infrastructure --startup-project .
```

5. **Apply migration**
```bash
dotnet ef database update --project ../PMSS.Infrastructure --startup-project .
```

6. **Run the application**
```bash
dotnet run --project PMSS.API
```

7. **Access Swagger UI**
```
https://localhost:5001/swagger
```

## API Endpoints

### Semesters
- `GET /api/semesters` - List all semesters (paginated)
- `GET /api/semesters/{id}` - Get semester by ID
- `POST /api/semesters` - Create semester
- `PUT /api/semesters/{id}` - Update semester
- `DELETE /api/semesters/{id}` - Delete semester

### Users
- `GET /api/users` - List all users (paginated, filterable by role)
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create user
- `PUT /api/users/{id}` - Update user
- `PUT /api/users/{id}/password` - Update password
- `DELETE /api/users/{id}` - Delete user

### Projects
- `GET /api/projects` - List all projects (paginated, filterable by course/teacher)
- `GET /api/projects/{id}` - Get project by ID
- `POST /api/projects` - Create project
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Delete project

*Similar patterns for: Courses, CourseEnrollments, ProjectMembers, GithubRepos, RepoContributors, JiraConfigs, AccessRequests*

## Security Features

### Password Hashing
- Uses PBKDF2 with SHA256
- 10,000 iterations
- Random salt per password
- Cryptographically secure comparison

### API Token Storage
- Encrypted storage recommended (implement encryption service)
- Currently stored as strings (production: use Azure Key Vault or similar)

## Extension Points

### To Add More Services

1. Create service interface in `PMSS.Application/Interfaces/Services/`
2. Implement in `PMSS.Infrastructure/Services/`
3. Register in `ServiceCollectionExtensions.cs`
4. Create controller in `PMSS.API/Controllers/`

### To Add Custom Filters

Extend `PaginationParams` in respective DTO files:
```csharp
public class CustomFilterParams : PaginationParams
{
    public string? CustomField { get; set; }
}
```

### To Add Validation

Install FluentValidation:
```bash
dotnet add package FluentValidation.AspNetCore
```

Create validators in `PMSS.Application/Validators/`

## Testing Recommendations

### Unit Tests
- Test services with mocked repositories
- Test repository logic with in-memory database
- Test DTOs and mappings

### Integration Tests
- Test API endpoints end-to-end
- Use WebApplicationFactory
- Test database operations

### Structure
```
PMSS.Tests/
??? UnitTests/
?   ??? Services/
?   ??? Repositories/
??? IntegrationTests/
?   ??? Controllers/
??? TestHelpers/
```

## Future Enhancements

1. **Authentication & Authorization**
   - JWT tokens
   - Role-based access control
   - OAuth integration with GitHub

2. **GitHub API Integration**
   - Fetch commits automatically
   - Sync contributors
   - Display contribution statistics

3. **Jira API Integration**
   - Fetch requirements
   - Export to SRS documents
   - AI-powered synthesis

4. **Caching**
   - Redis for frequently accessed data
   - Response caching

5. **Logging**
   - Structured logging with Serilog
   - Application Insights integration

6. **Health Checks**
   - Database connectivity
   - External API availability

## Troubleshooting

### Migration Issues
```bash
# Drop database and recreate
dotnet ef database drop --force
dotnet ef database update
```

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet build
```

### Connection Issues
- Verify SQL Server is running
- Check connection string
- Ensure database exists

## Contributing

Follow these principles:
1. Maintain Clean Architecture boundaries
2. Follow existing code patterns
3. Write unit tests for new features
4. Update documentation
5. Use meaningful commit messages

## License

[Your License Here]

## Contact

[Your Contact Information]
