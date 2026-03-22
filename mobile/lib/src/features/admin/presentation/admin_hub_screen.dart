import 'package:flutter/material.dart';
import '../../../constants/app_colors.dart';
import '../../auth/data/models/auth_models.dart';
import 'classes/admin_classes_screen.dart';
import 'courses/courses_management_screen.dart';
import 'semesters/semesters_management_screen.dart';

class AdminHubScreen extends StatelessWidget {
  final AuthUser currentUser;

  const AdminHubScreen({super.key, required this.currentUser});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text(
          'Management',
          style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
        ),
        backgroundColor: AppColors.secondary,
        elevation: 0,
      ),
      body: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          const SizedBox(height: 8),
          _SectionHeader(
            icon: Icons.admin_panel_settings_rounded,
            label: 'Admin Tools',
            subtitle: 'Manage semesters, courses & classes',
          ),
          const SizedBox(height: 20),
          _ManagementCard(
            icon: Icons.calendar_month_rounded,
            color: const Color(0xFF7C3AED),
            title: 'Semesters',
            subtitle: 'Create, edit and remove semesters',
            onTap: () => Navigator.push(
              context,
              MaterialPageRoute(
                builder: (_) =>
                    SemestersManagementScreen(currentUser: currentUser),
              ),
            ),
          ),
          const SizedBox(height: 14),
          _ManagementCard(
            icon: Icons.book_rounded,
            color: const Color(0xFF059669),
            title: 'Courses',
            subtitle: 'Create, edit and remove courses',
            onTap: () => Navigator.push(
              context,
              MaterialPageRoute(
                builder: (_) =>
                    CoursesManagementScreen(currentUser: currentUser),
              ),
            ),
          ),
          const SizedBox(height: 14),
          _ManagementCard(
            icon: Icons.class_rounded,
            color: AppColors.secondary,
            title: 'Classes',
            subtitle: 'Create, edit and remove classes',
            onTap: () => Navigator.push(
              context,
              MaterialPageRoute(
                builder: (_) => AdminClassesScreen(currentUser: currentUser),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _SectionHeader extends StatelessWidget {
  final IconData icon;
  final String label;
  final String subtitle;

  const _SectionHeader({
    required this.icon,
    required this.label,
    required this.subtitle,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Container(
          width: 48,
          height: 48,
          decoration: BoxDecoration(
            color: AppColors.secondary.withValues(alpha: 0.12),
            borderRadius: BorderRadius.circular(12),
          ),
          child: Icon(icon, color: AppColors.secondary, size: 26),
        ),
        const SizedBox(width: 14),
        Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              label,
              style: const TextStyle(
                fontSize: 20,
                fontWeight: FontWeight.bold,
                color: AppColors.textPrimary,
              ),
            ),
            Text(
              subtitle,
              style: const TextStyle(
                fontSize: 13,
                color: AppColors.textSecondary,
              ),
            ),
          ],
        ),
      ],
    );
  }
}

class _ManagementCard extends StatelessWidget {
  final IconData icon;
  final Color color;
  final String title;
  final String subtitle;
  final VoidCallback onTap;

  const _ManagementCard({
    required this.icon,
    required this.color,
    required this.title,
    required this.subtitle,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(16),
      child: Container(
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(16),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.05),
              blurRadius: 12,
              offset: const Offset(0, 4),
            ),
          ],
        ),
        child: Padding(
          padding: const EdgeInsets.all(18),
          child: Row(
            children: [
              Container(
                width: 52,
                height: 52,
                decoration: BoxDecoration(
                  color: color.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(14),
                ),
                child: Icon(icon, color: color, size: 28),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      title,
                      style: const TextStyle(
                        fontSize: 17,
                        fontWeight: FontWeight.bold,
                        color: AppColors.textPrimary,
                      ),
                    ),
                    const SizedBox(height: 3),
                    Text(
                      subtitle,
                      style: const TextStyle(
                        fontSize: 13,
                        color: AppColors.textSecondary,
                      ),
                    ),
                  ],
                ),
              ),
              const Icon(
                Icons.chevron_right_rounded,
                color: AppColors.textDisabled,
              ),
            ],
          ),
        ),
      ),
    );
  }
}
