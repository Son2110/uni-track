namespace PMSS.Domain.Entities;

public class Class
{
    public Guid ClassId { get; set; }
    public Guid SemesterId { get; set; }
    public Guid CourseId { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public Guid TeacherId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Semester Semester { get; set; } = null!;
    public virtual Course Course { get; set; } = null!;
    public virtual User Teacher { get; set; } = null!;
    public virtual ICollection<ClassEnrollment> ClassEnrollments { get; set; } = new List<ClassEnrollment>();
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
