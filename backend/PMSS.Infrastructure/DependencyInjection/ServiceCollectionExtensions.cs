using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Infrastructure.Data;
using PMSS.Infrastructure.Repositories;
using PMSS.Infrastructure.Services;

namespace PMSS.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<ISemesterRepository, SemesterRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<IClassEnrollmentRepository, ClassEnrollmentRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
        services.AddScoped<IGithubRepoRepository, GithubRepoRepository>();
        services.AddScoped<IRepoContributorRepository, RepoContributorRepository>();
        services.AddScoped<IJiraConfigRepository, JiraConfigRepository>();
        services.AddScoped<IAccessRequestRepository, AccessRequestRepository>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpClient();
        services.AddScoped<IGithubApiService, GithubApiService>();
        services.AddScoped<IJiraApiService, JiraApiService>();

        services.AddScoped<ISemesterService, SemesterService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IClassService, ClassService>();
        services.AddScoped<IClassEnrollmentService, ClassEnrollmentService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IProjectMemberService, ProjectMemberService>();
        services.AddScoped<IGithubRepoService, GithubRepoService>();

        return services;
    }
}
