import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../constants/app_constants.dart';
import '../models/notification_model.dart';

class NotificationGraphQLService {
  static const _graphqlPath = '/graphql';

  static const _getNotificationsQuery = r'''
    query GetMyNotifications($userId: UUID!) {
      notifications(
        where: { userId: { eq: $userId } }
        order: [{ createdAt: DESC }]
        first: 50
      ) {
        nodes {
          notificationId
          userId
          title
          message
          isRead
          createdAt
          readAt
        }
      }
    }
  ''';

  Future<List<NotificationModel>> getMyNotifications({
    required String userId,
    required String token,
  }) async {
    final uri = Uri.parse('${AppConstants.baseUrl}$_graphqlPath');

    final response = await http.post(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode({
        'query': _getNotificationsQuery,
        'variables': {'userId': userId},
      }),
    );

    final body = jsonDecode(response.body) as Map<String, dynamic>;

    if (response.statusCode == 200 && !body.containsKey('errors')) {
      final data = body['data'] as Map<String, dynamic>;
      final nodes =
          (data['notifications'] as Map<String, dynamic>)['nodes']
              as List<dynamic>;
      return nodes
          .map((e) => NotificationModel.fromGraphQL(e as Map<String, dynamic>))
          .toList();
    }

    final errors = body['errors'];
    final message = errors != null
        ? (errors[0] as Map<String, dynamic>)['message'] as String? ??
              'Failed to load notifications'
        : 'Failed to load notifications';
    throw Exception(message);
  }
}
