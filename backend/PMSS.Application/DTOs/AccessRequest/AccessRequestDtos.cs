using PMSS.Domain.Enums;

namespace PMSS.Application.DTOs.AccessRequest;

public class AccessRequestDto
{
    public Guid RequestId { get; set; }
    public Guid RequesterId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public AccessRequestStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class CreateAccessRequestDto
{
    public Guid RequesterId { get; set; }
    public Guid ProjectId { get; set; }
}

public class UpdateAccessRequestStatusDto
{
    public AccessRequestStatus Status { get; set; }
}

public class AccessRequestFilterParams : PMSS.Application.DTOs.Common.PaginationParams
{
    public Guid? RequesterId { get; set; }
    public Guid? ProjectId { get; set; }
    public AccessRequestStatus? Status { get; set; }
}
