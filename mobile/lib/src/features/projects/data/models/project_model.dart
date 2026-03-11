class ProjectModel {
  final String projectId;
  final String classId;
  final String name;
  final String? description;
  final int memberCount;
  final DateTime createdAt;
  final DateTime updatedAt;

  const ProjectModel({
    required this.projectId,
    required this.classId,
    required this.name,
    this.description,
    required this.memberCount,
    required this.createdAt,
    required this.updatedAt,
  });

  factory ProjectModel.fromGraphQL(Map<String, dynamic> json) {
    final members = json['projectMembers'] as List<dynamic>? ?? [];
    return ProjectModel(
      projectId: json['projectId'].toString(),
      classId: json['classId'].toString(),
      name: json['name'] as String,
      description: json['description'] as String?,
      memberCount: members.length,
      createdAt: DateTime.parse(json['createdAt'] as String),
      updatedAt: DateTime.parse(json['updatedAt'] as String),
    );
  }
}
