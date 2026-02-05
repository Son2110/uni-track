import React from 'react';
import { Card, CardContent } from '@/components/ui/Card';

export const ActivityChart: React.FC = () => {
    return (
        <Card>
            <CardContent className="p-6 md:p-8">
                {/* Chart Header */}
                <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-8 gap-4">
                    <div className="flex flex-col gap-1">
                        <h3 className="text-lg font-bold text-slate-900 dark:text-white">System Activity</h3>
                        <p className="text-sm text-slate-500 dark:text-slate-400">
                            User logins and submission volume over the last 30 days.
                        </p>
                    </div>
                </div>

                {/* Chart Container */}
                <div className="w-full h-80 relative">
                    {/* Y-Axis Labels */}
                    <div className="absolute left-0 top-0 bottom-8 w-8 flex flex-col justify-between text-xs text-slate-400 font-medium text-right pr-2">
                        <span>40k</span>
                        <span>30k</span>
                        <span>20k</span>
                        <span>10k</span>
                        <span>0</span>
                    </div>

                    {/* Chart Area */}
                    <div className="absolute left-10 right-0 top-2 bottom-6">
                        <div className="w-full h-full flex flex-col justify-between border-l border-b border-gray-100 dark:border-gray-800">
                            <div className="w-full h-px bg-gray-100 dark:bg-gray-800" />
                            <div className="w-full h-px bg-gray-100 dark:bg-gray-800" />
                            <div className="w-full h-px bg-gray-100 dark:bg-gray-800" />
                            <div className="w-full h-px bg-gray-100 dark:bg-gray-800" />
                            <div className="w-full h-px bg-gray-100 dark:bg-gray-800" />
                        </div>

                        {/* SVG Chart */}
                        <svg
                            className="absolute inset-0 w-full h-full overflow-visible"
                            preserveAspectRatio="none"
                            viewBox="0 0 1000 300"
                        >
                            <defs>
                                <linearGradient id="chartGradient" x1="0" x2="0" y1="0" y2="1">
                                    <stop offset="0%" stopColor="#f27121" stopOpacity="0.2" />
                                    <stop offset="100%" stopColor="#f27121" stopOpacity="0" />
                                </linearGradient>
                            </defs>
                            <path
                                d="M0,250 C100,250 150,100 250,120 S350,200 500,150 S650,50 750,80 S900,150 1000,100 V300 H0 Z"
                                fill="url(#chartGradient)"
                            />
                            <path
                                d="M0,250 C100,250 150,100 250,120 S350,200 500,150 S650,50 750,80 S900,150 1000,100"
                                fill="none"
                                stroke="#f27121"
                                strokeLinecap="round"
                                strokeWidth="4"
                                vectorEffect="non-scaling-stroke"
                            />
                            <circle cx="750" cy="80" fill="white" r="6" stroke="#f27121" strokeWidth="3" />
                        </svg>

                        {/* Tooltip */}
                        <div className="absolute top-[50px] left-[73%] transform -translate-x-1/2 bg-slate-800 text-white text-xs py-1 px-2 rounded shadow-lg pointer-events-none">
                            18,450 Users
                            <div className="absolute top-full left-1/2 -translate-x-1/2 -mt-1 border-4 border-transparent border-t-slate-800" />
                        </div>
                    </div>

                    {/* X-Axis Labels */}
                    <div className="absolute left-10 right-0 bottom-0 h-6 flex justify-between text-xs text-slate-400 font-medium pt-2">
                        <span>Week 1</span>
                        <span>Week 2</span>
                        <span>Week 3</span>
                        <span>Week 4</span>
                    </div>
                </div>
            </CardContent>
        </Card>
    );
};
