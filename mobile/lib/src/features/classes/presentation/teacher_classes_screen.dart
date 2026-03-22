import 'package:flutter/material.dart';
import '../../../constants/app_colors.dart';
import '../../auth/data/models/auth_models.dart';
import '../../projects/presentation/class_projects_screen.dart';
import '../data/models/class_model.dart';
import '../data/services/class_graphql_service.dart';

class TeacherClassesScreen extends StatefulWidget {
  final AuthUser currentUser;

  const TeacherClassesScreen({super.key, required this.currentUser});

  @override
  State<TeacherClassesScreen> createState() => _TeacherClassesScreenState();
}

class _TeacherClassesScreenState extends State<TeacherClassesScreen> {
  final _classGraphQLService = ClassGraphQLService();
  final _searchController = TextEditingController();

  static const _semesterFilterCurrent = '__current__';
  static const _semesterFilterAll = '__all__';

  late Future<List<ClassModel>> _classesFuture;
  String _searchQuery = '';
  String _selectedSemesterFilter = _semesterFilterCurrent;
  _ClassSortOption _selectedSort = _ClassSortOption.classCodeAsc;

  @override
  void initState() {
    super.initState();
    _loadClasses();
  }

  void _loadClasses() {
    _classesFuture = _classGraphQLService.getTeacherClasses(
      teacherId: widget.currentUser.userId,
      token: widget.currentUser.token,
    );
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _refresh() async {
    setState(() => _loadClasses());
    await _classesFuture;
  }

  List<_SemesterFilterOption> _buildSemesterOptions(List<ClassModel> classes) {
    final bySemester = <String, _SemesterFilterOption>{};

    for (final cls in classes) {
      bySemester.putIfAbsent(
        cls.semesterId,
        () => _SemesterFilterOption(
          value: cls.semesterId,
          label: cls.semesterName,
          startDate: cls.semesterStartDate,
          endDate: cls.semesterEndDate,
        ),
      );
    }

    final options = bySemester.values.toList()
      ..sort((a, b) {
        final aStart = a.startDate;
        final bStart = b.startDate;

        if (aStart != null && bStart != null) {
          return bStart.compareTo(aStart);
        }
        if (aStart != null) return -1;
        if (bStart != null) return 1;
        return a.label.toLowerCase().compareTo(b.label.toLowerCase());
      });

    return [
      const _SemesterFilterOption(
        value: _semesterFilterCurrent,
        label: 'Current semester',
      ),
      const _SemesterFilterOption(
        value: _semesterFilterAll,
        label: 'All semesters',
      ),
      ...options,
    ];
  }

  String? _resolveCurrentSemesterId(List<ClassModel> classes) {
    final now = DateTime.now();
    final bySemester = <String, _SemesterRange>{};

    for (final cls in classes) {
      final start = cls.semesterStartDate;
      final end = cls.semesterEndDate;
      if (start == null || end == null) continue;

      final existing = bySemester[cls.semesterId];
      if (existing == null) {
        bySemester[cls.semesterId] = _SemesterRange(
          semesterId: cls.semesterId,
          startDate: start,
          endDate: end,
        );
      } else {
        if (start.isBefore(existing.startDate)) {
          existing.startDate = start;
        }
        if (end.isAfter(existing.endDate)) {
          existing.endDate = end;
        }
      }
    }

    final activeSemesters =
        bySemester.values.where((semester) {
          final starts = !now.isBefore(semester.startDate);
          final ends = !now.isAfter(semester.endDate);
          return starts && ends;
        }).toList()..sort((a, b) {
          final byEnd = b.endDate.compareTo(a.endDate);
          if (byEnd != 0) return byEnd;
          return b.startDate.compareTo(a.startDate);
        });

    if (activeSemesters.isEmpty) return null;
    return activeSemesters.first.semesterId;
  }

  List<ClassModel> _applySearchSortAndFilter(
    List<ClassModel> allClasses,
    String effectiveSemesterFilter,
  ) {
    var result = List<ClassModel>.from(allClasses);

    if (effectiveSemesterFilter == _semesterFilterCurrent) {
      final currentSemesterId = _resolveCurrentSemesterId(result);
      if (currentSemesterId != null) {
        result = result
            .where((cls) => cls.semesterId == currentSemesterId)
            .toList();
      }
    } else if (effectiveSemesterFilter != _semesterFilterAll) {
      result = result
          .where((cls) => cls.semesterId == effectiveSemesterFilter)
          .toList();
    }

    final query = _searchQuery.trim().toLowerCase();
    if (query.isNotEmpty) {
      result = result.where((cls) {
        return cls.classCode.toLowerCase().contains(query) ||
            cls.courseCode.toLowerCase().contains(query) ||
            cls.courseName.toLowerCase().contains(query) ||
            cls.semesterName.toLowerCase().contains(query);
      }).toList();
    }

    result.sort((a, b) {
      switch (_selectedSort) {
        case _ClassSortOption.classCodeAsc:
          return a.classCode.toLowerCase().compareTo(b.classCode.toLowerCase());
        case _ClassSortOption.classCodeDesc:
          return b.classCode.toLowerCase().compareTo(a.classCode.toLowerCase());
        case _ClassSortOption.courseNameAsc:
          return a.courseName.toLowerCase().compareTo(
            b.courseName.toLowerCase(),
          );
        case _ClassSortOption.updatedNewest:
          return b.updatedAt.compareTo(a.updatedAt);
      }
    });

    return result;
  }

  void _clearFilters() {
    setState(() {
      _searchController.clear();
      _searchQuery = '';
      _selectedSemesterFilter = _semesterFilterCurrent;
      _selectedSort = _ClassSortOption.classCodeAsc;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text(
          'My Classes',
          style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
        ),
        backgroundColor: AppColors.secondary,
        elevation: 0,
      ),
      body: FutureBuilder<List<ClassModel>>(
        future: _classesFuture,
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

          final allClasses = snapshot.data ?? [];

          if (allClasses.isEmpty) {
            return Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(
                    Icons.class_outlined,
                    size: 72,
                    color: AppColors.textDisabled,
                  ),
                  const SizedBox(height: 16),
                  const Text(
                    'No classes found',
                    style: TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.w600,
                      color: AppColors.textPrimary,
                    ),
                  ),
                  const SizedBox(height: 8),
                  const Text(
                    'You have no assigned classes yet.',
                    style: TextStyle(color: AppColors.textSecondary),
                  ),
                ],
              ),
            );
          }

