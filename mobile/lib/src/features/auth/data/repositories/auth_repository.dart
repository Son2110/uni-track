import 'package:shared_preferences/shared_preferences.dart';
import '../../../../constants/app_constants.dart';
import '../models/auth_models.dart';
import '../services/auth_api_service.dart';

class AuthRepository {
  final AuthApiService _apiService;

  AuthRepository({AuthApiService? apiService})
    : _apiService = apiService ?? AuthApiService();

  Future<AuthUser> login(String email, String password) async {
    final user = await _apiService.login(
      LoginRequest(email: email, password: password),
    );
    await _persistUser(user);
    return user;
  }

  Future<void> logout() async {
    final prefs = await SharedPreferences.getInstance();
    await Future.wait([
      prefs.remove(AppConstants.tokenKey),
      prefs.remove(AppConstants.tokenExpiryKey),
      prefs.remove(AppConstants.userIdKey),
      prefs.remove(AppConstants.userNameKey),
      prefs.remove(AppConstants.userEmailKey),
      prefs.remove(AppConstants.userRoleKey),
    ]);
  }

  /// Returns the saved user if a valid (non-expired) token exists, else null.
  Future<AuthUser?> getSavedUser() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString(AppConstants.tokenKey);
    final expiryStr = prefs.getString(AppConstants.tokenExpiryKey);
    if (token == null || expiryStr == null) return null;

    final expiry = DateTime.parse(expiryStr);
    if (DateTime.now().isAfter(expiry)) {
      await logout();
      return null;
    }

    return AuthUser(
      token: token,
      expiresAt: expiry,
      userId: prefs.getString(AppConstants.userIdKey) ?? '',
      name: prefs.getString(AppConstants.userNameKey) ?? '',
      email: prefs.getString(AppConstants.userEmailKey) ?? '',
      role: prefs.getString(AppConstants.userRoleKey) ?? '',
    );
  }

  Future<void> _persistUser(AuthUser user) async {
    final prefs = await SharedPreferences.getInstance();
    await Future.wait([
      prefs.setString(AppConstants.tokenKey, user.token),
      prefs.setString(
        AppConstants.tokenExpiryKey,
        user.expiresAt.toIso8601String(),
      ),
      prefs.setString(AppConstants.userIdKey, user.userId),
      prefs.setString(AppConstants.userNameKey, user.name),
      prefs.setString(AppConstants.userEmailKey, user.email),
      prefs.setString(AppConstants.userRoleKey, user.role),
    ]);
  }
}
