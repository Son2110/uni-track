import { gql } from "graphql-request";
import type { Semester } from "@/types";

//dinh nghia cau query - GraphQL dùng Connection pattern
export const GET_SEMESTERS = gql`
  query GetSemesters {
    semesters {
      nodes {
        semesterId
        name
        startDate
        endDate
      }
    }
  }
`;

export const CREATE_SEMESTER = gql`
  mutation CreateSemester($input: CreateSemesterInput!) {
    createSemester(input: $input) {
      semesterId
      name
      startDate
      endDate
      createdAt
      updatedAt
    }
  }
`;

//dinh nghia response vi graphql thuong tra ve Ojbect {data: { semesters : [...]}}
export interface GetSemestersResponse {
  semesters: {
    nodes: Semester[];
  };
}

export interface CreateSemesterInput {
  name: string;
  startDate: string;
  endDate: string;
}

export interface CreateSemesterResponse {
  createSemester: Semester;
}
