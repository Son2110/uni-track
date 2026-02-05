// Database Schema Types - Matched to ERD
// PascalCase (DB) → camelCase (Frontend)

// ============================================
// ENUMS
// ============================================

export type UserRole = 'Admin' | 'Teacher' | 'Student';

// ============================================
// CORE ENTITIES
// ============================================

export interface User {
    userId: number;
    name: string;
    studentOrEmployeeId: string;
    email: string;
    gitHubUsername: string | null;
    gitHubEmail: string | null;
    role: UserRole;
    avatar?: string; // Optional, for UI
    createdAt: string;
    updatedAt: string;
}

export interface Semester {
    semesterId: number;
    name: string;
    startDate: string;
    endDate: string;
    createdAt: string;
    updatedAt: string;
}

export interface Course {
    courseId: number;
    code: string;
    name: string;
    description: string;
    createdAt: string;
    updatedAt: string;
}

export interface Class {
    classId: number;
    semesterId: number;
    courseId: number;
    classCode: string;
    teacherId: number;
    createdAt: string;
    updatedAt: string;
}

export interface Project {
    projectId: number;
    classId: number;
    name: string;
    description: string;
    createdAt: string;
    updatedAt: string;
}

export interface ClassEnrollment {
    classId: number;
    userId: number;
    enrolledAt: string;
}

// ============================================
// SUPPORTING TYPES
// ============================================

export interface Campus {
    id: string;
    name: string;
    code: string;
}

export interface LoginCredentials {
    campus: string;
    rememberMe?: boolean;
}

export interface DashboardStats {
    totalUsers: number;
    totalUsersTrend: number;
    activeCourses: number;
    activeCoursesTrend: number;
    ongoingProjects: number;
    ongoingProjectsTrend: number;
}
