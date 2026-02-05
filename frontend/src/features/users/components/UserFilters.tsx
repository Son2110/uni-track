import React from 'react';
import { Search, Filter } from 'lucide-react';
import { Card } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';

export const UserFilters: React.FC = () => {
    return (
        <Card className="p-4 flex flex-col sm:flex-row gap-4 items-center justify-between">
            {/* Search Input */}
            <div className="w-full sm:w-72">
                <Input
                    icon={Search}
                    placeholder="Search by name, email..."
                    type="text"
                />
            </div>

            {/* Filter Controls */}
            <div className="flex items-center gap-3 w-full sm:w-auto">
                <select className="form-select bg-gray-50 dark:bg-gray-800 border-none rounded-lg text-sm py-2 px-4 focus:ring-2 focus:ring-primary/50 text-slate-700 dark:text-white cursor-pointer">
                    <option>All Roles</option>
                    <option>Administrator</option>
                    <option>Professor</option>
                    <option>Student</option>
                </select>
                <button className="p-2 bg-gray-100 dark:bg-gray-700 rounded-lg text-slate-600 dark:text-slate-300 hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors">
                    <Filter className="w-5 h-5" />
                </button>
            </div>
        </Card>
    );
};
