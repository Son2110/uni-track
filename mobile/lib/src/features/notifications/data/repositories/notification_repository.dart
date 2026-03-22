import '../models/notification_model.dart';
import '../services/notification_api_service.dart';
import '../services/notification_graphql_service.dart';

class NotificationRepository {
  final NotificationGraphQLService _graphqlService;
  final NotificationApiService _apiService;

  NotificationRepository({
    NotificationGraphQLService? graphqlService,
    NotificationApiService? apiService,
  }) : _graphqlService = graphqlService ?? NotificationGraphQLService(),
       _apiService = apiService ?? NotificationApiService();

  Future<List<NotificationModel>> getMyNotifications({
    required String userId,
    required String token,
  }) => _graphqlService.getMyNotifications(userId: userId, token: token);

  Future<void> markAsRead({
    required String notificationId,
    required String token,
  }) => _apiService.markAsRead(notificationId: notificationId, token: token);

  Future<void> markAllAsRead({
    required List<String> notificationIds,
    required String token,
  }) =>
      _apiService.markAllAsRead(notificationIds: notificationIds, token: token);
}
