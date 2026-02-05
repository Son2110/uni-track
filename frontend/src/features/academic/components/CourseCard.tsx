import React from 'react';
import { User, Clock } from 'lucide-react';
import { Card } from '@/components/ui/Card';

// Extended Course interface with display fields
export interface CourseCardProps {
    course: {
        courseId: number;
        code: string;
        name: string;
        description: string;
        instructor?: string;
        schedule?: string;
        studentCount?: number;
    };
}

export const CourseCard: React.FC<CourseCardProps> = ({ course }) => {
    return (
        <Card className="overflow-hidden group hover:shadow-md transition-all">
            {/* Course Header */}
            <div className="h-32 bg-sidebar-blue relative p-6 flex flex-col justify-between">
                <span className="absolute top-4 right-4 bg-white/20 text-white text-xs px-2 py-1 rounded font-medium">
                    {course.code}
                </span>
                <h3 className="text-white font-bold text-xl mt-auto">
                    {course.name}
                </h3>
            </div>

            {/* Course Details */}
            <div className="p-6 flex flex-col gap-4">
                {course.instructor && (
                    <div className="flex items-center gap-2 text-slate-600 dark:text-slate-300 text-sm">
                        <User className="w-4 h-4" />
                        {course.instructor}
                    </div>
                )}
                {course.schedule && (
                    <div className="flex items-center gap-2 text-slate-600 dark:text-slate-300 text-sm">
                        <Clock className="w-4 h-4" />
                        {course.schedule}
                    </div>
                )}

                {/* Footer */}
                <div className="pt-4 border-t border-gray-100 dark:border-gray-700 flex justify-between items-center">
                    <span className="text-xs font-medium text-slate-400">
                        {course.studentCount || 0} Students
                    </span>
                    <button className="text-primary hover:text-primary-dark text-sm font-medium">
                        View Details
                    </button>
                </div>
            </div>
        </Card>
    );
};
