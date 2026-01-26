namespace PMSS.Domain.Entities;

public class ClassEnrollment
{
    public Guid ClassId { get; set; }
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public DateTime EnrolledAt { get; set; }

    public virtual Class Class { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Course Course { get; set; } = null!;
}
