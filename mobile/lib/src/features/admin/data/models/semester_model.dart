class SemesterModel {
  final String semesterId;
  final String name;
  final DateTime startDate;
  final DateTime endDate;
  final DateTime createdAt;
  final DateTime updatedAt;

  const SemesterModel({
    required this.semesterId,
    required this.name,
    required this.startDate,
    required this.endDate,
    required this.createdAt,
    required this.updatedAt,
  });

  factory SemesterModel.fromGraphQL(Map<String, dynamic> json) => SemesterModel(
    semesterId: json['semesterId'].toString(),
    name: json['name'] as String,
    startDate: DateTime.parse(json['startDate'] as String),
    endDate: DateTime.parse(json['endDate'] as String),
    createdAt: DateTime.parse(json['createdAt'] as String),
    updatedAt: DateTime.parse(json['updatedAt'] as String),
  );

  factory SemesterModel.fromJson(Map<String, dynamic> json) => SemesterModel(
    semesterId: json['semesterId'].toString(),
    name: json['name'] as String,
    startDate: DateTime.parse(json['startDate'] as String),
    endDate: DateTime.parse(json['endDate'] as String),
    createdAt: DateTime.parse(json['createdAt'] as String),
    updatedAt: DateTime.parse(json['updatedAt'] as String),
  );
}