          final semesterOptions = _buildSemesterOptions(allClasses);
          final hasSelectedSemester = semesterOptions.any(
            (option) => option.value == _selectedSemesterFilter,
          );
          final effectiveSemesterFilter = hasSelectedSemester
              ? _selectedSemesterFilter
              : _semesterFilterCurrent;

          final visibleClasses = _applySearchSortAndFilter(
            allClasses,
            effectiveSemesterFilter,
          );

          return RefreshIndicator(
            onRefresh: _refresh,
            color: AppColors.secondary,
            child: ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: visibleClasses.isEmpty ? 2 : visibleClasses.length + 1,
              itemBuilder: (context, index) {
                if (index == 0) {
                  return Padding(
                    padding: const EdgeInsets.only(bottom: 14),
                    child: _ClassesFilterBar(
                      searchController: _searchController,
                      onSearchChanged: (value) =>
                          setState(() => _searchQuery = value),
                      semesterOptions: semesterOptions,
                      selectedSemesterFilter: effectiveSemesterFilter,
                      onSemesterChanged: (value) {
                        if (value == null) return;
                        setState(() => _selectedSemesterFilter = value);
                      },
                      selectedSort: _selectedSort,
                      onSortChanged: (value) {
                        if (value == null) return;
                        setState(() => _selectedSort = value);
                      },
                      resultCount: visibleClasses.length,
                      totalCount: allClasses.length,
                      onClearFilters: _clearFilters,
                    ),
                  );
                }

                if (visibleClasses.isEmpty) {
                  return _NoClassMatchView(onClearFilters: _clearFilters);
                }

                final cls = visibleClasses[index - 1];
                return Padding(
                  padding: const EdgeInsets.only(bottom: 12),
                  child: _ClassCard(
                    cls: cls,
                    onTap: () => Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (_) => ClassProjectsScreen(
                          cls: cls,
                          token: widget.currentUser.token,
                        ),
                      ),
                    ),
                  ),
                );
              },
            ),
          );
        },
      ),
    );
  }
}

enum _ClassSortOption {
  classCodeAsc,
  classCodeDesc,
  courseNameAsc,
  updatedNewest,
}

class _ClassesFilterBar extends StatelessWidget {
  final TextEditingController searchController;
  final ValueChanged<String> onSearchChanged;
  final List<_SemesterFilterOption> semesterOptions;
  final String selectedSemesterFilter;
  final ValueChanged<String?> onSemesterChanged;
  final _ClassSortOption selectedSort;
  final ValueChanged<_ClassSortOption?> onSortChanged;
  final int resultCount;
  final int totalCount;
  final VoidCallback onClearFilters;

  const _ClassesFilterBar({
    required this.searchController,
    required this.onSearchChanged,
    required this.semesterOptions,
    required this.selectedSemesterFilter,
    required this.onSemesterChanged,
    required this.selectedSort,
    required this.onSortChanged,
    required this.resultCount,
    required this.totalCount,
    required this.onClearFilters,
  });

