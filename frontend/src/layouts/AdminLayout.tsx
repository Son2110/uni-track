import React from 'react';
import { Outlet } from 'react-router-dom';
import { Sidebar } from '@/components/Sidebar';
import { Header } from '@/components/Header';

export const AdminLayout: React.FC = () => {
    return (
        <div className="bg-background-light dark:bg-background-dark text-slate-900 dark:text-white h-screen overflow-hidden flex selection:bg-primary selection:text-white">
            {/* Sidebar */}
            <Sidebar />

            {/* Main Content Area */}
            <main className="flex-1 flex flex-col h-full overflow-hidden">
                {/* Header */}
                <Header />

                {/* Page Content (Router Outlet) */}
                <div className="flex-1 overflow-y-auto p-6 md:p-8 scroll-smooth">
                    <div className="max-w-7xl mx-auto w-full">
                        <Outlet />
                    </div>
                </div>
            </main>
        </div>
    );
};
