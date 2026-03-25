import 'package:flutter/material.dart';
import 'package:intl/intl.dart' show DateFormat;
import '../../../constants/app_colors.dart';
import '../data/models/github_contribution_model.dart';
import '../data/services/github_contribution_service.dart';

class ProjectGithubReportDetailScreen extends StatefulWidget {
  final String projectId;
  final String token;
  final GithubContributionReportSummary report;

  const ProjectGithubReportDetailScreen({
    super.key,
    required this.projectId,
    required this.token,
    required this.report,
  });

  @override
  State<ProjectGithubReportDetailScreen> createState() =>
      _ProjectGithubReportDetailScreenState();
}

class _ProjectGithubReportDetailScreenState
    extends State<ProjectGithubReportDetailScreen> {
  final _service = GithubContributionService();
  late Future<GithubContributionReportDetail> _future;

  @override
  void initState() {
    super.initState();
    _load();
  }

  void _load() {
    _future = _service.getProjectReportDetail(
      projectId: widget.projectId,
      reportId: widget.report.reportId,
      token: widget.token,
    );
  }

  Future<void> _refresh() async {
    setState(() => _load());
    await _future;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text(
          'Report Detail',
          style: TextStyle(
            color: Colors.white,
            fontWeight: FontWeight.bold,
            fontSize: 17,
          ),
        ),
        backgroundColor: AppColors.secondary,
        iconTheme: const IconThemeData(color: Colors.white),
      ),
      body: FutureBuilder<GithubContributionReportDetail>(
        future: _future,
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
                padding: const EdgeInsets.all(24),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(
                      Icons.error_outline_rounded,
                      size: 54,
                      color: AppColors.error,
                    ),
                    const SizedBox(height: 12),
                    Text(
                      msg,
                      textAlign: TextAlign.center,
                      style: const TextStyle(
                        color: AppColors.textSecondary,
                        fontSize: 14,
                      ),
                    ),
                    const SizedBox(height: 18),
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

          final report = snapshot.data!;
          return RefreshIndicator(
            onRefresh: _refresh,
            color: AppColors.secondary,
            child: ListView(
              padding: const EdgeInsets.all(16),
              children: [
                _DetailHeaderCard(report: report),
                const SizedBox(height: 12),
                _MarkdownCard(report: report),
              ],
            ),
          );
        },
      ),
    );
  }
}

class _DetailHeaderCard extends StatelessWidget {
  final GithubContributionReportDetail report;

  const _DetailHeaderCard({required this.report});

  @override
  Widget build(BuildContext context) {
    final periodText =
        '${DateFormat('MMM d').format(report.periodStart.toLocal())} - ${DateFormat('MMM d, yyyy').format(report.periodEnd.toLocal())}';

    return Container(
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [AppColors.secondary, Color(0xFF2563EB)],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: AppColors.secondary.withValues(alpha: 0.25),
            blurRadius: 14,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 38,
                height: 38,
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.18),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: const Icon(
                  Icons.description_rounded,
                  color: Colors.white,
                  size: 20,
                ),
              ),
              const SizedBox(width: 10),
              const Expanded(
                child: Text(
                  'GitHub Contribution Report',
                  style: TextStyle(
                    fontSize: 15,
                    fontWeight: FontWeight.w700,
                    color: Colors.white,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 10),
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: [
              _HeaderChip(
                icon: Icons.schedule_rounded,
                text: DateFormat(
                  'MMM d, yyyy - HH:mm',
                ).format(report.createdAt.toLocal()),
              ),
              _HeaderChip(icon: Icons.date_range_rounded, text: periodText),
            ],
          ),
          const SizedBox(height: 12),
          Text(
            report.executiveSummary.isEmpty
                ? 'No executive summary available.'
                : report.executiveSummary,
            style: const TextStyle(
              fontSize: 13,
              color: Colors.white,
              height: 1.45,
            ),
          ),
          const SizedBox(height: 12),
          Row(
            children: [
              Expanded(
                child: _HeaderStat(
                  label: 'Commits',
                  value: '${report.totalCommits}',
                  icon: Icons.commit_rounded,
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: _HeaderStat(
                  label: 'Active',
                  value: '${report.activeContributorCount}',
                  icon: Icons.person_rounded,
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: _HeaderStat(
                  label: 'Model',
                  value: report.modelName,
                  icon: Icons.auto_awesome_rounded,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _HeaderChip extends StatelessWidget {
  final IconData icon;
  final String text;

  const _HeaderChip({required this.icon, required this.text});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: Colors.white.withValues(alpha: 0.16),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 12, color: Colors.white),
          const SizedBox(width: 4),
          Text(
            text,
            style: const TextStyle(
              fontSize: 11,
              color: Colors.white,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ),
    );
  }
}

class _HeaderStat extends StatelessWidget {
  final String label;
  final String value;
  final IconData icon;

  const _HeaderStat({
    required this.label,
    required this.value,
    required this.icon,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 8),
      decoration: BoxDecoration(
        color: Colors.white.withValues(alpha: 0.14),
        borderRadius: BorderRadius.circular(10),
      ),
      child: Column(
        children: [
          Icon(icon, size: 14, color: Colors.white),
          const SizedBox(height: 4),
          Text(
            value,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.w700,
              color: Colors.white,
            ),
          ),
          const SizedBox(height: 2),
          Text(
            label,
            style: const TextStyle(fontSize: 10, color: Colors.white70),
          ),
        ],
      ),
    );
  }
}

class _MarkdownCard extends StatelessWidget {
  final GithubContributionReportDetail report;

  const _MarkdownCard({required this.report});

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.04),
            blurRadius: 10,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      padding: const EdgeInsets.all(14),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 34,
                height: 34,
                decoration: BoxDecoration(
                  color: AppColors.secondary.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: const Icon(
                  Icons.article_outlined,
                  color: AppColors.secondary,
                  size: 18,
                ),
              ),
              const SizedBox(width: 10),
              const Text(
                'Report Content',
                style: TextStyle(
                  fontSize: 15,
                  fontWeight: FontWeight.bold,
                  color: AppColors.textPrimary,
                ),
              ),
            ],
          ),
          const SizedBox(height: 10),
          Container(
            width: double.infinity,
            decoration: BoxDecoration(
              color: AppColors.background,
              borderRadius: BorderRadius.circular(10),
              border: Border.all(
                color: AppColors.textDisabled.withValues(alpha: 0.3),
              ),
            ),
            padding: const EdgeInsets.all(12),
            child: SelectableText(
              report.markdownContent.trim().isEmpty
                  ? 'No report content available.'
                  : report.markdownContent,
              style: const TextStyle(
                fontSize: 13,
                color: AppColors.textPrimary,
                height: 1.45,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
