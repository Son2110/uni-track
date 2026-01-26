namespace PMSS.Application.DTOs.Class;

public class ClassDto
{
    public Guid ClassId { get; set; }
    public Guid SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateClassDto
{
    public Guid SemesterId { get; set; }
    public Guid CourseId { get; set; }
    public string Section { get; set; } = string.Empty;
    public Guid TeacherId { get; set; }
}

public class UpdateClassDto
{
    public string Section { get; set; } = string.Empty;
    public Guid TeacherId { get; set; }
}

public class ClassFilterParams : PMSS.Application.DTOs.Common.PaginationParams
{
    public Guid? SemesterId { get; set; }
    public Guid? CourseId { get; set; }
    public Guid? TeacherId { get; set; }
}
