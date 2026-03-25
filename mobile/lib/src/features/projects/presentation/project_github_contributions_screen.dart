import 'dart:math' as math;
import 'package:flutter/material.dart';
import 'package:intl/intl.dart' show DateFormat;
import '../../../constants/app_colors.dart';
import '../data/models/github_contribution_model.dart';
import '../data/models/project_model.dart';
import '../data/services/github_contribution_service.dart';
import 'project_github_reports_list_screen.dart';

class ProjectGithubContributionsScreen extends StatefulWidget {
  final ProjectModel project;
  final String token;

  const ProjectGithubContributionsScreen({
    super.key,
    required this.project,
    required this.token,
  });

  @override
  State<ProjectGithubContributionsScreen> createState() =>
      _ProjectGithubContributionsScreenState();
}

class _ProjectGithubContributionsScreenState
    extends State<ProjectGithubContributionsScreen> {
  final _service = GithubContributionService();
  late Future<GithubContributionData> _future;

  @override
  void initState() {
    super.initState();
    _load();
  }

  void _load() {
    _future = _service.getProjectContributions(
      projectId: widget.project.projectId,
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
        title: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'GitHub Contributions',
              style: TextStyle(
                color: Colors.white,
                fontWeight: FontWeight.bold,
                fontSize: 17,
              ),
            ),
            Text(
              widget.project.name,
              style: const TextStyle(color: Colors.white70, fontSize: 12),
            ),
          ],
        ),
        backgroundColor: AppColors.secondary,
        elevation: 0,
        iconTheme: const IconThemeData(color: Colors.white),
      ),
      body: FutureBuilder<GithubContributionData>(
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

          final data = snapshot.data!;
          return RefreshIndicator(
            onRefresh: _refresh,
            color: AppColors.secondary,
            child: ListView(
              padding: const EdgeInsets.all(16),
              children: [
                _SummaryCard(data: data),
                if (data.overallCommitsOverTime.isNotEmpty) ...[
                  const SizedBox(height: 20),
                  const _SectionHeader(
                    icon: Icons.show_chart_rounded,
                    title: 'Commits Over Time',
                    count: -1,
                  ),
                  const SizedBox(height: 10),
                  _ChartCard(
                    child: _WeeklyBarChart(
                      weeks: data.overallCommitsOverTime
                          .map((w) => (w.weekStart, w.commitCount))
                          .toList(),
                      barColor: AppColors.secondary,
                    ),
                  ),
                ],
                if (data.repositories.isNotEmpty) ...[
                  const SizedBox(height: 20),
                  _SectionHeader(
                    icon: Icons.source_rounded,
                    title: 'Repositories',
                    count: data.repositories.length,
                  ),
                  const SizedBox(height: 10),
                  ...data.repositories.map((r) => _RepoCard(repo: r)),
                ],
                if (data.contributors.isNotEmpty) ...[
                  const SizedBox(height: 20),
                  _SectionHeader(
                    icon: Icons.people_rounded,
                    title: 'Contributors',
                    count: data.contributors.length,
                  ),
                  const SizedBox(height: 10),
                  ...data.contributors.map(
                    (c) => _ContributorCard(contributor: c),
                  ),
                ],
                if (data.contributors.isEmpty && data.repositories.isEmpty)
                  const Padding(
                    padding: EdgeInsets.symmetric(vertical: 48),
                    child: Center(
                      child: Text(
                        'No contribution data available yet.',
                        style: TextStyle(color: AppColors.textSecondary),
                      ),
                    ),
                  ),
                const SizedBox(height: 20),
                _ReportsNavigationCard(
                  onTap: () {
                    Navigator.of(context).push(
                      MaterialPageRoute(
                        builder: (_) => ProjectGithubReportsListScreen(
                          project: widget.project,
                          token: widget.token,
                        ),
                      ),
                    );
                  },
                ),
                const SizedBox(height: 16),
              ],
            ),
          );
        },
      ),
    );
  }
}

class _SummaryCard extends StatelessWidget {
  final GithubContributionData data;

  const _SummaryCard({required this.data});

