class ClassStudentModel {
  final String userId;
  final String name;
  final String email;

  const ClassStudentModel({
    required this.userId,
    required this.name,
    required this.email,
  });

  factory ClassStudentModel.fromGraphQL(Map<String, dynamic> json) {
    final user = json['user'] as Map<String, dynamic>? ?? const {};
    return ClassStudentModel(
      userId: json['userId'].toString(),
      name: (user['name'] as String?) ?? 'Unknown student',
      email: (user['email'] as String?) ?? '',
    );
  }
}
