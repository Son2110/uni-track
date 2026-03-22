import {
  createContext,
  useContext,
  useState,
  useEffect,
  type ReactNode,
} from "react";
import { useQueryClient } from "@tanstack/react-query";
import type { AuthUser, UserRole } from "@/types";
import { getAuthToken, getAuthUser, isTokenExpired } from "../api/authApi";

/**
 * Auth Context
 * Provides authentication state and methods throughout the app
 */

interface AuthContextType {
  user: AuthUser | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (user: AuthUser, token: string) => void;
  logout: () => void;
  hasRole: (roles: UserRole | UserRole[]) => boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// ============================================
// AUTH PROVIDER
// ============================================

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const queryClient = useQueryClient();

  // Initialize auth state from localStorage
  useEffect(() => {
    const initAuth = () => {
      const storedToken = getAuthToken();
      const storedUser = getAuthUser();

      // Check if token exists and is not expired
      if (storedToken && storedUser && !isTokenExpired()) {
        setToken(storedToken);
        setUser(storedUser);
      } else {
        // Clear invalid auth data
        localStorage.removeItem("authToken");
        localStorage.removeItem("authUser");
      }

      setIsLoading(false);
    };

    initAuth();
  }, []);

  const login = (authUser: AuthUser, authToken: string) => {
    setUser(authUser);
    setToken(authToken);
    localStorage.setItem("authToken", authToken);
    localStorage.setItem("authUser", JSON.stringify(authUser));
  };

  const logout = () => {
    setUser(null);
    setToken(null);
    localStorage.removeItem("authToken");

    // Clear React Query cache
    queryClient.clear();

    localStorage.removeItem("authUser");
    // Use window.location for redirect to avoid needing React Router context
    window.location.href = "/login";
  };

  const hasRole = (roles: UserRole | UserRole[]): boolean => {
    if (!user) return false;

    const roleArray = Array.isArray(roles) ? roles : [roles];
    return roleArray.includes(user.role);
  };

  const value: AuthContextType = {
    user,
    token,
    isAuthenticated: !!user && !!token,
    isLoading,
    login,
    logout,
    hasRole,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// ============================================
// USE AUTH HOOK
// ============================================

export function useAuth() {
  const context = useContext(AuthContext);

  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }

  return context;
}
