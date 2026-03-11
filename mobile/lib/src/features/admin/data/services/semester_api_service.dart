import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../constants/app_constants.dart';
import '../models/semester_model.dart';

class SemesterApiService {
  static const _basePath = '/api/v1/semesters';

  Future<SemesterModel> create({
    required String name,
    required DateTime startDate,
    required DateTime endDate,
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
        'name': name,
        'startDate': startDate.toIso8601String(),
        'endDate': endDate.toIso8601String(),
      }),
    );

    final body = jsonDecode(response.body) as Map<String, dynamic>;

    if ((response.statusCode == 200 || response.statusCode == 201) &&
        body['success'] == true) {
      return SemesterModel.fromJson(body['data'] as Map<String, dynamic>);
    }

    final message = body['message'] as String? ?? 'Failed to create semester';
    throw Exception(message);
  }

  Future<SemesterModel> update({
    required String semesterId,
    required String name,
    required DateTime startDate,
    required DateTime endDate,
    required String token,
  }) async {
    final uri = Uri.parse('${AppConstants.baseUrl}$_basePath/$semesterId');

    final response = await http.put(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode({
        'name': name,
        'startDate': startDate.toIso8601String(),
        'endDate': endDate.toIso8601String(),
      }),
    );

    final body = jsonDecode(response.body) as Map<String, dynamic>;

    if (response.statusCode == 200 && body['success'] == true) {
      return SemesterModel.fromJson(body['data'] as Map<String, dynamic>);
    }

    final message = body['message'] as String? ?? 'Failed to update semester';
    throw Exception(message);
  }

  Future<void> delete({
    required String semesterId,
    required String token,
  }) async {
    final uri = Uri.parse('${AppConstants.baseUrl}$_basePath/$semesterId');

    final response = await http.delete(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
    );

    if (response.statusCode == 204) return;

    final body = jsonDecode(response.body) as Map<String, dynamic>;
    final message = body['message'] as String? ?? 'Failed to delete semester';
    throw Exception(message);
  }
}
