namespace PMSS.Application.DTOs.Notification;

public class NotificationDto
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class CreateNotificationDto
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class MarkAsReadDto
{
    public List<Guid> NotificationIds { get; set; } = new();
}

public class NotificationFilterParams : PMSS.Application.DTOs.Common.PaginationParams
{
    public Guid? UserId { get; set; }
    public bool? IsRead { get; set; }
}
