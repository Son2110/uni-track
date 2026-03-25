import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../constants/app_constants.dart';
import '../models/class_model.dart';

class ClassGraphQLService {
  static const _graphqlPath = '/graphql';

  static const _getTeacherClassesQuery = r'''
    query GetTeacherClasses($teacherId: UUID!) {
      classes(
        where: { teacherId: { eq: $teacherId } }
        first: 50
      ) {
        nodes {
          classId
          classCode
          semesterId
          courseId
          teacherId
          createdAt
          updatedAt
          semester {
            name
            startDate
            endDate
          }
          course {
            name
            code
          }
          teacher {
            name
          }
        }
      }
    }
  ''';

  Future<List<ClassModel>> getTeacherClasses({
    required String teacherId,
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
        'query': _getTeacherClassesQuery,
        'variables': {'teacherId': teacherId},
      }),
    );

    final body = jsonDecode(response.body) as Map<String, dynamic>;

    if (response.statusCode == 200 && !body.containsKey('errors')) {
      final data = body['data'] as Map<String, dynamic>;
      final nodes =
          (data['classes'] as Map<String, dynamic>)['nodes'] as List<dynamic>;
      return nodes
          .map((e) => ClassModel.fromGraphQL(e as Map<String, dynamic>))
          .toList();
    }

    final errors = body['errors'];
    final message = errors != null
        ? (errors[0] as Map<String, dynamic>)['message'] as String? ??
              'Failed to load classes'
        : 'Failed to load classes';
    throw Exception(message);
  }
}
