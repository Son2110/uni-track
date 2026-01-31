using Microsoft.Extensions.Logging;
using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.GithubRepo;
using PMSS.Application.DTOs.Project;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Services;

public class ProjectService(IUnitOfWork unitOfWork, IGithubApiService githubApiService, ILogger<ProjectService> logger) : IProjectService
{
    private record ProjectData(Project Project, Semester Semester, List<GithubRepo> Repos, List<RepoContributor> Contributors, List<User> Users);

    public async Task<ApiResponse<PagedResult<ProjectDto>>> GetAllProjectsAsync(ProjectFilterParams filterParams)
    {
        try
        {
            logger.LogInformation("Getting all projects with filters: ClassId={ClassId}, PageNumber={PageNumber}",
                filterParams.ClassId, filterParams.PageNumber);

            var query = (await unitOfWork.Projects.GetAllAsync()).AsQueryable();
            query = ApplyFilters(query, filterParams);

            var totalCount = query.Count();
            query = ApplySorting(query, filterParams.SortBy, filterParams.SortDescending);

            var items = query
                .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .Select(MapToDto)
                .ToList();

            var result = new PagedResult<ProjectDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filterParams.PageNumber,
                PageSize = filterParams.PageSize
            };

            return ApiResponse<PagedResult<ProjectDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<ProjectDto>>.ErrorResponse("Error retrieving projects", ex.Message);
        }
    }

    private static IQueryable<Project> ApplyFilters(IQueryable<Project> query, ProjectFilterParams filterParams)
    {
        if (filterParams.ClassId.HasValue)
            query = query.Where(p => p.ClassId == filterParams.ClassId.Value);

        if (filterParams.CourseId.HasValue)
            query = query.Where(p => p.Class.CourseId == filterParams.CourseId.Value);

        if (filterParams.TeacherId.HasValue)
            query = query.Where(p => p.Class.TeacherId == filterParams.TeacherId.Value);

        if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
            query = query.Where(p => p.Name.Contains(filterParams.SearchTerm) ||
                (p.Description != null && p.Description.Contains(filterParams.SearchTerm)));

        return query;
    }

    public async Task<ApiResponse<ProjectDto>> GetProjectByIdAsync(Guid id)
    {
        try
        {
            var project = await unitOfWork.Projects.GetByIdAsync(id);
            return project == null
                ? ApiResponse<ProjectDto>.ErrorResponse("Project not found")
                : ApiResponse<ProjectDto>.SuccessResponse(MapToDto(project));
        }
        catch (Exception ex)
        {
            return ApiResponse<ProjectDto>.ErrorResponse("Error retrieving project", ex.Message);
        }
    }

    public async Task<ApiResponse<ProjectDto>> CreateProjectAsync(CreateProjectDto dto)
    {
        try
        {
            var classEntity = await unitOfWork.Classes.GetByIdAsync(dto.ClassId);
            if (classEntity == null)
                return ApiResponse<ProjectDto>.ErrorResponse("Class not found");

            var now = DateTime.UtcNow;
            var project = new Project
            {
                ClassId = dto.ClassId,
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = now,
                UpdatedAt = now
            };

            await unitOfWork.Projects.AddAsync(project);
            await unitOfWork.SaveChangesAsync();

            project = await unitOfWork.Projects.GetByIdAsync(project.ProjectId);
            return ApiResponse<ProjectDto>.SuccessResponse(MapToDto(project!), "Project created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProjectDto>.ErrorResponse("Error creating project", ex.Message);
        }
    }

    public async Task<ApiResponse<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectDto dto)
    {
        try
        {
            var project = await unitOfWork.Projects.GetByIdAsync(id);
            if (project == null)
                return ApiResponse<ProjectDto>.ErrorResponse("Project not found");

            project.Name = dto.Name;
            project.Description = dto.Description;
            project.UpdatedAt = DateTime.UtcNow;

            unitOfWork.Projects.Update(project);
            await unitOfWork.SaveChangesAsync();

            return ApiResponse<ProjectDto>.SuccessResponse(MapToDto(project), "Project updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<ProjectDto>.ErrorResponse("Error updating project", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteProjectAsync(Guid id)
    {
        try
        {
            var project = await unitOfWork.Projects.GetByIdAsync(id);
            if (project == null)
                return ApiResponse<bool>.ErrorResponse("Project not found");

            unitOfWork.Projects.Remove(project);
            await unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Project deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse("Error deleting project", ex.Message);
        }
    }

    public async Task<ApiResponse<ProjectGithubContributionDto>> GetProjectGithubContributionsAsync(Guid projectId)
    {
        try
        {
            var projectDataResult = await GetProjectDataAsync(projectId);
            if (!projectDataResult.IsSuccess)
                return ApiResponse<ProjectGithubContributionDto>.ErrorResponse(projectDataResult.ErrorMessage!);

            var (project, semester, githubRepos, contributors, users) = projectDataResult.Data!;

            logger.LogInformation(
                "Fetching GitHub statistics for project {ProjectId} across {RepoCount} repositories. Semester: {SemesterStart} to {SemesterEnd}",
                projectId, githubRepos.Count, semester.StartDate, semester.EndDate);

            var (allCommitActivity, allContributorStats) = await FetchAllGitHubDataAsync(githubRepos, contributors, users);

            logger.LogDebug("Raw GitHub data: {CommitWeeks} commit weeks, {ContributorCount} contributors",
                allCommitActivity.Count, allContributorStats.Count);

            var filteredCommitActivity = FilterCommitActivityBySemester(allCommitActivity, semester);
            var filteredContributorStats = FilterContributorStatsBySemester(allContributorStats, semester);

            logger.LogDebug("After filtering: {CommitWeeks} commit weeks, {ContributorCount} contributors",
                filteredCommitActivity.Count, filteredContributorStats.Count);

            SortContributorWeeklyActivity(filteredContributorStats);

            var response = BuildContributionResponse(project, semester, githubRepos, filteredCommitActivity, filteredContributorStats);

            logger.LogInformation(
                "GitHub contributions for project {ProjectId}: {TotalCommits} commits, {TotalAdditions} additions, {TotalDeletions} deletions",
                projectId, response.TotalCommitsInSemester, response.TotalAdditionsInSemester, response.TotalDeletionsInSemester);

            return ApiResponse<ProjectGithubContributionDto>.SuccessResponse(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving GitHub contributions for project {ProjectId}", projectId);
            return ApiResponse<ProjectGithubContributionDto>.ErrorResponse("Error retrieving project GitHub contributions", ex.Message);
        }
    }

    private async Task<(bool IsSuccess, string? ErrorMessage, ProjectData? Data)> GetProjectDataAsync(Guid projectId)
    {
        var project = await unitOfWork.Projects.FirstOrDefaultAsync(p => p.ProjectId == projectId);
        if (project == null)
            return (false, "Project not found", null);

        var classEntity = await unitOfWork.Classes.FirstOrDefaultAsync(c => c.ClassId == project.ClassId);
        if (classEntity == null)
            return (false, "Class not found", null);

        var semester = await unitOfWork.Semesters.FirstOrDefaultAsync(s => s.SemesterId == classEntity.SemesterId);
        if (semester == null)
            return (false, "Semester not found", null);

        var allRepos = await unitOfWork.GithubRepos.GetAllAsync();
        var githubRepos = allRepos.Where(r => r.ProjectId == projectId).ToList();
        if (githubRepos.Count == 0)
            return (false, "No GitHub repositories found for this project", null);

        var repoIds = githubRepos.Select(r => r.GithubRepoId).ToHashSet();
        var allContributors = await unitOfWork.RepoContributors.GetAllAsync();
        var contributors = allContributors.Where(c => repoIds.Contains(c.GithubRepoId)).ToList();

        var userIds = contributors.Where(c => c.UserId.HasValue).Select(c => c.UserId!.Value).ToHashSet();
        var allUsers = await unitOfWork.Users.GetAllAsync();
        var users = allUsers.Where(u => userIds.Contains(u.UserId)).ToList();

        return (true, null, new ProjectData(project, semester, githubRepos, contributors, users));
    }

    private async Task<(Dictionary<long, int> CommitActivity, Dictionary<string, ContributorStatsDto> ContributorStats)> FetchAllGitHubDataAsync(
        List<GithubRepo> githubRepos,
        List<RepoContributor> contributors,
        List<User> users)
    {
        var allCommitActivity = new Dictionary<long, int>();
        var contributorStatsMap = new Dictionary<string, ContributorStatsDto>(StringComparer.OrdinalIgnoreCase);

        foreach (var repo in githubRepos)
        {
            try
            {
                logger.LogDebug("Fetching GitHub data for repo: {Owner}/{Repo}", repo.RepoOwnerName, repo.RepoName);
                await AggregateCommitActivityAsync(repo, allCommitActivity);
                await AggregateContributorStatsAsync(repo, contributors, users, contributorStatsMap);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to fetch GitHub data for repo {Owner}/{Repo}", repo.RepoOwnerName, repo.RepoName);
            }
        }

        return (allCommitActivity, contributorStatsMap);
    }

    private async Task AggregateCommitActivityAsync(GithubRepo repo, Dictionary<long, int> commitActivity)
    {
        var repoActivity = await githubApiService.GetRepositoryCommitActivityAsync(
            repo.RepoOwnerName, repo.RepoName, repo.ApiToken);

        if (repoActivity is not { Count: > 0 })
        {
            logger.LogWarning("No commit activity for repo {Owner}/{Repo}", repo.RepoOwnerName, repo.RepoName);
            return;
        }

        foreach (var activity in repoActivity)
        {
            commitActivity.TryGetValue(activity.Timestamp, out var existing);
            commitActivity[activity.Timestamp] = existing + activity.Total;
        }
    }

    private async Task AggregateContributorStatsAsync(
        GithubRepo repo,
        List<RepoContributor> contributors,
        List<User> users,
        Dictionary<string, ContributorStatsDto> contributorStatsMap)
    {
        var repoStats = await githubApiService.GetRepositoryContributorStatsAsync(
            repo.RepoOwnerName, repo.RepoName, repo.ApiToken);

        if (repoStats?.Contributors is not { Count: > 0 })
        {
            logger.LogWarning("No contributor stats for repo {Owner}/{Repo}", repo.RepoOwnerName, repo.RepoName);
            return;
        }

        foreach (var contributor in repoStats.Contributors)
        {
            var stats = GetOrCreateContributorStats(contributor, contributors, users, contributorStatsMap);
            AggregateWeeklyActivity(contributor, stats);
        }
    }

    private static void AggregateWeeklyActivity(GithubContributor contributor, ContributorStatsDto stats)
    {
        foreach (var week in contributor.Weeks)
        {
            var weekDate = DateTimeOffset.FromUnixTimeSeconds(week.Timestamp).UtcDateTime;
            var existingWeek = stats.WeeklyActivity.FirstOrDefault(w => w.WeekStart.Date == weekDate.Date);

            if (existingWeek != null)
            {
                existingWeek.Commits += week.Commits;
                existingWeek.Additions += week.Additions;
                existingWeek.Deletions += week.Deletions;
            }
            else
            {
                stats.WeeklyActivity.Add(new WeeklyContributorActivityDto
                {
                    WeekStart = weekDate,
                    WeekEnd = weekDate.AddDays(7),
                    Commits = week.Commits,
                    Additions = week.Additions,
                    Deletions = week.Deletions
                });
            }

            stats.TotalCommits += week.Commits;
            stats.TotalAdditions += week.Additions;
            stats.TotalDeletions += week.Deletions;
        }
    }

    private static Dictionary<long, int> FilterCommitActivityBySemester(Dictionary<long, int> allCommitActivity, Semester semester)
    {
        return allCommitActivity
            .Where(kvp => IsWithinSemesterPeriod(DateTimeOffset.FromUnixTimeSeconds(kvp.Key).UtcDateTime, semester))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private static Dictionary<string, ContributorStatsDto> FilterContributorStatsBySemester(
        Dictionary<string, ContributorStatsDto> allContributorStats,
        Semester semester)
    {
        var filtered = new Dictionary<string, ContributorStatsDto>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, contributor) in allContributorStats)
        {
            var filteredWeeks = contributor.WeeklyActivity
                .Where(w => IsWithinSemesterPeriod(w.WeekStart, semester))
                .ToList();

            if (filteredWeeks.Count == 0 && contributor.WeeklyActivity.Count > 0)
                continue;

            var filteredStats = new ContributorStatsDto
            {
                GithubUsername = contributor.GithubUsername,
                GithubEmail = contributor.GithubEmail,
                UserId = contributor.UserId,
                UserFullName = contributor.UserFullName,
                TotalCommits = filteredWeeks.Sum(w => w.Commits),
                TotalAdditions = filteredWeeks.Sum(w => w.Additions),
                TotalDeletions = filteredWeeks.Sum(w => w.Deletions),
                WeeklyActivity = filteredWeeks
            };

            if (filteredStats.TotalCommits > 0 || filteredStats.TotalAdditions > 0 || filteredStats.TotalDeletions > 0)
                filtered[key] = filteredStats;
        }

        return filtered;
    }

    private static ContributorStatsDto GetOrCreateContributorStats(
        GithubContributor contributor,
        List<RepoContributor> contributors,
        List<User> users,
        Dictionary<string, ContributorStatsDto> contributorStatsMap)
    {
        if (contributorStatsMap.TryGetValue(contributor.Login, out var existingStats))
            return existingStats;

        var repoContributor = contributors.FirstOrDefault(c =>
            c.GithubUsername.Equals(contributor.Login, StringComparison.OrdinalIgnoreCase));
        var user = repoContributor?.UserId.HasValue == true
            ? users.FirstOrDefault(u => u.UserId == repoContributor.UserId!.Value)
            : null;

        var newStats = new ContributorStatsDto
        {
            GithubUsername = contributor.Login,
            GithubEmail = contributor.Email,
            UserId = repoContributor?.UserId,
            UserFullName = user?.Name,
            TotalCommits = 0,
            TotalAdditions = 0,
            TotalDeletions = 0,
            WeeklyActivity = []
        };

        contributorStatsMap[contributor.Login] = newStats;
        return newStats;
    }

    private static bool IsWithinSemesterPeriod(DateTime date, Semester semester) =>
        date >= semester.StartDate && date <= semester.EndDate;

    private static void SortContributorWeeklyActivity(Dictionary<string, ContributorStatsDto> contributorStats)
    {
        foreach (var contributor in contributorStats.Values)
        {
            contributor.WeeklyActivity = [.. contributor.WeeklyActivity.OrderBy(w => w.WeekStart)];
        }
    }

    private static ProjectGithubContributionDto BuildContributionResponse(
        Project project,
        Semester semester,
        List<GithubRepo> githubRepos,
        Dictionary<long, int> commitActivity,
        Dictionary<string, ContributorStatsDto> contributorStats)
    {
        var overallCommitsOverTime = commitActivity
            .OrderBy(kvp => kvp.Key)
            .Select(kvp =>
            {
                var weekStart = DateTimeOffset.FromUnixTimeSeconds(kvp.Key).UtcDateTime;
                return new WeeklyCommitDto
                {
                    WeekStart = weekStart,
                    WeekEnd = weekStart.AddDays(7),
                    CommitCount = kvp.Value
                };
            })
            .ToList();

        return new ProjectGithubContributionDto
        {
            ProjectId = project.ProjectId,
            ProjectName = project.Name,
            SemesterStartDate = semester.StartDate,
            SemesterEndDate = semester.EndDate,
            TotalCommitsInSemester = overallCommitsOverTime.Sum(w => w.CommitCount),
            TotalAdditionsInSemester = contributorStats.Values.Sum(c => c.TotalAdditions),
            TotalDeletionsInSemester = contributorStats.Values.Sum(c => c.TotalDeletions),
            Repositories = githubRepos.Select(r => new RepoContributionDto
            {
                GithubRepoId = r.GithubRepoId,
                RepoOwnerName = r.RepoOwnerName,
                RepoName = r.RepoName,
                RepoUrl = $"https://github.com/{r.RepoOwnerName}/{r.RepoName}"
            }).ToList(),
            OverallCommitsOverTime = overallCommitsOverTime,
            Contributors = [.. contributorStats.Values.OrderByDescending(c => c.TotalCommits)]
        };
    }




    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto
        {
            ProjectId = project.ProjectId,
            ClassId = project.ClassId,
            ClassName = $"{project.Class?.Course?.Code ?? ""} - Section {project.Class?.ClassCode ?? ""}",
            CourseCode = project.Class?.Course?.Code ?? string.Empty,
            CourseName = project.Class?.Course?.Name ?? string.Empty,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }

    private static IQueryable<Project> ApplySorting(IQueryable<Project> query, string? sortBy, bool descending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query.OrderByDescending(p => p.CreatedAt);

        return sortBy.ToLower() switch
        {
            "name" => descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "createdat" => descending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            "updatedat" => descending ? query.OrderByDescending(p => p.UpdatedAt) : query.OrderBy(p => p.UpdatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };
    }
}
