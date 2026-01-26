# GitHub Contribution API - Quick Reference

## Endpoint

```
GET /api/projects/{id}/github-contributions
```

## Response Structure

```typescript
interface ProjectGithubContributionDto {
  projectId: number;
  projectName: string;
  semesterStartDate: string;  // ISO 8601
  semesterEndDate: string;    // ISO 8601
  repositories: RepoContributionDto[];
  overallCommitsOverTime: WeeklyCommitDto[];
  contributors: ContributorStatsDto[];
}

interface RepoContributionDto {
  githubRepoId: number;
  repoOwnerName: string;
  repoName: string;
  repoUrl: string;
}

interface WeeklyCommitDto {
  weekStart: string;    // ISO 8601
  weekEnd: string;      // ISO 8601
  commitCount: number;
}

interface ContributorStatsDto {
  githubUsername: string;
  githubEmail: string | null;
  userId: number | null;
  userFullName: string | null;
  totalCommits: number;
  totalAdditions: number;
  totalDeletions: number;
  weeklyActivity: WeeklyContributorActivityDto[];
}

interface WeeklyContributorActivityDto {
  weekStart: string;    // ISO 8601
  weekEnd: string;      // ISO 8601
  commits: number;
  additions: number;
  deletions: number;
}
```

## Quick Test

### PowerShell
```powershell
Invoke-RestMethod -Uri "https://localhost:7001/api/projects/1/github-contributions"
```

### cURL
```bash
curl -X GET "https://localhost:7001/api/projects/1/github-contributions"
```

### JavaScript/Fetch
```javascript
const response = await fetch('/api/projects/1/github-contributions');
const result = await response.json();
console.log(result.data);
```

## Database Setup

### Add GitHub Repository
```sql
INSERT INTO GithubRepo (ProjectId, RepoOwnerName, RepoName, IsPrivate, ApiToken, CreatedAt, UpdatedAt)
VALUES (1, 'owner', 'repo-name', 0, NULL, GETUTCDATE(), GETUTCDATE());
```

### Add Access Token (Private Repos)
```sql
UPDATE GithubRepo 
SET ApiToken = 'ghp_your_token_here'
WHERE GithubRepoId = 1;
```

### Add Contributors
```sql
INSERT INTO RepoContributor (GithubRepoId, GithubUsername, GithubEmail, UserId, AddedAt)
VALUES (1, 'github-username', 'email@example.com', 10, GETUTCDATE());
```

### Link User to GitHub
```sql
UPDATE RepoContributor
SET UserId = 10
WHERE GithubUsername = 'github-username' AND GithubRepoId = 1;
```

## GitHub Token Setup

1. Go to: https://github.com/settings/tokens
2. Generate new token (classic)
3. Select scopes:
   - `public_repo` - for public repos
   - `repo` - for private repos
4. Copy token (starts with `ghp_`)
5. Store in database `GithubRepo.ApiToken`

## Common Issues

| Issue | Solution |
|-------|----------|
| "Project not found" | Check project ID exists |
| "No GitHub repositories found" | Add repos to `GithubRepo` table |
| GitHub 404 | Verify repo owner/name are correct |
| GitHub 401 | Update access token |
| GitHub 403 | Rate limit exceeded - add token or wait |
| GitHub 202 | API is computing stats - retry |
| Empty data | No commits in semester range |

## Rate Limits

- **With token**: 5,000 requests/hour
- **Without token**: 60 requests/hour
- **Reset**: Every hour at :00

## Response Time

- Single repo: 1-3 seconds
- Multiple repos: 2-8 seconds
- Consider caching for production

## GitHub API Docs

