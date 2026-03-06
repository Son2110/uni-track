import type { ContributorStatsDto } from "@/types";

interface ContributorCardProps {
  contributor: ContributorStatsDto;
  rank: number;
}

export function ContributorCard({ contributor, rank }: ContributorCardProps) {
  const maxCommits = Math.max(
    ...contributor.weeklyActivity.map((w) => w.commits),
    1,
  );

  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    return date.toLocaleDateString("en-US", { month: "short", day: "numeric" });
  };

  // Generate avatar color from username
  const getAvatarColor = (username: string) => {
    const colors = [
      "bg-blue-500",
      "bg-green-500",
      "bg-purple-500",
      "bg-pink-500",
      "bg-yellow-500",
      "bg-red-500",
      "bg-indigo-500",
    ];
    const index = username
      .split("")
      .reduce((acc, char) => acc + char.charCodeAt(0), 0);
    return colors[index % colors.length];
  };

  return (
    <div className="bg-white rounded-lg p-4 border border-gray-200 shadow-sm">
      <div className="flex items-start justify-between mb-4">
        <div className="flex items-center gap-3">
          {/* Avatar */}
          <div
            className={`w-10 h-10 rounded-full ${getAvatarColor(contributor.githubUsername)} flex items-center justify-center text-white font-semibold`}
          >
            {contributor.githubUsername.substring(0, 2).toUpperCase()}
          </div>

          {/* User info */}
          <div>
            <a
              href={`https://github.com/${contributor.githubUsername}`}
              target="_blank"
              rel="noopener noreferrer"
              className="text-blue-600 hover:text-blue-700 font-medium"
            >
              {contributor.userFullName || contributor.githubUsername}
            </a>
            <div className="text-sm text-gray-600">
              {contributor.totalCommits} commits{" "}
              <span className="text-green-600">
                {contributor.totalAdditions.toLocaleString()}++
              </span>{" "}
              <span className="text-red-600">
                {contributor.totalDeletions.toLocaleString()}--
              </span>
            </div>
          </div>
        </div>

        {/* Rank badge */}
        <div className="bg-gray-100 text-gray-700 text-sm font-medium px-2 py-1 rounded border border-gray-300">
          #{rank}
        </div>
      </div>

      {/* Personal commit chart */}
      <div className="relative h-24 mt-4">
        <div className="h-full flex items-end justify-center gap-0.5">
          {contributor.weeklyActivity.map((week, index) => {
            const heightPercent =
              maxCommits > 0 ? (week.commits / maxCommits) * 100 : 0;
            return (
              <div
                key={index}
                className="group relative cursor-pointer h-full flex items-end"
                style={{
                  width: `${100 / contributor.weeklyActivity.length}%`,
                  maxWidth: "40px",
                }}
              >
                <div
                  className="bg-blue-500 hover:bg-blue-600 transition-colors rounded-t w-full"
                  style={{
                    height: `${heightPercent}%`,
                    minHeight: heightPercent > 0 ? "3px" : "0",
                  }}
                />
                {/* Tooltip */}
                <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 hidden group-hover:block z-10">
                  <div className="bg-gray-900 text-white text-xs px-3 py-2 rounded shadow-lg whitespace-nowrap">
                    <div className="font-semibold">
                      {week.commits} commit{week.commits !== 1 ? "s" : ""}
                    </div>
                    <div className="text-green-400">
                      {week.additions.toLocaleString()}++
                    </div>
                    <div className="text-red-400">
                      {week.deletions.toLocaleString()}--
                    </div>
                    <div className="text-gray-300 mt-0.5">
                      {formatDate(week.weekStart)}
                    </div>
                  </div>
                </div>
              </div>
            );
          })}
        </div>

        {/* X-axis labels */}
        <div className="mt-2 flex justify-between text-xs text-gray-400">
          {contributor.weeklyActivity.map((week, index) => {
            // Show label for first, middle, and last
            if (
              index === 0 ||
              index === Math.floor(contributor.weeklyActivity.length / 2) ||
              index === contributor.weeklyActivity.length - 1
            ) {
              return (
                <span key={index} className="flex-1 text-center text-[10px]">
                  {formatDate(week.weekStart)}
                </span>
              );
            }
            return <span key={index} className="flex-1" />;
          })}
        </div>
      </div>
    </div>
  );
}
