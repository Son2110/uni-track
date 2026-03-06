import React from "react";
import { Card, CardContent } from "@/components/ui/Card";

export const ActivityChart: React.FC = () => {
  return (
    <Card>
      <CardContent className="p-6 md:p-8">
        {/* Chart Header */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-8 gap-4">
          <div className="flex flex-col gap-1">
            <h3 className="text-lg font-bold text-slate-900 dark:text-white">
              System Activity
            </h3>
            <p className="text-sm text-slate-500 dark:text-slate-400">
              User logins and submission volume over the last 30 days.
            </p>
          </div>
        </div>

        {/* Chart Container - No data available */}
        <div className="w-full h-80 flex items-center justify-center">
          <div className="text-center text-slate-400">
            <p className="text-sm">Activity data not available</p>
            <p className="text-xs mt-1">Backend endpoint not implemented</p>
          </div>
        </div>
      </CardContent>
    </Card>
  );
};
