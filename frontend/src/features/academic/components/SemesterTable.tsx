import React from 'react';
import { Edit } from 'lucide-react';
import { Card } from '@/components/ui/Card';
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from '@/components/ui/Table';
import { Badge } from '@/components/ui/Badge';
import type { Semester } from '@/types';

export interface SemesterTableProps {
    semesters: Semester[];
}

// Helper to determine status based on dates
const getSemesterStatus = (startDate: string, endDate: string): 'Active' | 'Upcoming' | 'Archived' => {
    const now = new Date();
    const start = new Date(startDate);
    const end = new Date(endDate);

    if (now >= start && now <= end) return 'Active';
    if (now < start) return 'Upcoming';
    return 'Archived';
};

// Format ISO date to display format
const formatDate = (isoDate: string): string => {
    return new Date(isoDate).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric'
    });
};

export const SemesterTable: React.FC<SemesterTableProps> = ({ semesters }) => {
    const getStatusVariant = (status: 'Active' | 'Upcoming' | 'Archived') => {
        const map = {
            'Active': 'active' as const,
            'Upcoming': 'upcoming' as const,
            'Archived': 'archived' as const,
        };
        return map[status];
    };

    return (
        <Card>
            <Table>
                <TableHeader>
                    <tr>
                        <TableHead>Name</TableHead>
                        <TableHead>Start Date</TableHead>
                        <TableHead>End Date</TableHead>
                        <TableHead>Status</TableHead>
                        <TableHead className="text-right">Actions</TableHead>
                    </tr>
                </TableHeader>
                <TableBody>
                    {semesters.map((semester) => {
                        const status = getSemesterStatus(semester.startDate, semester.endDate);
                        return (
                            <TableRow key={semester.semesterId}>
                                <TableCell className="font-medium text-slate-900 dark:text-white">
                                    {semester.name}
                                </TableCell>
                                <TableCell className="text-slate-600 dark:text-slate-300">
                                    {formatDate(semester.startDate)}
                                </TableCell>
                                <TableCell className="text-slate-600 dark:text-slate-300">
                                    {formatDate(semester.endDate)}
                                </TableCell>
                                <TableCell>
                                    <Badge variant={getStatusVariant(status)}>
                                        {status}
                                    </Badge>
                                </TableCell>
                                <TableCell className="text-right">
                                    <button className="text-slate-400 hover:text-primary transition-colors">
                                        <Edit className="w-4 h-4" />
                                    </button>
                                </TableCell>
                            </TableRow>
                        );
                    })}
                </TableBody>
            </Table>
        </Card>
    );
};
