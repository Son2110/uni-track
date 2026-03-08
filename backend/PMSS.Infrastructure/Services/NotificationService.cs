using AutoMapper;
using Microsoft.Extensions.Logging;
using PMSS.Application.DTOs.Common;
using PMSS.Application.DTOs.Notification;
using PMSS.Application.Interfaces.Repositories;
using PMSS.Application.Interfaces.Services;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationService> _logger;
    private readonly IMapper _mapper;

    public NotificationService(IUnitOfWork unitOfWork, ILogger<NotificationService> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResult<NotificationDto>>> GetAllNotificationsAsync(NotificationFilterParams filterParams)
    {
        try
        {
            _logger.LogInformation("Getting all notifications with filters: UserId={UserId}, IsRead={IsRead}", 
                filterParams.UserId, filterParams.IsRead);

            var query = _unitOfWork.Notifications.GetAllQueryable();
            
            if (filterParams.UserId.HasValue)
                query = query.Where(n => n.UserId == filterParams.UserId.Value);

            if (filterParams.IsRead.HasValue)
                query = query.Where(n => n.IsRead == filterParams.IsRead.Value);

            if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
                query = query.Where(n => n.Title.Contains(filterParams.SearchTerm) || 
                                        n.Message.Contains(filterParams.SearchTerm));

            var totalCount = query.Count();
            
            query = filterParams.SortDescending 
                ? query.OrderByDescending(n => n.CreatedAt) 
                : query.OrderBy(n => n.CreatedAt);

            var items = query
                .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .ToList();

            var itemDtos = _mapper.Map<List<NotificationDto>>(items);

            var result = new PagedResult<NotificationDto>
            {
                Items = itemDtos,
                TotalCount = totalCount,
                PageNumber = filterParams.PageNumber,
                PageSize = filterParams.PageSize
            };

            return ApiResponse<PagedResult<NotificationDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications");
            return ApiResponse<PagedResult<NotificationDto>>.ErrorResponse("Error retrieving notifications", ex.Message);
        }
    }

    public async Task<ApiResponse<NotificationDto>> GetNotificationByIdAsync(Guid id)
    {
        try
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
            if (notification == null)
                return ApiResponse<NotificationDto>.ErrorResponse("Notification not found");

            var dto = _mapper.Map<NotificationDto>(notification);
            return ApiResponse<NotificationDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification {NotificationId}", id);
            return ApiResponse<NotificationDto>.ErrorResponse("Error retrieving notification", ex.Message);
        }
    }

    public async Task<ApiResponse<NotificationDto>> CreateNotificationAsync(CreateNotificationDto dto)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
            if (user == null)
                return ApiResponse<NotificationDto>.ErrorResponse("User not found");

            var notification = new Notification
            {
                UserId = dto.UserId,
                Title = dto.Title,
                Message = dto.Message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created notification {NotificationId} for user {UserId}", 
                notification.NotificationId, dto.UserId);

            var notificationDto = _mapper.Map<NotificationDto>(notification);
            return ApiResponse<NotificationDto>.SuccessResponse(notificationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification");
            return ApiResponse<NotificationDto>.ErrorResponse("Error creating notification", ex.Message);
        }
    }

    public async Task<ApiResponse<List<NotificationDto>>> CreateBulkNotificationsAsync(List<CreateNotificationDto> dtos)
    {
        try
        {
            var notifications = new List<Notification>();

            foreach (var dto in dtos)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found, skipping notification", dto.UserId);
                    continue;
                }

                var notification = new Notification
                {
                    UserId = dto.UserId,
                    Title = dto.Title,
                    Message = dto.Message,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                notifications.Add(notification);
            }

            if (notifications.Any())
            {
                await _unitOfWork.Notifications.AddRangeAsync(notifications);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created {Count} bulk notifications", notifications.Count);
            }

            var notificationDtos = _mapper.Map<List<NotificationDto>>(notifications);
            return ApiResponse<List<NotificationDto>>.SuccessResponse(notificationDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bulk notifications");
            return ApiResponse<List<NotificationDto>>.ErrorResponse("Error creating bulk notifications", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(Guid id)
    {
        try
        {
            await _unitOfWork.Notifications.MarkAsReadAsync(id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Marked notification {NotificationId} as read", id);
            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", id);
            return ApiResponse<bool>.ErrorResponse("Error marking notification as read", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> MarkMultipleAsReadAsync(MarkAsReadDto dto)
    {
        try
        {
            await _unitOfWork.Notifications.MarkMultipleAsReadAsync(dto.NotificationIds);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Marked {Count} notifications as read", dto.NotificationIds.Count);
            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking multiple notifications as read");
            return ApiResponse<bool>.ErrorResponse("Error marking notifications as read", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> MarkAllAsReadAsync(Guid userId)
    {
        try
        {
            await _unitOfWork.Notifications.MarkAllAsReadByUserIdAsync(userId);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Marked all notifications as read for user {UserId}", userId);
            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
            return ApiResponse<bool>.ErrorResponse("Error marking all notifications as read", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteNotificationAsync(Guid id)
    {
        try
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
            if (notification == null)
                return ApiResponse<bool>.ErrorResponse("Notification not found");

            _unitOfWork.Notifications.Remove(notification);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted notification {NotificationId}", id);
            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {NotificationId}", id);
            return ApiResponse<bool>.ErrorResponse("Error deleting notification", ex.Message);
        }
    }

    public async Task<ApiResponse<int>> GetUnreadCountAsync(Guid userId)
    {
        try
        {
            var count = await _unitOfWork.Notifications.GetUnreadCountByUserIdAsync(userId);
            return ApiResponse<int>.SuccessResponse(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
            return ApiResponse<int>.ErrorResponse("Error getting unread count", ex.Message);
        }
    }

    public async Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(Guid userId, int count = 10)
    {
        try
        {
            var notifications = await _unitOfWork.Notifications.GetNotificationsByUserIdAsync(userId);
            var limitedNotifications = notifications.Take(count).ToList();
            var dtos = _mapper.Map<List<NotificationDto>>(limitedNotifications);

            return ApiResponse<List<NotificationDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
            return ApiResponse<List<NotificationDto>>.ErrorResponse("Error getting user notifications", ex.Message);
        }
    }

    // Helper methods for common notification scenarios
    public async Task NotifyProjectMemberAddedAsync(Guid userId, string projectName)
    {
        try
        {
            var dto = new CreateNotificationDto
            {
                UserId = userId,
                Title = "Added to Project",
                Message = $"You have been added to the project: {projectName}"
            };

            await CreateNotificationAsync(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending project member added notification");
        }
    }

    public async Task NotifyProjectAssignmentAsync(List<Guid> userIds, string projectName)
    {
        try
        {
            var dtos = userIds.Select(userId => new CreateNotificationDto
            {
                UserId = userId,
                Title = "Project Assignment",
                Message = $"You have been assigned to project: {projectName}"
            }).ToList();

            await CreateBulkNotificationsAsync(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending project assignment notifications");
        }
    }

    public async Task NotifyClassEnrollmentAsync(Guid userId, string className)
    {
        try
        {
            var dto = new CreateNotificationDto
            {
                UserId = userId,
                Title = "Class Enrollment",
                Message = $"You have been enrolled in class: {className}"
            };

            await CreateNotificationAsync(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending class enrollment notification");
        }
    }

    public async Task NotifyAccessRequestStatusAsync(Guid userId, string requestType, bool isApproved)
    {
        try
        {
            var status = isApproved ? "approved" : "rejected";
            var dto = new CreateNotificationDto
            {
                UserId = userId,
                Title = $"Access Request {status.ToUpper()}",
                Message = $"Your {requestType} access request has been {status}."
            };

            await CreateNotificationAsync(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending access request status notification");
        }
    }

    public async Task NotifyDeadlineReminderAsync(Guid userId, string projectName, DateTime deadline)
    {
        try
        {
            var dto = new CreateNotificationDto
            {
                UserId = userId,
                Title = "Project Deadline Reminder",
                Message = $"Reminder: Project '{projectName}' is due on {deadline:MMM dd, yyyy}"
            };

            await CreateNotificationAsync(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending deadline reminder notification");
        }
    }
}
