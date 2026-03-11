import React from "react";
import type { LucideIcon } from "lucide-react";
import { TrendingUp } from "lucide-react";

export interface StatsCardProps {
  icon: LucideIcon;
  title: string;
  value: string | number;
  trend?: number;
  bgColor: string;
  iconBg: string;
}

export const StatsCard: React.FC<StatsCardProps> = ({
  icon: Icon,
  title,
  value,
  trend,
  bgColor,
  iconBg,
}) => {
  return (
    <div
      className={`${bgColor} p-6 rounded-2xl shadow-lg hover:shadow-xl transition-all duration-300 transform hover:-translate-y-1 relative overflow-hidden`}
    >
      {/* Decorative gradient overlay */}
      <div className="absolute inset-0 bg-gradient-to-br from-white/10 to-transparent pointer-events-none"></div>

      {/* Icon container */}
      <div
        className={`${iconBg} w-14 h-14 rounded-xl flex items-center justify-center mb-4 relative z-10`}
      >
        <Icon className="w-7 h-7 text-white" />
      </div>

      {/* Stats Content */}
      <div className="relative z-10">
        <p className="text-white/80 text-sm font-medium mb-2">{title}</p>
        <h3 className="text-4xl font-bold text-white mb-2">{value}</h3>
        {trend !== undefined && (
          <div className="flex items-center gap-1 text-white/90">
            <TrendingUp className="w-4 h-4" />
            <span className="text-sm font-semibold">+{trend}%</span>
            <span className="text-xs text-white/70 ml-1">from last month</span>
          </div>
        )}
      </div>
    </div>
  );
};
