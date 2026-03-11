import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../../constants/app_colors.dart';
import '../../../auth/data/models/auth_models.dart';
import '../../data/models/semester_model.dart';
import '../../data/repositories/semester_repository.dart';

class SemesterFormScreen extends StatefulWidget {
  final AuthUser currentUser;
  final SemesterModel? existing;

  const SemesterFormScreen({
    super.key,
    required this.currentUser,
    this.existing,
  });

  @override
  State<SemesterFormScreen> createState() => _SemesterFormScreenState();
}

class _SemesterFormScreenState extends State<SemesterFormScreen> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _repository = SemesterRepository();

  DateTime? _startDate;
  DateTime? _endDate;
  bool _isLoading = false;

  bool get _isEditing => widget.existing != null;

  @override
  void initState() {
    super.initState();
    if (_isEditing) {
      _nameController.text = widget.existing!.name;
      _startDate = widget.existing!.startDate;
      _endDate = widget.existing!.endDate;
    }
  }

  @override
  void dispose() {
    _nameController.dispose();
    super.dispose();
  }

  Future<void> _pickDate({required bool isStart}) async {
    final initial = isStart
        ? (_startDate ?? DateTime.now())
        : (_endDate ?? (_startDate ?? DateTime.now()));

    final picked = await showDatePicker(
      context: context,
      initialDate: initial,
      firstDate: DateTime(2020),
      lastDate: DateTime(2035),
      builder: (ctx, child) => Theme(
        data: Theme.of(ctx).copyWith(
          colorScheme: const ColorScheme.light(primary: AppColors.secondary),
        ),
        child: child!,
      ),
    );

    if (picked == null) return;
    setState(() {
      if (isStart) {
        _startDate = picked;
        if (_endDate != null && _endDate!.isBefore(picked)) {
          _endDate = null;
        }
      } else {
        _endDate = picked;
      }
    });
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (_startDate == null) {
      _showError('Please select a start date.');
      return;
    }
    if (_endDate == null) {
      _showError('Please select an end date.');
      return;
    }
    if (!_endDate!.isAfter(_startDate!)) {
      _showError('End date must be after start date.');
      return;
    }

    setState(() => _isLoading = true);
    try {
      if (_isEditing) {
        await _repository.update(
          semesterId: widget.existing!.semesterId,
          name: _nameController.text.trim(),
          startDate: _startDate!,
          endDate: _endDate!,
          token: widget.currentUser.token,
        );
      } else {
        await _repository.create(
          name: _nameController.text.trim(),
          startDate: _startDate!,
          endDate: _endDate!,
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
    final fmt = DateFormat('MMM d, yyyy');

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text(
          _isEditing ? 'Edit Semester' : 'New Semester',
          style: const TextStyle(
            color: Colors.white,
            fontWeight: FontWeight.bold,
          ),
        ),
        backgroundColor: AppColors.secondary,
        elevation: 0,
        iconTheme: const IconThemeData(color: Colors.white),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              _FormCard(
                children: [
                  TextFormField(
                    controller: _nameController,
                    decoration: const InputDecoration(
                      labelText: 'Semester Name *',
                      hintText: 'e.g. Fall 2025',
                      prefixIcon: Icon(Icons.label_rounded),
                      border: OutlineInputBorder(),
                    ),
                    validator: (v) =>
                        (v == null || v.trim().isEmpty) ? 'Required' : null,
                    textInputAction: TextInputAction.next,
                  ),
                  const SizedBox(height: 16),
                  _DateField(
                    label: 'Start Date *',
                    value: _startDate != null ? fmt.format(_startDate!) : null,
                    icon: Icons.event_rounded,
                    onTap: () => _pickDate(isStart: true),
                  ),
                  const SizedBox(height: 16),
                  _DateField(
                    label: 'End Date *',
                    value: _endDate != null ? fmt.format(_endDate!) : null,
                    icon: Icons.event_available_rounded,
                    onTap: () => _pickDate(isStart: false),
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
                        _isEditing ? 'Save Changes' : 'Create Semester',
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

class _DateField extends StatelessWidget {
  final String label;
  final String? value;
  final IconData icon;
  final VoidCallback onTap;

  const _DateField({
    required this.label,
    required this.value,
    required this.icon,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(8),
      child: InputDecorator(
        decoration: InputDecoration(
          labelText: label,
          prefixIcon: Icon(icon),
          border: const OutlineInputBorder(),
          suffixIcon: const Icon(Icons.arrow_drop_down_rounded),
        ),
        child: Text(
          value ?? 'Select date',
          style: TextStyle(
            fontSize: 16,
            color: value != null
                ? AppColors.textPrimary
                : AppColors.textSecondary,
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
