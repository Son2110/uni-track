namespace PMSS.Application.DTOs.CourseEnrollment;

public class CourseEnrollmentDto
{
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
}

public class CreateCourseEnrollmentDto
{
    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }
}

public class CourseEnrollmentFilterParams : PMSS.Application.DTOs.Common.PaginationParams
{
    public Guid? CourseId { get; set; }
    public Guid? UserId { get; set; }
}
