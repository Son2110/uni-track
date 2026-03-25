import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../constants/app_constants.dart';
import '../models/github_contribution_model.dart';

class GithubContributionService {
  Future<GithubContributionData> getProjectContributions({
    required String projectId,
    required String token,
  }) async {
    final contributionsUri = Uri.parse(
      '${AppConstants.baseUrl}/api/v1/projects/$projectId/github-contributions',
    );

    final contributionsResponse = await http.get(
      contributionsUri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
    );

    final contributionsBody =
        jsonDecode(contributionsResponse.body) as Map<String, dynamic>;

    if (contributionsResponse.statusCode == 200 &&
        contributionsBody['success'] == true) {
      final data = contributionsBody['data'] as Map<String, dynamic>;
      return GithubContributionData.fromJson(data);
    }

    if (contributionsResponse.statusCode == 404) {
      throw Exception('No GitHub contributions found for this project');
    }

    final message =
        contributionsBody['message'] as String? ??
        'Failed to load contributions';
    throw Exception(message);
  }

  Future<List<GithubContributionReportSummary>> getProjectReports({
    required String projectId,
    required String token,
    int take = 100,
  }) async {
    final uri = Uri.parse(
      '${AppConstants.baseUrl}/api/v1/projects/$projectId/github-reports?take=$take',
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
      final data = body['data'] as List<dynamic>? ?? const [];
      final reports = data
          .whereType<Map<String, dynamic>>()
          .map(GithubContributionReportSummary.fromJson)
          .toList();

      reports.sort((a, b) => b.createdAt.compareTo(a.createdAt));
      return reports;
    }

    final message =
        body['message'] as String? ?? 'Failed to load GitHub reports';
    throw Exception(message);
  }

  Future<GithubContributionReportDetail> getProjectReportDetail({
    required String projectId,
    required String reportId,
    required String token,
  }) async {
    final uri = Uri.parse(
      '${AppConstants.baseUrl}/api/v1/projects/$projectId/github-reports/$reportId',
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
      return GithubContributionReportDetail.fromJson(data);
    }

    final message =
        body['message'] as String? ?? 'Failed to load report detail';
    throw Exception(message);
  }
}
