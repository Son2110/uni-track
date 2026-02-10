using Microsoft.Extensions.Logging;
using PMSS.Application.DTOs.Common;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Services;

/// <summary>
/// Service for synchronizing GitHub contribution data from GitHub API to the local database.
/// </summary>
public class GithubDataSyncService(
    IUnitOfWork unitOfWork,
    IGithubApiService githubApiService,
    ILogger<GithubDataSyncService> logger) : IGithubDataSyncService
{
    public async Task<ApiResponse<GithubSyncResultDto>> SyncAllRepositoriesAsync()
    {
        try
        {
            logger.LogInformation("Starting GitHub data sync for all repositories");

            var allRepos = await unitOfWork.GithubRepos.GetAllAsync();
            var repos = allRepos.ToList();

            var result = await SyncRepositoriesInternalAsync(repos);

            logger.LogInformation(
                "GitHub data sync completed: {Success}/{Total} repositories synced successfully",
                result.SuccessfulSyncs, result.TotalRepositories);

            return ApiResponse<GithubSyncResultDto>.SuccessResponse(result, "GitHub data sync completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during GitHub data sync for all repositories");
            return ApiResponse<GithubSyncResultDto>.ErrorResponse("Error syncing GitHub data", ex.Message);
        }
    }

    public async Task<ApiResponse<GithubSyncResultDto>> SyncProjectRepositoriesAsync(Guid projectId)
    {
        try
        {
            logger.LogInformation("Starting GitHub data sync for project {ProjectId}", projectId);

            var project = await unitOfWork.Projects.GetByIdAsync(projectId);
            if (project == null)
                return ApiResponse<GithubSyncResultDto>.ErrorResponse("Project not found");

            var allRepos = await unitOfWork.GithubRepos.GetAllAsync();
            var repos = allRepos.Where(r => r.ProjectId == projectId).ToList();

            if (repos.Count == 0)
                return ApiResponse<GithubSyncResultDto>.ErrorResponse("No GitHub repositories found for this project");

            var result = await SyncRepositoriesInternalAsync(repos);

            logger.LogInformation(
                "GitHub data sync for project {ProjectId} completed: {Success}/{Total} repositories synced",
                projectId, result.SuccessfulSyncs, result.TotalRepositories);

            return ApiResponse<GithubSyncResultDto>.SuccessResponse(result, "Project GitHub data sync completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during GitHub data sync for project {ProjectId}", projectId);
            return ApiResponse<GithubSyncResultDto>.ErrorResponse("Error syncing project GitHub data", ex.Message);
        }
    }

    public async Task<ApiResponse<GithubRepoSyncResultDto>> SyncRepositoryAsync(Guid githubRepoId)
    {
        try
        {
            logger.LogInformation("Starting GitHub data sync for repository {RepoId}", githubRepoId);

            var repo = await unitOfWork.GithubRepos.GetByIdAsync(githubRepoId);
            if (repo == null)
                return ApiResponse<GithubRepoSyncResultDto>.ErrorResponse("Repository not found");

            var result = await SyncSingleRepositoryAsync(repo);

            if (result.Success)
            {
                logger.LogInformation(
                    "GitHub data sync for repository {Owner}/{Repo} completed: {Commits} commits, {Additions} additions, {Deletions} deletions",
                    repo.RepoOwnerName, repo.RepoName, result.TotalCommits, result.TotalAdditions, result.TotalDeletions);
            }
            else
            {
                logger.LogWarning("GitHub data sync for repository {Owner}/{Repo} failed: {Error}",
                    repo.RepoOwnerName, repo.RepoName, result.ErrorMessage);
            }

            return result.Success
                ? ApiResponse<GithubRepoSyncResultDto>.SuccessResponse(result, "Repository sync completed")
                : ApiResponse<GithubRepoSyncResultDto>.ErrorResponse(result.ErrorMessage ?? "Sync failed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during GitHub data sync for repository {RepoId}", githubRepoId);
            return ApiResponse<GithubRepoSyncResultDto>.ErrorResponse("Error syncing repository", ex.Message);
        }
    }

    private async Task<GithubSyncResultDto> SyncRepositoriesInternalAsync(List<GithubRepo> repos)
    {
        var result = new GithubSyncResultDto
        {
            TotalRepositories = repos.Count,
            SyncedAt = DateTime.UtcNow
        };

        foreach (var repo in repos)
        {
            var repoResult = await SyncSingleRepositoryAsync(repo);
            result.RepositoryResults.Add(repoResult);

            if (repoResult.Success)
                result.SuccessfulSyncs++;
            else
                result.FailedSyncs++;
        }

        return result;
    }

    private async Task<GithubRepoSyncResultDto> SyncSingleRepositoryAsync(GithubRepo repo)
    {
        var result = new GithubRepoSyncResultDto
        {
            GithubRepoId = repo.GithubRepoId,
            RepoOwnerName = repo.RepoOwnerName,
            RepoName = repo.RepoName,
            SyncedAt = DateTime.UtcNow
        };

        try
        {
            logger.LogDebug("Fetching contributor stats for {Owner}/{Repo}", repo.RepoOwnerName, repo.RepoName);

            var contributorStats = await githubApiService.GetRepositoryContributorStatsAsync(
                repo.RepoOwnerName, repo.RepoName, repo.ApiToken);

            if (contributorStats?.Contributors == null || contributorStats.Contributors.Count == 0)
            {
                logger.LogWarning("No contributor stats returned for {Owner}/{Repo}", repo.RepoOwnerName, repo.RepoName);
                result.Success = true;
                result.ErrorMessage = "No contributors found";
                return result;
            }

            // Get existing contributors to map GitHub usernames to system users
            var allContributors = await unitOfWork.RepoContributors.GetAllAsync();
            var repoContributors = allContributors.Where(c => c.GithubRepoId == repo.GithubRepoId).ToList();

            var now = DateTime.UtcNow;
            int totalCommits = 0, totalAdditions = 0, totalDeletions = 0;

            // Group contributions by week timestamp to aggregate weekly data
            var weeklyData = new Dictionary<long, (DateTime WeekStart, int Commits, int Additions, int Deletions, List<(string Username, Guid? UserId, int Commits, int Additions, int Deletions)> UserContribs)>();

            foreach (var contributor in contributorStats.Contributors)
            {
                result.ContributorsProcessed++;

                // Find matching system user through repo contributor
                var repoContributor = repoContributors.FirstOrDefault(c =>
                    c.GithubUsername.Equals(contributor.Login, StringComparison.OrdinalIgnoreCase));
                var userId = repoContributor?.UserId;

                foreach (var week in contributor.Weeks)
                {
                    result.WeeksProcessed++;
                    totalCommits += week.Commits;
                    totalAdditions += week.Additions;
                    totalDeletions += week.Deletions;

                    var weekStart = DateTimeOffset.FromUnixTimeSeconds(week.Timestamp).DateTime;

                    if (!weeklyData.ContainsKey(week.Timestamp))
                    {
                        weeklyData[week.Timestamp] = (weekStart, 0, 0, 0, []);
                    }

                    var weekData = weeklyData[week.Timestamp];
                    weekData.Commits += week.Commits;
                    weekData.Additions += week.Additions;
                    weekData.Deletions += week.Deletions;
                    weekData.UserContribs.Add((contributor.Login, userId, week.Commits, week.Additions, week.Deletions));
                    weeklyData[week.Timestamp] = weekData;
                }

                // Update or create repo contributor if not exists
                if (repoContributor == null)
                {
                    var newRepoContributor = new RepoContributor
                    {
                        GithubRepoId = repo.GithubRepoId,
                        GithubUsername = contributor.Login,
                        GithubEmail = contributor.Email,
                        AddedAt = now
                    };
                    await unitOfWork.RepoContributors.AddAsync(newRepoContributor);
                }
            }

            // Process weekly contributions with user data
            foreach (var (weekTimestamp, weekData) in weeklyData)
            {
                var weekEnd = weekData.WeekStart.AddDays(7);

                // Get or create the weekly contribution record
                var weeklyContribution = await unitOfWork.WeeklyContributions.GetByRepoIdAndWeekAsync(repo.GithubRepoId, weekTimestamp);

                if (weeklyContribution == null)
                {
                    weeklyContribution = new WeeklyContribution
                    {
                        WeeklyContributionId = Guid.NewGuid(),
                        GithubRepoId = repo.GithubRepoId,
                        WeekTimestamp = weekTimestamp,
                        WeekStart = weekData.WeekStart,
                        WeekEnd = weekEnd,
                        TotalCommits = weekData.Commits,
                        TotalAdditions = weekData.Additions,
                        TotalDeletions = weekData.Deletions,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    await unitOfWork.WeeklyContributions.AddAsync(weeklyContribution);
                    await unitOfWork.SaveChangesAsync(); // Save to get the ID
                }
                else
                {
                    weeklyContribution.TotalCommits = weekData.Commits;
                    weeklyContribution.TotalAdditions = weekData.Additions;
                    weeklyContribution.TotalDeletions = weekData.Deletions;
                    weeklyContribution.UpdatedAt = now;
                    unitOfWork.WeeklyContributions.Update(weeklyContribution);
                }

                // Process each user's contribution for this week
                foreach (var (username, userId, commits, additions, deletions) in weekData.UserContribs)
                {
                    var userContrib = await unitOfWork.UserWeeklyContributions
                        .GetByWeeklyContributionAndUsernameAsync(weeklyContribution.WeeklyContributionId, username);

                    if (userContrib == null)
                    {
                        userContrib = new UserWeeklyContribution
                        {
                            UserWeeklyContributionId = Guid.NewGuid(),
                            WeeklyContributionId = weeklyContribution.WeeklyContributionId,
                            GithubUsername = username,
                            UserId = userId,
                            Commits = commits,
                            Additions = additions,
                            Deletions = deletions,
                            CreatedAt = now,
                            UpdatedAt = now
                        };
                        await unitOfWork.UserWeeklyContributions.AddAsync(userContrib);
                    }
                    else
                    {
                        userContrib.Commits = commits;
                        userContrib.Additions = additions;
                        userContrib.Deletions = deletions;
                        userContrib.UserId = userId;
                        userContrib.UpdatedAt = now;
                        unitOfWork.UserWeeklyContributions.Update(userContrib);
                    }
                }
            }

            // Update repository totals
            repo.TotalCommits = totalCommits;
            repo.TotalAdditions = totalAdditions;
            repo.TotalDeletions = totalDeletions;
            repo.LastSyncedAt = now;
            repo.UpdatedAt = now;
            unitOfWork.GithubRepos.Update(repo);

            await unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.TotalCommits = totalCommits;
            result.TotalAdditions = totalAdditions;
            result.TotalDeletions = totalDeletions;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error syncing repository {Owner}/{Repo}", repo.RepoOwnerName, repo.RepoName);
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }
}
