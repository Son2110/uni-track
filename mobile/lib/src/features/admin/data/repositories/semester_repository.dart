import '../models/semester_model.dart';
import '../services/semester_api_service.dart';
import '../services/semester_graphql_service.dart';

class SemesterRepository {
  final SemesterGraphQLService _graphqlService;
  final SemesterApiService _apiService;

  SemesterRepository({
    SemesterGraphQLService? graphqlService,
    SemesterApiService? apiService,
  }) : _graphqlService = graphqlService ?? SemesterGraphQLService(),
       _apiService = apiService ?? SemesterApiService();

  Future<List<SemesterModel>> getAllSemesters({required String token}) =>
      _graphqlService.getAllSemesters(token: token);

  Future<SemesterModel> create({
    required String name,
    required DateTime startDate,
    required DateTime endDate,
    required String token,
  }) => _apiService.create(
    name: name,
    startDate: startDate,
    endDate: endDate,
    token: token,
  );

  Future<SemesterModel> update({
    required String semesterId,
    required String name,
    required DateTime startDate,
    required DateTime endDate,
    required String token,
  }) => _apiService.update(
    semesterId: semesterId,
    name: name,
    startDate: startDate,
    endDate: endDate,
    token: token,
  );

  Future<void> delete({required String semesterId, required String token}) =>
      _apiService.delete(semesterId: semesterId, token: token);
}
