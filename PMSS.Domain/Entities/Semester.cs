namespace PMSS.Domain.Entities;

public class Semester
{
    public Guid SemesterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
