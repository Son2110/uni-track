import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../constants/app_constants.dart';
import '../models/class_student_model.dart';
import '../models/project_model.dart';

class ProjectGraphQLService {
  static const _graphqlPath = '/graphql';
  static const _projectsPath = '/api/v1/projects';

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

  static const _getClassStudentsQuery = r'''
    query GetClassStudents($classId: UUID!) {
      classEnrollments(
        where: { classId: { eq: $classId } }
        first: 200
        order: [{ enrolledAt: ASC }]
      ) {
        nodes {
          userId
          user {
            name
            email
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

  Future<List<ClassStudentModel>> getClassStudents({
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
        'query': _getClassStudentsQuery,
        'variables': {'classId': classId},
      }),
    );

    final body = jsonDecode(response.body) as Map<String, dynamic>;

    if (response.statusCode == 200 && !body.containsKey('errors')) {
      final data = body['data'] as Map<String, dynamic>;
      final nodes =
          (data['classEnrollments'] as Map<String, dynamic>)['nodes']
              as List<dynamic>;

      final students = nodes
          .map((e) => ClassStudentModel.fromGraphQL(e as Map<String, dynamic>))
          .toList();

      students.sort(
        (a, b) => a.name.toLowerCase().compareTo(b.name.toLowerCase()),
      );
      return students;
    }

    final errors = body['errors'];
    final message = errors != null
        ? (errors[0] as Map<String, dynamic>)['message'] as String? ??
              'Failed to load students'
        : 'Failed to load students';
    throw Exception(message);
  }

  Future<String> createProject({
    required String classId,
    required String name,
    String? description,
    required String token,
  }) async {
    final uri = Uri.parse('${AppConstants.baseUrl}$_projectsPath');

    final response = await http.post(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode({
        'classId': classId,
        'name': name,
        'description': description,
      }),
    );

    final body = jsonDecode(response.body) as Map<String, dynamic>;

    if ((response.statusCode == 200 || response.statusCode == 201) &&
        body['success'] == true) {
      final data = body['data'] as Map<String, dynamic>;
      return data['projectId'].toString();
    }

    final message = body['message'] as String? ?? 'Failed to create project';
    throw Exception(message);
  }

  Future<void> addMemberToProject({
    required String projectId,
    required String userId,
    required String token,
  }) async {
    final uri = Uri.parse(
      '${AppConstants.baseUrl}$_projectsPath/$projectId/members',
    );

    final response = await http.post(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode({'userId': userId}),
    );

    if (response.statusCode == 200 || response.statusCode == 201) {
      return;
    }

    final rawBody = response.body.trim();
    if (rawBody.isEmpty) {
      throw Exception('Failed to assign student to project');
    }

    final body = jsonDecode(rawBody) as Map<String, dynamic>;
    final message = body['message'] as String? ?? 'Failed to assign student';
    throw Exception(message);
  }
}
