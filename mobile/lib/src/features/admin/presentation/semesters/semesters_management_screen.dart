import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../../constants/app_colors.dart';
import '../../../auth/data/models/auth_models.dart';
import '../../data/models/semester_model.dart';
import '../../data/repositories/semester_repository.dart';
import 'semester_form_screen.dart';

class SemestersManagementScreen extends StatefulWidget {
  final AuthUser currentUser;

  const SemestersManagementScreen({super.key, required this.currentUser});

  @override
  State<SemestersManagementScreen> createState() =>
      _SemestersManagementScreenState();
}

class _SemestersManagementScreenState extends State<SemestersManagementScreen> {
  final _repository = SemesterRepository();
  late Future<List<SemesterModel>> _semestersFuture;

  @override
  void initState() {
    super.initState();
    _load();
  }

  void _load() {
    _semestersFuture = _repository.getAllSemesters(
      token: widget.currentUser.token,
    );
  }

  Future<void> _refresh() async {
    setState(() => _load());
    await _semestersFuture;
  }

  Future<void> _openForm({SemesterModel? existing}) async {
    final saved = await Navigator.push<bool>(
      context,
      MaterialPageRoute(
        builder: (_) => SemesterFormScreen(
          currentUser: widget.currentUser,
          existing: existing,
        ),
      ),
    );
    if (saved == true) _refresh();
  }

  Future<void> _confirmDelete(SemesterModel semester) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Delete Semester'),
        content: Text(
          'Are you sure you want to delete "${semester.name}"? This cannot be undone.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Cancel'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            style: TextButton.styleFrom(foregroundColor: AppColors.error),
            child: const Text('Delete'),
          ),
        ],
      ),
    );

    if (confirmed != true) return;

    try {
      await _repository.delete(
        semesterId: semester.semesterId,
        token: widget.currentUser.token,
      );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Semester deleted'),
          backgroundColor: AppColors.success,
        ),
      );
      _refresh();
    } catch (e) {
      if (!mounted) return;
      String msg = e.toString();
      if (msg.startsWith('Exception: ')) msg = msg.substring(11);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(msg), backgroundColor: AppColors.error),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text(
          'Semesters',
          style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
        ),
        backgroundColor: AppColors.secondary,
        elevation: 0,
        iconTheme: const IconThemeData(color: Colors.white),
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _openForm(),
        backgroundColor: AppColors.secondary,
        foregroundColor: Colors.white,
        icon: const Icon(Icons.add_rounded),
        label: const Text('New Semester'),
      ),
      body: FutureBuilder<List<SemesterModel>>(
        future: _semestersFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(
              child: CircularProgressIndicator(color: AppColors.secondary),
            );
          }

          if (snapshot.hasError) {
            String msg = snapshot.error.toString();
            if (msg.startsWith('Exception: ')) msg = msg.substring(11);
            return _ErrorView(message: msg, onRetry: _refresh);
          }

          final semesters = snapshot.data ?? [];

          if (semesters.isEmpty) {
            return _EmptyView(onAdd: () => _openForm());
          }

          return RefreshIndicator(
            onRefresh: _refresh,
            color: AppColors.secondary,
            child: ListView.separated(
              padding: const EdgeInsets.fromLTRB(16, 16, 16, 100),
              itemCount: semesters.length,
              separatorBuilder: (_, _) => const SizedBox(height: 10),
              itemBuilder: (context, index) => _SemesterCard(
                semester: semesters[index],
                onEdit: () => _openForm(existing: semesters[index]),
                onDelete: () => _confirmDelete(semesters[index]),
              ),
            ),
          );
        },
      ),
    );
  }
}

class _SemesterCard extends StatelessWidget {
  final SemesterModel semester;
  final VoidCallback onEdit;
  final VoidCallback onDelete;

  const _SemesterCard({
    required this.semester,
    required this.onEdit,
    required this.onDelete,
  });

  @override
  Widget build(BuildContext context) {
    final fmt = DateFormat('MMM d, yyyy');
    return Container(
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(14),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.05),
            blurRadius: 10,
            offset: const Offset(0, 3),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        child: Row(
          children: [
            Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                color: const Color(0xFF7C3AED).withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(10),
              ),
              child: const Icon(
                Icons.calendar_month_rounded,
                color: Color(0xFF7C3AED),
                size: 22,
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    semester.name,
                    style: const TextStyle(
                      fontSize: 15,
                      fontWeight: FontWeight.bold,
                      color: AppColors.textPrimary,
                    ),
                  ),
                  const SizedBox(height: 3),
                  Text(
                    '${fmt.format(semester.startDate)} – ${fmt.format(semester.endDate)}',
                    style: const TextStyle(
                      fontSize: 13,
                      color: AppColors.textSecondary,
                    ),
                  ),
                ],
              ),
            ),
            IconButton(
              onPressed: onEdit,
              icon: const Icon(Icons.edit_rounded, size: 20),
              color: AppColors.secondary,
              tooltip: 'Edit',
            ),
            IconButton(
              onPressed: onDelete,
              icon: const Icon(Icons.delete_outline_rounded, size: 20),
              color: AppColors.error,
              tooltip: 'Delete',
            ),
          ],
        ),
      ),
    );
  }
}

class _ErrorView extends StatelessWidget {
  final String message;
  final VoidCallback onRetry;

  const _ErrorView({required this.message, required this.onRetry});

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(
              Icons.error_outline_rounded,
              size: 56,
              color: AppColors.error,
            ),
            const SizedBox(height: 16),
            Text(
              message,
              textAlign: TextAlign.center,
              style: const TextStyle(color: AppColors.textSecondary),
            ),
            const SizedBox(height: 24),
            ElevatedButton.icon(
              onPressed: onRetry,
              icon: const Icon(Icons.refresh_rounded),
              label: const Text('Retry'),
              style: ElevatedButton.styleFrom(
                backgroundColor: AppColors.secondary,
                foregroundColor: Colors.white,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _EmptyView extends StatelessWidget {
  final VoidCallback onAdd;

  const _EmptyView({required this.onAdd});

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            Icons.calendar_month_outlined,
            size: 72,
            color: AppColors.textDisabled,
          ),
          const SizedBox(height: 16),
          const Text(
            'No semesters yet',
            style: TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.w600,
              color: AppColors.textPrimary,
            ),
          ),
          const SizedBox(height: 8),
          const Text(
            'Tap the button below to add one.',
            style: TextStyle(color: AppColors.textSecondary),
          ),
          const SizedBox(height: 24),
          ElevatedButton.icon(
            onPressed: onAdd,
            icon: const Icon(Icons.add_rounded),
            label: const Text('Add Semester'),
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.secondary,
              foregroundColor: Colors.white,
            ),
          ),
        ],
      ),
    );
  }
}
