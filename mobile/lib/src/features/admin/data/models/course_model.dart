class CourseModel {
  final String courseId;
  final String code;
  final String name;
  final String? description;
  final DateTime createdAt;
  final DateTime updatedAt;

  const CourseModel({
    required this.courseId,
    required this.code,
    required this.name,
    this.description,
    required this.createdAt,
    required this.updatedAt,
  });

  factory CourseModel.fromGraphQL(Map<String, dynamic> json) => CourseModel(
    courseId: json['courseId'].toString(),
    code: json['code'] as String,
    name: json['name'] as String,
    description: json['description'] as String?,
    createdAt: DateTime.parse(json['createdAt'] as String),
    updatedAt: DateTime.parse(json['updatedAt'] as String),
  );

  factory CourseModel.fromJson(Map<String, dynamic> json) => CourseModel(
    courseId: json['courseId'].toString(),
    code: json['code'] as String,
    name: json['name'] as String,
    description: json['description'] as String?,
    createdAt: DateTime.parse(json['createdAt'] as String),
    updatedAt: DateTime.parse(json['updatedAt'] as String),
  );
}
