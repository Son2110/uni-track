namespace PMSS.Application.DTOs.Project;

public class ProjectDto
{
    public Guid ProjectId { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateProjectDto
{
    public Guid ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ProjectFilterParams : PMSS.Application.DTOs.Common.PaginationParams
{
    public Guid? ClassId { get; set; }
    public Guid? CourseId { get; set; }
    public Guid? TeacherId { get; set; }
}
