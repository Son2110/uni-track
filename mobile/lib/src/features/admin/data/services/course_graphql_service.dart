import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../constants/app_constants.dart';
import '../models/course_model.dart';

class CourseGraphQLService {
  static const _graphqlPath = '/graphql';

  static const _getAllCoursesQuery = r'''
    query GetAllCourses {
      courses(
        first: 100
        order: [{ name: ASC }]
      ) {
        nodes {
          courseId
          code
          name
          description
          createdAt
          updatedAt
        }
      }
    }
  ''';

  Future<List<CourseModel>> getAllCourses({required String token}) async {
    final uri = Uri.parse('${AppConstants.baseUrl}$_graphqlPath');

    final response = await http.post(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode({'query': _getAllCoursesQuery}),
    );

    final body = jsonDecode(response.body) as Map<String, dynamic>;

    if (response.statusCode == 200 && !body.containsKey('errors')) {
      final data = body['data'] as Map<String, dynamic>;
      final nodes =
          (data['courses'] as Map<String, dynamic>)['nodes'] as List<dynamic>;
      return nodes
          .map((e) => CourseModel.fromGraphQL(e as Map<String, dynamic>))
          .toList();
    }

    final errors = body['errors'];
    final message = errors != null
        ? (errors[0] as Map<String, dynamic>)['message'] as String? ??
              'Failed to load courses'
        : 'Failed to load courses';
    throw Exception(message);
  }
}
