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
      {/* Page Header - InApp Style */}
      <div className="bg-gradient-to-r from-primary/80 to-primary/60 rounded-2xl p-8 shadow-lg">
        <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
          <div className="flex flex-col gap-2">
            <h2 className="text-4xl font-bold text-white tracking-tight">
              Dashboard Overview
            </h2>
            <p className="text-white/90 text-lg font-medium">
              Welcome back, Administrator. Here's what's happening today.
            </p>
          </div>
          <Button
            variant="secondary"
            className="bg-white hover:bg-gray-50 text-primary shadow-md"
          >
            <Download className="w-4 h-4" />
            Export Report
          </Button>
        </div>
      </div>

      {/* Stats Grid - InApp Style */}
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
              bgColor="bg-gradient-to-br from-blue-400 to-blue-500"
              iconBg="bg-blue-600/30"
            />
            <StatsCard
              icon={BookOpen}
              title="Active Courses"
              value={stats?.totalCourses.toLocaleString() || "0"}
              trend={12}
              bgColor="bg-gradient-to-br from-purple-400 to-purple-500"
              iconBg="bg-purple-600/30"
            />
            <StatsCard
              icon={Briefcase}
              title="Ongoing Projects"
              value={stats?.totalProjects.toLocaleString() || "0"}
              trend={2}
              bgColor="bg-gradient-to-br from-green-400 to-green-500"
              iconBg="bg-green-600/30"
            />
            <StatsCard
              icon={DoorOpen}
              title="Total Classes"
              value={stats?.totalClasses.toLocaleString() || "0"}
              trend={8}
              bgColor="bg-gradient-to-br from-orange-400 to-orange-500"
              iconBg="bg-orange-600/30"
            />
          </>
        )}
      </div>

      {/* Activity Chart Section */}
      <ActivityChart />
    </div>
  );
};
