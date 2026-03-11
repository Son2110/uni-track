import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../constants/app_constants.dart';
import '../models/auth_models.dart';

class AuthApiService {
  static const _loginPath = '/api/v1/auth/login';

  Future<AuthUser> login(LoginRequest request) async {
    final uri = Uri.parse('${AppConstants.baseUrl}$_loginPath');

    final response = await http.post(
      uri,
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(request.toJson()),
    );

    final body = jsonDecode(response.body) as Map<String, dynamic>;

    if (response.statusCode == 200 && body['success'] == true) {
      return AuthUser.fromJson(body['data'] as Map<String, dynamic>);
    }

    final message = body['message'] as String? ?? 'Login failed';
    throw Exception(message);
  }
}
