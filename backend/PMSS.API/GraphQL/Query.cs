using HotChocolate.Data;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Domain.Entities;

namespace PMSS.API.GraphQL;

/// <summary>
/// GraphQL Query root type following Clean Architecture principles.
/// Uses repositories from the Application layer instead of direct DbContext access.
/// </summary>
public class Query
{
    /// <summary>
    /// Get all users with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetUsers([Service] IUserRepository repository)
        => repository.GetAllQueryable();

    /// <summary>
    /// Get all projects with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Project> GetProjects([Service] IProjectRepository repository)
        => repository.GetAllQueryable();

    /// <summary>
    /// Get all project members with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProjectMember> GetProjectMembers([Service] IProjectMemberRepository repository)
        => repository.GetAllQueryable();

    /// <summary>
    /// Get all classes with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Class> GetClasses([Service] IClassRepository repository)
        => repository.GetAllQueryable();

    /// <summary>
    /// Get all class enrollments with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ClassEnrollment> GetClassEnrollments([Service] IClassEnrollmentRepository repository)
        => repository.GetAllQueryable();

    /// <summary>
    /// Get all courses with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Course> GetCourses([Service] ICourseRepository repository)
        => repository.GetAllQueryable();

    /// <summary>
    /// Get all semesters with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Semester> GetSemesters([Service] ISemesterRepository repository)
        => repository.GetAllQueryable();

    /// <summary>
    /// Get all GitHub repos with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<GithubRepo> GetGithubRepos([Service] IGithubRepoRepository repository)
        => repository.GetAllQueryable();

    /// <summary>
    /// Get all repo contributors with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<RepoContributor> GetRepoContributors([Service] IRepoContributorRepository repository)
        => repository.GetAllQueryable();

    /// <summary>
    /// Get all Jira configs with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<JiraConfig> GetJiraConfigs([Service] IJiraConfigRepository repository)
        => repository.GetAllQueryable();

    /// <summary>
    /// Get all access requests with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AccessRequest> GetAccessRequests([Service] IAccessRequestRepository repository)
        => repository.GetAllQueryable();

    /// <summary>
    /// Get all notifications with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Notification> GetNotifications([Service] INotificationRepository repository)
        => repository.GetAllQueryable();
}
