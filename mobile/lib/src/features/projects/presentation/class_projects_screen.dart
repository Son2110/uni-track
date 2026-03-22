import 'package:flutter/material.dart';
import '../../../constants/app_colors.dart';
import '../../classes/data/models/class_model.dart';
import '../data/models/class_student_model.dart';
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
  final _searchController = TextEditingController();
  late Future<List<ProjectModel>> _projectsFuture;
  String _searchQuery = '';
  _ProjectFilterOption _selectedFilter = _ProjectFilterOption.all;
  _ProjectSortOption _selectedSort = _ProjectSortOption.updatedNewest;
  bool _isCreatingProject = false;

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

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _refresh() async {
    setState(() => _load());
    await _projectsFuture;
  }

  List<ProjectModel> _applySearchFilterSort(List<ProjectModel> projects) {
    var result = List<ProjectModel>.from(projects);

    final query = _searchQuery.trim().toLowerCase();
    if (query.isNotEmpty) {
      result = result.where((project) {
        return project.name.toLowerCase().contains(query) ||
            (project.description?.toLowerCase().contains(query) ?? false);
      }).toList();
    }

    switch (_selectedFilter) {
      case _ProjectFilterOption.all:
        break;
      case _ProjectFilterOption.withMembers:
        result = result.where((project) => project.memberCount > 0).toList();
      case _ProjectFilterOption.withoutMembers:
        result = result.where((project) => project.memberCount == 0).toList();
    }

    result.sort((a, b) {
      switch (_selectedSort) {
        case _ProjectSortOption.updatedNewest:
          return b.updatedAt.compareTo(a.updatedAt);
        case _ProjectSortOption.updatedOldest:
          return a.updatedAt.compareTo(b.updatedAt);
        case _ProjectSortOption.nameAsc:
          return a.name.toLowerCase().compareTo(b.name.toLowerCase());
        case _ProjectSortOption.nameDesc:
          return b.name.toLowerCase().compareTo(a.name.toLowerCase());
        case _ProjectSortOption.membersHighToLow:
          return b.memberCount.compareTo(a.memberCount);
      }
    });

    return result;
  }

  void _clearFilters() {
    setState(() {
      _searchController.clear();
      _searchQuery = '';
      _selectedFilter = _ProjectFilterOption.all;
      _selectedSort = _ProjectSortOption.updatedNewest;
    });
  }

  Future<void> _openCreateProjectDialog() async {
    final payload = await showDialog<_CreateProjectPayload>(
      context: context,
      builder: (context) => _CreateProjectDialog(
        cls: widget.cls,
        token: widget.token,
        projectService: _projectService,
      ),
    );

    if (payload == null) return;

    setState(() => _isCreatingProject = true);

    try {
      final projectId = await _projectService.createProject(
        classId: widget.cls.classId,
        name: payload.name,
        description: payload.description,
        token: widget.token,
      );

      var assigned = 0;
      var failed = 0;
      for (final userId in payload.studentIds) {
        try {
          await _projectService.addMemberToProject(
            projectId: projectId,
            userId: userId,
            token: widget.token,
          );
          assigned++;
        } catch (_) {
          failed++;
        }
      }

      if (!mounted) return;
      final base = 'Project created successfully.';
      final assignedMessage = payload.studentIds.isEmpty
          ? ' No students were assigned.'
          : ' Assigned $assigned/${payload.studentIds.length} students.';
      final failureMessage = failed > 0
          ? ' $failed student assignment(s) failed.'
          : '';

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('$base$assignedMessage$failureMessage')),
      );

      await _refresh();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(SnackBar(content: Text('Failed to create project: $e')));
    } finally {
      if (mounted) {
        setState(() => _isCreatingProject = false);
      }
    }
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
        actions: [
          IconButton(
            tooltip: 'Create project',
            icon: _isCreatingProject
                ? const SizedBox(
                    width: 18,
                    height: 18,
                    child: CircularProgressIndicator(
                      strokeWidth: 2,
                      color: Colors.white,
                    ),
                  )
                : const Icon(Icons.add_circle_outline_rounded),
            onPressed: _isCreatingProject ? null : _openCreateProjectDialog,
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton.extended(
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        onPressed: _isCreatingProject ? null : _openCreateProjectDialog,
        icon: _isCreatingProject
            ? const SizedBox(
                width: 18,
                height: 18,
                child: CircularProgressIndicator(
                  strokeWidth: 2,
                  color: Colors.white,
                ),
              )
            : const Icon(Icons.add_rounded),
        label: const Text('Create Project'),
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
          final visibleProjects = _applySearchFilterSort(projects);

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
                  const SizedBox(height: 16),
                  ElevatedButton.icon(
                    onPressed: _isCreatingProject
                        ? null
                        : _openCreateProjectDialog,
                    icon: const Icon(Icons.add_rounded),
                    label: const Text('Create project'),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppColors.primary,
                      foregroundColor: Colors.white,
                    ),
                  ),
                ],
              ),
            );
          }

          return RefreshIndicator(
            onRefresh: _refresh,
            color: AppColors.secondary,
            child: ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: visibleProjects.isEmpty
                  ? 2
                  : visibleProjects.length + 1,
              itemBuilder: (context, index) {
                if (index == 0) {
                  return Padding(
                    padding: const EdgeInsets.only(bottom: 14),
                    child: _ProjectFilterBar(
                      searchController: _searchController,
                      onSearchChanged: (value) =>
                          setState(() => _searchQuery = value),
                      selectedFilter: _selectedFilter,
                      onFilterChanged: (value) {
                        if (value == null) return;
                        setState(() => _selectedFilter = value);
                      },
                      selectedSort: _selectedSort,
                      onSortChanged: (value) {
                        if (value == null) return;
                        setState(() => _selectedSort = value);
                      },
                      resultCount: visibleProjects.length,
                      totalCount: projects.length,
                      onClearFilters: _clearFilters,
                    ),
                  );
                }

                if (visibleProjects.isEmpty) {
                  return _NoProjectMatchView(onClearFilters: _clearFilters);
                }

                final project = visibleProjects[index - 1];
                return Padding(
                  padding: const EdgeInsets.only(bottom: 12),
                  child: _ProjectCard(project: project, token: widget.token),
                );
              },
            ),
          );
        },
      ),
    );
  }
}

