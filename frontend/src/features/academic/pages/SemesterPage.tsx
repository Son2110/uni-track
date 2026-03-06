import React, { useState } from "react";
import { Plus } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { SemesterTable } from "@/features/academic/components/SemesterTable";
import { LoadingPage, ErrorPage } from "@/components/ui/Loading";
import { useSemesters } from "@/features/academic/api/useSemesters";
import {
  SemesterFormModal,
  DeleteSemesterDialog,
} from "@/features/academic/components/SemesterFormModal";
import type { Semester } from "@/types";

export const SemesterPage: React.FC = () => {
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [selectedSemester, setSelectedSemester] = useState<Semester | null>(
    null,
  );
  const [formMode, setFormMode] = useState<"create" | "edit">("create");

  const {
    data: semesters,
    isLoading,
    isError,
    error,
    refetch,
  } = useSemesters();

  const handleCreate = () => {
    setSelectedSemester(null);
    setFormMode("create");
    setIsFormOpen(true);
  };

  const handleEdit = (semester: Semester) => {
    setSelectedSemester(semester);
    setFormMode("edit");
    setIsFormOpen(true);
  };

  const handleDelete = (semester: Semester) => {
    setSelectedSemester(semester);
    setIsDeleteOpen(true);
  };

  const handleFormClose = () => {
    setIsFormOpen(false);
    setSelectedSemester(null);
  };

  const handleDeleteClose = () => {
    setIsDeleteOpen(false);
    setSelectedSemester(null);
  };

  if (isLoading) {
    return <LoadingPage message="Loading semesters..." />;
  }

  if (isError) {
    return (
      <ErrorPage
        message={error?.message || "Failed to load semesters"}
        onRetry={() => refetch()}
      />
    );
  }

  return (
    <div className="flex flex-col gap-8 pb-10">
      {/* Page Header */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div className="flex flex-col gap-1">
          <h2 className="text-3xl font-black text-slate-900 dark:text-white tracking-tight">
            Semesters
          </h2>
          <p className="text-slate-500 dark:text-slate-400 font-medium">
            Manage academic terms and timelines.
          </p>
        </div>
        <Button variant="primary" onClick={handleCreate}>
          <Plus className="w-4 h-4" />
          New Semester
        </Button>
      </div>

      {/* Semester Table */}
      <SemesterTable
        semesters={semesters || []}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />

      {/* Form Modal */}
      <SemesterFormModal
        isOpen={isFormOpen}
        onClose={handleFormClose}
        semester={selectedSemester}
        mode={formMode}
      />

      {/* Delete Dialog */}
      <DeleteSemesterDialog
        isOpen={isDeleteOpen}
        onClose={handleDeleteClose}
        semester={selectedSemester}
      />
    </div>
  );
};
