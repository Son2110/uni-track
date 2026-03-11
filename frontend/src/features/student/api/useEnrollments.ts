import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api";
import { graphqlClient } from "@/lib/graphql";
import { gql } from "graphql-request";
import type { ClassEnrollmentDto } from "@/types";

/**
 * GraphQL Queries and Hooks for Enrollments
 * Replacing REST GET endpoints with GraphQL for better performance
 */

// ============================================
// GraphQL QUERIES
// ============================================

export const GET_USER_ENROLLMENTS = gql`
  query GetUserEnrollments($userId: UUID!) {
    classEnrollments(where: { userId: { eq: $userId } }) {
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
`;

export interface GetUserEnrollmentsResponse {
  classEnrollments: {
    nodes: Array<{
      classId: string;
      userId: string;
      enrolledAt: string;
      class: {
        classId: string;
        classCode: string;
        course: {
          courseId: string;
          code: string;
          name: string;
        };
        semester: {
          semesterId: string;
          name: string;
        };
        teacher: {
          userId: string;
          name: string;
        };
      };
      user: {
        userId: string;
        name: string;
        email: string;
      };
    }>;
  };
}

export const GET_CLASS_ENROLLMENTS = gql`
  query GetClassEnrollments($classId: UUID!) {
    classEnrollments(where: { classId: { eq: $classId } }) {
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
`;

export interface GetClassEnrollmentsResponse {
  classEnrollments: {
    nodes: Array<{
      classId: string;
      userId: string;
      enrolledAt: string;
      class: {
        classId: string;
        classCode: string;
        course: {
          courseId: string;
          code: string;
          name: string;
        };
        semester: {
          semesterId: string;
          name: string;
        };
        teacher: {
          userId: string;
          name: string;
        };
      };
      user: {
        userId: string;
        name: string;
        email: string;
      };
    }>;
  };
}

// GraphQL Query for enrollment count
export const GET_ENROLLMENT_COUNT = gql`
  query GetEnrollmentCount($classId: UUID!) {
    classEnrollments(where: { classId: { eq: $classId } }) {
      totalCount
    }
  }
`;

export interface GetEnrollmentCountResponse {
  classEnrollments: {
    totalCount: number;
  };
}

// GraphQL Query to get minimal enrollment data for counting per class
export const GET_ALL_ENROLLMENT_IDS = gql`
  query GetAllEnrollmentIds {
    classEnrollments {
      nodes {
        classId
        userId
      }
    }
  }
`;

export interface GetAllEnrollmentIdsResponse {
  classEnrollments: {
    nodes: Array<{
      classId: string;
      userId: string;
    }>;
  };
}

// GraphQL Query for specific enrollment
export const GET_ENROLLMENT = gql`
  query GetEnrollment($classId: UUID!, $userId: UUID!) {
    classEnrollments(
      where: {
        and: [{ classId: { eq: $classId } }, { userId: { eq: $userId } }]
      }
    ) {
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
`;

export interface GetEnrollmentResponse {
  classEnrollments: {
    nodes: Array<{
      classId: string;
      userId: string;
      enrolledAt: string;
      class: {
        classId: string;
        classCode: string;
        course: {
          courseId: string;
          code: string;
          name: string;
        };
        semester: {
          semesterId: string;
          name: string;
        };
        teacher: {
          userId: string;
          name: string;
        };
      };
      user: {
        userId: string;
        name: string;
        email: string;
      };
    }>;
  };
}

// ============================================
// TRANSFORMERS
// ============================================

/**
 * Helper function to transform GraphQL response to DTO format
 */
export const transformToEnrollmentDto = (
  node: GetUserEnrollmentsResponse["classEnrollments"]["nodes"][0],
): ClassEnrollmentDto => {
  return {
    classId: node.classId,
    className: `${node.class.course.code} - ${node.class.classCode}`,
    courseCode: node.class.course.code,
    courseName: node.class.course.name,
    classCode: node.class.classCode,
    semesterName: node.class.semester.name,
    teacherName: node.class.teacher.name,
    userId: node.userId,
    studentName: node.user.name,
    studentEmail: node.user.email,
    courseId: node.class.course.courseId,
    enrolledAt: node.enrolledAt,
  };
};

// ============================================
// HOOKS - GET ALL ENROLLMENTS (with filtering)
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
// GET ENROLLMENT COUNT BY CLASS ID - GraphQL
// ============================================

export const useEnrollmentCount = (classId: string) => {
  return useQuery({
    queryKey: ["enrollments", "class", classId, "count"],
    queryFn: async () => {
      const data = await graphqlClient.request<GetEnrollmentCountResponse>(
        GET_ENROLLMENT_COUNT,
        { classId },
      );
      return data.classEnrollments.totalCount;
    },
    enabled: !!classId,
  });
};

// ============================================
// GET SPECIFIC ENROLLMENT - GraphQL
// ============================================

export const useEnrollment = (classId: string, userId: string) => {
  return useQuery({
    queryKey: ["enrollments", classId, userId],
    queryFn: async () => {
      const data = await graphqlClient.request<GetEnrollmentResponse>(
        GET_ENROLLMENT,
        { classId, userId },
      );
      const node = data.classEnrollments.nodes[0];
      return node ? transformToEnrollmentDto(node) : null;
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

// ============================================
// GET ENROLLMENT COUNTS (Optimized for ClassPage)
// ============================================

/**
 * Hook: useEnrollmentCounts
 * Purpose: Fetch minimal enrollment data and compute count per class
 * Used in: ClassPage (admin) - to display student counts efficiently
 * Returns: Map<classId, count>
 */
export const useEnrollmentCounts = () => {
  return useQuery({
    queryKey: ["enrollments", "counts"],
    queryFn: async () => {
      const data = await graphqlClient.request<GetAllEnrollmentIdsResponse>(
        GET_ALL_ENROLLMENT_IDS,
      );

      // Create a map of classId -> student count
      const countMap = new Map<string, number>();
      data.classEnrollments.nodes.forEach((enrollment) => {
        const count = countMap.get(enrollment.classId) || 0;
        countMap.set(enrollment.classId, count + 1);
      });

      return countMap;
    },
  });
};