enum _ProjectFilterOption { all, withMembers, withoutMembers }

enum _ProjectSortOption {
  updatedNewest,
  updatedOldest,
  nameAsc,
  nameDesc,
  membersHighToLow,
}

class _ProjectFilterBar extends StatelessWidget {
  final TextEditingController searchController;
  final ValueChanged<String> onSearchChanged;
  final _ProjectFilterOption selectedFilter;
  final ValueChanged<_ProjectFilterOption?> onFilterChanged;
  final _ProjectSortOption selectedSort;
  final ValueChanged<_ProjectSortOption?> onSortChanged;
  final int resultCount;
  final int totalCount;
  final VoidCallback onClearFilters;

  const _ProjectFilterBar({
    required this.searchController,
    required this.onSearchChanged,
    required this.selectedFilter,
    required this.onFilterChanged,
    required this.selectedSort,
    required this.onSortChanged,
    required this.resultCount,
    required this.totalCount,
    required this.onClearFilters,
  });

  @override
  Widget build(BuildContext context) {
    final showClear =
        searchController.text.isNotEmpty ||
        selectedFilter != _ProjectFilterOption.all ||
        selectedSort != _ProjectSortOption.updatedNewest ||
        resultCount != totalCount;

    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(
          color: AppColors.textDisabled.withValues(alpha: 0.3),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          TextField(
            controller: searchController,
            onChanged: onSearchChanged,
            decoration: InputDecoration(
              hintText: 'Search by project name or description',
              prefixIcon: const Icon(Icons.search_rounded),
              isDense: true,
              filled: true,
              fillColor: AppColors.background,
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(10),
                borderSide: BorderSide.none,
              ),
            ),
          ),
          const SizedBox(height: 12),
          Row(
            children: [
              Expanded(
                child: DropdownButtonFormField<_ProjectFilterOption>(
                  initialValue: selectedFilter,
                  decoration: const InputDecoration(
                    labelText: 'Filter',
                    isDense: true,
                    border: OutlineInputBorder(),
                  ),
                  items: const [
                    DropdownMenuItem(
                      value: _ProjectFilterOption.all,
                      child: Text('All projects'),
                    ),
                    DropdownMenuItem(
                      value: _ProjectFilterOption.withMembers,
                      child: Text('Has members'),
                    ),
                    DropdownMenuItem(
                      value: _ProjectFilterOption.withoutMembers,
                      child: Text('No members'),
                    ),
                  ],
                  onChanged: onFilterChanged,
                ),
              ),
              const SizedBox(width: 10),
              Expanded(
                child: DropdownButtonFormField<_ProjectSortOption>(
                  initialValue: selectedSort,
                  decoration: const InputDecoration(
                    labelText: 'Sort',
                    isDense: true,
                    border: OutlineInputBorder(),
                  ),
                  items: const [
                    DropdownMenuItem(
                      value: _ProjectSortOption.updatedNewest,
                      child: Text('Recently updated'),
                    ),
                    DropdownMenuItem(
                      value: _ProjectSortOption.updatedOldest,
                      child: Text('Oldest updated'),
                    ),
                    DropdownMenuItem(
                      value: _ProjectSortOption.nameAsc,
                      child: Text('Name A-Z'),
                    ),
                    DropdownMenuItem(
                      value: _ProjectSortOption.nameDesc,
                      child: Text('Name Z-A'),
                    ),
                    DropdownMenuItem(
                      value: _ProjectSortOption.membersHighToLow,
                      child: Text('Most members'),
                    ),
                  ],
                  onChanged: onSortChanged,
                ),
              ),
            ],
          ),
          const SizedBox(height: 10),
          Row(
            children: [
              Text(
                '$resultCount of $totalCount projects',
                style: const TextStyle(
                  fontSize: 13,
                  fontWeight: FontWeight.w500,
                  color: AppColors.textSecondary,
                ),
              ),
              const Spacer(),
              if (showClear)
                TextButton.icon(
                  onPressed: onClearFilters,
                  icon: const Icon(Icons.filter_alt_off_rounded, size: 18),
                  label: const Text('Clear'),
                  style: TextButton.styleFrom(
                    foregroundColor: AppColors.secondary,
                    padding: const EdgeInsets.symmetric(horizontal: 8),
                    minimumSize: Size.zero,
                    tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                  ),
                ),
            ],
          ),
        ],
      ),
    );
  }
}

