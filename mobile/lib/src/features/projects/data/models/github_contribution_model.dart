class WeeklyCommit {
  final DateTime weekStart;
  final int commitCount;

  const WeeklyCommit({required this.weekStart, required this.commitCount});

  factory WeeklyCommit.fromJson(Map<String, dynamic> json) => WeeklyCommit(
    weekStart: DateTime.parse(json['weekStart'] as String),
    commitCount: (json['commitCount'] as num?)?.toInt() ?? 0,
  );
}

class WeeklyActivity {
  final DateTime weekStart;
  final int commits;
  final int additions;
  final int deletions;

  const WeeklyActivity({
    required this.weekStart,
    required this.commits,
    required this.additions,
    required this.deletions,
  });

  factory WeeklyActivity.fromJson(Map<String, dynamic> json) => WeeklyActivity(
    weekStart: DateTime.parse(json['weekStart'] as String),
    commits: (json['commits'] as num?)?.toInt() ?? 0,
    additions: (json['additions'] as num?)?.toInt() ?? 0,
    deletions: (json['deletions'] as num?)?.toInt() ?? 0,
  );
}

class ContributorStats {
  final String githubUsername;
  final String? githubEmail;
  final String? userId;
  final String? userFullName;
  final int totalCommits;
  final int totalAdditions;
  final int totalDeletions;
  final List<WeeklyActivity> weeklyActivity;

  const ContributorStats({
    required this.githubUsername,
    this.githubEmail,
    this.userId,
    this.userFullName,
    required this.totalCommits,
    required this.totalAdditions,
    required this.totalDeletions,
    required this.weeklyActivity,
  });

  factory ContributorStats.fromJson(Map<String, dynamic> json) =>
      ContributorStats(
        githubUsername: json['githubUsername'] as String,
        githubEmail: json['githubEmail'] as String?,
        userId: json['userId']?.toString(),
        userFullName: json['userFullName'] as String?,
        totalCommits: (json['totalCommits'] as num?)?.toInt() ?? 0,
        totalAdditions: (json['totalAdditions'] as num?)?.toInt() ?? 0,
        totalDeletions: (json['totalDeletions'] as num?)?.toInt() ?? 0,
        weeklyActivity: (json['weeklyActivity'] as List<dynamic>? ?? [])
            .map((e) => WeeklyActivity.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}

class RepoContribution {
  final String repoOwnerName;
  final String repoName;
  final String repoUrl;
  final int totalCommits;
  final int totalAdditions;
  final int totalDeletions;
  final DateTime? lastSyncedAt;

  const RepoContribution({
    required this.repoOwnerName,
    required this.repoName,
    required this.repoUrl,
    required this.totalCommits,
    required this.totalAdditions,
    required this.totalDeletions,
    this.lastSyncedAt,
  });

  factory RepoContribution.fromJson(Map<String, dynamic> json) =>
      RepoContribution(
        repoOwnerName: json['repoOwnerName'] as String,
        repoName: json['repoName'] as String,
        repoUrl: json['repoUrl'] as String,
        totalCommits: (json['totalCommits'] as num?)?.toInt() ?? 0,
        totalAdditions: (json['totalAdditions'] as num?)?.toInt() ?? 0,
        totalDeletions: (json['totalDeletions'] as num?)?.toInt() ?? 0,
        lastSyncedAt: json['lastSyncedAt'] != null
            ? DateTime.parse(json['lastSyncedAt'] as String)
            : null,
      );
}

class GithubContributionData {
  final String projectId;
  final String projectName;
  final int totalCommitsInSemester;
  final int totalAdditionsInSemester;
  final int totalDeletionsInSemester;
  final List<WeeklyCommit> overallCommitsOverTime;
  final List<RepoContribution> repositories;
  final List<ContributorStats> contributors;

  const GithubContributionData({
    required this.projectId,
    required this.projectName,
    required this.totalCommitsInSemester,
    required this.totalAdditionsInSemester,
    required this.totalDeletionsInSemester,
    required this.overallCommitsOverTime,
    required this.repositories,
    required this.contributors,
  });

  factory GithubContributionData.fromJson(Map<String, dynamic> json) =>
      GithubContributionData(
        projectId: json['projectId'].toString(),
        projectName: json['projectName'] as String,
        totalCommitsInSemester:
            (json['totalCommitsInSemester'] as num?)?.toInt() ?? 0,
        totalAdditionsInSemester:
            (json['totalAdditionsInSemester'] as num?)?.toInt() ?? 0,
        totalDeletionsInSemester:
            (json['totalDeletionsInSemester'] as num?)?.toInt() ?? 0,
        overallCommitsOverTime:
            (json['overallCommitsOverTime'] as List<dynamic>? ?? [])
                .map((e) => WeeklyCommit.fromJson(e as Map<String, dynamic>))
                .toList(),
        repositories: (json['repositories'] as List<dynamic>? ?? [])
            .map((e) => RepoContribution.fromJson(e as Map<String, dynamic>))
            .toList(),
        contributors: (json['contributors'] as List<dynamic>? ?? [])
            .map((e) => ContributorStats.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}
