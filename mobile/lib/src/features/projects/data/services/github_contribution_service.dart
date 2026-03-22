import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../constants/app_constants.dart';
import '../models/github_contribution_model.dart';

class GithubContributionService {
  Future<GithubContributionData> getProjectContributions({
    required String projectId,
    required String token,
  }) async {
    final uri = Uri.parse(
      '${AppConstants.baseUrl}/api/v1/projects/$projectId/github-contributions',
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
      return GithubContributionData.fromJson(data);
    }

    if (response.statusCode == 404) {
      throw Exception('No GitHub contributions found for this project');
    }

    final message =
        body['message'] as String? ?? 'Failed to load contributions';
    throw Exception(message);
  }
}