class _NoProjectMatchView extends StatelessWidget {
  final VoidCallback onClearFilters;

  const _NoProjectMatchView({required this.onClearFilters});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 28),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(
          color: AppColors.textDisabled.withValues(alpha: 0.3),
        ),
      ),
      child: Column(
        children: [
          Icon(
            Icons.filter_alt_off_rounded,
            size: 46,
            color: AppColors.textDisabled,
          ),
          const SizedBox(height: 12),
          const Text(
            'No projects match your filters',
            style: TextStyle(
              fontSize: 16,
              fontWeight: FontWeight.w600,
              color: AppColors.textPrimary,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 6),
          const Text(
            'Try another keyword or adjust filter and sort options.',
            style: TextStyle(color: AppColors.textSecondary),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 14),
          OutlinedButton.icon(
            onPressed: onClearFilters,
            icon: const Icon(Icons.restart_alt_rounded),
            label: const Text('Reset filters'),
          ),
        ],
      ),
    );
  }
}

class _CreateProjectPayload {
  final String name;
  final String? description;
  final List<String> studentIds;

  const _CreateProjectPayload({
    required this.name,
    required this.description,
    required this.studentIds,
  });
}

class _CreateProjectDialog extends StatefulWidget {
  final ClassModel cls;
  final String token;
  final ProjectGraphQLService projectService;

  const _CreateProjectDialog({
    required this.cls,
    required this.token,
    required this.projectService,
  });

  @override
  State<_CreateProjectDialog> createState() => _CreateProjectDialogState();
}

