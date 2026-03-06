import React from "react";
import { Users, BookOpen, Briefcase, DoorOpen } from "lucide-react";
import { Download } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { StatsCard } from "@/features/dashboard/components/StatsCard";
import { ActivityChart } from "@/features/dashboard/components/ActivityChart";
import { LoadingSpinner } from "@/components/ui/Loading";
import { useDashboardStats } from "@/features/dashboard/api/useDashboard";

export const DashboardPage: React.FC = () => {
  const { data: stats, isLoading } = useDashboardStats();

  return (
    <div className="flex flex-col gap-8 pb-10">
      {/* Page Header */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div className="flex flex-col gap-1">
          <h2 className="text-3xl font-black text-slate-900 dark:text-white tracking-tight">
            Dashboard Overview
          </h2>
          <p className="text-slate-500 dark:text-slate-400 font-medium">
            Welcome back, Administrator. Here's what's happening today.
          </p>
        </div>
        <Button variant="secondary">
          <Download className="w-4 h-4" />
          Export Report
        </Button>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {isLoading ? (
          <div className="col-span-full flex justify-center py-8">
            <LoadingSpinner size="lg" />
          </div>
        ) : (
          <>
            <StatsCard
              icon={Users}
              title="Total Users"
              value={stats?.totalUsers.toLocaleString() || "0"}
              trend={5}
              bgIcon={Users}
            />
            <StatsCard
              icon={BookOpen}
              title="Active Courses"
              value={stats?.totalCourses.toLocaleString() || "0"}
              trend={12}
              bgIcon={BookOpen}
            />
            <StatsCard
              icon={Briefcase}
              title="Ongoing Projects"
              value={stats?.totalProjects.toLocaleString() || "0"}
              trend={2}
              bgIcon={Briefcase}
            />
            <StatsCard
              icon={DoorOpen}
              title="Total Classes"
              value={stats?.totalClasses.toLocaleString() || "0"}
              trend={8}
              bgIcon={DoorOpen}
            />
          </>
        )}
      </div>

      {/* Activity Chart Section */}
      <ActivityChart />
    </div>
  );
};
