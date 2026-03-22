import React from "react";
import { Card, CardContent } from "@/components/ui/Card";
import { Calendar, TrendingUp, BarChart3 } from "lucide-react";

export const ActivityChart: React.FC = () => {
  return (
    <Card className="border-0 shadow-lg">
      <CardContent className="p-8">
        {/* Chart Header - InApp Style */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-8 gap-4">
          <div className="flex items-start gap-4">
            <div className="bg-gradient-to-br from-indigo-400 to-purple-500 w-12 h-12 rounded-xl flex items-center justify-center">
              <BarChart3 className="w-6 h-6 text-white" />
            </div>
            <div className="flex flex-col gap-1">
              <h3 className="text-2xl font-bold text-slate-900 dark:text-white">
                System Activity
              </h3>
              <p className="text-sm text-slate-500 dark:text-slate-400 font-medium">
                User logins and submission volume over the last 30 days
              </p>
            </div>
          </div>

          <div className="flex items-center gap-2">
            <button className="px-4 py-2 text-sm font-semibold text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg transition-colors">
              <Calendar className="w-4 h-4 inline mr-2" />
              Last 30 days
            </button>
          </div>
        </div>

        {/* Chart Container */}
        <div className="w-full h-80 flex items-center justify-center bg-gradient-to-br from-slate-50 to-slate-100 dark:from-slate-800 dark:to-slate-900 rounded-xl border-2 border-dashed border-slate-300 dark:border-slate-700">
          <div className="text-center">
            <div className="bg-slate-200 dark:bg-slate-700 w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4">
              <TrendingUp className="w-8 h-8 text-slate-400 dark:text-slate-500" />
            </div>
            <p className="text-lg font-semibold text-slate-600 dark:text-slate-300 mb-2">
              Activity Data Coming Soon
            </p>
            <p className="text-sm text-slate-400 dark:text-slate-500">
              Backend endpoint not yet implemented
            </p>
          </div>
        </div>
      </CardContent>
    </Card>
  );
};
