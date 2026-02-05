import React from 'react';
import type { LucideIcon } from 'lucide-react';
import { TrendingUp } from 'lucide-react';

export interface StatsCardProps {
    icon: LucideIcon;
    title: string;
    value: string | number;
    trend: number;
    bgIcon: LucideIcon;
}

export const StatsCard: React.FC<StatsCardProps> = ({ icon: Icon, title, value, trend, bgIcon: BgIcon }) => {
    return (
        <div className="bg-card-light dark:bg-card-dark p-6 rounded-xl shadow-sm border border-gray-100 dark:border-gray-800 flex flex-col justify-between h-36 relative overflow-hidden group hover:shadow-md transition-shadow">
            {/* Background Icon */}
            <div className="absolute right-0 top-0 p-4 opacity-10 group-hover:opacity-20 transition-opacity">
                <BgIcon className="w-20 h-20 text-primary" />
            </div>

            {/* Header with Icon and Trend */}
            <div className="flex justify-between items-start z-10">
                <div className="flex items-center justify-center w-12 h-12 rounded-lg bg-primary/10 text-primary">
                    <Icon className="w-6 h-6" />
                </div>
                <span className="flex items-center text-xs font-bold text-green-600 bg-green-50 dark:bg-green-900/20 px-2 py-1 rounded-full">
                    <TrendingUp className="w-3.5 h-3.5 mr-1" />
                    +{trend}%
                </span>
            </div>

            {/* Stats Content */}
            <div className="flex flex-col gap-1 z-10">
                <p className="text-slate-500 dark:text-slate-400 text-sm font-medium">{title}</p>
                <h3 className="text-3xl font-bold text-slate-900 dark:text-white">{value}</h3>
            </div>
        </div>
    );
};
