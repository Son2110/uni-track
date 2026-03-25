import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { graphqlClient } from "@/lib/graphql";
import { apiClient } from "@/lib/api";
import {
  GET_USERS,
  GET_USERS_WITH_SEARCH,
  GET_USERS_WITH_ROLE,
  GET_USERS_WITH_BOTH,
  GET_USER_BY_ID,
  type GetUsersResponse,
  type GetUsersWithFiltersResponse,
  type GetUserByIdResponse,
  type UserFilterParams,
  type CreateUserInput,
  type UpdateUserInput,
} from "./queries";

// Helper function to map string role to numeric for REST API
const roleToNumber = (role: string): number => {
  const upperRole = role.toUpperCase();
  switch (upperRole) {
    case "ADMIN":
      return 0;
    case "TEACHER":
      return 1;
    case "STUDENT":
      return 2;
    default:
      return 2;
  }
};

// Helper function to map numeric role from REST API to string
const numberToRole = (role: number): "ADMIN" | "TEACHER" | "STUDENT" => {
  switch (role) {
    case 0:
      return "ADMIN";
    case 1:
      return "TEACHER";
    case 2:
      return "STUDENT";
    default:
      return "STUDENT";
  }
};

// Query keys
export const userKeys = {
  all: ["users"] as const,
  lists: () => [...userKeys.all, "list"] as const,
  list: (filters: UserFilterParams) => [...userKeys.lists(), filters] as const,
  details: () => [...userKeys.all, "detail"] as const,
  detail: (id: string) => [...userKeys.details(), id] as const,
};

// Fetch all users via GraphQL
export const useUsers = () => {
  return useQuery({
    queryKey: userKeys.all,
    queryFn: async () => {
      const data = await graphqlClient.request<GetUsersResponse>(GET_USERS);
      // GraphQL returns role as ADMIN/TEACHER/STUDENT (UPPERCASE)
      return data.users?.nodes || [];
    },
  });
};

// Fetch users with filters via GraphQL
export const useUsersWithFilters = (filters: UserFilterParams = {}) => {
  return useQuery({
    queryKey: userKeys.list(filters),
    queryFn: async () => {
      const hasSearch = !!filters.search;
      const hasRole = !!filters.role;

      // No filters - use simple query
      if (!hasSearch && !hasRole) {
        const data = await graphqlClient.request<GetUsersResponse>(GET_USERS);
        return data.users?.nodes || [];
      }

      // Only search filter
      if (hasSearch && !hasRole) {
        const data = await graphqlClient.request<GetUsersWithFiltersResponse>(
          GET_USERS_WITH_SEARCH,
          { search: filters.search },
        );
        return data.users.nodes;
      }

      // Only role filter
      if (!hasSearch && hasRole) {
        const data = await graphqlClient.request<GetUsersWithFiltersResponse>(
          GET_USERS_WITH_ROLE,
          { role: filters.role!.toUpperCase() },
        );
        return data.users.nodes;
      }

      // Both search and role filters
      const data = await graphqlClient.request<GetUsersWithFiltersResponse>(
        GET_USERS_WITH_BOTH,
        {
          search: filters.search!,
          role: filters.role!.toUpperCase(),
        },
      );
      return data.users.nodes;
    },
  });
};

// Fetch single user via GraphQL
export const useUser = (userId: string) => {
  return useQuery({
    queryKey: userKeys.detail(userId),
    queryFn: async () => {
      const data = await graphqlClient.request<GetUserByIdResponse>(
        GET_USER_BY_ID,
        { userId },
      );
      // Return the first node from the filtered result
      return data.users.nodes[0] || null;
    },
    enabled: !!userId,
  });
};

// Create user mutation
export const useCreateUser = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateUserInput) => {
      const payload = {
        ...data,
        role: roleToNumber(data.role),
      };
      const response = await apiClient.post<any>("/api/v1/users", payload);
      return {
        ...response.data,
        role: numberToRole(response.data.role),
      };
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userKeys.all });
    },
  });
};

// Update user mutation
export const useUpdateUser = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, data }: { id: string; data: UpdateUserInput }) => {
      const payload = {
        ...data,
        ...(data.role && { role: roleToNumber(data.role) }),
      };
      const response = await apiClient.put<any>(`/api/v1/users/${id}`, payload);
      return {
        ...response.data,
        role: numberToRole(response.data.role),
      };
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userKeys.all });
    },
  });
};

// Delete user mutation
export const useDeleteUser = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.delete(`/api/v1/users/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userKeys.all });
    },
  });
};

// Update password mutation
export const useUpdatePassword = () => {
  return useMutation({
    mutationFn: async ({
      id,
      currentPassword,
      newPassword,
    }: {
      id: string;
      currentPassword: string;
      newPassword: string;
    }) => {
      const response = await apiClient.patch<any>(
        `/api/v1/users/${id}/password`,
        {
          currentPassword,
          newPassword,
        },
      );
      return response.data;
    },
  });
};

// Re-export types for convenience
export type {
  UserFilterParams,
  CreateUserInput,
  UpdateUserInput,
} from "./queries";
