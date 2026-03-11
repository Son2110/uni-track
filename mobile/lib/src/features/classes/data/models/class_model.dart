class ClassModel {
  final String classId;
  final String semesterId;
  final String semesterName;
  final String courseId;
  final String courseCode;
  final String courseName;
  final String classCode;
  final String teacherId;
  final String teacherName;
  final DateTime createdAt;
  final DateTime updatedAt;

  const ClassModel({
    required this.classId,
    required this.semesterId,
    required this.semesterName,
    required this.courseId,
    required this.courseCode,
    required this.courseName,
    required this.classCode,
    required this.teacherId,
    required this.teacherName,
    required this.createdAt,
    required this.updatedAt,
  });

  factory ClassModel.fromJson(Map<String, dynamic> json) => ClassModel(
    classId: json['classId'].toString(),
    semesterId: json['semesterId'].toString(),
    semesterName: json['semesterName'] as String,
    courseId: json['courseId'].toString(),
    courseCode: json['courseCode'] as String,
    courseName: json['courseName'] as String,
    classCode: json['classCode'] as String,
    teacherId: json['teacherId'].toString(),
    teacherName: json['teacherName'] as String,
    createdAt: DateTime.parse(json['createdAt'] as String),
    updatedAt: DateTime.parse(json['updatedAt'] as String),
  );
}
