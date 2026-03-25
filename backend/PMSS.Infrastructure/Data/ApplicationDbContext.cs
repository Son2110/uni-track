using Microsoft.EntityFrameworkCore;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Semester> Semesters { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<ClassEnrollment> ClassEnrollments { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<GithubRepo> GithubRepos { get; set; }
    public DbSet<RepoContributor> RepoContributors { get; set; }
    public DbSet<WeeklyContribution> WeeklyContributions { get; set; }
    public DbSet<UserWeeklyContribution> UserWeeklyContributions { get; set; }
    public DbSet<GithubContributionReport> GithubContributionReports { get; set; }
    public DbSet<JiraConfig> JiraConfigs { get; set; }
    public DbSet<AccessRequest> AccessRequests { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
