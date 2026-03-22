import type { WeeklyCommitDto } from "@/types";

interface CommitsOverTimeChartProps {
  data: WeeklyCommitDto[];
  title?: string;
  subtitle?: string;
}

export function CommitsOverTimeChart({
  data,
  title = "Commits over time",
  subtitle,
}: CommitsOverTimeChartProps) {
  if (!data || data.length === 0) {
    return null;
  }

  const maxCommits = Math.max(...data.map((d) => d.commitCount), 1);

  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    return date.toLocaleDateString("en-US", { month: "short", day: "numeric" });
  };

  return (
    <div className="bg-white rounded-lg p-6 border border-gray-200 shadow-sm">
      <div className="flex items-center justify-between mb-2">
        <h3 className="text-base font-semibold text-gray-900">{title}</h3>
        <button className="text-gray-400 hover:text-gray-600">
          <svg
            className="w-5 h-5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z"
            />
          </svg>
        </button>
      </div>

      {subtitle && <p className="text-sm text-gray-500 mb-4">{subtitle}</p>}

      <div className="relative h-48 mt-6">
        {/* Y-axis labels */}
        <div className="absolute left-0 top-0 bottom-8 flex flex-col justify-between text-xs text-gray-400">
          <span>{maxCommits}</span>
          <span>{Math.floor(maxCommits * 0.75)}</span>
          <span>{Math.floor(maxCommits * 0.5)}</span>
          <span>{Math.floor(maxCommits * 0.25)}</span>
          <span>0</span>
        </div>

        {/* Chart area */}
        <div className="ml-8 h-full border-b border-l border-gray-300">
          <div className="h-full flex items-end justify-center gap-1.5 px-2">
            {data.map((week, index) => {
              const heightPercent =
                maxCommits > 0 ? (week.commitCount / maxCommits) * 100 : 0;
              return (
                <div
                  key={index}
                  className="group relative cursor-pointer h-full flex items-end"
                  style={{ width: `${100 / data.length}%`, maxWidth: "60px" }}
                >
                  <div
                    className="bg-blue-500 hover:bg-blue-600 transition-colors rounded-t w-full"
                    style={{
                      height: `${heightPercent}%`,
                      minHeight: heightPercent > 0 ? "4px" : "0",
                    }}
                  />
                  {/* Tooltip */}
                  <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 hidden group-hover:block z-10">
                    <div className="bg-gray-900 text-white text-xs px-3 py-2 rounded shadow-lg whitespace-nowrap">
                      <div className="font-semibold">
                        {week.commitCount} commit
                        {week.commitCount !== 1 ? "s" : ""}
                      </div>
                      <div className="text-gray-300">
                        {formatDate(week.weekStart)} -{" "}
                        {formatDate(week.weekEnd)}
                      </div>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        {/* X-axis labels */}
        <div className="ml-8 mt-2 flex justify-between text-xs text-gray-400">
          {data.map((week, index) => {
            // Show label for first, middle, and last
            if (
              index === 0 ||
              index === Math.floor(data.length / 2) ||
              index === data.length - 1
            ) {
              return (
                <span key={index} className="flex-1 text-center">
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
