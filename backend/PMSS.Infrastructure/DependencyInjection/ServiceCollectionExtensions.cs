using AutoMapper;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Application.Mappings;
using PMSS.Infrastructure.Configuration;
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

        // Register AutoMapper
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        services.AddSingleton<IMapper>(config.CreateMapper());

        // Configure JWT settings
        var jwtSettings = configuration.GetSection("JwtSettings");
        services.Configure<JwtSettings>(jwtSettings);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddScoped<ISemesterRepository, SemesterRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<IClassEnrollmentRepository, ClassEnrollmentRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
        services.AddScoped<IGithubRepoRepository, GithubRepoRepository>();
        services.AddScoped<IRepoContributorRepository, RepoContributorRepository>();
        services.AddScoped<IWeeklyContributionRepository, WeeklyContributionRepository>();
        services.AddScoped<IUserWeeklyContributionRepository, UserWeeklyContributionRepository>();
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
        services.AddScoped<IGithubDataSyncService, GithubDataSyncService>();
        services.AddScoped<IAuthService, AuthService>();

        // Register background service for automated GitHub data sync at midnight
        services.AddHostedService<GithubDataSyncBackgroundService>();

        return services;
    }
}
