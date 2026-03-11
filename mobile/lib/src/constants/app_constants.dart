class AppConstants {
  AppConstants._();

  // API Base URL
  // Android emulator: http://10.0.2.2:5068
  // iOS simulator / web / desktop: http://localhost:5068
  static const String baseUrl = 'http://10.0.2.2:5068';

  // Shared Preferences Keys
  static const String tokenKey = 'auth_token';
  static const String tokenExpiryKey = 'auth_token_expiry';
  static const String userIdKey = 'auth_user_id';
  static const String userNameKey = 'auth_user_name';
  static const String userEmailKey = 'auth_user_email';
  static const String userRoleKey = 'auth_user_role';
}
