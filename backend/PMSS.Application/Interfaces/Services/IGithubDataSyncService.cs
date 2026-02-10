using PMSS.Application.DTOs.Common;

namespace PMSS.Application.Interfaces.Services;

/// <summary>
/// Service interface for synchronizing GitHub contribution data to the local database.
/// This service fetches data from GitHub API and stores it for faster querying.
/// </summary>
public interface IGithubDataSyncService
{
    /// <summary>
    /// Synchronizes GitHub contribution data for all repositories in the database.
    /// </summary>
    Task<ApiResponse<GithubSyncResultDto>> SyncAllRepositoriesAsync();

    /// <summary>
    /// Synchronizes GitHub contribution data for a specific repository.
    /// </summary>
    Task<ApiResponse<GithubRepoSyncResultDto>> SyncRepositoryAsync(Guid githubRepoId);

    /// <summary>
    /// Synchronizes GitHub contribution data for all repositories in a specific project.
    /// </summary>
    Task<ApiResponse<GithubSyncResultDto>> SyncProjectRepositoriesAsync(Guid projectId);
}

public class GithubSyncResultDto
{
    public int TotalRepositories { get; set; }
    public int SuccessfulSyncs { get; set; }
    public int FailedSyncs { get; set; }
    public List<GithubRepoSyncResultDto> RepositoryResults { get; set; } = new();
    public DateTime SyncedAt { get; set; }
}

public class GithubRepoSyncResultDto
{
    public Guid GithubRepoId { get; set; }
    public string RepoOwnerName { get; set; } = string.Empty;
    public string RepoName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int ContributorsProcessed { get; set; }
    public int WeeksProcessed { get; set; }
    public int TotalCommits { get; set; }
    public int TotalAdditions { get; set; }
    public int TotalDeletions { get; set; }
    public DateTime SyncedAt { get; set; }
}
