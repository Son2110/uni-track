import { useQuery } from "@tanstack/react-query";
import { gql } from "graphql-request";

// GraphQL queries for dashboard statistics
export const GET_DASHBOARD_STATS = gql`
  query GetDashboardStats {
    users {
      totalCount
    }
    courses {
      totalCount
    }
    projects {
      totalCount
    }
    classes {
      totalCount
    }
  }
`;

export interface DashboardStatsResponse {
  users: { totalCount: number };
  courses: { totalCount: number };
  projects: { totalCount: number };
  classes: { totalCount: number };
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

// Fetch dashboard statistics
// Backend chưa có endpoint - feature disabled
export const useDashboardStats = () => {
  return useQuery({
    queryKey: dashboardKeys.stats(),
    queryFn: async (): Promise<DashboardStats> => {
      throw new Error("Dashboard stats endpoint not implemented");
    },
    enabled: false,
  });
};
