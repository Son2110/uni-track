using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.GithubRepo;
using PMSS.Application.DTOs.Project;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGithubApiService _githubApiService;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(IUnitOfWork unitOfWork, IGithubApiService githubApiService, ILogger<ProjectService> logger)
    {
        _unitOfWork = unitOfWork;
        _githubApiService = githubApiService;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<ProjectDto>>> GetAllProjectsAsync(ProjectFilterParams filterParams)
    {
        try
        {
            _logger.LogInformation("Getting all projects with filters: ClassId={ClassId}, PageNumber={PageNumber}", 
                filterParams.ClassId, filterParams.PageNumber);

            var query = (await _unitOfWork.Projects.GetAllAsync()).AsQueryable();

            if (filterParams.ClassId.HasValue)
                query = query.Where(p => p.ClassId == filterParams.ClassId.Value);

            if (filterParams.CourseId.HasValue)
                query = query.Where(p => p.Class.CourseId == filterParams.CourseId.Value);

            if (filterParams.TeacherId.HasValue)
                query = query.Where(p => p.Class.TeacherId == filterParams.TeacherId.Value);

            if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
                query = query.Where(p => p.Name.Contains(filterParams.SearchTerm) || 
                    (p.Description != null && p.Description.Contains(filterParams.SearchTerm)));

            var totalCount = query.Count();

            query = ApplySorting(query, filterParams.SortBy, filterParams.SortDescending);

            var items = query
                .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .Select(p => MapToDto(p))
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

    public async Task<ApiResponse<ProjectDto>> GetProjectByIdAsync(Guid id)
    {
        try
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project == null)
                return ApiResponse<ProjectDto>.ErrorResponse("Project not found");

            return ApiResponse<ProjectDto>.SuccessResponse(MapToDto(project));
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
            var classEntity = await _unitOfWork.Classes.GetByIdAsync(dto.ClassId);
            if (classEntity == null)
                return ApiResponse<ProjectDto>.ErrorResponse("Class not found");

            var project = new Project
            {
                ClassId = dto.ClassId,
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.Projects.AddAsync(project);
            await _unitOfWork.SaveChangesAsync();

            project = await _unitOfWork.Projects.GetByIdAsync(project.ProjectId);
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
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project == null)
                return ApiResponse<ProjectDto>.ErrorResponse("Project not found");

            project.Name = dto.Name;
            project.Description = dto.Description;
            project.UpdatedAt = DateTime.Now;

            _unitOfWork.Projects.Update(project);
            await _unitOfWork.SaveChangesAsync();

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
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project == null)
                return ApiResponse<bool>.ErrorResponse("Project not found");

            _unitOfWork.Projects.Remove(project);
            await _unitOfWork.SaveChangesAsync();

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
            var projects = await _unitOfWork.Projects.GetAllAsync();
            var project = projects.FirstOrDefault(p => p.ProjectId == projectId);
            
            if (project == null)
                return ApiResponse<ProjectGithubContributionDto>.ErrorResponse("Project not found");

            // Get semester dates through class
            var classes = await _unitOfWork.Classes.GetAllAsync();
            var classEntity = classes.FirstOrDefault(c => c.ClassId == project.ClassId);
            
            if (classEntity == null)
                return ApiResponse<ProjectGithubContributionDto>.ErrorResponse("Class not found");

            var semesters = await _unitOfWork.Semesters.GetAllAsync();
            var semester = semesters.FirstOrDefault(s => s.SemesterId == classEntity.SemesterId);
            
            if (semester == null)
                return ApiResponse<ProjectGithubContributionDto>.ErrorResponse("Semester not found");

            // Get all GitHub repos for this project
            var allRepos = await _unitOfWork.GithubRepos.GetAllAsync();
            var githubRepos = allRepos.Where(r => r.ProjectId == projectId).ToList();

            if (!githubRepos.Any())
                return ApiResponse<ProjectGithubContributionDto>.ErrorResponse("No GitHub repositories found for this project");

            // Get all contributors for these repos
            var allContributors = await _unitOfWork.RepoContributors.GetAllAsync();
            var repoIds = githubRepos.Select(r => r.GithubRepoId).ToList();
            var contributors = allContributors.Where(c => repoIds.Contains(c.GithubRepoId)).ToList();

            // Get user information for contributors
            var allUsers = await _unitOfWork.Users.GetAllAsync();
            var userIds = contributors.Where(c => c.UserId.HasValue).Select(c => c.UserId!.Value).ToList();
            var users = allUsers.Where(u => userIds.Contains(u.UserId)).ToList();

            // Fetch real GitHub data from all repositories
            var allCommitActivity = new Dictionary<long, int>();
            var contributorStatsMap = new Dictionary<string, ContributorStatsDto>();

            foreach (var repo in githubRepos)
            {
                // Fetch commit activity for overall stats
                var commitActivity = await _githubApiService.GetRepositoryCommitActivityAsync(
                    repo.RepoOwnerName, 
                    repo.RepoName, 
                    repo.ApiToken);

                if (commitActivity != null)
                {
                    foreach (var activity in commitActivity)
                    {
                        if (allCommitActivity.ContainsKey(activity.Timestamp))
                            allCommitActivity[activity.Timestamp] += activity.Total;
                        else
                            allCommitActivity[activity.Timestamp] = activity.Total;
                    }
                }

                // Fetch contributor stats
                var contributorStats = await _githubApiService.GetRepositoryContributorStatsAsync(
                    repo.RepoOwnerName, 
                    repo.RepoName, 
                    repo.ApiToken);

                if (contributorStats?.Contributors != null)
                {
                    foreach (var contributor in contributorStats.Contributors)
                    {
                        if (!contributorStatsMap.ContainsKey(contributor.Login))
                        {
                            var repoContributor = contributors.FirstOrDefault(c => 
                                c.GithubUsername.Equals(contributor.Login, StringComparison.OrdinalIgnoreCase));
                            var user = repoContributor != null && repoContributor.UserId.HasValue 
                                ? users.FirstOrDefault(u => u.UserId == repoContributor.UserId.Value)
                                : null;

                            contributorStatsMap[contributor.Login] = new ContributorStatsDto
                            {
                                GithubUsername = contributor.Login,
                                GithubEmail = contributor.Email,
                                UserId = repoContributor?.UserId,
                                UserFullName = user?.Name,
                                TotalCommits = 0,
                                TotalAdditions = 0,
                                TotalDeletions = 0,
                                WeeklyActivity = new List<WeeklyContributorActivityDto>()
                            };
                        }

                        var stats = contributorStatsMap[contributor.Login];
                        stats.TotalCommits += contributor.TotalCommits;
                        stats.TotalAdditions += contributor.TotalAdditions;
                        stats.TotalDeletions += contributor.TotalDeletions;

                        // Merge weekly activity
                        foreach (var week in contributor.Weeks)
                        {
                            var existingWeek = stats.WeeklyActivity.FirstOrDefault(w => 
                                DateTimeOffset.FromUnixTimeSeconds(w.WeekStart.Ticks / TimeSpan.TicksPerSecond).ToUnixTimeSeconds() == week.Timestamp);
                            
                            if (existingWeek != null)
                            {
                                existingWeek.Commits += week.Commits;
                                existingWeek.Additions += week.Additions;
                                existingWeek.Deletions += week.Deletions;
                            }
                            else
                            {
                                var weekDate = DateTimeOffset.FromUnixTimeSeconds(week.Timestamp).DateTime;
                                stats.WeeklyActivity.Add(new WeeklyContributorActivityDto
                                {
                                    WeekStart = weekDate,
                                    WeekEnd = weekDate.AddDays(7),
                                    Commits = week.Commits,
                                    Additions = week.Additions,
                                    Deletions = week.Deletions
                                });
                            }
                        }
                    }
                }
            }

            // Filter by semester date range
            var filteredCommitActivity = allCommitActivity
                .Where(kvp =>
                {
                    var date = DateTimeOffset.FromUnixTimeSeconds(kvp.Key).DateTime;
                    return date >= semester.StartDate && date <= semester.EndDate;
                })
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => new WeeklyCommitDto
                {
                    WeekStart = DateTimeOffset.FromUnixTimeSeconds(kvp.Key).DateTime,
                    WeekEnd = DateTimeOffset.FromUnixTimeSeconds(kvp.Key).DateTime.AddDays(7),
                    CommitCount = kvp.Value
                })
                .ToList();

            // Filter contributor weekly activity by semester date range
            foreach (var contributor in contributorStatsMap.Values)
            {
                contributor.WeeklyActivity = contributor.WeeklyActivity
                    .Where(w => w.WeekStart >= semester.StartDate && w.WeekStart <= semester.EndDate)
                    .OrderBy(w => w.WeekStart)
                    .ToList();
            }

            // Build the response DTO
            var response = new ProjectGithubContributionDto
            {
                ProjectId = project.ProjectId,
                ProjectName = project.Name,
                SemesterStartDate = semester.StartDate,
                SemesterEndDate = semester.EndDate,
                Repositories = githubRepos.Select(r => new RepoContributionDto
                {
                    GithubRepoId = r.GithubRepoId,
                    RepoOwnerName = r.RepoOwnerName,
                    RepoName = r.RepoName,
                    RepoUrl = $"https://github.com/{r.RepoOwnerName}/{r.RepoName}"
                }).ToList(),
                OverallCommitsOverTime = filteredCommitActivity,
                Contributors = contributorStatsMap.Values
                    .OrderByDescending(c => c.TotalCommits)
                    .ToList()
            };

            return ApiResponse<ProjectGithubContributionDto>.SuccessResponse(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<ProjectGithubContributionDto>.ErrorResponse("Error retrieving project GitHub contributions", ex.Message);
        }
    }

    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto
        {
            ProjectId = project.ProjectId,
            ClassId = project.ClassId,
            ClassName = $"{project.Class?.Course?.Code ?? ""} - Section {project.Class?.Section ?? ""}",
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