  @override
  Widget build(BuildContext context) {
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
            color: AppColors.secondary.withValues(alpha: 0.3),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              const Icon(Icons.hub_rounded, color: Colors.white, size: 20),
              const SizedBox(width: 8),
              const Text(
                'Semester Overview',
                style: TextStyle(
                  color: Colors.white,
                  fontWeight: FontWeight.bold,
                  fontSize: 15,
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          Row(
            children: [
              Expanded(
                child: _StatItem(
                  label: 'Commits',
                  value: '${data.totalCommitsInSemester}',
                  icon: Icons.commit_rounded,
                  color: Colors.white,
                ),
              ),
              Expanded(
                child: _StatItem(
                  label: 'Additions',
                  value: '+${data.totalAdditionsInSemester}',
                  icon: Icons.add_circle_outline_rounded,
                  color: const Color(0xFF86EFAC),
                ),
              ),
              Expanded(
                child: _StatItem(
                  label: 'Deletions',
                  value: '-${data.totalDeletionsInSemester}',
                  icon: Icons.remove_circle_outline_rounded,
                  color: const Color(0xFFFCA5A5),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _StatItem extends StatelessWidget {
  final String label;
  final String value;
  final IconData icon;
  final Color color;

  const _StatItem({
    required this.label,
    required this.value,
    required this.icon,
    required this.color,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Icon(icon, color: color, size: 22),
        const SizedBox(height: 6),
        Text(
          value,
          style: TextStyle(
            color: color,
            fontWeight: FontWeight.bold,
            fontSize: 18,
          ),
        ),
        const SizedBox(height: 2),
        Text(
          label,
          style: const TextStyle(color: Colors.white70, fontSize: 12),
        ),
      ],
    );
  }
}

class _SectionHeader extends StatelessWidget {
  final IconData icon;
  final String title;
  final int count;

  const _SectionHeader({
    required this.icon,
    required this.title,
    required this.count,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Icon(icon, size: 18, color: AppColors.secondary),
        const SizedBox(width: 8),
        Text(
          title,
          style: const TextStyle(
            fontSize: 16,
            fontWeight: FontWeight.bold,
            color: AppColors.textPrimary,
          ),
        ),
        if (count >= 0) ...[
          const SizedBox(width: 8),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
            decoration: BoxDecoration(
              color: AppColors.secondary.withValues(alpha: 0.12),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Text(
              '$count',
              style: const TextStyle(
                fontSize: 12,
                fontWeight: FontWeight.w600,
                color: AppColors.secondary,
              ),
            ),
          ),
        ],
      ],
    );
  }
}

class _RepoCard extends StatelessWidget {
  final RepoContribution repo;

  const _RepoCard({required this.repo});

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 10),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.04),
            blurRadius: 8,
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
                width: 36,
                height: 36,
                decoration: BoxDecoration(
                  color: AppColors.primary.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: const Icon(
                  Icons.source_rounded,
                  color: AppColors.primary,
                  size: 18,
                ),
              ),
              const SizedBox(width: 10),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      repo.repoName,
                      style: const TextStyle(
                        fontSize: 14,
                        fontWeight: FontWeight.bold,
                        color: AppColors.textPrimary,
                      ),
                    ),
                    Text(
                      repo.repoOwnerName,
                      style: const TextStyle(
                        fontSize: 12,
                        color: AppColors.textSecondary,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 10),
          const Divider(height: 1),
          const SizedBox(height: 10),
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              _MiniStat(
                label: 'Commits',
                value: '${repo.totalCommits}',
                color: AppColors.secondary,
                centerAligned: true,
              ),
              const SizedBox(width: 16),
              _MiniStat(
                label: 'Additions',
                value: '+${repo.totalAdditions}',
                color: AppColors.success,
                centerAligned: true,
              ),
              const SizedBox(width: 16),
              _MiniStat(
                label: 'Deletions',
                value: '-${repo.totalDeletions}',
                color: AppColors.error,
                centerAligned: true,
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _ChartCard extends StatelessWidget {
  final Widget child;
  const _ChartCard({required this.child});

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.04),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      padding: const EdgeInsets.fromLTRB(16, 14, 16, 14),
      child: child,
    );
  }
}

class _ContributorCard extends StatefulWidget {
  final ContributorStats contributor;
  const _ContributorCard({required this.contributor});

  @override
  State<_ContributorCard> createState() => _ContributorCardState();
}

class _ContributorCardState extends State<_ContributorCard> {
  bool _expanded = false;

  @override
  Widget build(BuildContext context) {
    final contributor = widget.contributor;
    final displayName = contributor.userFullName ?? contributor.githubUsername;
    final subtitle = contributor.userFullName != null
        ? '@${contributor.githubUsername}'
        : null;
    final hasWeekly = contributor.weeklyActivity.isNotEmpty;

    return Container(
      margin: const EdgeInsets.only(bottom: 10),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.04),
            blurRadius: 8,
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
              CircleAvatar(
                radius: 22,
                backgroundColor: AppColors.primary.withValues(alpha: 0.15),
                child: Text(
                  displayName.isNotEmpty ? displayName[0].toUpperCase() : '?',
                  style: const TextStyle(
                    fontWeight: FontWeight.bold,
                    color: AppColors.primary,
                    fontSize: 18,
                  ),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      displayName,
                      style: const TextStyle(
                        fontSize: 14,
                        fontWeight: FontWeight.bold,
                        color: AppColors.textPrimary,
                      ),
                    ),
                    if (subtitle != null) ...[
                      const SizedBox(height: 2),
                      Text(
                        subtitle,
                        style: const TextStyle(
                          fontSize: 12,
                          color: AppColors.textSecondary,
                        ),
                      ),
                    ],
                    const SizedBox(height: 8),
                    Row(
                      children: [
                        _MiniStat(
                          label: 'Commits',
                          value: '${contributor.totalCommits}',
                          color: AppColors.secondary,
                        ),
                        const SizedBox(width: 14),
                        _MiniStat(
                          label: 'Add',
                          value: '+${contributor.totalAdditions}',
                          color: AppColors.success,
                        ),
                        const SizedBox(width: 14),
                        _MiniStat(
                          label: 'Del',
                          value: '-${contributor.totalDeletions}',
                          color: AppColors.error,
                        ),
                      ],
                    ),
                  ],
                ),
              ),
              if (hasWeekly)
                Align(
                  alignment: Alignment.centerRight,
                  child: TextButton.icon(
                    onPressed: () => setState(() => _expanded = !_expanded),
                    icon: Icon(
                      _expanded
                          ? Icons.expand_less_rounded
                          : Icons.expand_more_rounded,
                      size: 16,
                      color: AppColors.secondary,
                    ),
                    label: Text(
                      _expanded ? 'Hide chart' : 'View weekly',
                      style: const TextStyle(
                        fontSize: 12,
                        color: AppColors.secondary,
                      ),
                    ),
                    style: TextButton.styleFrom(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 8,
                        vertical: 4,
                      ),
                      minimumSize: Size.zero,
                      tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                    ),
                  ),
                ),
            ],
          ),
          if (hasWeekly && _expanded) ...[
            const SizedBox(height: 10),
            const Divider(height: 1),
            const SizedBox(height: 10),
            const Text(
              'Weekly Commits',
              style: TextStyle(
                fontSize: 12,
                fontWeight: FontWeight.w600,
                color: AppColors.textSecondary,
              ),
            ),
            const SizedBox(height: 8),
            _WeeklyBarChart(
              weeks: contributor.weeklyActivity
                  .map((w) => (w.weekStart, w.commits))
                  .toList(),
              barColor: AppColors.primary,
            ),
          ],
        ],
      ),
    );
  }
}

