import '../../../classes/data/models/class_model.dart';
import '../services/admin_class_api_service.dart';
import '../services/admin_class_graphql_service.dart';

class AdminClassRepository {
  final AdminClassGraphQLService _graphqlService;
  final AdminClassApiService _apiService;

  AdminClassRepository({
    AdminClassGraphQLService? graphqlService,
    AdminClassApiService? apiService,
  }) : _graphqlService = graphqlService ?? AdminClassGraphQLService(),
       _apiService = apiService ?? AdminClassApiService();

  Future<List<ClassModel>> getAllClasses({required String token}) =>
      _graphqlService.getAllClasses(token: token);

  Future<ClassModel> create({
    required String semesterId,
    required String courseId,
    required String classCode,
    required String teacherId,
    required String token,
  }) => _apiService.create(
    semesterId: semesterId,
    courseId: courseId,
    classCode: classCode,
    teacherId: teacherId,
    token: token,
  );

  Future<ClassModel> update({
    required String classId,
    required String semesterId,
    required String courseId,
    required String classCode,
    required String teacherId,
    required String token,
  }) => _apiService.update(
    classId: classId,
    semesterId: semesterId,
    courseId: courseId,
    classCode: classCode,
    teacherId: teacherId,
    token: token,
  );

  Future<void> delete({required String classId, required String token}) =>
      _apiService.delete(classId: classId, token: token);
}
