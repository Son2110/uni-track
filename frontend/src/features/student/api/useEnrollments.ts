import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api";
import { graphqlClient } from "@/lib/graphql";
import { gql } from "graphql-request";
import type { ClassEnrollmentDto } from "@/types";
import {
  GET_USER_ENROLLMENTS,
  GET_CLASS_ENROLLMENTS,
  transformToEnrollmentDto,
  type GetUserEnrollmentsResponse,
  type GetClassEnrollmentsResponse,
} from "./enrollmentQueries";

// ============================================
// GET ALL ENROLLMENTS (with filtering) - GraphQL
// ============================================

interface EnrollmentFilterParams {
  userId?: string;
  classId?: string;
  courseId?: string;
  semesterId?: string;
}

export const useEnrollments = (filterParams?: EnrollmentFilterParams) => {
  return useQuery({
    queryKey: ["enrollments", filterParams],
    queryFn: async () => {
      // Use specific query based on filters to avoid null filter errors
      if (filterParams?.userId) {
        const data = await graphqlClient.request<GetUserEnrollmentsResponse>(
          GET_USER_ENROLLMENTS,
          { userId: filterParams.userId },
        );
        return data.classEnrollments.nodes.map(transformToEnrollmentDto);
      } else if (filterParams?.classId) {
        const data = await graphqlClient.request<GetClassEnrollmentsResponse>(
          GET_CLASS_ENROLLMENTS,
          { classId: filterParams.classId },
        );
        return data.classEnrollments.nodes.map(transformToEnrollmentDto);
      } else {
        // Get all enrollments without filters
        const data = await graphqlClient.request<GetUserEnrollmentsResponse>(
          gql`
            query GetAllEnrollments {
              classEnrollments {
                nodes {
                  classId
                  userId
                  enrolledAt
                  class {
                    classId
                    classCode
                    course {
                      courseId
                      code
                      name
                    }
                    semester {
                      semesterId
                      name
                    }
                    teacher {
                      userId
                      name
                    }
                  }
                  user {
                    userId
                    name
                    email
                  }
                }
              }
            }
          `,
        );
        return data.classEnrollments.nodes.map(transformToEnrollmentDto);
      }
    },
  });
};

// ============================================
// GET ENROLLMENTS BY USER ID
// ============================================

export const useUserEnrollments = (userId: string) => {
  return useQuery({
    queryKey: ["enrollments", "user", userId],
    queryFn: async () => {
      const data = await graphqlClient.request<GetUserEnrollmentsResponse>(
        GET_USER_ENROLLMENTS,
        { userId },
      );
      return data.classEnrollments.nodes.map(transformToEnrollmentDto);
    },
    enabled: !!userId,
  });
};

// ============================================
// GET ENROLLMENTS BY CLASS ID
// ============================================

export const useClassEnrollments = (classId: string) => {
  return useQuery({
    queryKey: ["enrollments", "class", classId],
    queryFn: async () => {
      const data = await graphqlClient.request<GetClassEnrollmentsResponse>(
        GET_CLASS_ENROLLMENTS,
        { classId },
      );
      return data.classEnrollments.nodes.map(transformToEnrollmentDto);
    },
    enabled: !!classId,
  });
};

// ============================================
// GET ENROLLMENT COUNT BY CLASS ID
// ============================================

export const useEnrollmentCount = (classId: string) => {
  return useQuery({
    queryKey: ["enrollments", "class", classId, "count"],
    queryFn: async () => {
      const response = await apiClient.get<number>(
        `/api/v1/classes/${classId}/enrollments/count`,
      );
      return response.data;
    },
    enabled: !!classId,
  });
};

// ============================================
// GET SPECIFIC ENROLLMENT
// ============================================

export const useEnrollment = (classId: string, userId: string) => {
  return useQuery({
    queryKey: ["enrollments", classId, userId],
    queryFn: async () => {
      const response = await apiClient.get<ClassEnrollmentDto>(
        `/api/v1/classes/${classId}/enrollments/${userId}`,
      );
      return response.data;
    },
    enabled: !!classId && !!userId,
  });
};

// ============================================
// CREATE ENROLLMENT (Student enrolls in class)
// ============================================

interface CreateEnrollmentDto {
  classId: string;
  userId: string;
}

export const useCreateEnrollment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateEnrollmentDto) => {
      const response = await apiClient.post<ClassEnrollmentDto>(
        `/api/v1/classes/${data.classId}/enrollments`,
        { userId: data.userId },
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["enrollments"] });
      queryClient.invalidateQueries({ queryKey: ["classes"] });
    },
  });
};

// ============================================
// BULK ENROLLMENT
// ============================================

interface BulkEnrollmentDto {
  classId: string;
  userIds: string[];
}

export const useBulkEnrollment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: BulkEnrollmentDto) => {
      const response = await apiClient.post<any>(
        `/api/v1/classes/${data.classId}/enrollments/bulk`,
        { userIds: data.userIds },
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["enrollments"] });
      queryClient.invalidateQueries({ queryKey: ["classes"] });
    },
  });
};

// ============================================
// DELETE ENROLLMENT (Unenroll from class)
// ============================================

interface UnenrollData {
  classId: string;
  userId: string;
}

export const useUnenrollment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ classId, userId }: UnenrollData) => {
      await apiClient.delete(
        `/api/v1/classes/${classId}/enrollments/${userId}`,
      );
      return { classId, userId };
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["enrollments"] });
      queryClient.invalidateQueries({ queryKey: ["classes"] });
    },
  });
};
