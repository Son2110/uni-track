namespace PMSS.Application.DTOs.GithubRepo;

public class GithubRepoDto
{
    public Guid GithubRepoId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string RepoOwnerName { get; set; } = string.Empty;
    public string RepoName { get; set; } = string.Empty;
    public string RepoUrl { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public int ContributorCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<RepoContributorDto> Contributors { get; set; } = new();
}

public class RepoContributorDto
{
    public string GithubUsername { get; set; } = string.Empty;
    public string? GithubEmail { get; set; }
    public Guid? UserId { get; set; }
    public string? UserFullName { get; set; }
    public DateTime AddedAt { get; set; }
}

public class CreateGithubRepoDto
{
    public Guid ProjectId { get; set; }
    public string RepoOwnerName { get; set; } = string.Empty;
    public string RepoName { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public string? ApiToken { get; set; }
}

public class UpdateGithubRepoDto
{
    public string RepoOwnerName { get; set; } = string.Empty;
    public string RepoName { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public string? ApiToken { get; set; }
}

public class GithubRepoFilterParams : PMSS.Application.DTOs.Common.PaginationParams
{
    public Guid? ProjectId { get; set; }
    public Guid? CourseId { get; set; }
    public Guid? UserId { get; set; }
    public string? RepoOwnerName { get; set; }
    public bool? IsPrivate { get; set; }
}

public class ProjectGithubContributionDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime SemesterStartDate { get; set; }
    public DateTime SemesterEndDate { get; set; }
    public int TotalCommitsInSemester { get; set; }
    public int TotalAdditionsInSemester { get; set; }
    public int TotalDeletionsInSemester { get; set; }
    public List<RepoContributionDto> Repositories { get; set; } = new();
    public List<WeeklyCommitDto> OverallCommitsOverTime { get; set; } = new();
    public List<ContributorStatsDto> Contributors { get; set; } = new();
}

public class RepoContributionDto
{
    public Guid GithubRepoId { get; set; }
    public string RepoOwnerName { get; set; } = string.Empty;
    public string RepoName { get; set; } = string.Empty;
    public string RepoUrl { get; set; } = string.Empty;
    public int TotalCommits { get; set; }
    public int TotalAdditions { get; set; }
    public int TotalDeletions { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}

public class WeeklyCommitDto
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public int CommitCount { get; set; }
}

public class ContributorStatsDto
{
    public string GithubUsername { get; set; } = string.Empty;
    public string? GithubEmail { get; set; }
    public Guid? UserId { get; set; }
    public string? UserFullName { get; set; }
    public int TotalCommits { get; set; }
    public int TotalAdditions { get; set; }
    public int TotalDeletions { get; set; }
    public List<WeeklyContributorActivityDto> WeeklyActivity { get; set; } = new();
}

public class WeeklyContributorActivityDto
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public int Commits { get; set; }
    public int Additions { get; set; }
    public int Deletions { get; set; }
}

public class AddContributorToRepoDto
{
    public Guid GithubRepoId { get; set; }
    public Guid UserId { get; set; }
}

public class RemoveContributorFromRepoDto
{
    public Guid GithubRepoId { get; set; }
    public Guid UserId { get; set; }
}

public class CourseGithubReposDto
{
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public int TotalRepos { get; set; }
    public List<GithubRepoDto> Repositories { get; set; } = new();
}
