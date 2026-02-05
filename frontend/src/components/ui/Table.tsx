import React from 'react';
import { cn } from '@/lib/utils';

export interface TableProps {
    children: React.ReactNode;
    className?: string;
}

export const Table: React.FC<TableProps> = ({ children, className }) => {
    return (
        <div className="overflow-x-auto">
            <table className={cn('w-full text-sm text-left', className)}>
                {children}
            </table>
        </div>
    );
};

export const TableHeader: React.FC<TableProps> = ({ children, className }) => {
    return (
        <thead className={cn('text-xs text-slate-500 dark:text-slate-400 uppercase bg-gray-50 dark:bg-gray-800/50', className)}>
            {children}
        </thead>
    );
};

export const TableBody: React.FC<TableProps> = ({ children, className }) => {
    return (
        <tbody className={cn('divide-y divide-gray-100 dark:divide-gray-800', className)}>
            {children}
        </tbody>
    );
};

export const TableRow: React.FC<TableProps> = ({ children, className }) => {
    return (
        <tr className={cn('hover:bg-gray-50 dark:hover:bg-gray-800/30 transition-colors', className)}>
            {children}
        </tr>
    );
};

export const TableHead: React.FC<TableProps> = ({ children, className }) => {
    return (
        <th className={cn('px-6 py-4 font-medium', className)}>
            {children}
        </th>
    );
};

export const TableCell: React.FC<TableProps> = ({ children, className }) => {
    return (
        <td className={cn('px-6 py-4', className)}>
            {children}
        </td>
    );
};
