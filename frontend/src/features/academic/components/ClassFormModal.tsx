import React, { useState, useEffect } from "react";
import { Modal, ConfirmDialog } from "@/components/ui/Modal";
import { Input } from "@/components/ui/Input";
import { Select } from "@/components/ui/Select";
import { Button } from "@/components/ui/Button";
import {
  useCreateClass,
  useUpdateClass,
  useDeleteClass,
  type ClassWithRelations,
} from "../api/useClasses";
import { useSemesters } from "../api/useSemesters";
import { useCourses } from "../api/useCourses";
import { useUsers } from "@/features/users/api/useUsers";

interface ClassFormData {
  semesterId: string;
  courseId: string;
  classCode: string;
  teacherId: string;
}

const initialFormData: ClassFormData = {
  semesterId: "",
  courseId: "",
  classCode: "",
  teacherId: "",
};

interface ClassFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  classData?: ClassWithRelations | null;
  mode: "create" | "edit";
}

export const ClassFormModal: React.FC<ClassFormModalProps> = ({
  isOpen,
  onClose,
  classData,
  mode,
}) => {
  const [formData, setFormData] = useState<ClassFormData>(initialFormData);
  const [errors, setErrors] = useState<Partial<ClassFormData>>({});

  const createMutation = useCreateClass();
  const updateMutation = useUpdateClass();

  const { data: semesters = [] } = useSemesters();
  const { data: courses = [] } = useCourses();
  const { data: users, isLoading: isLoadingUsers } = useUsers();

  // Filter teachers only (check both formats: GraphQL returns UPPERCASE, REST may return PascalCase)
  const teachers = (users || []).filter(
    (u) => u.role === "TEACHER" || u.role === "Teacher",
  );

  const isLoading = createMutation.isPending || updateMutation.isPending;

  useEffect(() => {
    if (classData && mode === "edit") {
      setFormData({
        semesterId: classData.semesterId,
        courseId: classData.courseId,
        classCode: classData.classCode,
        teacherId: classData.teacherId,
      });
    } else {
      setFormData(initialFormData);
    }
    setErrors({});
  }, [classData, mode, isOpen]);

  const validate = (): boolean => {
    const newErrors: Partial<ClassFormData> = {};

    if (mode === "create") {
      if (!formData.semesterId) {
        newErrors.semesterId = "Semester is required";
      }

      if (!formData.courseId) {
        newErrors.courseId = "Course is required";
      }
    }

    if (!formData.classCode.trim()) {
      newErrors.classCode = "Class code is required";
    }

    if (!formData.teacherId) {
      newErrors.teacherId = "Teacher is required";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validate()) return;

    try {
      if (mode === "create") {
        await createMutation.mutateAsync({
          semesterId: formData.semesterId,
          courseId: formData.courseId,
          classCode: formData.classCode,
          teacherId: formData.teacherId,
        });
      } else if (classData) {
        await updateMutation.mutateAsync({
          id: classData.classId,
          data: {
            classCode: formData.classCode,
            teacherId: formData.teacherId,
          },
        });
      }
      onClose();
    } catch (error) {
      console.error("Failed to save class:", error);
    }
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>,
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    if (errors[name as keyof ClassFormData]) {
      setErrors((prev) => ({ ...prev, [name]: undefined }));
    }
  };

  const semesterOptions = semesters.map((s) => ({
    value: s.semesterId,
    label: s.name,
  }));

  const courseOptions = courses.map((c) => ({
    value: c.courseId,
    label: `${c.code} - ${c.name}`,
  }));

  const teacherOptions = teachers.map((t) => ({
    value: t.userId,
    label: `${t.name} (${t.email})`,
  }));

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={mode === "create" ? "Create New Class" : "Edit Class"}
      description={
        mode === "create"
          ? "Set up a new class for a course and semester."
          : "Update the class information."
      }
      size="lg"
    >
      <form onSubmit={handleSubmit} className="space-y-5">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Select
            label="Semester *"
            name="semesterId"
            value={formData.semesterId}
            onChange={handleChange}
            options={semesterOptions}
            placeholder="Select semester"
            error={errors.semesterId}
            disabled={mode === "edit"}
          />

          <Select
            label="Course *"
            name="courseId"
            value={formData.courseId}
            onChange={handleChange}
            options={courseOptions}
            placeholder="Select course"
            error={errors.courseId}
            disabled={mode === "edit"}
          />

          <Input
            label="Class Code *"
            name="classCode"
            value={formData.classCode}
            onChange={handleChange}
            placeholder="e.g., SE1801, SE1802"
            error={errors.classCode}
          />

          <Select
            label="Teacher *"
            name="teacherId"
            value={formData.teacherId}
            onChange={handleChange}
            options={teacherOptions}
            placeholder={
              isLoadingUsers
                ? "Loading teachers..."
                : teachers.length === 0
                  ? "No teachers available"
                  : "Select teacher"
            }
            error={errors.teacherId}
            disabled={isLoadingUsers}
          />
        </div>

        <div className="flex justify-end gap-3 pt-4 border-t border-slate-200 dark:border-slate-700">
          <Button type="button" variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" variant="primary" disabled={isLoading}>
            {isLoading
              ? "Saving..."
              : mode === "create"
                ? "Create Class"
                : "Save Changes"}
          </Button>
        </div>
      </form>
    </Modal>
  );
};

// Delete confirmation dialog
interface DeleteClassDialogProps {
  isOpen: boolean;
  onClose: () => void;
  classData: ClassWithRelations | null;
}

export const DeleteClassDialog: React.FC<DeleteClassDialogProps> = ({
  isOpen,
  onClose,
  classData,
}) => {
  const deleteMutation = useDeleteClass();

  const handleConfirm = async () => {
    if (!classData) return;

    try {
      await deleteMutation.mutateAsync(classData.classId);
      onClose();
    } catch (error) {
      console.error("Failed to delete class:", error);
    }
  };

  return (
    <ConfirmDialog
      isOpen={isOpen}
      onClose={onClose}
      onConfirm={handleConfirm}
      title="Delete Class"
      message={`Are you sure you want to delete class "${classData?.classCode}"? This will also remove all enrollments and projects.`}
      confirmText="Delete"
      variant="danger"
      isLoading={deleteMutation.isPending}
    />
  );
};
