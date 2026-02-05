import 'package:flutter/material.dart';
import '../constants/app_colors.dart';
import '../features/home/presentation/home_screen.dart';
import '../features/workspace/presentation/workspace_screen.dart';
import '../features/profile/presentation/profile_screen.dart';

/// Main app shell with persistent bottom navigation bar
class ScaffoldWithNavBar extends StatefulWidget {
  const ScaffoldWithNavBar({super.key});

  @override
  State<ScaffoldWithNavBar> createState() => _ScaffoldWithNavBarState();
}

class _ScaffoldWithNavBarState extends State<ScaffoldWithNavBar> {
  int _selectedIndex = 0;

  // Define the screens for each tab
  static const List<Widget> _screens = [
    HomeScreen(),
    WorkspaceScreen(),
    ProfileScreen(),
  ];

  void _onItemTapped(int index) {
    setState(() {
      _selectedIndex = index;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: _screens[_selectedIndex],
      bottomNavigationBar: Container(
        decoration: BoxDecoration(
          color: AppColors.navBackground,
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.1),
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
          items: const [
            BottomNavigationBarItem(
              icon: Icon(Icons.home_rounded),
              activeIcon: Icon(Icons.home_rounded, size: 28),
              label: 'Home',
            ),
            BottomNavigationBarItem(
              icon: Icon(Icons.work_rounded),
              activeIcon: Icon(Icons.work_rounded, size: 28),
              label: 'Workspace',
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
