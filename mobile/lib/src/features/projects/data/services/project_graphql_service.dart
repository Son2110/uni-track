import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../constants/app_constants.dart';
import '../models/project_model.dart';

class ProjectGraphQLService {
  static const _graphqlPath = '/graphql';

  static const _getClassProjectsQuery = r'''
    query GetClassProjects($classId: UUID!) {
      projects(
        where: { classId: { eq: $classId } }
        first: 50
      ) {
        nodes {
          projectId
          classId
          name
          description
          createdAt
          updatedAt
          projectMembers {
            userId
          }
        }
      }
    }
  ''';

  Future<List<ProjectModel>> getClassProjects({
    required String classId,
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
        'query': _getClassProjectsQuery,
        'variables': {'classId': classId},
      }),
    );

    final body = jsonDecode(response.body) as Map<String, dynamic>;

    if (response.statusCode == 200 && !body.containsKey('errors')) {
      final data = body['data'] as Map<String, dynamic>;
      final nodes =
          (data['projects'] as Map<String, dynamic>)['nodes'] as List<dynamic>;
      return nodes
          .map((e) => ProjectModel.fromGraphQL(e as Map<String, dynamic>))
          .toList();
    }

    final errors = body['errors'];
    final message = errors != null
        ? (errors[0] as Map<String, dynamic>)['message'] as String? ??
              'Failed to load projects'
        : 'Failed to load projects';
    throw Exception(message);
  }
}
