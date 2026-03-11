class NotificationModel {
  final String notificationId;
  final String userId;
  final String title;
  final String message;
  final bool isRead;
  final DateTime createdAt;
  final DateTime? readAt;

  const NotificationModel({
    required this.notificationId,
    required this.userId,
    required this.title,
    required this.message,
    required this.isRead,
    required this.createdAt,
    this.readAt,
  });

  factory NotificationModel.fromGraphQL(Map<String, dynamic> json) =>
      NotificationModel(
        notificationId: json['notificationId'].toString(),
        userId: json['userId'].toString(),
        title: json['title'] as String,
        message: json['message'] as String,
        isRead: json['isRead'] as bool,
        createdAt: DateTime.parse(json['createdAt'] as String),
        readAt: json['readAt'] != null
            ? DateTime.parse(json['readAt'] as String)
            : null,
      );

  NotificationModel copyWith({bool? isRead, DateTime? readAt}) =>
      NotificationModel(
        notificationId: notificationId,
        userId: userId,
        title: title,
        message: message,
        isRead: isRead ?? this.isRead,
        createdAt: createdAt,
        readAt: readAt ?? this.readAt,
      );
}
