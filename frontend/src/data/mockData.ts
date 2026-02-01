// Mock Data - Strictly Matched to types/index.ts
import type { User, Semester, Course, Class, Project, ClassEnrollment } from '@/types';

// ============================================
// USERS
// ============================================

export const mockUsers: User[] = [
    // Admin
    {
        userId: 1,
        name: 'Nguyen Van An',
        studentOrEmployeeId: 'EMP001',
        email: 'annv@fpt.edu.vn',
        gitHubUsername: 'annv-fpt',
        gitHubEmail: 'annv@fpt.edu.vn',
        role: 'Admin',
        avatar: 'https://ui-avatars.com/api/?name=Nguyen+Van+An&background=1E5BB8&color=fff',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
    },

    // Teachers
    {
        userId: 2,
        name: 'Tran Thi Binh',
        studentOrEmployeeId: 'EMP002',
        email: 'binhtt@fpt.edu.vn',
        gitHubUsername: 'binhtt-fpt',
        gitHubEmail: 'binhtt@fpt.edu.vn',
        role: 'Teacher',
        avatar: 'https://ui-avatars.com/api/?name=Tran+Thi+Binh&background=1E5BB8&color=fff',
        createdAt: '2024-01-05T00:00:00Z',
        updatedAt: '2024-01-05T00:00:00Z',
    },
    {
        userId: 3,
        name: 'Le Van Cuong',
        studentOrEmployeeId: 'EMP003',
        email: 'cuonglv@fpt.edu.vn',
        gitHubUsername: 'cuonglv-fpt',
        gitHubEmail: 'cuonglv@fpt.edu.vn',
        role: 'Teacher',
        avatar: 'https://ui-avatars.com/api/?name=Le+Van+Cuong&background=1E5BB8&color=fff',
        createdAt: '2024-01-05T00:00:00Z',
        updatedAt: '2024-01-05T00:00:00Z',
    },

    // Students
    {
        userId: 4,
        name: 'Nguyen Minh Khang',
        studentOrEmployeeId: 'SE171234',
        email: 'khangnm@student.fpt.edu.vn',
        gitHubUsername: 'khangnm-se171234',
        gitHubEmail: 'khangnm@student.fpt.edu.vn',
        role: 'Student',
        avatar: 'https://ui-avatars.com/api/?name=Nguyen+Minh+Khang&background=4CAF50&color=fff',
        createdAt: '2024-01-10T00:00:00Z',
        updatedAt: '2024-01-10T00:00:00Z',
    },
    {
        userId: 5,
        name: 'Tran Hoang Long',
        studentOrEmployeeId: 'SE171235',
        email: 'longth@student.fpt.edu.vn',
        gitHubUsername: 'longth-se171235',
        gitHubEmail: 'longth@student.fpt.edu.vn',
        role: 'Student',
        avatar: 'https://ui-avatars.com/api/?name=Tran+Hoang+Long&background=4CAF50&color=fff',
        createdAt: '2024-01-10T00:00:00Z',
        updatedAt: '2024-01-10T00:00:00Z',
    },
    {
        userId: 6,
        name: 'Le Thi Mai',
        studentOrEmployeeId: 'SE171236',
        email: 'mailt@student.fpt.edu.vn',
        gitHubUsername: 'mailt-se171236',
        gitHubEmail: 'mailt@student.fpt.edu.vn',
        role: 'Student',
        avatar: 'https://ui-avatars.com/api/?name=Le+Thi+Mai&background=4CAF50&color=fff',
        createdAt: '2024-01-10T00:00:00Z',
        updatedAt: '2024-01-10T00:00:00Z',
    },
];

// ============================================
// SEMESTERS
// ============================================

export const mockSemesters: Semester[] = [
    {
        semesterId: 1,
        name: 'Spring 2024',
        startDate: '2024-01-15T00:00:00Z',
        endDate: '2024-05-10T00:00:00Z',
        createdAt: '2023-12-01T00:00:00Z',
        updatedAt: '2023-12-01T00:00:00Z',
    },
    {
        semesterId: 2,
        name: 'Summer 2024',
        startDate: '2024-06-01T00:00:00Z',
        endDate: '2024-08-20T00:00:00Z',
        createdAt: '2024-05-01T00:00:00Z',
        updatedAt: '2024-05-01T00:00:00Z',
    },
    {
        semesterId: 3,
        name: 'Fall 2024',
        startDate: '2024-09-01T00:00:00Z',
        endDate: '2024-12-20T00:00:00Z',
        createdAt: '2024-08-01T00:00:00Z',
        updatedAt: '2024-08-01T00:00:00Z',
    },
];

