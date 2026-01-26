namespace PMSS.Domain.Entities;

public class Course
{
    public Guid CourseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
