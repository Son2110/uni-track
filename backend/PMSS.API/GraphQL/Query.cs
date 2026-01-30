using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using PMSS.Domain.Entities;
using PMSS.Infrastructure.Data;

namespace PMSS.API.GraphQL;

public class Query
{
    /// <summary>
    /// Get all users with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetUsers(ApplicationDbContext context)
        => context.Users.AsNoTracking();

    /// <summary>
    /// Get all projects with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Project> GetProjects(ApplicationDbContext context)
        => context.Projects.AsNoTracking();

    /// <summary>
    /// Get all project members with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProjectMember> GetProjectMembers(ApplicationDbContext context)
        => context.ProjectMembers.AsNoTracking();

    /// <summary>
    /// Get all classes with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Class> GetClasses(ApplicationDbContext context)
        => context.Classes.AsNoTracking();

    /// <summary>
    /// Get all class enrollments with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ClassEnrollment> GetClassEnrollments(ApplicationDbContext context)
        => context.ClassEnrollments.AsNoTracking();

    /// <summary>
    /// Get all courses with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Course> GetCourses(ApplicationDbContext context)
        => context.Courses.AsNoTracking();

    /// <summary>
    /// Get all semesters with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Semester> GetSemesters(ApplicationDbContext context)
        => context.Semesters.AsNoTracking();

    /// <summary>
    /// Get all GitHub repos with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<GithubRepo> GetGithubRepos(ApplicationDbContext context)
        => context.GithubRepos.AsNoTracking();

    /// <summary>
    /// Get all repo contributors with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<RepoContributor> GetRepoContributors(ApplicationDbContext context)
        => context.RepoContributors.AsNoTracking();

    /// <summary>
    /// Get all Jira configs with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<JiraConfig> GetJiraConfigs(ApplicationDbContext context)
        => context.JiraConfigs.AsNoTracking();

    /// <summary>
    /// Get all access requests with filtering, sorting, and pagination support
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AccessRequest> GetAccessRequests(ApplicationDbContext context)
        => context.AccessRequests.AsNoTracking();
}
