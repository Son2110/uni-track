import React from 'react';
import { Bell, HelpCircle, Search } from 'lucide-react';
import { Input } from '@/components/ui/Input';

export const Header: React.FC = () => {
    return (
        <header className="h-16 bg-white dark:bg-card-dark border-b border-gray-200 dark:border-gray-800 flex items-center justify-between px-6 md:px-8 flex-shrink-0">
            {/* Search Section */}
            <div className="flex items-center gap-4">
                <div className="hidden md:block">
                    <Input
                        icon={Search}
                        placeholder="Search data..."
                        type="text"
                        className="w-64"
                    />
                </div>
            </div>

            {/* Actions Section */}
            <div className="flex items-center gap-4">
                {/* Notifications Button */}
                <button className="relative p-2 text-gray-500 hover:text-primary transition-colors rounded-full hover:bg-gray-50 dark:hover:bg-gray-800">
                    <Bell className="w-5 h-5" />
                    <span className="absolute top-2 right-2 w-2 h-2 bg-red-500 rounded-full border-2 border-white dark:border-card-dark" />
                </button>

                {/* Help Button */}
                <button className="p-2 text-gray-500 hover:text-primary transition-colors rounded-full hover:bg-gray-50 dark:hover:bg-gray-800">
                    <HelpCircle className="w-5 h-5" />
                </button>
            </div>
        </header>
    );
};
