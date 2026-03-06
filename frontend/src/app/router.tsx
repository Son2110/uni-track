import { createBrowserRouter, Navigate } from "react-router-dom";
import { AdminLayout } from "@/layouts/AdminLayout";
import { StudentLayout } from "@/layouts/StudentLayout";
import { TeacherLayout } from "@/layouts/TeacherLayout";
import { AuthLayout } from "@/layouts/AuthLayout";
import { ProtectedRoute } from "@/components/auth/ProtectedRoute";

// Admin Pages
import { DashboardPage } from "@/features/dashboard/pages/DashboardPage";
import { SemesterPage } from "@/features/academic/pages/SemesterPage";
import { CoursePage } from "@/features/academic/pages/CoursePage";
import { CourseDetailPage } from "@/features/academic/pages/CourseDetailPage";
import { ClassPage } from "@/features/academic/pages/ClassPage";
import { ClassDetailPage } from "@/features/academic/pages/ClassDetailPage";
import { UserPage } from "@/features/users/pages/UserPage";

// Teacher Pages
import { TeacherSemestersPage } from "@/features/teacher/pages/TeacherSemestersPage";
import { TeacherCoursesPage } from "@/features/teacher/pages/TeacherCoursesPage";
import { TeacherClassesPage } from "@/features/teacher/pages/TeacherClassesPage";
import { TeacherProjectsPage } from "@/features/teacher/pages/TeacherProjectsPage";
import { TeacherProjectDetailPage } from "@/features/teacher/pages/TeacherProjectDetailPage";
import { TeacherSettingsPage } from "@/features/teacher/pages/TeacherSettingsPage";

// Student Pages
import { StudentClassesPage } from "@/features/student/pages/StudentClassesPage";
import { StudentEnrollPage } from "@/features/student/pages/StudentEnrollPage";
import { StudentWorkspaceListPage } from "@/features/student/pages/StudentWorkspaceListPage";
import { StudentWorkspacePage } from "@/features/student/pages/StudentWorkspacePage";
import { StudentClassProjectsPage } from "@/features/student/pages/StudentClassProjectsPage";
import { StudentSettingsPage } from "@/features/student/pages/StudentSettingsPage";

// Auth Pages
import { LoginPage } from "@/features/auth/pages/LoginPage";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <AuthLayout />,
    children: [
      {
        index: true,
        element: <Navigate to="/login" replace />,
      },
      {
        path: "login",
        element: <LoginPage />,
      },
    ],
  },
  {
    path: "/admin",
    element: (
      <ProtectedRoute allowedRoles={["ADMIN", "Admin"]}>
        <AdminLayout />
      </ProtectedRoute>
    ),
    children: [
      {
        index: true,
        element: <Navigate to="/admin/dashboard" replace />,
      },
      {
        path: "dashboard",
        element: <DashboardPage />,
      },
      {
        path: "semesters",
        element: <SemesterPage />,
      },
      {
        path: "courses",
        element: <CoursePage />,
      },
      {
        path: "courses/:courseId",
        element: <CourseDetailPage />,
      },
      {
        path: "classes",
        element: <ClassPage />,
      },
      {
        path: "classes/:classId",
        element: <ClassDetailPage />,
      },
      {
        path: "users",
        element: <UserPage />,
      },
    ],
  },
  {
    path: "/teacher",
    element: (
      <ProtectedRoute allowedRoles={["ADMIN", "Admin", "TEACHER", "Teacher"]}>
        <TeacherLayout />
      </ProtectedRoute>
    ),
    children: [
      {
        index: true,
        element: <Navigate to="/teacher/classes" replace />,
      },
      {
        path: "semesters",
        element: <TeacherSemestersPage />,
      },
      {
        path: "courses",
        element: <TeacherCoursesPage />,
      },
      {
        path: "classes",
        element: <TeacherClassesPage />,
      },
      {
        path: "projects",
        element: <TeacherProjectsPage />,
      },
      {
        path: "projects/:projectId",
        element: <TeacherProjectDetailPage />,
      },
      {
        path: "settings",
        element: <TeacherSettingsPage />,
      },
    ],
  },
  {
    path: "/student",
    element: (
      <ProtectedRoute allowedRoles={["STUDENT", "Student"]}>
        <StudentLayout />
      </ProtectedRoute>
    ),
    children: [
      {
        index: true,
        element: <Navigate to="/student/classes" replace />,
      },
      {
        path: "classes",
        element: <StudentClassesPage />,
      },
      {
        path: "enroll",
        element: <StudentEnrollPage />,
      },
      {
        path: "classes/:classId/projects",
        element: <StudentClassProjectsPage />,
      },
      {
        path: "workspace",
        element: <StudentWorkspaceListPage />,
      },
      {
        path: "workspace/:projectId",
        element: <StudentWorkspacePage />,
      },
      {
        path: "settings",
        element: <StudentSettingsPage />,
      },
    ],
  },
]);
