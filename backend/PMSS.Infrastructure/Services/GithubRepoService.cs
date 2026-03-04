using AutoMapper;
using Microsoft.Extensions.Logging;
using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.GithubRepo;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Services;

public class GithubRepoService : IGithubRepoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GithubRepoService> _logger;
    private readonly IGithubApiService _githubApiService;
    private readonly IMapper _mapper;

    public GithubRepoService(IUnitOfWork unitOfWork, ILogger<GithubRepoService> logger, IGithubApiService githubApiService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _githubApiService = githubApiService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResult<GithubRepoDto>>> GetAllReposAsync(GithubRepoFilterParams filterParams)
    {
        try
        {
            _logger.LogInformation("Getting all Github repos with filters: ProjectId={ProjectId}, CourseId={CourseId}, UserId={UserId}",
                filterParams.ProjectId, filterParams.CourseId, filterParams.UserId);

            var query = (await _unitOfWork.GithubRepos.GetAllWithDetailsAsync()).AsQueryable();

            if (filterParams.ProjectId.HasValue)
                query = query.Where(gr => gr.ProjectId == filterParams.ProjectId.Value);

            if (filterParams.CourseId.HasValue)
                query = query.Where(gr => gr.Project.Class.CourseId == filterParams.CourseId.Value);

            if (filterParams.UserId.HasValue)
                query = query.Where(gr => gr.Project.ProjectMembers.Any(pm => pm.UserId == filterParams.UserId.Value) ||
                                         gr.RepoContributors.Any(rc => rc.UserId == filterParams.UserId.Value));

            if (!string.IsNullOrWhiteSpace(filterParams.RepoOwnerName))
                query = query.Where(gr => gr.RepoOwnerName.Contains(filterParams.RepoOwnerName));

            if (filterParams.IsPrivate.HasValue)
                query = query.Where(gr => gr.IsPrivate == filterParams.IsPrivate.Value);

            if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
                query = query.Where(gr => gr.RepoName.Contains(filterParams.SearchTerm) ||
                                         gr.RepoOwnerName.Contains(filterParams.SearchTerm) ||
                                         gr.Project.Name.Contains(filterParams.SearchTerm));

            var totalCount = query.Count();

            query = ApplySorting(query, filterParams.SortBy, filterParams.SortDescending);

            var items = query
                .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .ToList();

            var itemDtos = _mapper.Map<List<GithubRepoDto>>(items);

            var result = new PagedResult<GithubRepoDto>
            {
                Items = itemDtos,
                TotalCount = totalCount,
                PageNumber = filterParams.PageNumber,
                PageSize = filterParams.PageSize
            };

            return ApiResponse<PagedResult<GithubRepoDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Github repos");
            return ApiResponse<PagedResult<GithubRepoDto>>.ErrorResponse("Error retrieving Github repos", ex.Message);
        }
    }

    public async Task<ApiResponse<GithubRepoDto>> GetRepoByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting Github repo by id: {RepoId}", id);

            var repo = await _unitOfWork.GithubRepos.GetRepoWithDetailsAsync(id);
            if (repo == null)
                return ApiResponse<GithubRepoDto>.ErrorResponse("Github repository not found");

            return ApiResponse<GithubRepoDto>.SuccessResponse(_mapper.Map<GithubRepoDto>(repo));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Github repo with id: {RepoId}", id);
            return ApiResponse<GithubRepoDto>.ErrorResponse("Error retrieving Github repository", ex.Message);
        }
    }

    public async Task<ApiResponse<GithubRepoDto>> CreateRepoAsync(CreateGithubRepoDto dto, Guid createdByUserId)
    {
        try
        {
            _logger.LogInformation("Creating Github repo for project: {ProjectId} by user: {UserId}", dto.ProjectId, createdByUserId);

            var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId);
            if (project == null)
                return ApiResponse<GithubRepoDto>.ErrorResponse("Project not found");

            var canManage = await CanUserManageProjectAsync(dto.ProjectId, createdByUserId);
            if (!canManage)
                return ApiResponse<GithubRepoDto>.ErrorResponse("You do not have permission to create repositories for this project. Only project members can manage repositories.");

            var existingRepo = await _unitOfWork.GithubRepos.GetByOwnerAndNameAsync(dto.RepoOwnerName, dto.RepoName);
            if (existingRepo != null)
                return ApiResponse<GithubRepoDto>.ErrorResponse("A repository with this owner and name already exists");

            var repo = new GithubRepo
            {
                ProjectId = dto.ProjectId,
                RepoOwnerName = dto.RepoOwnerName,
                RepoName = dto.RepoName,
                IsPrivate = dto.IsPrivate,
                ApiToken = dto.ApiToken,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.GithubRepos.AddAsync(repo);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Github repo created successfully with id: {RepoId}", repo.GithubRepoId);

            var createdRepo = await _unitOfWork.GithubRepos.GetRepoWithDetailsAsync(repo.GithubRepoId);
            return ApiResponse<GithubRepoDto>.SuccessResponse(_mapper.Map<GithubRepoDto>(createdRepo!), "Github repository created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Github repo");
            return ApiResponse<GithubRepoDto>.ErrorResponse("Error creating Github repository", ex.Message);
        }
    }

    public async Task<ApiResponse<GithubRepoDto>> UpdateRepoAsync(Guid id, UpdateGithubRepoDto dto, Guid updatedByUserId)
    {
        try
        {
            _logger.LogInformation("Updating Github repo: {RepoId} by user: {UserId}", id, updatedByUserId);

            var repo = await _unitOfWork.GithubRepos.GetRepoWithDetailsAsync(id);
            if (repo == null)
                return ApiResponse<GithubRepoDto>.ErrorResponse("Github repository not found");

            var canManage = await CanUserManageProjectAsync(repo.ProjectId, updatedByUserId);
            if (!canManage)
                return ApiResponse<GithubRepoDto>.ErrorResponse("You do not have permission to update this repository. Only project members can manage repositories.");

            if (dto.RepoOwnerName != repo.RepoOwnerName || dto.RepoName != repo.RepoName)
            {
                var existingRepo = await _unitOfWork.GithubRepos.GetByOwnerAndNameAsync(dto.RepoOwnerName, dto.RepoName);
                if (existingRepo != null && existingRepo.GithubRepoId != id)
                    return ApiResponse<GithubRepoDto>.ErrorResponse("A repository with this owner and name already exists");
            }

            repo.RepoOwnerName = dto.RepoOwnerName;
            repo.RepoName = dto.RepoName;
            repo.IsPrivate = dto.IsPrivate;
            repo.ApiToken = dto.ApiToken;
            repo.UpdatedAt = DateTime.Now;

            _unitOfWork.GithubRepos.Update(repo);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Github repo updated successfully: {RepoId}", id);

            var updatedRepo = await _unitOfWork.GithubRepos.GetRepoWithDetailsAsync(id);
            return ApiResponse<GithubRepoDto>.SuccessResponse(_mapper.Map<GithubRepoDto>(updatedRepo!), "Github repository updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Github repo: {RepoId}", id);
            return ApiResponse<GithubRepoDto>.ErrorResponse("Error updating Github repository", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteRepoAsync(Guid id, Guid deletedByUserId)
    {
        try
        {
            _logger.LogInformation("Deleting Github repo: {RepoId} by user: {UserId}", id, deletedByUserId);

            var repo = await _unitOfWork.GithubRepos.GetRepoWithDetailsAsync(id);
            if (repo == null)
                return ApiResponse<bool>.ErrorResponse("Github repository not found");

            var canManage = await CanUserManageProjectAsync(repo.ProjectId, deletedByUserId);
            if (!canManage)
                return ApiResponse<bool>.ErrorResponse("You do not have permission to delete this repository. Only project members can manage repositories.");

            _unitOfWork.GithubRepos.Remove(repo);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Github repo deleted successfully: {RepoId}", id);

            return ApiResponse<bool>.SuccessResponse(true, "Github repository deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Github repo: {RepoId}", id);
            return ApiResponse<bool>.ErrorResponse("Error deleting Github repository", ex.Message);
        }
    }

    public async Task<ApiResponse<CourseGithubReposDto>> GetReposByCourseIdAsync(Guid courseId)
    {
        try
        {
            _logger.LogInformation("Getting Github repos for course: {CourseId}", courseId);

            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null)
                return ApiResponse<CourseGithubReposDto>.ErrorResponse("Course not found");

            var repos = await _unitOfWork.GithubRepos.GetReposByCourseIdAsync(courseId);

            var result = new CourseGithubReposDto
            {
                CourseId = course.CourseId,
                CourseName = course.Name,
                CourseCode = course.Code,
                TotalRepos = repos.Count(),
                Repositories = _mapper.Map<List<GithubRepoDto>>(repos)
            };

            return ApiResponse<CourseGithubReposDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Github repos for course: {CourseId}", courseId);
            return ApiResponse<CourseGithubReposDto>.ErrorResponse("Error retrieving Github repositories for course", ex.Message);
        }
    }

    public async Task<ApiResponse<List<GithubRepoDto>>> GetReposByUserIdAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Getting Github repos for user: {UserId}", userId);

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse<List<GithubRepoDto>>.ErrorResponse("User not found");

            var repos = await _unitOfWork.GithubRepos.GetReposByUserIdAsync(userId);
            var repoDtos = _mapper.Map<List<GithubRepoDto>>(repos);

            return ApiResponse<List<GithubRepoDto>>.SuccessResponse(repoDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Github repos for user: {UserId}", userId);
            return ApiResponse<List<GithubRepoDto>>.ErrorResponse("Error retrieving user's Github repositories", ex.Message);
        }
    }

    public async Task<ApiResponse<GithubRepoDto>> AddContributorToRepoAsync(Guid repoId, Guid userId, Guid addedByUserId)
    {
        try
        {
            _logger.LogInformation("Adding contributor {UserId} to repo {RepoId} by user {AddedByUserId}", userId, repoId, addedByUserId);

            var repo = await _unitOfWork.GithubRepos.GetRepoWithDetailsAsync(repoId);
            if (repo == null)
                return ApiResponse<GithubRepoDto>.ErrorResponse("Github repository not found");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse<GithubRepoDto>.ErrorResponse("User not found");

            var isProjectMember = repo.Project.ProjectMembers.Any(pm => pm.UserId == userId);
            if (!isProjectMember)
                return ApiResponse<GithubRepoDto>.ErrorResponse("User must be a member of the project to be added as a contributor");

            var canManage = await CanUserManageProjectAsync(repo.ProjectId, addedByUserId);
            if (!canManage && addedByUserId != userId)
                return ApiResponse<GithubRepoDto>.ErrorResponse("You do not have permission to add contributors to this repository");

            var existingContributor = repo.RepoContributors.FirstOrDefault(rc => rc.UserId == userId);
            if (existingContributor != null)
                return ApiResponse<GithubRepoDto>.ErrorResponse("User is already a contributor to this repository");

            var contributor = new RepoContributor
            {
                GithubRepoId = repoId,
                UserId = userId,
                GithubUsername = user.GithubUsername ?? user.Email,
                GithubEmail = user.GithubEmail ?? user.Email,
                AddedAt = DateTime.Now
            };

            await _unitOfWork.RepoContributors.AddAsync(contributor);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Contributor {UserId} added successfully to repo {RepoId}", userId, repoId);

            var updatedRepo = await _unitOfWork.GithubRepos.GetRepoWithDetailsAsync(repoId);
            return ApiResponse<GithubRepoDto>.SuccessResponse(_mapper.Map<GithubRepoDto>(updatedRepo!), "Contributor added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding contributor to Github repo");
            return ApiResponse<GithubRepoDto>.ErrorResponse("Error adding contributor to repository", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> RemoveContributorFromRepoAsync(Guid repoId, Guid userId, Guid removedByUserId)
    {
        try
        {
            _logger.LogInformation("Removing contributor {UserId} from repo {RepoId} by user {RemovedByUserId}", userId, repoId, removedByUserId);

            var repo = await _unitOfWork.GithubRepos.GetRepoWithDetailsAsync(repoId);
            if (repo == null)
                return ApiResponse<bool>.ErrorResponse("Github repository not found");

            var canManage = await CanUserManageProjectAsync(repo.ProjectId, removedByUserId);
            if (!canManage && removedByUserId != userId)
                return ApiResponse<bool>.ErrorResponse("You do not have permission to remove contributors from this repository");

            var contributor = repo.RepoContributors.FirstOrDefault(rc => rc.UserId == userId);
            if (contributor == null)
                return ApiResponse<bool>.ErrorResponse("User is not a contributor to this repository");


            _unitOfWork.RepoContributors.Remove(contributor);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Contributor {UserId} removed successfully from repo {RepoId}", userId, repoId);

            return ApiResponse<bool>.SuccessResponse(true, "Contributor removed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing contributor from Github repo");
            return ApiResponse<bool>.ErrorResponse("Error removing contributor from repository", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> CanUserManageRepoAsync(Guid repoId, Guid userId)
    {
        try
        {
            var repo = await _unitOfWork.GithubRepos.GetRepoWithDetailsAsync(repoId);
            if (repo == null)
                return ApiResponse<bool>.ErrorResponse("Github repository not found");

            var canManage = await CanUserManageProjectAsync(repo.ProjectId, userId);
            return ApiResponse<bool>.SuccessResponse(canManage);
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user permission for repo");
                return ApiResponse<bool>.ErrorResponse("Error checking permissions", ex.Message);
            }
        }

        public async Task<ApiResponse<ProjectGithubContributionDto>> GetGithubContributionsAsync(Guid projectId)
        {
            try
            {
                _logger.LogInformation("Getting Github contributions for project: {ProjectId}", projectId);

                var repos = (await _unitOfWork.GithubRepos.GetReposByProjectIdWithSemesterAsync(projectId)).ToList();
                if (!repos.Any())
                    return ApiResponse<ProjectGithubContributionDto>.ErrorResponse("No GitHub repositories found for this project");

                var firstRepo = repos.First();
                var project = firstRepo.Project;
                var semester = project.Class.Semester;

                var semesterStartDate = semester.StartDate;
                var semesterEndDate = semester.EndDate;

                var result = new ProjectGithubContributionDto
                {
                    ProjectId = projectId,
                    ProjectName = project.Name,
                    SemesterStartDate = semesterStartDate,
                    SemesterEndDate = semesterEndDate,
                    Repositories = _mapper.Map<List<RepoContributionDto>>(repos)
                };

                var allContributorStats = new Dictionary<string, ContributorStatsDto>();
                var weeklyCommitsAggregated = new Dictionary<DateTime, int>();

                foreach (var repo in repos)
                {
                    var contributorStats = await _githubApiService.GetRepositoryContributorStatsAsync(
                        repo.RepoOwnerName,
                        repo.RepoName,
                        repo.ApiToken);

                    if (contributorStats?.Contributors != null)
                    {
                        foreach (var contributor in contributorStats.Contributors)
                        {
                            var contributorKey = contributor.Login.ToLowerInvariant();

                            if (!allContributorStats.TryGetValue(contributorKey, out var existingStats))
                            {
                                var repoContributor = repo.RepoContributors
                                    .FirstOrDefault(rc => rc.GithubUsername.Equals(contributor.Login, StringComparison.OrdinalIgnoreCase));

                                existingStats = new ContributorStatsDto
                                {
                                    GithubUsername = contributor.Login,
                                    GithubEmail = contributor.Email ?? repoContributor?.GithubEmail,
                                    UserId = repoContributor?.UserId,
                                    UserFullName = repoContributor?.User?.Name,
                                    TotalCommits = 0,
                                    TotalAdditions = 0,
                                    TotalDeletions = 0,
                                    WeeklyActivity = []
                                };
                                allContributorStats[contributorKey] = existingStats;
                            }

                            var weeklyActivityMap = existingStats.WeeklyActivity
                                .ToDictionary(w => w.WeekStart, w => w);

                            foreach (var week in contributor.Weeks)
                            {
                                var weekStart = DateTimeOffset.FromUnixTimeSeconds(week.Timestamp).UtcDateTime.Date;
                                var weekEnd = weekStart.AddDays(6);

                                if (weekStart > semesterEndDate || weekEnd < semesterStartDate)
                                    continue;

                                existingStats.TotalCommits += week.Commits;
                                existingStats.TotalAdditions += week.Additions;
                                existingStats.TotalDeletions += week.Deletions;

                                if (weeklyActivityMap.TryGetValue(weekStart, out var existingWeek))
                                {
                                    existingWeek.Commits += week.Commits;
                                    existingWeek.Additions += week.Additions;
                                    existingWeek.Deletions += week.Deletions;
                                }
                                else
                                {
                                    var newWeekActivity = new WeeklyContributorActivityDto
                                    {
                                        WeekStart = weekStart,
                                        WeekEnd = weekEnd,
                                        Commits = week.Commits,
                                        Additions = week.Additions,
                                        Deletions = week.Deletions
                                    };
                                    existingStats.WeeklyActivity.Add(newWeekActivity);
                                    weeklyActivityMap[weekStart] = newWeekActivity;
                                }

                                if (weeklyCommitsAggregated.TryGetValue(weekStart, out var existingCommits))
                                {
                                    weeklyCommitsAggregated[weekStart] = existingCommits + week.Commits;
                                }
                                else
                                {
                                    weeklyCommitsAggregated[weekStart] = week.Commits;
                                }
                            }

                            existingStats.WeeklyActivity = [.. existingStats.WeeklyActivity.OrderBy(w => w.WeekStart)];
                        }
                    }
                }

                result.Contributors = [.. allContributorStats.Values.OrderByDescending(c => c.TotalCommits)];
                result.TotalCommitsInSemester = result.Contributors.Sum(c => c.TotalCommits);
                result.TotalAdditionsInSemester = result.Contributors.Sum(c => c.TotalAdditions);
                result.TotalDeletionsInSemester = result.Contributors.Sum(c => c.TotalDeletions);

                result.OverallCommitsOverTime = weeklyCommitsAggregated
                    .OrderBy(kvp => kvp.Key)
                    .Select(kvp => new WeeklyCommitDto
                    {
                        WeekStart = kvp.Key,
                        WeekEnd = kvp.Key.AddDays(6),
                        CommitCount = kvp.Value
                    })
                    .ToList();

                _logger.LogInformation("Successfully retrieved Github contributions for project: {ProjectId}", projectId);

                return ApiResponse<ProjectGithubContributionDto>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Github contributions for project: {ProjectId}", projectId);
                return ApiResponse<ProjectGithubContributionDto>.ErrorResponse("Error retrieving Github contributions", ex.Message);
            }
        }

        private async Task<bool> CanUserManageProjectAsync(Guid projectId, Guid userId)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
        if (project == null)
            return false;

        var projectMembers = await _unitOfWork.ProjectMembers.GetByProjectIdAsync(projectId);
        var isMember = projectMembers.Any(pm => pm.UserId == userId);

        return isMember;
    }

    private static IQueryable<GithubRepo> ApplySorting(IQueryable<GithubRepo> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return sortDescending ? query.OrderByDescending(gr => gr.CreatedAt) : query.OrderBy(gr => gr.CreatedAt);

        return sortBy.ToLower() switch
        {
            "reponame" => sortDescending ? query.OrderByDescending(gr => gr.RepoName) : query.OrderBy(gr => gr.RepoName),
            "repoowner" => sortDescending ? query.OrderByDescending(gr => gr.RepoOwnerName) : query.OrderBy(gr => gr.RepoOwnerName),
            "projectname" => sortDescending ? query.OrderByDescending(gr => gr.Project.Name) : query.OrderBy(gr => gr.Project.Name),
            "createdat" => sortDescending ? query.OrderByDescending(gr => gr.CreatedAt) : query.OrderBy(gr => gr.CreatedAt),
            "updatedat" => sortDescending ? query.OrderByDescending(gr => gr.UpdatedAt) : query.OrderBy(gr => gr.UpdatedAt),
            _ => sortDescending ? query.OrderByDescending(gr => gr.CreatedAt) : query.OrderBy(gr => gr.CreatedAt)
        };
    }
}
