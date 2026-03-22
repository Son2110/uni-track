using PMSS.Application.DTOs.Common;

namespace PMSS.Application.Interfaces.Services;

/// <summary>
/// Service for monitoring student coding activity on GitHub and notifying teachers
/// about the least active students in their classes.
/// </summary>
public interface IStudentActivityMonitorService
{
    /// <summary>
    /// Checks coding activity for all students across all classes and sends notifications
    /// to each teacher with the top 10 least active students per class.
    /// </summary>
    Task<ApiResponse<StudentActivityCheckResultDto>> CheckAndNotifyAllAsync(int recentWeeks = 4);

    /// <summary>
    /// Checks coding activity for students in a specific class and sends a notification
    /// to the teacher with the top 10 least active students.
    /// </summary>
    Task<ApiResponse<ClassActivityReportDto>> CheckAndNotifyByClassAsync(Guid classId, int recentWeeks = 4);

    /// <summary>
    /// Returns student activity reports for all classes taught by a specific teacher
    /// without sending any notifications.
    /// </summary>
    Task<ApiResponse<StudentActivityCheckResultDto>> GetActivityByTeacherAsync(Guid teacherId, int recentWeeks = 4);
}

public class StudentActivityCheckResultDto
{
    public int TotalClassesProcessed { get; set; }
    public int TotalNotificationsSent { get; set; }
    public List<ClassActivityReportDto> ClassReports { get; set; } = new();
    public DateTime CheckedAt { get; set; }
}

public class ClassActivityReportDto
{
    public Guid ClassId { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public List<StudentActivityDto> LeastActiveStudents { get; set; } = new();
}

public class StudentActivityDto
{
    public Guid UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalCommits { get; set; }
    public int TotalAdditions { get; set; }
    public int TotalDeletions { get; set; }
}
