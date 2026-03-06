import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { graphqlClient } from "@/lib/graphql";
import { apiClient } from "@/lib/api";
import {
  GET_USERS,
  type GetUsersResponse,
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

// Fetch users with filters via REST API
export const useUsersWithFilters = (filters: UserFilterParams = {}) => {
  return useQuery({
    queryKey: userKeys.list(filters),
    queryFn: async () => {
      const response = await apiClient.get<any[]>("/api/v1/users", {
        pageNumber: filters.pageNumber,
        pageSize: filters.pageSize,
        search: filters.search,
        role: filters.role,
        sortBy: filters.sortBy,
        sortOrder: filters.sortOrder,
      });
      return (response.data || []).map((user: any) => ({
        ...user,
        role: numberToRole(user.role), // Convert number to uppercase string
      }));
    },
  });
};

// Fetch single user
export const useUser = (userId: string) => {
  return useQuery({
    queryKey: userKeys.detail(userId),
    queryFn: async () => {
      const response = await apiClient.get<any>(`/api/v1/users/${userId}`);
      return {
        ...response.data,
        role: numberToRole(response.data.role),
      };
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
