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

  static String _readString(
    Map<String, dynamic> json,
    List<String> keys, {
    String fallback = '',
  }) {
    for (final key in keys) {
      final value = json[key];
      if (value == null) continue;
      final text = value.toString().trim();
      if (text.isNotEmpty) return text;
    }
    return fallback;
  }

  static DateTime _parseDate(dynamic value, {DateTime? fallback}) {
    if (value is String && value.isNotEmpty) {
      return DateTime.tryParse(value) ?? fallback ?? DateTime.now();
    }
    return fallback ?? DateTime.now();
  }

  factory NotificationModel.fromGraphQL(Map<String, dynamic> json) =>
      NotificationModel(
        notificationId: _readString(json, const [
          'notificationId',
          'id',
        ], fallback: 'unknown'),
        userId: _readString(json, const ['userId'], fallback: 'unknown'),
        title: _readString(json, const [
          'title',
          'subject',
        ], fallback: 'Notification'),
        message: _readString(json, const [
          'message',
          'content',
          'body',
          'description',
        ], fallback: 'No message content.'),
        isRead: json['isRead'] as bool? ?? false,
        createdAt: _parseDate(json['createdAt']),
        readAt: json['readAt'] != null ? _parseDate(json['readAt']) : null,
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
