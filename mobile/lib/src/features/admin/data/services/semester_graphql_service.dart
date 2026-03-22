import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../constants/app_constants.dart';
import '../models/semester_model.dart';

class SemesterGraphQLService {
  static const _graphqlPath = '/graphql';

  static const _getAllSemestersQuery = r'''
    query GetAllSemesters {
      semesters(
        first: 100
        order: [{ startDate: DESC }]
      ) {
        nodes {
          semesterId
          name
          startDate
          endDate
          createdAt
          updatedAt
        }
      }
    }
  ''';

  Future<List<SemesterModel>> getAllSemesters({required String token}) async {
    final uri = Uri.parse('${AppConstants.baseUrl}$_graphqlPath');

    final response = await http.post(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode({'query': _getAllSemestersQuery}),
    );

    final body = jsonDecode(response.body) as Map<String, dynamic>;

    if (response.statusCode == 200 && !body.containsKey('errors')) {
      final data = body['data'] as Map<String, dynamic>;
      final nodes =
          (data['semesters'] as Map<String, dynamic>)['nodes'] as List<dynamic>;
      return nodes
          .map((e) => SemesterModel.fromGraphQL(e as Map<String, dynamic>))
          .toList();
    }

    final errors = body['errors'];
    final message = errors != null
        ? (errors[0] as Map<String, dynamic>)['message'] as String? ??
              'Failed to load semesters'
        : 'Failed to load semesters';
    throw Exception(message);
  }
}
