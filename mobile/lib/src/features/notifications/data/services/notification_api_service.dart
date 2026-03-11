import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../constants/app_constants.dart';

class NotificationApiService {
  static const _basePath = '/api/v1/notifications';

  Future<void> markAsRead({
    required String notificationId,
    required String token,
  }) async {
    final uri = Uri.parse(
      '${AppConstants.baseUrl}$_basePath/$notificationId/mark-read',
    );

    final response = await http.patch(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
    );

    if (response.statusCode != 200) {
      final body = jsonDecode(response.body) as Map<String, dynamic>;
      final message =
          body['message'] as String? ?? 'Failed to mark notification as read';
      throw Exception(message);
    }
  }

  Future<void> markAllAsRead({
    required List<String> notificationIds,
    required String token,
  }) async {
    final uri = Uri.parse('${AppConstants.baseUrl}$_basePath/mark-read');

    final response = await http.patch(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode({'notificationIds': notificationIds}),
    );

    if (response.statusCode != 200) {
      final body = jsonDecode(response.body) as Map<String, dynamic>;
      final message =
          body['message'] as String? ?? 'Failed to mark notifications as read';
      throw Exception(message);
    }
  }
}
