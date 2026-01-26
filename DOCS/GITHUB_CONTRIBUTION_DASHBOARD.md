# GitHub Contribution Dashboard - Implementation Guide

## Overview
This feature allows teachers to view detailed GitHub contribution statistics for student projects within a semester. The system fetches real-time data from GitHub's REST API to display:
- Overall commit activity over time (weekly basis)
- Individual contributor statistics (commits, additions, deletions)
- Weekly contribution breakdown per contributor

## Architecture

### Components Created

1. **DTOs** (`PMSS.Application/DTOs/GithubRepo/GithubRepoDtos.cs`)
   - `ProjectGithubContributionDto` - Main response containing all contribution data
   - `RepoContributionDto` - Repository information
   - `WeeklyCommitDto` - Overall weekly commit statistics
   - `ContributorStatsDto` - Individual contributor statistics
   - `WeeklyContributorActivityDto` - Weekly activity per contributor

2. **GitHub API Service Interface** (`PMSS.Application/Interfaces/Services/IGithubApiService.cs`)
   - `GetRepositoryContributorStatsAsync()` - Fetches contributor statistics from GitHub
   - `GetRepositoryCommitActivityAsync()` - Fetches commit activity timeline from GitHub

3. **GitHub API Service Implementation** (`PMSS.Infrastructure/Services/GithubApiService.cs`)
   - Integrates with GitHub REST API
   - Handles authentication with access tokens
   - Implements retry logic for GitHub's 202 Accepted responses
   - Maps GitHub API responses to internal DTOs

4. **Project Service Enhancement** (`PMSS.Infrastructure/Services/ProjectService.cs`)
   - `GetProjectGithubContributionsAsync()` - Aggregates data from multiple repositories
   - Filters data by semester date range
   - Merges statistics from multiple repositories
   - Links GitHub contributors to system users

5. **API Controller Endpoint** (`PMSS.API/Controllers/ProjectsController.cs`)
   - `GET /api/projects/{id}/github-contributions` - Returns contribution dashboard data

## API Endpoint Usage

### Get Project GitHub Contributions

**Endpoint:** `GET /api/projects/{id}/github-contributions`

**Parameters:**
- `id` (path parameter) - The project ID

**Response:**
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

## GitHub API Integration

### API Endpoints Used

1. **Contributor Statistics**
   - Endpoint: `GET /repos/{owner}/{repo}/stats/contributors`
   - Documentation: https://docs.github.com/en/rest/metrics/statistics#get-all-contributor-commit-activity
   - Returns: Weekly commit counts, additions, and deletions per contributor
   - Note: May return 202 Accepted if data is being computed (requires retry)

2. **Commit Activity**
   - Endpoint: `GET /repos/{owner}/{repo}/stats/commit_activity`
   - Documentation: https://docs.github.com/en/rest/metrics/statistics#get-the-last-year-of-commit-activity
   - Returns: Last 52 weeks of commit activity
   - Note: May return 202 Accepted if data is being computed (requires retry)

### Authentication

The system supports GitHub API authentication through:
- **Personal Access Tokens** - Stored in the `GithubRepo.ApiToken` field
- **No Authentication** - For public repositories (rate-limited to 60 requests/hour)

**Recommended Token Permissions:**
- `repo` scope for private repositories
- `public_repo` scope for public repositories only

### Rate Limiting

GitHub API rate limits:
- **Authenticated requests:** 5,000 requests per hour
- **Unauthenticated requests:** 60 requests per hour

The implementation handles rate limits by:
- Using access tokens when available
- Fetching data on-demand (not cached)
- Consider implementing caching for production use

## Data Flow

1. **Teacher requests project contributions**
   ```
   GET /api/projects/123/github-contributions
   ```

2. **System retrieves project information**
   - Validates project exists
   - Gets associated course and semester
   - Retrieves all GitHub repositories linked to the project

3. **Fetches data from GitHub API** (for each repository)
   - Calls `stats/contributors` for contributor statistics
   - Calls `stats/commit_activity` for overall commit timeline
   - Uses stored access token if available

4. **Aggregates and processes data**
   - Merges statistics from multiple repositories
   - Filters data by semester date range
   - Links GitHub usernames to system users
   - Sorts contributors by total commits

5. **Returns formatted response**
   - Overall commit timeline (weekly)
   - Per-contributor statistics
   - Weekly activity breakdown

## Database Schema

The feature uses existing tables:

