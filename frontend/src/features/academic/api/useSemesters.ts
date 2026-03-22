import { graphqlClient } from "@/lib/graphql";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api";
import { gql } from "graphql-request";
import type { Semester } from "@/types";

// GraphQL Queries
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

// GraphQL Response Types
export interface GetSemestersResponse {
  semesters: {
    nodes: Semester[];
  };
}

// GraphQL Query for single semester by ID
export const GET_SEMESTER_BY_ID = gql`
  query GetSemesterById($semesterId: UUID!) {
    semesters(where: { semesterId: { eq: $semesterId } }) {
      nodes {
        semesterId
        name
        startDate
        endDate
      }
    }
  }
`;

export interface GetSemesterByIdResponse {
  semesters: {
    nodes: Semester[];
  };
}

// Types for mutations
export interface CreateSemesterInput {
  name: string;
  startDate: string;
  endDate: string;
}

export interface UpdateSemesterInput {
  name: string;
  startDate: string;
  endDate: string;
}

// Query keys
export const semesterKeys = {
  all: ["semesters"] as const,
  lists: () => [...semesterKeys.all, "list"] as const,
  details: () => [...semesterKeys.all, "detail"] as const,
  detail: (id: string) => [...semesterKeys.details(), id] as const,
};

// Fetch all semesters via GraphQL
export const useSemesters = () => {
  return useQuery({
    queryKey: semesterKeys.all,
    queryFn: async () => {
      const data =
        await graphqlClient.request<GetSemestersResponse>(GET_SEMESTERS);
      return data.semesters.nodes;
    },
  });
};

// Fetch single semester via GraphQL
export const useSemester = (semesterId: string) => {
  return useQuery({
    queryKey: semesterKeys.detail(semesterId),
    queryFn: async () => {
      const data = await graphqlClient.request<GetSemesterByIdResponse>(
        GET_SEMESTER_BY_ID,
        { semesterId },
      );
      return data.semesters.nodes[0] || null;
    },
    enabled: !!semesterId,
  });
};

// Create semester mutation
export const useCreateSemester = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateSemesterInput) => {
      const response = await apiClient.post<Semester>(
        "/api/v1/semesters",
        data,
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: semesterKeys.all });
    },
  });
};

// Update semester mutation
export const useUpdateSemester = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      id,
      data,
    }: {
      id: string;
      data: UpdateSemesterInput;
    }) => {
      const response = await apiClient.put<Semester>(
        `/api/v1/semesters/${id}`,
        data,
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: semesterKeys.all });
    },
  });
};

// Delete semester mutation
export const useDeleteSemester = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.delete(`/api/v1/semesters/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: semesterKeys.all });
    },
  });
};
