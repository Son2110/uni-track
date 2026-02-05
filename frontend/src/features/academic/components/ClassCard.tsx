import React from 'react';
import { User, Users } from 'lucide-react';
import { Card } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';

// Extended class interface for display
export interface ClassCardProps {
    classItem: {
        classId: number;
        classCode: string;
        courseName?: string;
        lecturer?: string;
        studentCount?: number;
        status?: 'Active' | 'Scheduled' | 'Full';
        category?: string;
    };
}

export const ClassCard: React.FC<ClassCardProps> = ({ classItem }) => {
    const getStatusVariant = (status: 'Active' | 'Scheduled' | 'Full') => {
        const map = {
            'Active': 'active' as const,
            'Scheduled': 'scheduled' as const,
            'Full': 'full' as const,
        };
        return map[status] || 'active';
    };

    return (
        <Card className="p-5 flex flex-col gap-4 hover:shadow-md transition-shadow">
            {/* Header */}
            <div className="flex justify-between items-start">
                <div className="w-12 h-12 rounded-lg bg-blue-50 dark:bg-blue-900/30 text-sidebar-blue dark:text-blue-300 flex items-center justify-center font-bold text-sm">
                    {classItem.category || classItem.classCode.substring(0, 4)}
                </div>
                <Badge variant={getStatusVariant(classItem.status || 'Active')}>
                    {classItem.status || 'Active'}
                </Badge>
            </div>

            {/* Class Info */}
            <div>
                <h3 className="font-bold text-lg text-slate-900 dark:text-white mb-1">
                    {classItem.classCode}
                </h3>
                <p className="text-sm text-slate-500 dark:text-slate-400">
                    {classItem.courseName || 'Course Name'}
                </p>
            </div>

            {/* Details */}
            <div className="mt-auto pt-4 border-t border-gray-100 dark:border-gray-800 flex flex-col gap-2">
                <div className="flex items-center justify-between text-sm">
                    <span className="text-slate-400 flex items-center gap-1">
                        <User className="w-4 h-4" />
                        Lecturer
                    </span>
                    <span className="font-medium text-slate-700 dark:text-slate-200">
                        {classItem.lecturer || 'TBA'}
                    </span>
                </div>
                <div className="flex items-center justify-between text-sm">
                    <span className="text-slate-400 flex items-center gap-1">
                        <Users className="w-4 h-4" />
                        Students
                    </span>
                    <span className="font-medium text-slate-700 dark:text-slate-200">
                        {classItem.studentCount || 0}
                    </span>
                </div>
            </div>
        </Card>
    );
};
