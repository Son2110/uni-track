import React, { useState, useMemo } from "react";
import { Plus } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/Button";
import { ClassCard } from "@/features/academic/components/ClassCard";
import { LoadingPage, ErrorPage } from "@/components/ui/Loading";
import {
  useClasses,
  type ClassWithRelations,
} from "@/features/academic/api/useClasses";
import {
  ClassFormModal,
  DeleteClassDialog,
} from "@/features/academic/components/ClassFormModal";
import { useEnrollments } from "@/features/student/api/useEnrollments";

export const ClassPage: React.FC = () => {
  const navigate = useNavigate();
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [selectedClass, setSelectedClass] = useState<ClassWithRelations | null>(
    null,
  );
  const [formMode, setFormMode] = useState<"create" | "edit">("create");

  const { data: classes, isLoading, isError, error, refetch } = useClasses();
  const { data: enrollments, isLoading: isEnrollmentsLoading } =
    useEnrollments();

  // Create a map of classId -> student count
  const studentCountMap = useMemo(() => {
    if (!enrollments) return new Map<string, number>();

    const countMap = new Map<string, number>();
    enrollments.forEach((enrollment) => {
      const count = countMap.get(enrollment.classId) || 0;
      countMap.set(enrollment.classId, count + 1);
    });
    return countMap;
  }, [enrollments]);

  const handleCreate = () => {
    setSelectedClass(null);
    setFormMode("create");
    setIsFormOpen(true);
  };

  const handleEdit = (classItem: ClassWithRelations) => {
    setSelectedClass(classItem);
    setFormMode("edit");
    setIsFormOpen(true);
  };

  const handleDelete = (classItem: ClassWithRelations) => {
    setSelectedClass(classItem);
    setIsDeleteOpen(true);
  };

  const handleViewDetails = (classId: string) => {
    navigate(`/admin/classes/${classId}`);
  };

  const handleFormClose = () => {
    setIsFormOpen(false);
    setSelectedClass(null);
  };

  const handleDeleteClose = () => {
    setIsDeleteOpen(false);
    setSelectedClass(null);
  };

  if (isLoading || isEnrollmentsLoading) {
    return <LoadingPage message="Loading classes..." />;
  }

  if (isError) {
    return (
      <ErrorPage
        message={error?.message || "Failed to load classes"}
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
            Classes
          </h2>
          <p className="text-slate-500 dark:text-slate-400 font-medium">
            Manage and organize class schedules and assignments.
          </p>
        </div>
        <Button variant="primary" onClick={handleCreate}>
          <Plus className="w-4 h-4" />
          Create Class
        </Button>
      </div>

      {/* Class Grid */}
      {!classes?.length ? (
        <div className="text-center py-12 text-slate-500 dark:text-slate-400">
          No classes found. Create your first class to get started.
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {classes?.map((classItem) => {
            const displayClass = {
              classId: classItem.classId,
              classCode: classItem.classCode,
              courseName: classItem.course?.name || "Unknown Course",
              lecturer: classItem.teacher?.name || "TBA",
              studentCount: studentCountMap.get(classItem.classId) || 0,
              category: classItem.classCode.substring(0, 4),
            };
            return (
              <ClassCard
                key={classItem.classId}
                classItem={displayClass}
                onEdit={() => handleEdit(classItem)}
                onDelete={() => handleDelete(classItem)}
                onViewDetails={() => handleViewDetails(classItem.classId)}
              />
            );
          })}
        </div>
      )}

      {/* Form Modal */}
      <ClassFormModal
        isOpen={isFormOpen}
        onClose={handleFormClose}
        classData={selectedClass}
        mode={formMode}
      />

      {/* Delete Dialog */}
      <DeleteClassDialog
        isOpen={isDeleteOpen}
        onClose={handleDeleteClose}
        classData={selectedClass}
      />
    </div>
  );
};
