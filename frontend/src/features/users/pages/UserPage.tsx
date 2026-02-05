import React from 'react';
import { Upload, UserPlus } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { UserFilters } from '@/features/users/components/UserFilters';
import { UserTable } from '@/features/users/components/UserTable';
import { mockUsers } from '@/data/mockData';

export const UserPage: React.FC = () => {
    return (
        <div className="flex flex-col gap-8 pb-10">
            {/* Page Header */}
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                <div className="flex flex-col gap-1">
                    <h2 className="text-3xl font-black text-slate-900 dark:text-white tracking-tight">
                        User Management
                    </h2>
                    <p className="text-slate-500 dark:text-slate-400 font-medium">
                        Control user access and roles.
                    </p>
                </div>
                <div className="flex gap-3">
                    <Button variant="success">
                        <Upload className="w-4 h-4" />
                        Import Excel
                    </Button>
                    <Button variant="primary">
                        <UserPlus className="w-4 h-4" />
                        Add User
                    </Button>
                </div>
            </div>

            {/* Filters */}
            <UserFilters />

            {/* User Table */}
            <UserTable users={mockUsers} />
        </div>
    );
};