class _MiniStat extends StatelessWidget {
  final String label;
  final String value;
  final Color color;
  final bool centerAligned;

  const _MiniStat({
    required this.label,
    required this.value,
    required this.color,
    this.centerAligned = false,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: centerAligned
          ? CrossAxisAlignment.center
          : CrossAxisAlignment.start,
      children: [
        Text(
          value,
          textAlign: centerAligned ? TextAlign.center : TextAlign.start,
          style: TextStyle(
            fontSize: 13,
            fontWeight: FontWeight.bold,
            color: color,
          ),
        ),
        Text(
          label,
          textAlign: centerAligned ? TextAlign.center : TextAlign.start,
          style: const TextStyle(fontSize: 11, color: AppColors.textSecondary),
        ),
      ],
    );
  }
}

class _ReportsNavigationCard extends StatelessWidget {
  final VoidCallback onTap;

  const _ReportsNavigationCard({required this.onTap});

  @override
  Widget build(BuildContext context) {
    return Material(
      color: AppColors.surface,
      borderRadius: BorderRadius.circular(12),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: onTap,
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Row(
            children: [
              Container(
                width: 40,
                height: 40,
                decoration: BoxDecoration(
                  color: AppColors.secondary.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: const Icon(
                  Icons.description_rounded,
                  color: AppColors.secondary,
                ),
              ),
              const SizedBox(width: 12),
              const Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'GitHub Contribution Reports',
                      style: TextStyle(
                        fontSize: 14,
                        fontWeight: FontWeight.bold,
                        color: AppColors.textPrimary,
                      ),
                    ),
                    SizedBox(height: 2),
                    Text(
                      'View all reports of this project',
                      style: TextStyle(
                        fontSize: 12,
                        color: AppColors.textSecondary,
                      ),
                    ),
                  ],
                ),
              ),
              const Icon(
                Icons.chevron_right_rounded,
                color: AppColors.textSecondary,
              ),
            ],
          ),
        ),
      ),
    );
  }
}

// ─── Weekly bar chart (pure-Flutter, no external package) ─────────────────────

