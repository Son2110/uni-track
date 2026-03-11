using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.Notification;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Enums;
using System.Text;

namespace PMSS.Infrastructure.Services;

public class StudentActivityMonitorService : IStudentActivityMonitorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<StudentActivityMonitorService> _logger;
    private const int TopLeastActiveCount = 10;

    public StudentActivityMonitorService(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<StudentActivityMonitorService> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<ApiResponse<StudentActivityCheckResultDto>> CheckAndNotifyAllAsync(int recentWeeks = 4)
    {
        _logger.LogInformation("Starting student activity check for all classes (recent {Weeks} weeks)", recentWeeks);

        try
        {
            var teachers = await _unitOfWork.Users.FindAsync(u => u.Role == UserRole.Teacher);
            var result = new StudentActivityCheckResultDto
            {
                CheckedAt = DateTime.UtcNow
            };

            foreach (var teacher in teachers)
            {
                var classes = await _unitOfWork.Classes.GetClassesByTeacherIdAsync(teacher.UserId);

                foreach (var cls in classes)
                {
                    var report = await BuildClassActivityReportAsync(cls.ClassId, recentWeeks);
                    if (report == null)
                        continue;

                    result.ClassReports.Add(report);

                    if (report.LeastActiveStudents.Count > 0)
                    {
                        await SendTeacherNotificationAsync(teacher.UserId, report);
                        result.TotalNotificationsSent++;
                    }

                    result.TotalClassesProcessed++;
                }
            }

            _logger.LogInformation(
                "Student activity check completed: {Classes} classes processed, {Notifications} notifications sent",
                result.TotalClassesProcessed, result.TotalNotificationsSent);

            return ApiResponse<StudentActivityCheckResultDto>.SuccessResponse(result, "Student activity check completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during student activity check");
            return ApiResponse<StudentActivityCheckResultDto>.ErrorResponse("Error during student activity check", ex.Message);
        }
    }

    public async Task<ApiResponse<ClassActivityReportDto>> CheckAndNotifyByClassAsync(Guid classId, int recentWeeks = 4)
    {
        _logger.LogInformation("Starting student activity check for class {ClassId} (recent {Weeks} weeks)", classId, recentWeeks);

        try
        {
            var cls = await _unitOfWork.Classes.GetByIdWithDetailsAsync(classId);
            if (cls == null)
                return ApiResponse<ClassActivityReportDto>.ErrorResponse("Class not found");

            var report = await BuildClassActivityReportAsync(classId, recentWeeks);
            if (report == null)
                return ApiResponse<ClassActivityReportDto>.ErrorResponse("Could not build activity report for this class");

            if (report.LeastActiveStudents.Count > 0)
            {
                await SendTeacherNotificationAsync(cls.TeacherId, report);
            }

            return ApiResponse<ClassActivityReportDto>.SuccessResponse(report, "Activity check completed and teacher notified");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during student activity check for class {ClassId}", classId);
            return ApiResponse<ClassActivityReportDto>.ErrorResponse("Error during student activity check", ex.Message);
        }
    }

    private async Task<ClassActivityReportDto?> BuildClassActivityReportAsync(Guid classId, int recentWeeks)
    {
        var cls = await _unitOfWork.Classes.GetByIdWithDetailsAsync(classId);
        if (cls == null)
            return null;

        // Get enrolled students
        var enrollments = await _unitOfWork.ClassEnrollments.GetEnrollmentsByClassIdAsync(classId);
        var studentEnrollments = enrollments
            .Where(e => e.User.Role == UserRole.Student)
            .ToList();

        if (studentEnrollments.Count == 0)
            return null;

        // Get all projects for this class
        var allProjects = await _unitOfWork.Projects.GetAllAsync();
        var classProjects = allProjects.Where(p => p.ClassId == classId).ToList();

        if (classProjects.Count == 0)
        {
            // No projects, all students have zero activity
            return BuildZeroActivityReport(cls, studentEnrollments);
        }

        // Get all github repos for these projects
        var repoIds = new List<Guid>();
        foreach (var project in classProjects)
        {
            var repos = await _unitOfWork.GithubRepos.GetReposByProjectIdAsync(project.ProjectId);
            repoIds.AddRange(repos.Select(r => r.GithubRepoId));
        }

        if (repoIds.Count == 0)
        {
            return BuildZeroActivityReport(cls, studentEnrollments);
        }

        // Get recent weekly contributions for these repos
        var cutoffDate = DateTime.UtcNow.AddDays(-7 * recentWeeks);
        var weeklyContributions = await _unitOfWork.WeeklyContributions
            .GetByRepoIdsAndDateRangeAsync(repoIds, cutoffDate, DateTime.UtcNow);

        var weeklyContributionIds = weeklyContributions.Select(wc => wc.WeeklyContributionId).ToList();

        // Get user weekly contributions for these weeks
        var studentUserIds = studentEnrollments.Select(e => e.UserId).ToHashSet();
        var userContributions = weeklyContributionIds.Count > 0
            ? await _unitOfWork.UserWeeklyContributions.GetByWeeklyContributionIdsAsync(weeklyContributionIds)
            : Enumerable.Empty<Domain.Entities.UserWeeklyContribution>();

        // Aggregate activity per student
        var activityByUser = userContributions
            .Where(uc => uc.UserId.HasValue && studentUserIds.Contains(uc.UserId.Value))
            .GroupBy(uc => uc.UserId!.Value)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    TotalCommits = g.Sum(x => x.Commits),
                    TotalAdditions = g.Sum(x => x.Additions),
                    TotalDeletions = g.Sum(x => x.Deletions)
                });

        // Build student activity list (including students with zero activity)
        var studentActivities = studentEnrollments.Select(enrollment =>
        {
            var hasActivity = activityByUser.TryGetValue(enrollment.UserId, out var activity);
            return new StudentActivityDto
            {
                UserId = enrollment.UserId,
                StudentName = enrollment.User.Name,
                Email = enrollment.User.Email,
                TotalCommits = hasActivity ? activity.TotalCommits : 0,
                TotalAdditions = hasActivity ? activity.TotalAdditions : 0,
                TotalDeletions = hasActivity ? activity.TotalDeletions : 0
            };
        })
        .OrderBy(s => s.TotalCommits)
        .ThenBy(s => s.TotalAdditions + s.TotalDeletions)
        .Take(TopLeastActiveCount)
        .ToList();

        return new ClassActivityReportDto
        {
            ClassId = cls.ClassId,
            ClassCode = cls.ClassCode,
            TeacherId = cls.TeacherId,
            TeacherName = cls.Teacher.Name,
            TotalStudents = studentEnrollments.Count,
            LeastActiveStudents = studentActivities
        };
    }

    private static ClassActivityReportDto BuildZeroActivityReport(
        Domain.Entities.Class cls,
        List<Domain.Entities.ClassEnrollment> studentEnrollments)
    {
        var students = studentEnrollments
            .Select(e => new StudentActivityDto
            {
                UserId = e.UserId,
                StudentName = e.User.Name,
                Email = e.User.Email,
                TotalCommits = 0,
                TotalAdditions = 0,
                TotalDeletions = 0
            })
            .Take(TopLeastActiveCount)
            .ToList();

        return new ClassActivityReportDto
        {
            ClassId = cls.ClassId,
            ClassCode = cls.ClassCode,
            TeacherId = cls.TeacherId,
            TeacherName = cls.Teacher.Name,
            TotalStudents = studentEnrollments.Count,
            LeastActiveStudents = students
        };
    }

    private async Task SendTeacherNotificationAsync(Guid teacherId, ClassActivityReportDto report)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Class: {report.ClassCode} ({report.TotalStudents} students)");
        sb.AppendLine($"Top {report.LeastActiveStudents.Count} least active students (last few weeks):");
        sb.AppendLine();

        for (int i = 0; i < report.LeastActiveStudents.Count; i++)
        {
            var student = report.LeastActiveStudents[i];
            sb.AppendLine($"{i + 1}. {student.StudentName} ({student.Email})");
            sb.AppendLine($"   Commits: {student.TotalCommits}, Additions: {student.TotalAdditions}, Deletions: {student.TotalDeletions}");
        }

        var notification = new CreateNotificationDto
        {
            UserId = teacherId,
            Title = $"Low Activity Alert - {report.ClassCode}",
            Message = sb.ToString()
        };

        await _notificationService.CreateNotificationAsync(notification);

        _logger.LogInformation(
            "Sent low activity notification to teacher {TeacherId} for class {ClassCode}",
            teacherId, report.ClassCode);
    }

    public async Task<ApiResponse<StudentActivityCheckResultDto>> GetActivityByTeacherAsync(Guid teacherId, int recentWeeks = 4)
    {
        _logger.LogInformation("Getting student activity for teacher {TeacherId} (recent {Weeks} weeks)", teacherId, recentWeeks);

        try
        {
            var teacher = await _unitOfWork.Users.GetByIdAsync(teacherId);
            if (teacher == null)
                return ApiResponse<StudentActivityCheckResultDto>.ErrorResponse("Teacher not found");

            var classes = await _unitOfWork.Classes.GetClassesByTeacherIdAsync(teacherId);
            var result = new StudentActivityCheckResultDto
            {
                CheckedAt = DateTime.UtcNow
            };

            foreach (var cls in classes)
            {
                var report = await BuildClassActivityReportAsync(cls.ClassId, recentWeeks);
                if (report == null)
                    continue;

                result.ClassReports.Add(report);
                result.TotalClassesProcessed++;
            }

            return ApiResponse<StudentActivityCheckResultDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student activity for teacher {TeacherId}", teacherId);
            return ApiResponse<StudentActivityCheckResultDto>.ErrorResponse("Error retrieving student activity", ex.Message);
        }
    }
}
