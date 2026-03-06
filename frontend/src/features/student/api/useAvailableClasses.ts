import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/api";
import type { ClassDto } from "@/types";

/**
 * Fetch all available classes in denormalized DTO format
 * Useful for enrollment forms where we need courseName, semesterName, teacherName
 */
export const useAvailableClasses = () => {
  return useQuery({
    queryKey: ["available-classes"],
    queryFn: async () => {
      const response = await apiClient.get<ClassDto[]>("/api/v1/classes");
      return response.data;
    },
  });
};