class _CreateProjectDialogState extends State<_CreateProjectDialog> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _descriptionController = TextEditingController();
  final _studentSearchController = TextEditingController();
  final Set<String> _selectedStudentIds = <String>{};
  String _studentQuery = '';
  bool _submitting = false;

  late Future<List<ClassStudentModel>> _studentsFuture;

  @override
  void initState() {
    super.initState();
    _studentsFuture = widget.projectService.getClassStudents(
      classId: widget.cls.classId,
      token: widget.token,
    );
  }

  @override
  void dispose() {
    _nameController.dispose();
    _descriptionController.dispose();
    _studentSearchController.dispose();
    super.dispose();
  }

  List<ClassStudentModel> _filterStudents(List<ClassStudentModel> students) {
    final query = _studentQuery.trim().toLowerCase();
    if (query.isEmpty) return students;

    return students.where((student) {
      return student.name.toLowerCase().contains(query) ||
          student.email.toLowerCase().contains(query);
    }).toList();
  }

  void _toggleStudent(String userId, bool selected) {
    setState(() {
      if (selected) {
        _selectedStudentIds.add(userId);
      } else {
        _selectedStudentIds.remove(userId);
      }
    });
  }

  void _submit() {
    if (_submitting) return;
    if (!_formKey.currentState!.validate()) return;

    setState(() => _submitting = true);

    Navigator.of(context).pop(
      _CreateProjectPayload(
        name: _nameController.text.trim(),
        description: _descriptionController.text.trim().isEmpty
            ? null
            : _descriptionController.text.trim(),
        studentIds: _selectedStudentIds.toList(),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Create project'),
      content: SizedBox(
        width: 520,
        child: SingleChildScrollView(
          child: Form(
            key: _formKey,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  '${widget.cls.classCode} · ${widget.cls.courseName}',
                  style: const TextStyle(
                    color: AppColors.textSecondary,
                    fontSize: 13,
                  ),
                ),
                const SizedBox(height: 12),
                TextFormField(
                  controller: _nameController,
                  decoration: const InputDecoration(
                    labelText: 'Project name',
                    border: OutlineInputBorder(),
                  ),
                  validator: (value) {
                    final text = value?.trim() ?? '';
                    if (text.isEmpty) return 'Project name is required';
                    if (text.length < 3) {
                      return 'Project name must be at least 3 characters';
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 12),
                TextFormField(
                  controller: _descriptionController,
                  minLines: 2,
                  maxLines: 4,
                  decoration: const InputDecoration(
                    labelText: 'Description (optional)',
                    border: OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 14),
                const Text(
                  'Assign students',
                  style: TextStyle(
                    fontWeight: FontWeight.w600,
                    color: AppColors.textPrimary,
                  ),
                ),
                const SizedBox(height: 6),
                const Text(
                  'Select class students to add into this project now.',
                  style: TextStyle(
                    fontSize: 12,
                    color: AppColors.textSecondary,
                  ),
                ),
                const SizedBox(height: 10),
                TextField(
                  controller: _studentSearchController,
                  onChanged: (value) => setState(() => _studentQuery = value),
                  decoration: InputDecoration(
                    hintText: 'Search by student name or email',
                    prefixIcon: const Icon(Icons.search_rounded),
                    isDense: true,
                    border: const OutlineInputBorder(),
                    suffixIcon: _studentQuery.trim().isEmpty
                        ? null
                        : IconButton(
                            icon: const Icon(Icons.close_rounded),
                            tooltip: 'Clear search',
                            onPressed: () {
                              _studentSearchController.clear();
                              setState(() => _studentQuery = '');
                            },
                          ),
                  ),
                ),
                const SizedBox(height: 10),
                FutureBuilder<List<ClassStudentModel>>(
                  future: _studentsFuture,
                  builder: (context, snapshot) {
                    if (snapshot.connectionState == ConnectionState.waiting) {
                      return const Padding(
                        padding: EdgeInsets.symmetric(vertical: 12),
                        child: Center(
                          child: CircularProgressIndicator(
                            color: AppColors.secondary,
                          ),
                        ),
                      );
                    }

                    if (snapshot.hasError) {
                      return Text(
                        'Could not load students: ${snapshot.error}',
                        style: const TextStyle(color: AppColors.error),
                      );
                    }

                    final students = snapshot.data ?? [];
                    if (students.isEmpty) {
                      return const Text(
                        'No students found in this class.',
                        style: TextStyle(color: AppColors.textSecondary),
                      );
                    }

                    final visibleStudents = _filterStudents(students);
                    if (visibleStudents.isEmpty) {
                      return const Text(
                        'No students match your search.',
                        style: TextStyle(color: AppColors.textSecondary),
                      );
                    }

                    return Container(
                      constraints: const BoxConstraints(maxHeight: 220),
                      decoration: BoxDecoration(
                        border: Border.all(
                          color: AppColors.textDisabled.withValues(alpha: 0.4),
                        ),
                        borderRadius: BorderRadius.circular(10),
                      ),
                      child: ListView.separated(
                        shrinkWrap: true,
                        itemCount: visibleStudents.length,
                        separatorBuilder: (_, _) => const Divider(height: 1),
                        itemBuilder: (context, index) {
                          final student = visibleStudents[index];
                          final selected = _selectedStudentIds.contains(
                            student.userId,
                          );
                          return CheckboxListTile(
                            value: selected,
                            dense: true,
                            controlAffinity: ListTileControlAffinity.leading,
                            onChanged: (value) =>
                                _toggleStudent(student.userId, value ?? false),
                            title: Text(
                              student.name,
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                            subtitle: student.email.isEmpty
                                ? null
                                : Text(
                                    student.email,
                                    maxLines: 1,
                                    overflow: TextOverflow.ellipsis,
                                  ),
                          );
                        },
                      ),
                    );
                  },
                ),
              ],
            ),
          ),
        ),
      ),
      actions: [
        TextButton(
          onPressed: _submitting ? null : () => Navigator.of(context).pop(),
          child: const Text('Cancel'),
        ),
        ElevatedButton.icon(
          onPressed: _submitting ? null : _submit,
          icon: const Icon(Icons.check_rounded),
          label: Text(
            _selectedStudentIds.isEmpty
                ? 'Create'
                : 'Create + Assign (${_selectedStudentIds.length})',
          ),
          style: ElevatedButton.styleFrom(
            backgroundColor: AppColors.primary,
            foregroundColor: Colors.white,
          ),
        ),
      ],
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
