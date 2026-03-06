import { graphqlClient } from "@/lib/graphql";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  CREATE_SEMESTER,
  type CreateSemesterInput,
  type CreateSemesterResponse,
} from "./queries";

export const useCreateSemester = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (input: CreateSemesterInput) => {
      const data = await graphqlClient.request<CreateSemesterResponse>(
        CREATE_SEMESTER,
        { input },
      );
      return data.createSemester;
    },
    onSuccess: () => {
      // Invalidate và refetch danh sách semesters sau khi tạo thành công
      queryClient.invalidateQueries({ queryKey: ["semesters"] });
    },
    onError: (error) => {
      console.error("Error creating semester:", error);
    },
  });
};