  @override
  Widget build(BuildContext context) {
    final showClear =
        resultCount != totalCount || searchController.text.isNotEmpty;

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
              hintText: 'Search by class, course, or semester',
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
                child: DropdownButtonFormField<String>(
                  initialValue: selectedSemesterFilter,
                  isExpanded: true,
                  decoration: const InputDecoration(
                    labelText: 'Semester',
                    isDense: true,
                    border: OutlineInputBorder(),
                  ),
                  items: semesterOptions
                      .map(
                        (option) => DropdownMenuItem<String>(
                          value: option.value,
                          child: Text(
                            option.label,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                      )
                      .toList(),
                  onChanged: onSemesterChanged,
                ),
              ),
              const SizedBox(width: 10),
              Expanded(
                child: DropdownButtonFormField<_ClassSortOption>(
                  initialValue: selectedSort,
                  isExpanded: true,
                  decoration: const InputDecoration(
                    labelText: 'Sort',
                    isDense: true,
                    border: OutlineInputBorder(),
                  ),
                  items: const [
                    DropdownMenuItem(
                      value: _ClassSortOption.classCodeAsc,
                      child: Text('Class code A-Z'),
                    ),
                    DropdownMenuItem(
                      value: _ClassSortOption.classCodeDesc,
                      child: Text('Class code Z-A'),
                    ),
                    DropdownMenuItem(
                      value: _ClassSortOption.courseNameAsc,
                      child: Text('Course name A-Z'),
                    ),
                    DropdownMenuItem(
                      value: _ClassSortOption.updatedNewest,
                      child: Text('Recently updated'),
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
                '$resultCount of $totalCount classes',
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

class _NoClassMatchView extends StatelessWidget {
  final VoidCallback onClearFilters;

  const _NoClassMatchView({required this.onClearFilters});

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
            'No classes match your filters',
            style: TextStyle(
              fontSize: 16,
              fontWeight: FontWeight.w600,
              color: AppColors.textPrimary,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 6),
          const Text(
            'Try another keyword or change semester and sort options.',
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

class _SemesterFilterOption {
  final String value;
  final String label;
  final DateTime? startDate;
  final DateTime? endDate;

  const _SemesterFilterOption({
    required this.value,
    required this.label,
    this.startDate,
    this.endDate,
  });
}

class _SemesterRange {
  final String semesterId;
  DateTime startDate;
  DateTime endDate;

  _SemesterRange({
    required this.semesterId,
    required this.startDate,
    required this.endDate,
  });
}

class _ClassCard extends StatelessWidget {
  final ClassModel cls;
  final VoidCallback onTap;

  const _ClassCard({required this.cls, required this.onTap});

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(14),
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
              // Header row: class code + semester badge
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Class icon
                  Container(
                    width: 44,
                    height: 44,
                    decoration: BoxDecoration(
                      color: AppColors.secondary.withValues(alpha: 0.12),
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: const Icon(
                      Icons.class_rounded,
                      color: AppColors.secondary,
                      size: 22,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          cls.classCode,
                          style: const TextStyle(
                            fontSize: 17,
                            fontWeight: FontWeight.bold,
                            color: AppColors.textPrimary,
                          ),
                        ),
                        const SizedBox(height: 2),
                        Text(
                          cls.courseName,
                          style: const TextStyle(
                            fontSize: 14,
                            color: AppColors.textSecondary,
                          ),
                        ),
                      ],
                    ),
                  ),
                  // Semester badge
                  Container(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 10,
                      vertical: 4,
                    ),
                    decoration: BoxDecoration(
                      color: AppColors.primary.withValues(alpha: 0.12),
                      borderRadius: BorderRadius.circular(20),
                    ),
                    child: Text(
                      cls.semesterName,
                      style: const TextStyle(
                        fontSize: 12,
                        fontWeight: FontWeight.w600,
                        color: AppColors.primary,
                      ),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 12),
              const Divider(height: 1),
              const SizedBox(height: 10),
              // Course code chip
              Row(
                children: [
                  _Chip(icon: Icons.book_outlined, label: cls.courseCode),
                  const Spacer(),
                  const Icon(
                    Icons.chevron_right_rounded,
                    color: AppColors.textDisabled,
                    size: 20,
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

class _Chip extends StatelessWidget {
  final IconData icon;
  final String label;

  const _Chip({required this.icon, required this.label});

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon, size: 14, color: AppColors.textSecondary),
        const SizedBox(width: 4),
        Text(
          label,
          style: const TextStyle(fontSize: 13, color: AppColors.textSecondary),
        ),
      ],
    );
  }
}
