using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.GithubRepo;
using PMSS.Application.DTOs.Project;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Entities;
using PMSS.Infrastructure.Configuration;
using PMSS.Infrastructure.Utilities;

namespace PMSS.Infrastructure.Services;

public class ProjectService(IUnitOfWork unitOfWork, ILogger<ProjectService> logger, IMapper mapper, IOptions<JwtSettings> jwtSettings) : IProjectService
{
    private readonly string _encryptionKey = jwtSettings.Value.SecretKey;
    private record ProjectData(Project Project, Semester Semester, List<GithubRepo> Repos, List<RepoContributor> Contributors, List<User> Users, List<WeeklyContribution> WeeklyContributions);

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
                .ToList();

            var itemDtos = mapper.Map<List<ProjectDto>>(items);

            var result = new PagedResult<ProjectDto>
            {
                Items = itemDtos,
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
                : ApiResponse<ProjectDto>.SuccessResponse(mapper.Map<ProjectDto>(project));
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

            // Auto-create Jira config if all Jira fields are provided
            if (!string.IsNullOrWhiteSpace(dto.JiraUrl) &&
                !string.IsNullOrWhiteSpace(dto.JiraEmail) &&
                !string.IsNullOrWhiteSpace(dto.JiraApiToken) &&
                !string.IsNullOrWhiteSpace(dto.JiraProjectKey))
            {
                var jiraConfig = new JiraConfig
                {
                    JiraConfigId = Guid.NewGuid(),
                    ProjectId = project.ProjectId,
                    JiraUrl = dto.JiraUrl.TrimEnd('/'),
                    Email = dto.JiraEmail,
                    ApiToken = AesEncryptionHelper.Encrypt(dto.JiraApiToken, _encryptionKey),
                    ProjectKey = dto.JiraProjectKey.ToUpperInvariant(),
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await unitOfWork.JiraConfigs.AddAsync(jiraConfig);
                await unitOfWork.SaveChangesAsync();
                logger.LogInformation("Jira config auto-created for project {ProjectId}", project.ProjectId);
            }

            project = await unitOfWork.Projects.GetByIdAsync(project.ProjectId);
            return ApiResponse<ProjectDto>.SuccessResponse(mapper.Map<ProjectDto>(project!), "Project created successfully");
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
            project.UpdatedAt = DateTime.Now;

            unitOfWork.Projects.Update(project);
            await unitOfWork.SaveChangesAsync();

            return ApiResponse<ProjectDto>.SuccessResponse(mapper.Map<ProjectDto>(project), "Project updated successfully");
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

            var (project, semester, githubRepos, contributors, users, weeklyContributions) = projectDataResult.Data!;

            logger.LogInformation(
                "Retrieving GitHub statistics for project {ProjectId} from database. {RepoCount} repositories. Semester: {SemesterStart} to {SemesterEnd}",
                projectId, githubRepos.Count, semester.StartDate, semester.EndDate);

            // Filter weekly contributions by semester
            var filteredContributions = weeklyContributions
                .Where(wc => IsWithinSemesterPeriod(wc.WeekStart, semester))
                .ToList();

            logger.LogDebug("After filtering by semester: {ContributionCount} weekly contributions",
                filteredContributions.Count);

            // Build the response from database data
            var response = BuildContributionResponseFromDatabase(
                project, semester, githubRepos, contributors, users, filteredContributions);

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

        // Fetch weekly contributions with user contributions from database
        var weeklyContributions = (await unitOfWork.WeeklyContributions.GetWithUserContributionsByRepoIdsAsync(repoIds)).ToList();

        return (true, null, new ProjectData(project, semester, githubRepos, contributors, users, weeklyContributions));
    }


    private static ProjectGithubContributionDto BuildContributionResponseFromDatabase(
        Project project,
        Semester semester,
        List<GithubRepo> githubRepos,
        List<RepoContributor> contributors,
        List<User> users,
        List<WeeklyContribution> weeklyContributions)
    {
        // Build overall commits over time (aggregated across all repos)
        var commitsOverTime = weeklyContributions
            .GroupBy(wc => wc.WeekTimestamp)
            .OrderBy(g => g.Key)
            .Select(g => new WeeklyCommitDto
            {
                WeekStart = g.First().WeekStart,
                WeekEnd = g.First().WeekEnd,
                CommitCount = g.Sum(wc => wc.TotalCommits)
            })
            .ToList();

        // Build contributor stats from UserWeeklyContributions (aggregated across all repos)
        var contributorStatsMap = new Dictionary<string, ContributorStatsDto>(StringComparer.OrdinalIgnoreCase);

        foreach (var weeklyContribution in weeklyContributions)
        {
            foreach (var userContrib in weeklyContribution.UserContributions)
            {
                if (!contributorStatsMap.TryGetValue(userContrib.GithubUsername, out var stats))
                {
                    // Find matching repo contributor and user
                    var repoContributor = contributors.FirstOrDefault(c =>
                        c.GithubUsername.Equals(userContrib.GithubUsername, StringComparison.OrdinalIgnoreCase));
                    var user = userContrib.UserId.HasValue
                        ? users.FirstOrDefault(u => u.UserId == userContrib.UserId.Value)
                        : (repoContributor?.UserId.HasValue == true
                            ? users.FirstOrDefault(u => u.UserId == repoContributor.UserId!.Value)
                            : null);

                    stats = new ContributorStatsDto
                    {
                        GithubUsername = userContrib.GithubUsername,
                        GithubEmail = repoContributor?.GithubEmail,
                        UserId = userContrib.UserId ?? repoContributor?.UserId,
                        UserFullName = user?.Name,
                        TotalCommits = 0,
                        TotalAdditions = 0,
                        TotalDeletions = 0,
                        WeeklyActivity = []
                    };
                    contributorStatsMap[userContrib.GithubUsername] = stats;
                }

                // Check if this week already exists (for aggregation across repos)
                var existingWeek = stats.WeeklyActivity.FirstOrDefault(w => w.WeekStart.Date == weeklyContribution.WeekStart.Date);
                if (existingWeek != null)
                {
                    existingWeek.Commits += userContrib.Commits;
                    existingWeek.Additions += userContrib.Additions;
                    existingWeek.Deletions += userContrib.Deletions;
                }
                else
                {
                    stats.WeeklyActivity.Add(new WeeklyContributorActivityDto
                    {
                        WeekStart = weeklyContribution.WeekStart,
                        WeekEnd = weeklyContribution.WeekEnd,
                        Commits = userContrib.Commits,
                        Additions = userContrib.Additions,
                        Deletions = userContrib.Deletions
                    });
                }

                stats.TotalCommits += userContrib.Commits;
                stats.TotalAdditions += userContrib.Additions;
                stats.TotalDeletions += userContrib.Deletions;
            }
        }

        // Sort weekly activity for each contributor
        foreach (var stats in contributorStatsMap.Values)
        {
            stats.WeeklyActivity = [.. stats.WeeklyActivity.OrderBy(w => w.WeekStart)];
        }

        // Build repository info with totals from cached data
        var repositories = githubRepos.Select(r => new RepoContributionDto
        {
            GithubRepoId = r.GithubRepoId,
            RepoOwnerName = r.RepoOwnerName,
            RepoName = r.RepoName,
            RepoUrl = $"https://github.com/{r.RepoOwnerName}/{r.RepoName}",
            TotalCommits = r.TotalCommits,
            TotalAdditions = r.TotalAdditions,
            TotalDeletions = r.TotalDeletions,
            LastSyncedAt = r.LastSyncedAt
        }).ToList();

        return new ProjectGithubContributionDto
        {
            ProjectId = project.ProjectId,
            ProjectName = project.Name,
            SemesterStartDate = semester.StartDate,
            SemesterEndDate = semester.EndDate,
            TotalCommitsInSemester = commitsOverTime.Sum(w => w.CommitCount),
            TotalAdditionsInSemester = contributorStatsMap.Values.Sum(c => c.TotalAdditions),
            TotalDeletionsInSemester = contributorStatsMap.Values.Sum(c => c.TotalDeletions),
            Repositories = repositories,
            OverallCommitsOverTime = commitsOverTime,
            Contributors = [.. contributorStatsMap.Values
                .Where(c => c.TotalCommits > 0 || c.TotalAdditions > 0 || c.TotalDeletions > 0)
                .OrderByDescending(c => c.TotalCommits)]
        };
    }

    private static bool IsWithinSemesterPeriod(DateTime date, Semester semester) =>
        date >= semester.StartDate && date <= semester.EndDate;

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
