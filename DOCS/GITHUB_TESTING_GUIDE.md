# GitHub Contribution Dashboard - Quick Test Guide

## Prerequisites
1. A project with associated GitHub repositories must exist
2. GitHub repositories must be configured in the database
3. RepoContributor records must be created

## Sample Data Setup

### 1. Create Test Data (SQL)

```sql
-- Create a semester
INSERT INTO Semester (Name, StartDate, EndDate, CreatedAt, UpdatedAt)
VALUES ('Spring 2025', '2025-01-01', '2025-05-31', GETUTCDATE(), GETUTCDATE());

-- Get the semester ID
DECLARE @SemesterId INT = SCOPE_IDENTITY();

-- Create a course (assuming teacher user ID 1 exists)
INSERT INTO Course (SemesterId, Code, Name, TeacherId, CreatedAt, UpdatedAt)
VALUES (@SemesterId, 'CS101', 'Software Engineering', 1, GETUTCDATE(), GETUTCDATE());

-- Get the course ID
DECLARE @CourseId INT = SCOPE_IDENTITY();

-- Create a project
INSERT INTO Project (CourseId, Name, Description, CreatedAt, UpdatedAt)
VALUES (@CourseId, 'Student Management System', 'A system to manage students', GETUTCDATE(), GETUTCDATE());

-- Get the project ID
DECLARE @ProjectId INT = SCOPE_IDENTITY();

-- Add a GitHub repository (public example)
INSERT INTO GithubRepo (ProjectId, RepoOwnerName, RepoName, IsPrivate, CreatedAt, UpdatedAt)
VALUES (@ProjectId, 'microsoft', 'vscode', 0, GETUTCDATE(), GETUTCDATE());
-- Note: Replace 'microsoft/vscode' with actual student repository

-- Get the repo ID
DECLARE @RepoId INT = SCOPE_IDENTITY();

-- Add contributors (these should match actual GitHub usernames)
INSERT INTO RepoContributor (GithubRepoId, GithubUsername, GithubEmail, AddedAt)
VALUES 
    (@RepoId, 'student1', 'student1@example.com', GETUTCDATE()),
    (@RepoId, 'student2', 'student2@example.com', GETUTCDATE());
```

### 2. Add GitHub Access Token (for private repos)

```sql
UPDATE GithubRepo 
SET ApiToken = 'ghp_your_personal_access_token_here'
WHERE RepoOwnerName = 'your_org' AND RepoName = 'your_repo';
```

**How to create a GitHub Personal Access Token:**
1. Go to GitHub ? Settings ? Developer settings ? Personal access tokens ? Tokens (classic)
2. Click "Generate new token (classic)"
3. Give it a name: "PMSS API Access"
4. Select scopes:
   - For public repos: `public_repo`
   - For private repos: `repo`
5. Click "Generate token"
6. Copy the token (starts with `ghp_`) and store it in the database

## API Testing

### Using cURL

```bash
# Test the endpoint
curl -X GET "https://localhost:7001/api/projects/1/github-contributions" \
     -H "accept: application/json" \
     -k

# With authentication (if your API requires it)
curl -X GET "https://localhost:7001/api/projects/1/github-contributions" \
     -H "accept: application/json" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -k
```

### Using PowerShell

```powershell
# Test the endpoint
$response = Invoke-RestMethod -Uri "https://localhost:7001/api/projects/1/github-contributions" -Method GET
$response | ConvertTo-Json -Depth 10

# Pretty print
Write-Host "Project: $($response.data.projectName)"
Write-Host "Semester: $($response.data.semesterStartDate) to $($response.data.semesterEndDate)"
Write-Host "Total Contributors: $($response.data.contributors.Count)"
Write-Host "`nContributors:"
foreach ($contributor in $response.data.contributors) {
    Write-Host "  - $($contributor.githubUsername): $($contributor.totalCommits) commits, +$($contributor.totalAdditions)/-$($contributor.totalDeletions) lines"
}
```

### Using Postman

1. Create a new GET request
2. URL: `https://localhost:7001/api/projects/1/github-contributions`
3. Headers:
   - `Accept: application/json`
   - `Authorization: Bearer YOUR_TOKEN` (if needed)
4. Send the request
5. View the response in the Pretty/Raw/Preview tabs

### Using Swagger UI

1. Start the application
2. Navigate to `https://localhost:7001/swagger`
3. Find the endpoint: `GET /api/projects/{id}/github-contributions`
4. Click "Try it out"
5. Enter the project ID (e.g., `1`)
6. Click "Execute"
7. View the response

## Expected Response Structure

