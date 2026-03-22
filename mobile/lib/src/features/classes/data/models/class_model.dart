class ClassModel {
  final String classId;
  final String semesterId;
  final String semesterName;
  final DateTime? semesterStartDate;
  final DateTime? semesterEndDate;
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
    this.semesterStartDate,
    this.semesterEndDate,
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
    semesterStartDate: _tryParseDate(json['semesterStartDate']),
    semesterEndDate: _tryParseDate(json['semesterEndDate']),
    courseId: json['courseId'].toString(),
    courseCode: json['courseCode'] as String,
    courseName: json['courseName'] as String,
    classCode: json['classCode'] as String,
    teacherId: json['teacherId'].toString(),
    teacherName: json['teacherName'] as String,
    createdAt: DateTime.parse(json['createdAt'] as String),
    updatedAt: DateTime.parse(json['updatedAt'] as String),
  );

  factory ClassModel.fromGraphQL(Map<String, dynamic> json) {
    final semester = json['semester'] as Map<String, dynamic>;
    final course = json['course'] as Map<String, dynamic>;
    final teacher = json['teacher'] as Map<String, dynamic>;
    return ClassModel(
      classId: json['classId'].toString(),
      semesterId: json['semesterId'].toString(),
      semesterName: semester['name'] as String,
      semesterStartDate: _tryParseDate(semester['startDate']),
      semesterEndDate: _tryParseDate(semester['endDate']),
      courseId: json['courseId'].toString(),
      courseCode: course['code'] as String,
      courseName: course['name'] as String,
      classCode: json['classCode'] as String,
      teacherId: json['teacherId'].toString(),
      teacherName: teacher['name'] as String,
      createdAt: DateTime.parse(json['createdAt'] as String),
      updatedAt: DateTime.parse(json['updatedAt'] as String),
    );
  }

  static DateTime? _tryParseDate(dynamic value) {
    if (value == null) return null;
    if (value is String && value.isNotEmpty) {
      return DateTime.tryParse(value);
    }
    return null;
  }
}
