class AppConstants {
  AppConstants._();

  // API Base URL
  // Deployed backend (Azure App Service)
  static const String baseUrl =
      'https://unitrackpmss-bkhqhkazfpc4dchr.southeastasia-01.azurewebsites.net';

  // Shared Preferences Keys
  static const String tokenKey = 'auth_token';
  static const String tokenExpiryKey = 'auth_token_expiry';
  static const String userIdKey = 'auth_user_id';
  static const String userNameKey = 'auth_user_name';
  static const String userEmailKey = 'auth_user_email';
  static const String userRoleKey = 'auth_user_role';
}
