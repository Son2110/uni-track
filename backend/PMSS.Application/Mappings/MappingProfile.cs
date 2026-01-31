using AutoMapper;
using PMSS.Application.DTOs.AccessRequest;
using PMSS.Application.DTOs.Class;
using PMSS.Application.DTOs.ClassEnrollment;
using PMSS.Application.DTOs.Course;
using PMSS.Application.DTOs.GithubRepo;
using PMSS.Application.DTOs.JiraConfig;
using PMSS.Application.DTOs.Project;
using PMSS.Application.DTOs.ProjectMember;
using PMSS.Application.DTOs.Semester;
using PMSS.Application.DTOs.User;
using PMSS.Domain.Entities;

namespace PMSS.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>();
        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.HashedPassword, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TaughtClasses, opt => opt.Ignore())
            .ForMember(dest => dest.ClassEnrollments, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectMembers, opt => opt.Ignore())
            .ForMember(dest => dest.RepoContributors, opt => opt.Ignore())
            .ForMember(dest => dest.AccessRequests, opt => opt.Ignore());

        // Semester mappings
        CreateMap<Semester, SemesterDto>();
        CreateMap<CreateSemesterDto, Semester>()
            .ForMember(dest => dest.SemesterId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Classes, opt => opt.Ignore());

        // Course mappings
        CreateMap<Course, CourseDto>();
        CreateMap<CreateCourseDto, Course>()
            .ForMember(dest => dest.CourseId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Classes, opt => opt.Ignore());

        // Class mappings
        CreateMap<Class, ClassDto>()
            .ForMember(dest => dest.SemesterName, opt => opt.MapFrom(src => src.Semester != null ? src.Semester.Name : string.Empty))
            .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.Course != null ? src.Course.Code : string.Empty))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course != null ? src.Course.Name : string.Empty))
            .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher != null ? src.Teacher.Name : string.Empty));
        CreateMap<CreateClassDto, Class>()
            .ForMember(dest => dest.ClassId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Semester, opt => opt.Ignore())
            .ForMember(dest => dest.Course, opt => opt.Ignore())
            .ForMember(dest => dest.Teacher, opt => opt.Ignore())
            .ForMember(dest => dest.ClassEnrollments, opt => opt.Ignore())
            .ForMember(dest => dest.Projects, opt => opt.Ignore());

        // ClassEnrollment mappings
        CreateMap<ClassEnrollment, ClassEnrollmentDto>()
            .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => $"{(src.Course != null ? src.Course.Code : "")} - {(src.Class != null ? src.Class.ClassCode : "")}"))
            .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.Course != null ? src.Course.Code : string.Empty))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course != null ? src.Course.Name : string.Empty))
            .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.Class != null ? src.Class.ClassCode : string.Empty))
            .ForMember(dest => dest.SemesterName, opt => opt.MapFrom(src => src.Class != null && src.Class.Semester != null ? src.Class.Semester.Name : string.Empty))
            .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Class != null && src.Class.Teacher != null ? src.Class.Teacher.Name : string.Empty))
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.User != null ? src.User.Name : string.Empty))
            .ForMember(dest => dest.StudentEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty));
        CreateMap<CreateClassEnrollmentDto, ClassEnrollment>()
            .ForMember(dest => dest.CourseId, opt => opt.Ignore())
            .ForMember(dest => dest.EnrolledAt, opt => opt.Ignore())
            .ForMember(dest => dest.Class, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Course, opt => opt.Ignore());

        // Project mappings
        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => 
                $"{(src.Class != null && src.Class.Course != null ? src.Class.Course.Code : "")} - Section {(src.Class != null ? src.Class.ClassCode : "")}"))
            .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.Class != null && src.Class.Course != null ? src.Class.Course.Code : string.Empty))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Class != null && src.Class.Course != null ? src.Class.Course.Name : string.Empty));
        CreateMap<CreateProjectDto, Project>()
            .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Class, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectMembers, opt => opt.Ignore())
            .ForMember(dest => dest.GithubRepos, opt => opt.Ignore())
            .ForMember(dest => dest.JiraConfig, opt => opt.Ignore())
            .ForMember(dest => dest.AccessRequests, opt => opt.Ignore());

        // ProjectMember mappings
        CreateMap<ProjectMember, ProjectMemberDto>()
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : string.Empty))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Name : string.Empty))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
            .ForMember(dest => dest.GithubUsername, opt => opt.MapFrom(src => src.User != null ? src.User.GithubUsername : null));
        CreateMap<CreateProjectMemberDto, ProjectMember>()
            .ForMember(dest => dest.JoinedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());

        // GithubRepo mappings
        CreateMap<GithubRepo, GithubRepoDto>()
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : string.Empty))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.Project != null && src.Project.Class != null ? src.Project.Class.CourseId : Guid.Empty))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Project != null && src.Project.Class != null && src.Project.Class.Course != null ? src.Project.Class.Course.Name : string.Empty))
            .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.Project != null && src.Project.Class != null && src.Project.Class.Course != null ? src.Project.Class.Course.Code : string.Empty))
            .ForMember(dest => dest.RepoUrl, opt => opt.MapFrom(src => $"https://github.com/{src.RepoOwnerName}/{src.RepoName}"))
            .ForMember(dest => dest.ContributorCount, opt => opt.MapFrom(src => src.RepoContributors != null ? src.RepoContributors.Count : 0))
            .ForMember(dest => dest.Contributors, opt => opt.MapFrom(src => src.RepoContributors));
        CreateMap<CreateGithubRepoDto, GithubRepo>()
            .ForMember(dest => dest.GithubRepoId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore())
            .ForMember(dest => dest.RepoContributors, opt => opt.Ignore());

        // RepoContributor mappings (for GithubRepoDto.Contributors)
        CreateMap<RepoContributor, DTOs.GithubRepo.RepoContributorDto>()
            .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User != null ? src.User.Name : null));

        // RepoContributor mappings (for standalone RepoContributorDto)
        CreateMap<RepoContributor, DTOs.RepoContributor.RepoContributorDto>()
            .ForMember(dest => dest.RepoName, opt => opt.MapFrom(src => src.GithubRepo != null ? src.GithubRepo.RepoName : string.Empty))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Name : null));
        CreateMap<DTOs.RepoContributor.CreateRepoContributorDto, RepoContributor>()
            .ForMember(dest => dest.AddedAt, opt => opt.Ignore())
            .ForMember(dest => dest.GithubRepo, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());

        // JiraConfig mappings
        CreateMap<JiraConfig, JiraConfigDto>()
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : string.Empty));
        CreateMap<CreateJiraConfigDto, JiraConfig>()
            .ForMember(dest => dest.JiraConfigId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore());

        // AccessRequest mappings
        CreateMap<AccessRequest, AccessRequestDto>()
            .ForMember(dest => dest.RequesterName, opt => opt.MapFrom(src => src.Requester != null ? src.Requester.Name : string.Empty))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : string.Empty));
        CreateMap<CreateAccessRequestDto, AccessRequest>()
            .ForMember(dest => dest.RequestId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.RequestedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ResolvedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Requester, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore());

        // GithubRepo contribution mappings
        CreateMap<GithubRepo, RepoContributionDto>()
            .ForMember(dest => dest.RepoUrl, opt => opt.MapFrom(src => $"https://github.com/{src.RepoOwnerName}/{src.RepoName}"));
    }
}
