import 'package:flutter/material.dart';
import '../../../../constants/app_colors.dart';
import '../../../auth/data/models/auth_models.dart';
import '../../../classes/data/models/class_model.dart';
import '../../data/models/course_model.dart';
import '../../data/models/semester_model.dart';
import '../../data/repositories/admin_class_repository.dart';
import '../../data/repositories/course_repository.dart';
import '../../data/repositories/semester_repository.dart';

class AdminClassFormScreen extends StatefulWidget {
  final AuthUser currentUser;
  final ClassModel? existing;

  const AdminClassFormScreen({
    super.key,
    required this.currentUser,
    this.existing,
  });

  @override
  State<AdminClassFormScreen> createState() => _AdminClassFormScreenState();
}

class _AdminClassFormScreenState extends State<AdminClassFormScreen> {
  final _formKey = GlobalKey<FormState>();
  final _classCodeController = TextEditingController();
  final _teacherIdController = TextEditingController();

  final _classRepository = AdminClassRepository();
  final _semesterRepository = SemesterRepository();
  final _courseRepository = CourseRepository();

  List<SemesterModel> _semesters = [];
  List<CourseModel> _courses = [];
  SemesterModel? _selectedSemester;
  CourseModel? _selectedCourse;

  bool _isLoading = false;
  bool _isLoadingDropdowns = true;
  String? _dropdownError;

  bool get _isEditing => widget.existing != null;

  @override
  void initState() {
    super.initState();
    if (_isEditing) {
      _classCodeController.text = widget.existing!.classCode;
      _teacherIdController.text = widget.existing!.teacherId;
    } else {
      _teacherIdController.text = widget.currentUser.userId;
    }
    _loadDropdowns();
  }

  @override
  void dispose() {
    _classCodeController.dispose();
    _teacherIdController.dispose();
    super.dispose();
  }

