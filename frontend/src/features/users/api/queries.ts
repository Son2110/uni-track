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

// GraphQL Query with search only
export const GET_USERS_WITH_SEARCH = gql`
  query GetUsersWithSearch($search: String!) {
    users(
      where: {
        or: [
          { name: { contains: $search } }
          { email: { contains: $search } }
          { githubUsername: { contains: $search } }
        ]
      }
    ) {
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

// GraphQL Query with role only
export const GET_USERS_WITH_ROLE = gql`
  query GetUsersWithRole($role: UserRole!) {
    users(where: { role: { eq: $role } }) {
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

// GraphQL Query with both search and role
export const GET_USERS_WITH_BOTH = gql`
  query GetUsersWithBoth($search: String!, $role: UserRole!) {
    users(
      where: {
        or: [
          { name: { contains: $search } }
          { email: { contains: $search } }
          { githubUsername: { contains: $search } }
        ]
        role: { eq: $role }
      }
    ) {
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

// GraphQL Query for single user by ID
export const GET_USER_BY_ID = gql`
  query GetUserById($userId: UUID!) {
    users(where: { userId: { eq: $userId } }) {
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

export interface GetUsersWithFiltersResponse {
  users: {
    nodes: User[];
  };
}

export interface GetUserByIdResponse {
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
