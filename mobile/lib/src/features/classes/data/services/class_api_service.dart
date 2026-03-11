import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../constants/app_constants.dart';
import '../models/class_model.dart';

class ClassApiService {
  static const _classesPath = '/api/v1/classes';

  /// Fetches all classes belonging to [teacherId].
  /// [token] is the JWT bearer token from the logged-in user.
  Future<List<ClassModel>> getTeacherClasses({
    required String teacherId,
    required String token,
  }) async {
    final uri = Uri.parse('${AppConstants.baseUrl}$_classesPath').replace(
      queryParameters: {
        'teacherId': teacherId,
        'pageSize': '100',
        'pageNumber': '1',
      },
    );

    final response = await http.get(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
    );

    final body = jsonDecode(response.body) as Map<String, dynamic>;

    if (response.statusCode == 200 && body['success'] == true) {
      final data = body['data'] as Map<String, dynamic>;
      final items = data['items'] as List<dynamic>;
      return items
          .map((e) => ClassModel.fromJson(e as Map<String, dynamic>))
          .toList();
    }

    final message = body['message'] as String? ?? 'Failed to load classes';
    throw Exception(message);
  }
}
