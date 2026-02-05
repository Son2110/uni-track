import { create } from 'zustand';

interface AppState {
    // User Information
    user: {
        name: string;
        email: string;
        initials: string;
    };

    // UI State
    sidebarCollapsed: boolean;

    // Actions
    setSidebarCollapsed: (collapsed: boolean) => void;
}

export const useStore = create<AppState>((set) => ({
    // Default user from HTML prototype
    user: {
        name: 'Admin User',
        email: 'admin@unitrack.edu',
        initials: 'AD',
    },

    // UI State
    sidebarCollapsed: false,

    // Actions
    setSidebarCollapsed: (collapsed) => set({ sidebarCollapsed: collapsed }),
}));