- [Contributor Stats](https://docs.github.com/en/rest/metrics/statistics#get-all-contributor-commit-activity)
- [Commit Activity](https://docs.github.com/en/rest/metrics/statistics#get-the-last-year-of-commit-activity)
- [Authentication](https://docs.github.com/en/rest/overview/authenticating-to-the-rest-api)

## Sample Frontend Code

### React/TypeScript

```tsx
import { useState, useEffect } from 'react';

interface ProjectContributions {
  // ... (see Response Structure above)
}

function useProjectContributions(projectId: number) {
  const [data, setData] = useState<ProjectContributions | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetch(`/api/projects/${projectId}/github-contributions`)
      .then(res => res.json())
      .then(result => {
        if (result.success) {
          setData(result.data);
        } else {
          setError(result.message || 'Failed to fetch');
        }
      })
      .catch(err => setError(err.message))
      .finally(() => setLoading(false));
  }, [projectId]);

  return { data, loading, error };
}

// Usage
function ProjectDashboard({ projectId }: { projectId: number }) {
  const { data, loading, error } = useProjectContributions(projectId);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;
  if (!data) return null;

  return (
    <div>
      <h1>{data.projectName}</h1>
      <h2>Contributors ({data.contributors.length})</h2>
      {data.contributors.map(c => (
        <div key={c.githubUsername}>
          <strong>{c.githubUsername}</strong>
          <p>{c.totalCommits} commits</p>
          <p>+{c.totalAdditions} / -{c.totalDeletions} lines</p>
        </div>
      ))}
    </div>
  );
}
```

### Chart.js Example

```javascript
// Commits over time chart
const ctx = document.getElementById('commitsChart').getContext('2d');
new Chart(ctx, {
  type: 'line',
  data: {
    labels: data.overallCommitsOverTime.map(w => 
      new Date(w.weekStart).toLocaleDateString()
    ),
    datasets: [{
      label: 'Commits per Week',
      data: data.overallCommitsOverTime.map(w => w.commitCount),
      borderColor: 'rgb(75, 192, 192)',
      tension: 0.1
    }]
  }
});

// Contributor comparison chart
const ctx2 = document.getElementById('contributorsChart').getContext('2d');
new Chart(ctx2, {
  type: 'bar',
  data: {
    labels: data.contributors.map(c => c.githubUsername),
    datasets: [
      {
        label: 'Commits',
        data: data.contributors.map(c => c.totalCommits),
        backgroundColor: 'rgba(54, 162, 235, 0.5)'
      },
      {
        label: 'Additions',
        data: data.contributors.map(c => c.totalAdditions),
        backgroundColor: 'rgba(75, 192, 192, 0.5)'
      },
      {
        label: 'Deletions',
        data: data.contributors.map(c => c.totalDeletions),
        backgroundColor: 'rgba(255, 99, 132, 0.5)'
      }
    ]
  }
});
```

## Caching Example (C#)

```csharp
// Add to Startup/Program.cs
builder.Services.AddMemoryCache();

// In ProjectService
private readonly IMemoryCache _cache;

public async Task<ApiResponse<ProjectGithubContributionDto>> GetProjectGithubContributionsAsync(int projectId)
{
    var cacheKey = $"github_contributions_{projectId}";
    
    if (_cache.TryGetValue(cacheKey, out ProjectGithubContributionDto cachedData))
    {
        return ApiResponse<ProjectGithubContributionDto>.SuccessResponse(cachedData);
    }
    
    // Fetch from GitHub...
    var result = await FetchFromGitHub(projectId);
    
    // Cache for 1 hour
    _cache.Set(cacheKey, result.Data, TimeSpan.FromHours(1));
    
    return result;
}
```

## Testing Checklist

- [ ] Project exists in database
- [ ] Course and semester are configured
- [ ] GitHub repositories added to `GithubRepo` table
- [ ] Repository owner/name are correct
- [ ] Access token configured (for private repos)
- [ ] Contributors added to `RepoContributor` table
- [ ] API endpoint returns 200 OK
- [ ] Response contains expected data structure
- [ ] Commits are filtered by semester dates
- [ ] Multiple repositories are aggregated correctly
- [ ] Contributors are ranked by commit count

## Production Deployment

### Environment Variables
```bash
# appsettings.json or environment
"GitHub": {
  "ApiBaseUrl": "https://api.github.com",
  "DefaultRateLimit": 60,
  "AuthenticatedRateLimit": 5000
}
```

### Monitoring
```csharp
// Log GitHub API calls
_logger.LogInformation(
    "Fetching GitHub stats for {Owner}/{Repo}", 
    owner, 
    repo
);

// Track API usage
_telemetry.TrackEvent("GitHubApiCall", new Dictionary<string, string> {
    { "Owner", owner },
    { "Repo", repo },
    { "StatusCode", statusCode.ToString() }
});
```

### Health Check
```csharp
// Add health check endpoint
app.MapGet("/health/github", async (IGithubApiService github) =>
{
    var canConnect = await github.GetRepositoryContributorStatsAsync(
        "octocat", 
        "Hello-World"
    );
    return canConnect != null ? Results.Ok() : Results.ServiceUnavailable();
});
```
