import { Navigate } from "react-router-dom";
import { useAuth } from "@/features/auth/context/AuthContext";
import type { UserRole } from "@/types";

/**
 * Protected Route Component
 * Wraps routes that require authentication or specific roles
 *
 * @example
 * // Require authentication
 * <ProtectedRoute>
 *   <DashboardPage />
 * </ProtectedRoute>
 *
 * @example
 * // Require specific role
 * <ProtectedRoute allowedRoles="Admin">
 *   <AdminPage />
 * </ProtectedRoute>
 *
 * @example
 * // Require one of multiple roles
 * <ProtectedRoute allowedRoles={["Admin", "Teacher"]}>
 *   <TeacherPage />
 * </ProtectedRoute>
 */

interface ProtectedRouteProps {
  children: React.ReactNode;
  allowedRoles?: UserRole | UserRole[];
}

export function ProtectedRoute({
  children,
  allowedRoles,
}: ProtectedRouteProps) {
  const { isAuthenticated, hasRole, isLoading } = useAuth();

  // Show loading state while checking auth
  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="text-lg">Loading...</div>
      </div>
    );
  }

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Check role-based access if allowedRoles specified
  if (allowedRoles && !hasRole(allowedRoles)) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-red-600">Access Denied</h1>
          <p className="mt-2 text-gray-600">
            You don't have permission to access this page.
          </p>
        </div>
      </div>
    );
  }

  return <>{children}</>;
}
