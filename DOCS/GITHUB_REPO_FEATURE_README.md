# GitHub Repository Management Feature

## Overview
This implementation adds comprehensive GitHub repository management functionality for the PMSS (Project Management Student System), allowing **all project members** to create and manage GitHub repositories for their courses with equal permissions.

## Key Features

### 1. **Equal Member Permissions**
- **All project members** have equal permissions to:
  - Create GitHub repositories for their projects
  - Update repository details
  - Delete repositories
  - Add/remove contributors to repositories

### 2. **Multi-Repository Support**
- Each project can have multiple GitHub repositories
- Repositories are linked to projects, which are linked to courses
- Full tracking of repository metadata (owner, name, privacy, creation date)

### 3. **Contributor Management**
- Project members can be added as contributors to repositories
- Users can add themselves to repos (if they are project members)
- Any project member can add/remove any project member
- Full contributor tracking with user information

### 4. **Course-Level Repository Viewing**
- Get all repositories for a specific course
- Aggregated view showing:
  - Total repository count per course
  - All repositories with their details
  - Course information

### 5. **User-Specific Repository Access**
- Users can view all repositories they have access to
- Includes repos where they are:
  - Project members
  - Direct contributors

## API Endpoints

### Repository Management
```
GET    /api/githubrepos                          - Get all repos (with filtering)
GET    /api/githubrepos/{id}                     - Get repo by ID
POST   /api/githubrepos                          - Create repo (members only)
PUT    /api/githubrepos/{id}                     - Update repo (members only)
DELETE /api/githubrepos/{id}                     - Delete repo (members only)
```

### Course & User Queries
```
GET    /api/githubrepos/course/{courseId}        - Get all repos for a course
GET    /api/githubrepos/user/{userId}            - Get all repos for a user
```

### Contributor Management
```
POST   /api/githubrepos/{repoId}/contributors/{userId}    - Add contributor
DELETE /api/githubrepos/{repoId}/contributors/{userId}    - Remove contributor
GET    /api/githubrepos/{repoId}/can-manage?userId=...    - Check permissions
```

## Request/Response Examples

### Create Repository (Any Project Member)
```json
POST /api/githubrepos
Headers: X-User-Id: {memberId}
Body:
{
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "repoOwnerName": "my-organization",
  "repoName": "project-backend",
  "isPrivate": true,
  "apiToken": "ghp_xxxxxxxxxxxx"
}
```

### Add Contributor
```json
POST /api/githubrepos/{repoId}/contributors/{userId}
Headers: X-User-Id: {memberId}
```

### Get Course Repositories
```json
GET /api/githubrepos/course/{courseId}

Response:
{
  "success": true,
  "data": {
    "courseId": "...",
    "courseName": "Software Development",
    "courseCode": "SWD392",
    "totalRepos": 5,
    "repositories": [
      {
        "githubRepoId": "...",
        "projectName": "E-commerce Platform",
        "repoOwnerName": "team-a",
        "repoName": "ecommerce-backend",
        "repoUrl": "https://github.com/team-a/ecommerce-backend",
        "isPrivate": true,
        "contributorCount": 4,
        "contributors": [...]
      }
    ]
  }
}
```

## Database Structure

### Relationships
```
Course ? Class ? Project ? GithubRepo
                  ?         ?
            ProjectMember  RepoContributor
                  ?         ?
                User ????????
```

## Authorization Rules

1. **Create/Update/Delete Repositories**
   - User must be a project member

2. **Add Contributors**
   - Any project member can add any other project member
   - Users can add themselves if they are project members

3. **Remove Contributors**
   - Any project member can remove anyone
   - Users can remove themselves

4. **View Repositories**
   - Anyone can view repositories they have access to
   - Project members can view project repositories
   - Contributors can view repositories they contribute to

## Usage Notes

- The `X-User-Id` header is used for authorization (replace with proper authentication in production)
- Repository URLs are automatically generated as: `https://github.com/{owner}/{name}`
- API tokens are stored securely and not returned in responses
- All timestamps are in UTC

## Migration Required

After implementing these changes, create and run a migration:

```bash
dotnet ef migrations add AddGithubRepoFeature --project PMSS.Infrastructure --startup-project PMSS.API
dotnet ef database update --project PMSS.Infrastructure --startup-project PMSS.API
```

## Files Modified/Created

### Created Files:
1. `PMSS.Infrastructure/Services/GithubRepoService.cs` - Service implementation
2. `PMSS.API/Controllers/GithubReposController.cs` - API controller

### Modified Files:
1. `PMSS.Application/DTOs/GithubRepo/GithubRepoDtos.cs` - Enhanced DTOs
2. `PMSS.Application/Interfaces/Services/IGithubRepoService.cs` - Extended interface
3. `PMSS.Application/Interfaces/Repositories/IGithubRepoRepository.cs` - Added methods
4. `PMSS.Infrastructure/Repositories/GithubRepoRepository.cs` - Implemented methods
5. `PMSS.Infrastructure/Repositories/RepoContributorRepository.cs` - Updated methods
6. `PMSS.Infrastructure/Repositories/ProjectMemberRepository.cs` - Added GetByProjectIdAsync
7. `PMSS.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs` - Registered service
