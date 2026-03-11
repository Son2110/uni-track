using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.Notification;

namespace PMSS.Application.Interfaces.Services;

public interface INotificationService
{
    Task<ApiResponse<PagedResult<NotificationDto>>> GetAllNotificationsAsync(NotificationFilterParams filterParams);
    Task<ApiResponse<NotificationDto>> GetNotificationByIdAsync(Guid id);
    Task<ApiResponse<NotificationDto>> CreateNotificationAsync(CreateNotificationDto dto);
    Task<ApiResponse<List<NotificationDto>>> CreateBulkNotificationsAsync(List<CreateNotificationDto> dtos);
    Task<ApiResponse<bool>> MarkAsReadAsync(Guid id);
    Task<ApiResponse<bool>> MarkMultipleAsReadAsync(MarkAsReadDto dto);
    Task<ApiResponse<bool>> MarkAllAsReadAsync(Guid userId);
    Task<ApiResponse<bool>> DeleteNotificationAsync(Guid id);
    Task<ApiResponse<int>> GetUnreadCountAsync(Guid userId);
    Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(Guid userId, int count = 10);
    
    // Notification creation helpers for common scenarios
    Task NotifyProjectMemberAddedAsync(Guid userId, string projectName);
    Task NotifyProjectAssignmentAsync(List<Guid> userIds, string projectName);
    Task NotifyClassEnrollmentAsync(Guid userId, string className);
    Task NotifyAccessRequestStatusAsync(Guid userId, string requestType, bool isApproved);
    Task NotifyDeadlineReminderAsync(Guid userId, string projectName, DateTime deadline);
}