  Future<void> _loadDropdowns() async {
    setState(() {
      _isLoadingDropdowns = true;
      _dropdownError = null;
    });
    try {
      final results = await Future.wait([
        _semesterRepository.getAllSemesters(token: widget.currentUser.token),
        _courseRepository.getAllCourses(token: widget.currentUser.token),
      ]);
      if (!mounted) return;
      setState(() {
        _semesters = results[0] as List<SemesterModel>;
        _courses = results[1] as List<CourseModel>;
        _isLoadingDropdowns = false;

        if (_isEditing) {
          _selectedSemester = _semesters.cast<SemesterModel?>().firstWhere(
            (s) => s?.semesterId == widget.existing!.semesterId,
            orElse: () => null,
          );
          _selectedCourse = _courses.cast<CourseModel?>().firstWhere(
            (c) => c?.courseId == widget.existing!.courseId,
            orElse: () => null,
          );
        }
      });
    } catch (e) {
      if (!mounted) return;
      String msg = e.toString();
      if (msg.startsWith('Exception: ')) msg = msg.substring(11);
      setState(() {
        _dropdownError = msg;
        _isLoadingDropdowns = false;
      });
    }
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (_selectedSemester == null) {
      _showError('Please select a semester.');
      return;
    }
    if (_selectedCourse == null) {
      _showError('Please select a course.');
      return;
    }

    setState(() => _isLoading = true);
    try {
      if (_isEditing) {
        await _classRepository.update(
          classId: widget.existing!.classId,
          classCode: _classCodeController.text.trim(),
          teacherId: _teacherIdController.text.trim(),
          token: widget.currentUser.token,
        );
      } else {
        await _classRepository.create(
          semesterId: _selectedSemester!.semesterId,
          courseId: _selectedCourse!.courseId,
          classCode: _classCodeController.text.trim(),
          teacherId: _teacherIdController.text.trim(),
          token: widget.currentUser.token,
        );
      }
      if (!mounted) return;
      Navigator.pop(context, true);
    } catch (e) {
      if (!mounted) return;
      String msg = e.toString();
      if (msg.startsWith('Exception: ')) msg = msg.substring(11);
      _showError(msg);
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  void _showError(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message), backgroundColor: AppColors.error),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text(
          _isEditing ? 'Edit Class' : 'New Class',
          style: const TextStyle(
            color: Colors.white,
            fontWeight: FontWeight.bold,
          ),
        ),
        backgroundColor: AppColors.secondary,
        elevation: 0,
        iconTheme: const IconThemeData(color: Colors.white),
      ),
      body: _isLoadingDropdowns
          ? const Center(
              child: CircularProgressIndicator(color: AppColors.secondary),
            )
          : _dropdownError != null
          ? Center(
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
                      _dropdownError!,
                      textAlign: TextAlign.center,
                      style: const TextStyle(color: AppColors.textSecondary),
                    ),
                    const SizedBox(height: 24),
                    ElevatedButton.icon(
                      onPressed: _loadDropdowns,
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
            )
          : SingleChildScrollView(
              padding: const EdgeInsets.all(20),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    _FormCard(
                      children: [
                        // Semester dropdown — disabled on edit since only classCode & teacherId can change
                        DropdownButtonFormField<SemesterModel>(
                          value: _selectedSemester,
                          decoration: const InputDecoration(
                            labelText: 'Semester *',
                            prefixIcon: Icon(Icons.calendar_month_rounded),
                            border: OutlineInputBorder(),
                          ),
                          items: _semesters
                              .map(
                                (s) => DropdownMenuItem(
                                  value: s,
                                  child: Text(s.name),
                                ),
                              )
                              .toList(),
                          onChanged: _isEditing
                              ? null
                              : (v) => setState(() => _selectedSemester = v),
                          validator: (_) =>
                              _selectedSemester == null ? 'Required' : null,
                        ),
                        const SizedBox(height: 16),
                        DropdownButtonFormField<CourseModel>(
                          value: _selectedCourse,
                          decoration: const InputDecoration(
                            labelText: 'Course *',
                            prefixIcon: Icon(Icons.book_rounded),
                            border: OutlineInputBorder(),
                          ),
                          items: _courses
                              .map(
                                (c) => DropdownMenuItem(
                                  value: c,
                                  child: Text('${c.code} – ${c.name}'),
                                ),
                              )
                              .toList(),
                          onChanged: _isEditing
                              ? null
                              : (v) => setState(() => _selectedCourse = v),
                          validator: (_) =>
                              _selectedCourse == null ? 'Required' : null,
                        ),
                        const SizedBox(height: 16),
                        TextFormField(
                          controller: _classCodeController,
                          decoration: const InputDecoration(
                            labelText: 'Class Code *',
                            hintText: 'e.g. SE1701',
                            prefixIcon: Icon(Icons.class_rounded),
                            border: OutlineInputBorder(),
                          ),
                          textCapitalization: TextCapitalization.characters,
                          validator: (v) => (v == null || v.trim().isEmpty)
                              ? 'Required'
                              : null,
                          textInputAction: TextInputAction.next,
                        ),
                        const SizedBox(height: 16),
                        TextFormField(
                          controller: _teacherIdController,
                          decoration: const InputDecoration(
                            labelText: 'Teacher ID *',
                            hintText: 'UUID of the assigned teacher',
                            prefixIcon: Icon(Icons.person_rounded),
                            border: OutlineInputBorder(),
                          ),
                          validator: (v) => (v == null || v.trim().isEmpty)
                              ? 'Required'
                              : null,
                          textInputAction: TextInputAction.done,
                        ),
                      ],
                    ),
                    const SizedBox(height: 28),
                    ElevatedButton(
                      onPressed: _isLoading ? null : _submit,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppColors.secondary,
                        foregroundColor: Colors.white,
                        minimumSize: const Size.fromHeight(52),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                      ),
                      child: _isLoading
                          ? const SizedBox(
                              width: 22,
                              height: 22,
                              child: CircularProgressIndicator(
                                color: Colors.white,
                                strokeWidth: 2.5,
                              ),
                            )
                          : Text(
                              _isEditing ? 'Save Changes' : 'Create Class',
                              style: const TextStyle(
                                fontSize: 16,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                    ),
                  ],
                ),
              ),
            ),
    );
  }
}

class _FormCard extends StatelessWidget {
  final List<Widget> children;

  const _FormCard({required this.children});

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.05),
            blurRadius: 10,
            offset: const Offset(0, 3),
          ),
        ],
      ),
      padding: const EdgeInsets.all(18),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: children,
      ),
    );
  }
}
