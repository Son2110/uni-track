import 'package:flutter/material.dart';
import '../constants/app_colors.dart';
import '../features/auth/data/models/auth_models.dart';
import '../features/classes/presentation/teacher_classes_screen.dart';
import '../features/notifications/data/repositories/notification_repository.dart';
import '../features/notifications/presentation/notifications_screen.dart';
import '../features/profile/presentation/profile_screen.dart';

/// Main app shell with persistent bottom navigation bar
class ScaffoldWithNavBar extends StatefulWidget {
  final AuthUser currentUser;
  final VoidCallback onLogout;

  const ScaffoldWithNavBar({
    super.key,
    required this.currentUser,
    required this.onLogout,
  });

  @override
  State<ScaffoldWithNavBar> createState() => _ScaffoldWithNavBarState();
}

class _ScaffoldWithNavBarState extends State<ScaffoldWithNavBar> {
  int _selectedIndex = 0;
  final _notificationRepository = NotificationRepository();
  int _unreadCount = 0;

  @override
  void initState() {
    super.initState();
    _loadUnreadCount();
  }

  Future<void> _loadUnreadCount() async {
    try {
      final notifications = await _notificationRepository.getMyNotifications(
        userId: widget.currentUser.userId,
        token: widget.currentUser.token,
      );
      if (mounted) {
        setState(() {
          _unreadCount = notifications.where((n) => !n.isRead).length;
        });
      }
    } catch (_) {
      // badge is non-critical, silently ignore errors
    }
  }

  void _onItemTapped(int index) {
    setState(() {
      _selectedIndex = index;
    });
    // Refresh unread count when switching away from notifications tab
    if (index != 1) _loadUnreadCount();
  }

  @override
  Widget build(BuildContext context) {
    final screens = [
      TeacherClassesScreen(currentUser: widget.currentUser),
      NotificationsScreen(currentUser: widget.currentUser),
      ProfileScreen(currentUser: widget.currentUser, onLogout: widget.onLogout),
    ];

    return Scaffold(
      body: screens[_selectedIndex],
      bottomNavigationBar: Container(
        decoration: BoxDecoration(
          color: AppColors.navBackground,
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.1),
              blurRadius: 8,
              offset: const Offset(0, -2),
            ),
          ],
        ),
        child: BottomNavigationBar(
          currentIndex: _selectedIndex,
          onTap: _onItemTapped,
          backgroundColor: AppColors.navBackground,
          selectedItemColor: AppColors.navSelected,
          unselectedItemColor: AppColors.navUnselected,
          selectedFontSize: 12,
          unselectedFontSize: 12,
          type: BottomNavigationBarType.fixed,
          elevation: 0,
          selectedLabelStyle: const TextStyle(fontWeight: FontWeight.w600),
          items: [
            BottomNavigationBarItem(
              icon: Icon(Icons.class_outlined),
              activeIcon: Icon(Icons.class_rounded, size: 28),
              label: 'My Classes',
            ),
            BottomNavigationBarItem(
              icon: Badge(
                isLabelVisible: _unreadCount > 0,
                label: Text(
                  _unreadCount > 99 ? '99+' : '$_unreadCount',
                  style: const TextStyle(fontSize: 10),
                ),
                child: const Icon(Icons.notifications_outlined),
              ),
              activeIcon: Badge(
                isLabelVisible: _unreadCount > 0,
                label: Text(
                  _unreadCount > 99 ? '99+' : '$_unreadCount',
                  style: const TextStyle(fontSize: 10),
                ),
                child: const Icon(Icons.notifications_rounded, size: 28),
              ),
              label: 'Notifications',
            ),
            BottomNavigationBarItem(
              icon: Icon(Icons.person_rounded),
              activeIcon: Icon(Icons.person_rounded, size: 28),
              label: 'Profile',
            ),
          ],
        ),
      ),
    );
  }
}
