import { createBrowserRouter, Navigate } from 'react-router-dom';
import { AdminLayout } from '@/layouts/AdminLayout';
import { AuthLayout } from '@/layouts/AuthLayout';

// Admin Pages
import { DashboardPage } from '@/features/dashboard/pages/DashboardPage';
import { SemesterPage } from '@/features/academic/pages/SemesterPage';
import { CoursePage } from '@/features/academic/pages/CoursePage';
import { ClassPage } from '@/features/academic/pages/ClassPage';
import { UserPage } from '@/features/users/pages/UserPage';

// Auth Pages
import { LoginPage } from '@/features/auth/pages/LoginPage';

export const router = createBrowserRouter([
    {
        path: '/',
        element: <AuthLayout />,
        children: [
            {
                index: true,
                element: <Navigate to="/login" replace />,
            },
            {
                path: 'login',
                element: <LoginPage />,
            },
        ],
    },
    {
        path: '/admin',
        element: <AdminLayout />,
        children: [
            {
                index: true,
                element: <Navigate to="/admin/dashboard" replace />,
            },
            {
                path: 'dashboard',
                element: <DashboardPage />,
            },
            {
                path: 'semesters',
                element: <SemesterPage />,
            },
            {
                path: 'courses',
                element: <CoursePage />,
            },
            {
                path: 'classes',
                element: <ClassPage />,
            },
            {
                path: 'users',
                element: <UserPage />,
            },
        ],
    },
]);