```
Project (ProjectId, CourseId, Name)
??? Course (CourseId, SemesterId)
?   ??? Semester (SemesterId, StartDate, EndDate)
??? GithubRepo (GithubRepoId, ProjectId, RepoOwnerName, RepoName, ApiToken)
?   ??? RepoContributor (GithubRepoId, GithubUsername, UserId)
??? User (UserId, Name, GithubUsername)
```

## Setup Instructions

### 1. Configure GitHub Access Tokens

For private repositories, store access tokens in the database:

```sql
UPDATE GithubRepo 
SET ApiToken = 'ghp_your_token_here'
WHERE GithubRepoId = 1;
```

### 2. Link Contributors to Users

Associate GitHub usernames with system users:

```sql
-- Option 1: Automatic matching by username
UPDATE RepoContributor rc
SET UserId = (
    SELECT UserId 
    FROM [User] u 
    WHERE u.GithubUsername = rc.GithubUsername
)
WHERE UserId IS NULL;

-- Option 2: Manual association
UPDATE RepoContributor
SET UserId = 10
WHERE GithubUsername = 'student1' AND GithubRepoId = 1;
```

### 3. Test the Endpoint

```bash
# Using curl
curl -X GET "https://localhost:7001/api/projects/1/github-contributions" \
     -H "accept: application/json"

# Using PowerShell
Invoke-RestMethod -Uri "https://localhost:7001/api/projects/1/github-contributions" -Method GET
```

## Frontend Integration Example

```typescript
async function fetchProjectContributions(projectId: number) {
  const response = await fetch(`/api/projects/${projectId}/github-contributions`);
  const result = await response.json();
  
  if (result.success) {
    const data = result.data;
    
    // Display overall commits chart
    renderCommitsOverTimeChart(data.overallCommitsOverTime);
    
    // Display contributor cards
    data.contributors.forEach(contributor => {
      renderContributorCard({
        username: contributor.githubUsername,
        name: contributor.userFullName,
        commits: contributor.totalCommits,
        additions: contributor.totalAdditions,
        deletions: contributor.totalDeletions,
        weeklyData: contributor.weeklyActivity
      });
    });
  }
}
```

## Production Considerations

### 1. Caching
Implement caching to reduce GitHub API calls:

```csharp
// Example: Cache for 1 hour
services.AddMemoryCache();

// In service:
var cacheKey = $"github_contributions_{projectId}";
if (!_cache.TryGetValue(cacheKey, out ProjectGithubContributionDto result))
{
    result = await FetchFromGitHubAsync(projectId);
    _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
}
```

### 2. Background Jobs
For large projects, consider fetching data asynchronously:

```csharp
// Queue a background job to refresh data
_backgroundJobClient.Enqueue(() => RefreshProjectContributionsAsync(projectId));
```

### 3. Error Handling
- Handle GitHub API rate limit errors (403)
- Handle repository access errors (404, 401)
- Provide fallback for unavailable data
- Log errors for monitoring

### 4. Security
- Never expose access tokens in API responses
- Validate teacher access to project data
- Implement authorization checks
- Sanitize GitHub usernames to prevent injection

## Troubleshooting

### GitHub API Returns 202 Accepted
- This means GitHub is computing the statistics
- The service automatically retries after 1 second
- If it persists, the data may not be available yet

### Rate Limit Exceeded
- Check if access token is configured correctly
- Implement caching to reduce API calls
- Consider using GitHub Apps for higher limits

### Contributor Data Missing
- Ensure RepoContributor records exist
- Verify repository names and owners are correct
- Check if repository is accessible (not deleted)

### Weekly Data Not Matching Semester Range
- GitHub's commit_activity API only provides last 52 weeks
- For older semesters, data may be limited
- Consider storing historical data in your database

## Future Enhancements

1. **Real-time Updates**
   - Implement WebSocket for live updates
   - Use GitHub webhooks for push events

2. **Additional Metrics**
   - Code review statistics
   - Pull request metrics
   - Issue tracking integration

3. **Data Persistence**
   - Store historical statistics in database
   - Enable trend analysis over multiple semesters

4. **Jira Integration**
   - Correlate commits with Jira issues
   - Track feature completion

5. **Enhanced Visualization**
   - Contribution heatmaps
   - Language statistics
   - File change patterns

## API Reference

### GitHub REST API Documentation
- [Get all contributor commit activity](https://docs.github.com/en/rest/metrics/statistics#get-all-contributor-commit-activity)
- [Get the last year of commit activity](https://docs.github.com/en/rest/metrics/statistics#get-the-last-year-of-commit-activity)
- [Authentication](https://docs.github.com/en/rest/overview/authenticating-to-the-rest-api)
- [Rate limiting](https://docs.github.com/en/rest/overview/rate-limits-for-the-rest-api)
