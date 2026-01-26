namespace PMSS.Application.DTOs.RepoContributor;

public class RepoContributorDto
{
    public Guid GithubRepoId { get; set; }
    public string RepoName { get; set; } = string.Empty;
    public string GithubUsername { get; set; } = string.Empty;
    public string? GithubEmail { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime AddedAt { get; set; }
}

public class CreateRepoContributorDto
{
    public Guid GithubRepoId { get; set; }
    public string GithubUsername { get; set; } = string.Empty;
    public string? GithubEmail { get; set; }
    public Guid? UserId { get; set; }
}

public class RepoContributorFilterParams : PMSS.Application.DTOs.Common.PaginationParams
{
    public Guid? GithubRepoId { get; set; }
    public Guid? UserId { get; set; }
    public string? GithubUsername { get; set; }
}
