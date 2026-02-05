import React from 'react';
import { MoreVertical } from 'lucide-react';
import { Card } from '@/components/ui/Card';
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from '@/components/ui/Table';
import type { User } from '@/types';

export interface UserTableProps {
    users: User[];
}

export const UserTable: React.FC<UserTableProps> = ({ users }) => {
    // Generate avatar background color based on role
    const getAvatarClass = (role: User['role']) => {
        const map = {
            'Admin': 'bg-red-100 text-red-600 dark:bg-red-900 dark:text-red-300',
            'Teacher': 'bg-blue-100 text-blue-600 dark:bg-blue-900 dark:text-blue-300',
            'Student': 'bg-purple-100 text-purple-600 dark:bg-purple-900 dark:text-purple-300',
        };
        return map[role];
    };

    // Get initials from name
    const getInitials = (name: string) => {
        return name
            .split(' ')
            .map(word => word[0])
            .join('')
            .toUpperCase()
            .slice(0, 2);
    };

    // Format last active time
    const getLastActive = (updatedAt: string) => {
        const date = new Date(updatedAt);
        const now = new Date();
        const diffMs = now.getTime() - date.getTime();
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 60) return `${diffMins} mins ago`;
        if (diffHours < 24) return `${diffHours} hours ago`;
        return `${diffDays} days ago`;
    };

    return (
        <Card>
            <Table>
                <TableHeader>
                    <tr>
                        <TableHead>User</TableHead>
                        <TableHead>Role</TableHead>
                        <TableHead>ID</TableHead>
                        <TableHead>Last Active</TableHead>
                        <TableHead className="text-right">Actions</TableHead>
                    </tr>
                </TableHeader>
                <TableBody>
                    {users.map((user) => (
                        <TableRow key={user.userId}>
                            <TableCell>
                                <div className="flex items-center gap-3">
                                    {user.avatar ? (
                                        <img
                                            src={user.avatar}
                                            alt={user.name}
                                            className="w-8 h-8 rounded-full"
                                        />
                                    ) : (
                                        <div className={`w-8 h-8 rounded-full flex items-center justify-center font-bold text-xs ${getAvatarClass(user.role)}`}>
                                            {getInitials(user.name)}
                                        </div>
                                    )}
                                    <div>
                                        <div className="font-medium text-slate-900 dark:text-white">
                                            {user.name}
                                        </div>
                                        <div className="text-xs text-slate-500">
                                            {user.email}
                                        </div>
                                    </div>
                                </div>
                            </TableCell>
                            <TableCell className="text-slate-600 dark:text-slate-300">
                                {user.role}
                            </TableCell>
                            <TableCell className="text-slate-600 dark:text-slate-300">
                                {user.studentOrEmployeeId}
                            </TableCell>
                            <TableCell className="text-slate-500 dark:text-slate-400">
                                {getLastActive(user.updatedAt)}
                            </TableCell>
                            <TableCell className="text-right">
                                <button className="text-slate-400 hover:text-primary transition-colors">
                                    <MoreVertical className="w-4 h-4" />
                                </button>
                            </TableCell>
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
        </Card>
    );
};
