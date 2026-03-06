import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { graphqlClient } from "@/lib/graphql";
import { apiClient } from "@/lib/api";
import { gql } from "graphql-request";
import type { Course } from "@/types";

// GraphQL Query
export const GET_COURSES = gql`
  query GetCourses {
    courses {
      nodes {
        courseId
        code
        name
        description
        createdAt
        updatedAt
      }
    }
  }
`;

export interface GetCoursesResponse {
  courses: {
    nodes: Course[];
  };
}

// Query keys
export const courseKeys = {
  all: ["courses"] as const,
  lists: () => [...courseKeys.all, "list"] as const,
  details: () => [...courseKeys.all, "detail"] as const,
  detail: (id: string) => [...courseKeys.details(), id] as const,
};

// Fetch all courses via GraphQL
export const useCourses = () => {
  return useQuery({
    queryKey: courseKeys.all,
    queryFn: async () => {
      const data = await graphqlClient.request<GetCoursesResponse>(GET_COURSES);
      return data.courses.nodes;
    },
  });
};

// Fetch single course
export const useCourse = (courseId: string) => {
  return useQuery({
    queryKey: courseKeys.detail(courseId),
    queryFn: async () => {
      const response = await apiClient.get<Course>(
        `/api/v1/courses/${courseId}`,
      );
      return response.data;
    },
    enabled: !!courseId,
  });
};

// Create course mutation
export const useCreateCourse = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: {
      code: string;
      name: string;
      description?: string;
    }) => {
      const response = await apiClient.post<Course>("/api/v1/courses", data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: courseKeys.all });
    },
  });
};

// Update course mutation
export const useUpdateCourse = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, data }: { id: string; data: Partial<Course> }) => {
      const response = await apiClient.put<Course>(
        `/api/v1/courses/${id}`,
        data,
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: courseKeys.all });
    },
  });
};

// Delete course mutation
export const useDeleteCourse = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.delete(`/api/v1/courses/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: courseKeys.all });
    },
  });
};
