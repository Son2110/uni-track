import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../constants/app_constants.dart';
import '../../../classes/data/models/class_model.dart';

class AdminClassGraphQLService {
  static const _graphqlPath = '/graphql';

  static const _getAllClassesQuery = r'''
    query GetAllClasses {
      classes(
        first: 200
        order: [{ createdAt: DESC }]
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

  Future<List<ClassModel>> getAllClasses({required String token}) async {
    final uri = Uri.parse('${AppConstants.baseUrl}$_graphqlPath');

    final response = await http.post(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode({'query': _getAllClassesQuery}),
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
