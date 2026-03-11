class LoginRequest {
  final String email;
  final String password;

  const LoginRequest({required this.email, required this.password});

  Map<String, dynamic> toJson() => {'email': email, 'password': password};
}

class AuthUser {
  final String token;
  final DateTime expiresAt;
  final String userId;
  final String name;
  final String email;
  final String role;

  const AuthUser({
    required this.token,
    required this.expiresAt,
    required this.userId,
    required this.name,
    required this.email,
    required this.role,
  });

  factory AuthUser.fromJson(Map<String, dynamic> json) => AuthUser(
    token: json['token'] as String,
    expiresAt: DateTime.parse(json['expiresAt'] as String),
    userId: json['userId'].toString(),
    name: json['name'] as String,
    email: json['email'] as String,
    role: json['role'] as String,
  );
}
