using PMSS.Domain.Entities;

namespace PMSS.Application.Interfaces.Repositories;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(Guid userId);
    Task<IEnumerable<Notification>> GetUnreadNotificationsByUserIdAsync(Guid userId);
    Task<int> GetUnreadCountByUserIdAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkMultipleAsReadAsync(List<Guid> notificationIds);
    Task MarkAllAsReadByUserIdAsync(Guid userId);
}
