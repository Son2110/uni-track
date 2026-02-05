import React from 'react';
import { Plus } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { SemesterTable } from '@/features/academic/components/SemesterTable';
import { mockSemesters } from '@/data/mockData';

export const SemesterPage: React.FC = () => {
    return (
        <div className="flex flex-col gap-8 pb-10">
            {/* Page Header */}
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                <div className="flex flex-col gap-1">
                    <h2 className="text-3xl font-black text-slate-900 dark:text-white tracking-tight">
                        Semesters
                    </h2>
                    <p className="text-slate-500 dark:text-slate-400 font-medium">
                        Manage academic terms and timelines.
                    </p>
                </div>
                <Button variant="primary">
                    <Plus className="w-4 h-4" />
                    New Semester
                </Button>
            </div>

            {/* Semester Table */}
            <SemesterTable semesters={mockSemesters} />
        </div>
    );
};
