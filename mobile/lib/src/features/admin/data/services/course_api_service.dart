import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../constants/app_constants.dart';
import '../models/course_model.dart';

class CourseApiService {
  static const _basePath = '/api/v1/courses';

  Future<CourseModel> create({
    required String code,
    required String name,
    String? description,
    required String token,
  }) async {
    final uri = Uri.parse('${AppConstants.baseUrl}$_basePath');

    final response = await http.post(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode({
        'code': code,
        'name': name,
        if (description != null && description.isNotEmpty)
          'description': description,
      }),
    );

    final body = jsonDecode(response.body) as Map<String, dynamic>;

    if ((response.statusCode == 200 || response.statusCode == 201) &&
        body['success'] == true) {
      return CourseModel.fromJson(body['data'] as Map<String, dynamic>);
    }

    final message = body['message'] as String? ?? 'Failed to create course';
    throw Exception(message);
  }

  Future<CourseModel> update({
    required String courseId,
    required String code,
    required String name,
    String? description,
    required String token,
  }) async {
    final uri = Uri.parse('${AppConstants.baseUrl}$_basePath/$courseId');

    final response = await http.put(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode({
        'code': code,
        'name': name,
        'description': description ?? '',
      }),
    );

    final body = jsonDecode(response.body) as Map<String, dynamic>;

    if (response.statusCode == 200 && body['success'] == true) {
      return CourseModel.fromJson(body['data'] as Map<String, dynamic>);
    }

    final message = body['message'] as String? ?? 'Failed to update course';
    throw Exception(message);
  }

  Future<void> delete({required String courseId, required String token}) async {
    final uri = Uri.parse('${AppConstants.baseUrl}$_basePath/$courseId');

    final response = await http.delete(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
    );

    if (response.statusCode == 204) return;

    final body = jsonDecode(response.body) as Map<String, dynamic>;
    final message = body['message'] as String? ?? 'Failed to delete course';
    throw Exception(message);
  }
}