```json
{
  "success": true,
  "message": null,
  "data": {
    "projectId": 1,
    "projectName": "Student Management System",
    "semesterStartDate": "2025-01-01T00:00:00Z",
    "semesterEndDate": "2025-05-31T00:00:00Z",
    "repositories": [
      {
        "githubRepoId": 1,
        "repoOwnerName": "studentorg",
        "repoName": "project-repo",
        "repoUrl": "https://github.com/studentorg/project-repo"
      }
    ],
    "overallCommitsOverTime": [
      {
        "weekStart": "2025-01-01T00:00:00Z",
        "weekEnd": "2025-01-08T00:00:00Z",
        "commitCount": 45
      }
    ],
    "contributors": [
      {
        "githubUsername": "student1",
        "githubEmail": "student1@example.com",
        "userId": 10,
        "userFullName": "John Doe",
        "totalCommits": 171,
        "totalAdditions": 24671,
        "totalDeletions": 9823,
        "weeklyActivity": [
          {
            "weekStart": "2025-01-01T00:00:00Z",
            "weekEnd": "2025-01-08T00:00:00Z",
            "commits": 25,
            "additions": 3500,
            "deletions": 150
          }
        ]
      }
    ]
  },
  "errors": []
}
```

## Troubleshooting

### Error: "Project not found"
- Verify the project ID exists in the database
- Check the URL parameter is correct

### Error: "No GitHub repositories found for this project"
- Verify GithubRepo records exist for the project
- Check the ProjectId foreign key is set correctly

### Error: GitHub API returns 404
- Verify repository owner and name are correct
- Check if repository exists and is accessible
- For private repos, ensure access token is configured

### Error: GitHub API returns 401 Unauthorized
- The access token is invalid or expired
- Generate a new token and update the database

### Error: GitHub API returns 403 Rate Limit Exceeded
- Wait for the rate limit to reset
- Use authenticated requests (access token)
- Implement caching to reduce API calls

### GitHub API returns 202 Accepted
- This is normal - GitHub is computing the statistics
- The service will automatically retry after 1 second
- If it persists, try again in a few minutes

### No data returned / Empty arrays
- The repository might be new with no commits
- Check the semester date range matches the commit dates
- Verify contributors have actually committed to the repository

## Testing with Real GitHub Repositories

### Public Repository Example

```sql
-- Use a public repository for testing
INSERT INTO GithubRepo (ProjectId, RepoOwnerName, RepoName, IsPrivate, CreatedAt, UpdatedAt)
VALUES (1, 'facebook', 'react', 0, GETUTCDATE(), GETUTCDATE());

-- Add some actual contributors
INSERT INTO RepoContributor (GithubRepoId, GithubUsername, AddedAt)
VALUES 
    (SCOPE_IDENTITY(), 'gaearon', GETUTCDATE()),
    (SCOPE_IDENTITY(), 'sophiebits', GETUTCDATE());
```

### Your Own Repository

```sql
-- Use your own repository
INSERT INTO GithubRepo (ProjectId, RepoOwnerName, RepoName, IsPrivate, ApiToken, CreatedAt, UpdatedAt)
VALUES (1, 'your-username', 'your-repo', 1, 'ghp_your_token', GETUTCDATE(), GETUTCDATE());

-- Add yourself as a contributor
INSERT INTO RepoContributor (GithubRepoId, GithubUsername, AddedAt)
VALUES (SCOPE_IDENTITY(), 'your-github-username', GETUTCDATE());
```

## Performance Testing

### Test with Multiple Repositories

```sql
-- Add multiple repositories to a project
INSERT INTO GithubRepo (ProjectId, RepoOwnerName, RepoName, IsPrivate, CreatedAt, UpdatedAt)
VALUES 
    (1, 'owner1', 'repo1', 0, GETUTCDATE(), GETUTCDATE()),
    (1, 'owner2', 'repo2', 0, GETUTCDATE(), GETUTCDATE()),
    (1, 'owner3', 'repo3', 0, GETUTCDATE(), GETUTCDATE());
```

Expected behavior:
- The service will aggregate data from all repositories
- Commits from all repos will be combined in `overallCommitsOverTime`
- Contributor stats will be merged if the same username appears in multiple repos

### Measure Response Time

```powershell
# Measure API response time
Measure-Command {
    Invoke-RestMethod -Uri "https://localhost:7001/api/projects/1/github-contributions" -Method GET
}
```

Typical response times:
- 1 repository: 1-3 seconds
- 3 repositories: 3-8 seconds
- 5+ repositories: Consider implementing caching

## Next Steps

1. **Implement Authorization**
   - Ensure only teachers can view their course projects
   - Add JWT authentication if not already implemented

2. **Add Caching**
   - Cache GitHub API responses for 1 hour
   - Implement cache invalidation strategy

3. **Frontend Integration**
   - Create charts for commit over time
   - Display contributor cards with statistics
   - Add filtering and sorting options

4. **Monitoring**
   - Log GitHub API errors
   - Track API usage and rate limits
   - Monitor response times

5. **Documentation**
   - Document API endpoint in Swagger
   - Create user guide for teachers
   - Add troubleshooting section
