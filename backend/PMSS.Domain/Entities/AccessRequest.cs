using PMSS.Domain.Enums;

namespace PMSS.Domain.Entities;

public class AccessRequest
{
    public Guid RequestId { get; set; }
    public Guid RequesterId { get; set; }
    public Guid ProjectId { get; set; }
    public AccessRequestStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public virtual User Requester { get; set; } = null!;
    public virtual Project Project { get; set; } = null!;
}
