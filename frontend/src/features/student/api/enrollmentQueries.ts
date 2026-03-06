import { gql } from "graphql-request";
import type { ClassEnrollmentDto } from "@/types";

/**
 * GraphQL Queries for Enrollments
 * Replacing REST GET endpoints with GraphQL for better performance
 */

// ============================================
// GET USER ENROLLMENTS
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

// ============================================
// GET CLASS ENROLLMENTS
// ============================================

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

// Helper function to transform GraphQL response to DTO format
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
