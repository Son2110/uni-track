import React from 'react';
import { NavLink } from 'react-router-dom';
import {
    LayoutDashboard,
    Calendar,
    BookOpen,
    DoorOpen,
    Users,
    Settings,
    GraduationCap
} from 'lucide-react';
import { useStore } from '@/app/store';
import { cn } from '@/lib/utils';

const navItems = [
    { to: '/admin/dashboard', icon: LayoutDashboard, label: 'Dashboard' },
    { to: '/admin/semesters', icon: Calendar, label: 'Semesters' },
    { to: '/admin/courses', icon: BookOpen, label: 'Courses' },
    { to: '/admin/classes', icon: DoorOpen, label: 'Classes' },
    { to: '/admin/users', icon: Users, label: 'User Management' },
];

export const Sidebar: React.FC = () => {
    const user = useStore((state) => state.user);

    return (
        <aside className="w-64 bg-sidebar-blue dark:bg-[#154288] flex flex-col h-full flex-shrink-0 transition-all duration-300">
            {/* Logo Section */}
            <div className="p-6 flex items-center gap-3">
                <div className="w-8 h-8 rounded bg-white flex items-center justify-center text-sidebar-blue">
                    <GraduationCap className="w-6 h-6" />
                </div>
                <h1 className="text-white text-xl font-bold tracking-tight">UniTrack</h1>
            </div>

            {/* Navigation */}
            <nav className="flex-1 px-3 py-4 flex flex-col gap-1 overflow-y-auto scrollbar-hide">
                {navItems.map((item) => (
                    <NavLink
                        key={item.to}
                        to={item.to}
                        className={({ isActive }) =>
                            cn(
                                'flex items-center gap-3 px-3 py-3 rounded-lg transition-colors cursor-pointer group border-l-4',
                                isActive
                                    ? 'bg-white/10 border-l-primary text-white'
                                    : 'border-l-transparent text-white/80 hover:bg-white/5 hover:text-white'
                            )
                        }
                    >
                        {({ isActive }) => (
                            <>
                                <item.icon className={cn(
                                    'w-5 h-5 transition-transform',
                                    !isActive && 'group-hover:scale-110'
                                )} />
                                <span className="text-sm font-medium">{item.label}</span>
                            </>
                        )}
                    </NavLink>
                ))}

                {/* Divider */}
                <div className="my-4 border-t border-white/10" />

                {/* Settings Link */}
                <a
                    href="#"
                    className="flex items-center gap-3 px-3 py-3 rounded-lg text-white/80 hover:bg-white/5 hover:text-white transition-colors cursor-pointer group border-l-4 border-l-transparent"
                >
                    <Settings className="w-5 h-5 transition-transform group-hover:scale-110" />
                    <span className="text-sm font-medium">System Settings</span>
                </a>
            </nav>

            {/* User Profile Section */}
            <div className="p-4 border-t border-white/10">
                <div className="flex items-center gap-3 p-2 rounded-lg hover:bg-white/5 cursor-pointer transition-colors">
                    <div className="w-10 h-10 rounded-full bg-white/20 flex items-center justify-center text-white font-bold text-sm">
                        {user.initials}
                    </div>
                    <div className="flex flex-col">
                        <p className="text-sm font-medium text-white">{user.name}</p>
                        <p className="text-xs text-white/60">{user.email}</p>
                    </div>
                </div>
            </div>
        </aside>
    );
};
