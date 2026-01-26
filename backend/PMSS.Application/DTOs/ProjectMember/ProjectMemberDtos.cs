namespace PMSS.Application.DTOs.ProjectMember;

public class ProjectMemberDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? GithubUsername { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class CreateProjectMemberDto
{
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
}

public class ProjectMemberFilterParams : PMSS.Application.DTOs.Common.PaginationParams
{
    public Guid? ProjectId { get; set; }
    public Guid? UserId { get; set; }
}
