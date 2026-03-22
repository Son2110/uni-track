import { gql } from "graphql-request";
import type { ClassDto } from "@/types";

/**
 * GraphQL Queries for Classes
 */

// ============================================
// GET ALL CLASSES
// ============================================

export const GET_ALL_CLASSES = gql`
  query GetAllClasses {
    classes {
      nodes {
        classId
        classCode
        semesterId
        courseId
        teacherId
        createdAt
        updatedAt
        semester {
          semesterId
          name
        }
        course {
          courseId
          code
          name
        }
        teacher {
          userId
          name
        }
      }
    }
  }
`;

export interface GetAllClassesResponse {
  classes: {
    nodes: Array<{
      classId: string;
      classCode: string;
      semesterId: string;
      courseId: string;
      teacherId: string;
      createdAt: string;
      updatedAt: string;
      semester: {
        semesterId: string;
        name: string;
      };
      course: {
        courseId: string;
        code: string;
        name: string;
      };
      teacher: {
        userId: string;
        name: string;
      };
    }>;
  };
}

/**
 * Transform GraphQL response to ClassDto format
 */
export const transformToClassDto = (
  node: GetAllClassesResponse["classes"]["nodes"][0],
): ClassDto => {
  return {
    classId: node.classId,
    semesterId: node.semesterId,
    semesterName: node.semester.name,
    courseId: node.courseId,
    courseCode: node.course.code,
    courseName: node.course.name,
    classCode: node.classCode,
    teacherId: node.teacherId,
    teacherName: node.teacher.name,
    createdAt: node.createdAt,
    updatedAt: node.updatedAt,
  };
};
