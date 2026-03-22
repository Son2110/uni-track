import { useQuery } from "@tanstack/react-query";
import { gql } from "graphql-request";
import { graphqlClient } from "@/lib/graphql";

// GraphQL queries for dashboard statistics
export const GET_DASHBOARD_STATS = gql`
  query GetDashboardStats {
    users {
      nodes {
        userId
      }
      pageInfo {
        hasNextPage
        hasPreviousPage
      }
    }
    courses {
      nodes {
        courseId
      }
      pageInfo {
        hasNextPage
        hasPreviousPage
      }
    }
    projects {
      nodes {
        projectId
      }
      pageInfo {
        hasNextPage
        hasPreviousPage
      }
    }
    classes {
      nodes {
        classId
      }
      pageInfo {
        hasNextPage
        hasPreviousPage
      }
    }
  }
`;

export interface DashboardStatsResponse {
  users: { nodes: Array<{ userId: string }> };
  courses: { nodes: Array<{ courseId: string }> };
  projects: { nodes: Array<{ projectId: string }> };
  classes: { nodes: Array<{ classId: string }> };
}

export interface DashboardStats {
  totalUsers: number;
  totalCourses: number;
  totalProjects: number;
  totalClasses: number;
}

// Query keys
export const dashboardKeys = {
  all: ["dashboard"] as const,
  stats: () => [...dashboardKeys.all, "stats"] as const,
};

// Fetch dashboard statistics using GraphQL
export const useDashboardStats = () => {
  return useQuery({
    queryKey: dashboardKeys.stats(),
    queryFn: async (): Promise<DashboardStats> => {
      const data =
        await graphqlClient.request<DashboardStatsResponse>(
          GET_DASHBOARD_STATS,
        );

      return {
        totalUsers: data.users.nodes.length,
        totalCourses: data.courses.nodes.length,
        totalProjects: data.projects.nodes.length,
        totalClasses: data.classes.nodes.length,
      };
    },
  });
};
