import React from 'react';
import { Plus } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { ClassCard } from '@/features/academic/components/ClassCard';
import { mockClasses, mockCourses, mockUsers, mockClassEnrollments } from '@/data/mockData';

export const ClassPage: React.FC = () => {
    // Enhance classes with related data
    const classesWithDetails = mockClasses.map(classItem => {
        const course = mockCourses.find(c => c.courseId === classItem.courseId);
        const teacher = mockUsers.find(u => u.userId === classItem.teacherId);
        const enrollments = mockClassEnrollments.filter(e => e.classId === classItem.classId);

        // Determine status with proper typing
        const status: 'Active' | 'Scheduled' | 'Full' = enrollments.length >= 30 ? 'Full' : 'Active';

        return {
            classId: classItem.classId,
            classCode: classItem.classCode,
            courseName: course?.name || 'Unknown Course',
            courseCode: course?.code || 'N/A',
            lecturer: teacher?.name || 'TBA',
            studentCount: enrollments.length,
            status,
            category: classItem.classCode.substring(0, 4), // e.g., "SE18" from "SE1801"
        };
    });

    return (
        <div className="flex flex-col gap-8 pb-10">
            {/* Page Header */}
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                <div className="flex flex-col gap-1">
                    <h2 className="text-3xl font-black text-slate-900 dark:text-white tracking-tight">
                        Classes
                    </h2>
                    <p className="text-slate-500 dark:text-slate-400 font-medium">
                        Manage and organize class schedules and assignments.
                    </p>
                </div>
                <Button variant="primary">
                    <Plus className="w-4 h-4" />
                    Create Class
                </Button>
            </div>

            {/* Class Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                {classesWithDetails.map((classItem) => (
                    <ClassCard key={classItem.classId} classItem={classItem} />
                ))}
            </div>
        </div>
    );
};
