import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../constants/app_constants.dart';
import '../../../classes/data/models/class_model.dart';

class AdminClassApiService {
  static const _basePath = '/api/v1/classes';

  Future<ClassModel> create({
    required String semesterId,
    required String courseId,
    required String classCode,
    required String teacherId,
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
        'semesterId': semesterId,
        'courseId': courseId,
        'classCode': classCode,
        'teacherId': teacherId,
      }),
    );

    final body = jsonDecode(response.body) as Map<String, dynamic>;

    if ((response.statusCode == 200 || response.statusCode == 201) &&
        body['success'] == true) {
      return ClassModel.fromJson(body['data'] as Map<String, dynamic>);
    }

    final message = body['message'] as String? ?? 'Failed to create class';
    throw Exception(message);
  }

  Future<ClassModel> update({
    required String classId,
    required String semesterId,
    required String courseId,
    required String classCode,
    required String teacherId,
    required String token,
  }) async {
    final uri = Uri.parse('${AppConstants.baseUrl}$_basePath/$classId');

    final response = await http.put(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode({
        'semesterId': semesterId,
        'courseId': courseId,
        'classCode': classCode,
        'teacherId': teacherId,
      }),
    );

    final body = jsonDecode(response.body) as Map<String, dynamic>;

    if (response.statusCode == 200 && body['success'] == true) {
      return ClassModel.fromJson(body['data'] as Map<String, dynamic>);
    }

    final message = body['message'] as String? ?? 'Failed to update class';
    throw Exception(message);
  }

  Future<void> delete({required String classId, required String token}) async {
    final uri = Uri.parse('${AppConstants.baseUrl}$_basePath/$classId');

    final response = await http.delete(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
    );

    if (response.statusCode == 204) return;

    final body = jsonDecode(response.body) as Map<String, dynamic>;
    final message = body['message'] as String? ?? 'Failed to delete class';
    throw Exception(message);
  }
}
