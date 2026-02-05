import React from 'react';
import { CourseCard } from '@/features/academic/components/CourseCard';
import { mockCourses, mockUsers, mockClasses } from '@/data/mockData';

export const CoursePage: React.FC = () => {
    // Enhance courses with teacher name and student count
    const coursesWithDetails = mockCourses.map(course => {
        // Find classes for this course
        const courseClasses = mockClasses.filter(c => c.courseId === course.courseId);

        // Get teacher from first class (or use a default)
        const firstClass = courseClasses[0];
        const teacher = firstClass ? mockUsers.find(u => u.userId === firstClass.teacherId) : null;

        return {
            ...course,
            instructor: teacher?.name || 'TBA',
            schedule: 'Mon/Wed 10:00 AM', // Default schedule
            studentCount: courseClasses.length * 30, // Approximate
        };
    });

    return (
        <div className="flex flex-col gap-8 pb-10">
            {/* Page Header */}
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                <div className="flex flex-col gap-1">
                    <h2 className="text-3xl font-black text-slate-900 dark:text-white tracking-tight">
                        Courses
                    </h2>
                    <p className="text-slate-500 dark:text-slate-400 font-medium">
                        Browse and manage academic courses.
                    </p>
                </div>
            </div>

            {/* Course Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {coursesWithDetails.map((course) => (
                    <CourseCard key={course.courseId} course={course} />
                ))}
            </div>
        </div>
    );
};