class _WeeklyBarChart extends StatelessWidget {
  /// Each entry: (weekStart, commitCount)
  final List<(DateTime, int)> weeks;
  final Color barColor;

  const _WeeklyBarChart({
    required this.weeks,
    this.barColor = AppColors.secondary,
  });

  @override
  Widget build(BuildContext context) {
    final allZero = weeks.every((w) => w.$2 == 0);
    if (weeks.isEmpty || allZero) {
      return const Padding(
        padding: EdgeInsets.symmetric(vertical: 8),
        child: Text(
          'No weekly commit data available.',
          style: TextStyle(color: AppColors.textSecondary, fontSize: 12),
        ),
      );
    }

    final maxVal = weeks.map((w) => w.$2).reduce(math.max);
    const barWidth = 26.0;
    const barSpacing = 10.0;
    const chartH = 80.0;
    const topPad = 18.0;
    const labelH = 36.0;
    final totalW = weeks.length * (barWidth + barSpacing);

    return LayoutBuilder(
      builder: (context, constraints) {
        return SingleChildScrollView(
          scrollDirection: Axis.horizontal,
          child: ConstrainedBox(
            constraints: BoxConstraints(minWidth: constraints.maxWidth),
            child: Center(
              child: SizedBox(
                height: topPad + chartH + labelH,
                width: math.max(totalW, 200),
                child: CustomPaint(
                  painter: _BarChartPainter(
                    weeks: weeks,
                    maxVal: maxVal,
                    barWidth: barWidth,
                    barSpacing: barSpacing,
                    chartH: chartH,
                    topPad: topPad,
                    barColor: barColor,
                  ),
                ),
              ),
            ),
          ),
        );
      },
    );
  }
}

class _BarChartPainter extends CustomPainter {
  final List<(DateTime, int)> weeks;
  final int maxVal;
  final double barWidth;
  final double barSpacing;
  final double chartH;
  final double topPad;
  final Color barColor;

  const _BarChartPainter({
    required this.weeks,
    required this.maxVal,
    required this.barWidth,
    required this.barSpacing,
    required this.chartH,
    required this.topPad,
    required this.barColor,
  });

  static final _fmt = DateFormat('MMM d');

  @override
  void paint(Canvas canvas, Size size) {
    final barPaint = Paint()
      ..color = barColor
      ..style = PaintingStyle.fill;
    final emptyPaint = Paint()
      ..color = barColor.withValues(alpha: 0.15)
      ..style = PaintingStyle.fill;

    for (int i = 0; i < weeks.length; i++) {
      final (weekStart, count) = weeks[i];
      final x = i * (barWidth + barSpacing);
      final fraction = maxVal > 0 ? count / maxVal : 0.0;
      final barH = math.max(fraction * chartH, count > 0 ? 4.0 : 2.0);
      final top = topPad + chartH - barH;

      // bar
      canvas.drawRRect(
        RRect.fromRectAndRadius(
          Rect.fromLTWH(x, top, barWidth, barH),
          const Radius.circular(4),
        ),
        count > 0 ? barPaint : emptyPaint,
      );

      // count label above bar
      if (count > 0) {
        _drawText(
          canvas,
          '$count',
          Offset(x + barWidth / 2, topPad + chartH - barH - 14),
          fontSize: 9,
          color: barColor,
          bold: true,
          centered: true,
        );
      }

      // rotated date label below bar
      _drawRotatedLabel(
        canvas,
        _fmt.format(weekStart),
        Offset(x + barWidth / 2, topPad + chartH + 4),
      );
    }
  }

  void _drawText(
    Canvas canvas,
    String text,
    Offset offset, {
    double fontSize = 10,
    Color color = AppColors.textSecondary,
    bool bold = false,
    bool centered = false,
  }) {
    final tp = TextPainter(
      text: TextSpan(
        text: text,
        style: TextStyle(
          fontSize: fontSize,
          color: color,
          fontWeight: bold ? FontWeight.bold : FontWeight.normal,
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    final dx = centered ? offset.dx - tp.width / 2 : offset.dx;
    tp.paint(canvas, Offset(dx, offset.dy));
  }

  void _drawRotatedLabel(Canvas canvas, String text, Offset pivot) {
    final tp = TextPainter(
      text: TextSpan(
        text: text,
        style: const TextStyle(fontSize: 9, color: AppColors.textDisabled),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    canvas.save();
    canvas.translate(pivot.dx, pivot.dy);
    canvas.rotate(-math.pi / 4);
    tp.paint(canvas, Offset(-tp.width / 2, 0));
    canvas.restore();
  }

  @override
  bool shouldRepaint(_BarChartPainter old) =>
      old.weeks != weeks || old.barColor != barColor;
}
