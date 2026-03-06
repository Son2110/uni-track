import { gql } from "graphql-request";
import type { User } from "@/types";

// GraphQL Queries
export const GET_USERS = gql`
  query GetUsers {
    users {
      nodes {
        userId
        name
        email
        githubUsername
        githubEmail
        role
        createdAt
        updatedAt
      }
    }
  }
`;

// Response types
export interface GetUsersResponse {
  users: {
    nodes: User[];
  };
}

// REST API types
export interface UserFilterParams {
  pageNumber?: number;
  pageSize?: number;
  search?: string;
  role?: string;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
}

export interface CreateUserInput {
  name: string;
  email: string;
  password: string;
  githubUsername?: string;
  githubEmail?: string;
  role: string;
}

export interface UpdateUserInput {
  name?: string;
  email?: string;
  githubUsername?: string;
  githubEmail?: string;
  role?: string;
}
