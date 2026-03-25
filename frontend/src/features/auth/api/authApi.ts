import { useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api";
import type { LoginDto, AuthResponseDto } from "@/types";

/**
 * Auth API Client
 * Handles authentication requests to the backend
 */

// ============================================
// LOGIN
// ============================================

export const useLogin = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (credentials: LoginDto) => {
      const response = await apiClient.post<AuthResponseDto>(
        "/api/v1/auth/login",
        credentials,
      );
      return response.data;
    },
    onSuccess: (data) => {
      // Store token in localStorage
      localStorage.setItem("authToken", data.token);
      localStorage.setItem(
        "authUser",
        JSON.stringify({
          userId: data.userId,
          name: data.name,
          email: data.email,
          role: data.role,
        }),
      );

      // Invalidate queries to refetch with new auth
      queryClient.invalidateQueries();
    },
  });
};

// ============================================
// LOGOUT
// ============================================

export const useLogout = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      // Clear local storage
      localStorage.removeItem("authToken");
      localStorage.removeItem("authUser");
    },
    onSuccess: () => {
      // Clear all queries
      queryClient.clear();
    },
  });
};

// ============================================
// HELPER FUNCTIONS
// ============================================

/**
 * Get stored auth token
 */
export const getAuthToken = (): string | null => {
  return localStorage.getItem("authToken");
};

/**
 * Get stored user info
 */
export const getAuthUser = () => {
  const userJson = localStorage.getItem("authUser");
  if (!userJson) return null;

  try {
    return JSON.parse(userJson);
  } catch {
    return null;
  }
};

/**
 * Check if user is authenticated
 */
export const isAuthenticated = (): boolean => {
  return !!getAuthToken();
};

/**
 * Check if token is expired (simple check)
 */
export const isTokenExpired = (): boolean => {
  const token = getAuthToken();
  if (!token) return true;

  try {
    // Decode JWT payload (simple base64 decode)
    const parts = token.split(".");
    if (parts.length !== 3) {
      return true; // Invalid format = expired
    }

    // Try to decode the payload
    let payload;
    try {
      // JWT uses base64url encoding, which may need normalization
      const base64 = parts[1].replace(/-/g, "+").replace(/_/g, "/");
      const jsonPayload = atob(base64);
      payload = JSON.parse(jsonPayload);
    } catch {
      return true; // Can't decode = invalid token = expired
    }

    if (!payload.exp) {
      return false; // If no exp field, let backend validate
    }

    const expirationTime = payload.exp * 1000; // Convert to milliseconds
    return Date.now() >= expirationTime;
  } catch {
    return true; // For unexpected errors, treat as expired
  }
};
