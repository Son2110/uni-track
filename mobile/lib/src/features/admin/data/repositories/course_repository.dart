import '../models/course_model.dart';
import '../services/course_api_service.dart';
import '../services/course_graphql_service.dart';

class CourseRepository {
  final CourseGraphQLService _graphqlService;
  final CourseApiService _apiService;

  CourseRepository({
    CourseGraphQLService? graphqlService,
    CourseApiService? apiService,
  }) : _graphqlService = graphqlService ?? CourseGraphQLService(),
       _apiService = apiService ?? CourseApiService();

  Future<List<CourseModel>> getAllCourses({required String token}) =>
      _graphqlService.getAllCourses(token: token);

  Future<CourseModel> create({
    required String code,
    required String name,
    String? description,
    required String token,
  }) => _apiService.create(
    code: code,
    name: name,
    description: description,
    token: token,
  );

  Future<CourseModel> update({
    required String courseId,
    required String code,
    required String name,
    String? description,
    required String token,
  }) => _apiService.update(
    courseId: courseId,
    code: code,
    name: name,
    description: description,
    token: token,
  );

  Future<void> delete({required String courseId, required String token}) =>
      _apiService.delete(courseId: courseId, token: token);
}