// ============================================
// COURSES
// ============================================

export const mockCourses: Course[] = [
    {
        courseId: 1,
        code: 'SWP391',
        name: 'Software Development Project',
        description: 'Team-based software development project focusing on full-stack application development.',
        createdAt: '2023-12-01T00:00:00Z',
        updatedAt: '2023-12-01T00:00:00Z',
    },
    {
        courseId: 2,
        code: 'PRN231',
        name: 'Building Cross-Platform Application with .NET',
        description: 'Learn to build cross-platform applications using .NET Core and ASP.NET Core.',
        createdAt: '2023-12-01T00:00:00Z',
        updatedAt: '2023-12-01T00:00:00Z',
    },
    {
        courseId: 3,
        code: 'SWR302',
        name: 'Software Requirement Engineering',
        description: 'Techniques for gathering, analyzing, and managing software requirements.',
        createdAt: '2023-12-01T00:00:00Z',
        updatedAt: '2023-12-01T00:00:00Z',
    },
    {
        courseId: 4,
        code: 'SWT301',
        name: 'Software Testing',
        description: 'Principles of software testing including unit testing and integration testing.',
        createdAt: '2023-12-01T00:00:00Z',
        updatedAt: '2023-12-01T00:00:00Z',
    },
];

// ============================================
// CLASSES
// ============================================

export const mockClasses: Class[] = [
    {
        classId: 1,
        semesterId: 3,
        courseId: 1,
        classCode: 'SE1801',
        teacherId: 2,
        createdAt: '2024-08-15T00:00:00Z',
        updatedAt: '2024-08-15T00:00:00Z',
    },
    {
        classId: 2,
        semesterId: 3,
        courseId: 1,
        classCode: 'SE1802',
        teacherId: 3,
        createdAt: '2024-08-15T00:00:00Z',
        updatedAt: '2024-08-15T00:00:00Z',
    },
    {
        classId: 3,
        semesterId: 3,
        courseId: 2,
        classCode: 'SE1803',
        teacherId: 2,
        createdAt: '2024-08-15T00:00:00Z',
        updatedAt: '2024-08-15T00:00:00Z',
    },
    {
        classId: 4,
        semesterId: 3,
        courseId: 3,
        classCode: 'SE1804',
        teacherId: 3,
        createdAt: '2024-08-15T00:00:00Z',
        updatedAt: '2024-08-15T00:00:00Z',
    },
];

// ============================================
// PROJECTS
// ============================================

export const mockProjects: Project[] = [
    {
        projectId: 1,
        classId: 1,
        name: 'UniTrack - Academic Project Management System',
        description: 'A comprehensive platform for managing student projects, tracking progress, and integrating with GitHub and Jira.',
        createdAt: '2024-09-01T00:00:00Z',
        updatedAt: '2024-09-01T00:00:00Z',
    },
    {
        projectId: 2,
        classId: 1,
        name: 'E-Commerce Platform',
        description: 'Full-stack e-commerce solution with payment integration and inventory management.',
        createdAt: '2024-09-01T00:00:00Z',
        updatedAt: '2024-09-01T00:00:00Z',
    },
    {
        projectId: 3,
        classId: 2,
        name: 'Healthcare Appointment System',
        description: 'Online booking system for medical appointments with doctor scheduling.',
        createdAt: '2024-09-01T00:00:00Z',
        updatedAt: '2024-09-01T00:00:00Z',
    },
];

// ============================================
// CLASS ENROLLMENTS
// ============================================

export const mockClassEnrollments: ClassEnrollment[] = [
    { classId: 1, userId: 4, enrolledAt: '2024-08-20T00:00:00Z' },
    { classId: 1, userId: 5, enrolledAt: '2024-08-20T00:00:00Z' },
    { classId: 2, userId: 6, enrolledAt: '2024-08-20T00:00:00Z' },
    { classId: 3, userId: 4, enrolledAt: '2024-08-20T00:00:00Z' },
];

// ============================================
// HELPER FUNCTIONS
// ============================================

export const getUserById = (userId: number) => mockUsers.find(u => u.userId === userId);
export const getSemesterById = (semesterId: number) => mockSemesters.find(s => s.semesterId === semesterId);
export const getCourseById = (courseId: number) => mockCourses.find(c => c.courseId === courseId);
export const getClassById = (classId: number) => mockClasses.find(c => c.classId === classId);
