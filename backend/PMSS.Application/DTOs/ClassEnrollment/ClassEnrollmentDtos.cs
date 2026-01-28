namespace PMSS.Application.DTOs.ClassEnrollment;

public class ClassEnrollmentDto
{
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public DateTime EnrolledAt { get; set; }
}

public class CreateClassEnrollmentDto
{
    public Guid ClassId { get; set; }
    public Guid UserId { get; set; }
}

public class BulkEnrollmentDto
{
    public Guid ClassId { get; set; }
    public List<Guid> UserIds { get; set; } = new List<Guid>();
}

public class ClassEnrollmentFilterParams : PMSS.Application.DTOs.Common.PaginationParams
{
    public Guid? ClassId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? CourseId { get; set; }
    public Guid? SemesterId { get; set; }
}
