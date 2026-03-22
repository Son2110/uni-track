import 'package:flutter/material.dart';
import '../../../constants/app_colors.dart';
import '../../classes/data/models/class_model.dart';
import '../data/models/project_model.dart';
import '../data/services/project_graphql_service.dart';
import 'project_github_contributions_screen.dart';

class ClassProjectsScreen extends StatefulWidget {
  final ClassModel cls;
  final String token;

  const ClassProjectsScreen({
    super.key,
    required this.cls,
    required this.token,
  });

  @override
  State<ClassProjectsScreen> createState() => _ClassProjectsScreenState();
}

class _ClassProjectsScreenState extends State<ClassProjectsScreen> {
  final _projectService = ProjectGraphQLService();
  late Future<List<ProjectModel>> _projectsFuture;

  @override
  void initState() {
    super.initState();
    _load();
  }

  void _load() {
    _projectsFuture = _projectService.getClassProjects(
      classId: widget.cls.classId,
      token: widget.token,
    );
  }

  Future<void> _refresh() async {
    setState(() => _load());
    await _projectsFuture;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              widget.cls.classCode,
              style: const TextStyle(
                color: Colors.white,
                fontWeight: FontWeight.bold,
                fontSize: 17,
              ),
            ),
            Text(
              '${widget.cls.courseName} · ${widget.cls.semesterName}',
              style: const TextStyle(color: Colors.white70, fontSize: 12),
            ),
          ],
        ),
        backgroundColor: AppColors.secondary,
        elevation: 0,
        iconTheme: const IconThemeData(color: Colors.white),
      ),
      body: FutureBuilder<List<ProjectModel>>(
        future: _projectsFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(
              child: CircularProgressIndicator(color: AppColors.secondary),
            );
          }

          if (snapshot.hasError) {
            String msg = snapshot.error.toString();
            if (msg.startsWith('Exception: ')) {
              msg = msg.substring('Exception: '.length);
            }
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
                      msg,
                      textAlign: TextAlign.center,
                      style: const TextStyle(
                        color: AppColors.textSecondary,
                        fontSize: 15,
                      ),
                    ),
                    const SizedBox(height: 24),
                    ElevatedButton.icon(
                      onPressed: _refresh,
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

          final projects = snapshot.data ?? [];

          if (projects.isEmpty) {
            return Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(
                    Icons.folder_open_rounded,
                    size: 72,
                    color: AppColors.textDisabled,
                  ),
                  const SizedBox(height: 16),
                  const Text(
                    'No projects yet',
                    style: TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.w600,
                      color: AppColors.textPrimary,
                    ),
                  ),
                  const SizedBox(height: 8),
                  const Text(
                    'This class has no projects assigned.',
                    style: TextStyle(color: AppColors.textSecondary),
                  ),
                ],
              ),
            );
          }

          return RefreshIndicator(
            onRefresh: _refresh,
            color: AppColors.secondary,
            child: ListView.separated(
              padding: const EdgeInsets.all(16),
              itemCount: projects.length,
              separatorBuilder: (_, _) => const SizedBox(height: 12),
              itemBuilder: (context, index) =>
                  _ProjectCard(project: projects[index], token: widget.token),
            ),
          );
        },
      ),
    );
  }
}

class _ProjectCard extends StatelessWidget {
  final ProjectModel project;
  final String token;

  const _ProjectCard({required this.project, required this.token});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: () => Navigator.push(
        context,
        MaterialPageRoute(
          builder: (_) =>
              ProjectGithubContributionsScreen(project: project, token: token),
        ),
      ),
      child: Container(
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
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Container(
                    width: 44,
                    height: 44,
                    decoration: BoxDecoration(
                      color: AppColors.primary.withValues(alpha: 0.12),
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: const Icon(
                      Icons.folder_rounded,
                      color: AppColors.primary,
                      size: 22,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          project.name,
                          style: const TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.bold,
                            color: AppColors.textPrimary,
                          ),
                        ),
                        if (project.description != null &&
                            project.description!.isNotEmpty) ...[
                          const SizedBox(height: 4),
                          Text(
                            project.description!,
                            maxLines: 2,
                            overflow: TextOverflow.ellipsis,
                            style: const TextStyle(
                              fontSize: 13,
                              color: AppColors.textSecondary,
                            ),
                          ),
                        ],
                      ],
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 12),
              const Divider(height: 1),
              const SizedBox(height: 10),
              Row(
                children: [
                  const Icon(
                    Icons.people_outline_rounded,
                    size: 14,
                    color: AppColors.textSecondary,
                  ),
                  const SizedBox(width: 4),
                  Text(
                    '${project.memberCount} member${project.memberCount == 1 ? '' : 's'}',
                    style: const TextStyle(
                      fontSize: 13,
                      color: AppColors.textSecondary,
                    ),
                  ),
                  const Spacer(),
                  const Icon(
                    Icons.chevron_right_rounded,
                    size: 18,
                    color: AppColors.textDisabled,
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}
