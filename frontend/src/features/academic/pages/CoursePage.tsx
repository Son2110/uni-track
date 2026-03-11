import React, { useState } from "react";
import { Plus } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/Button";
import { CourseCard } from "@/features/academic/components/CourseCard";
import { LoadingPage, ErrorPage } from "@/components/ui/Loading";
import { useCourses } from "@/features/academic/api/useCourses";
import {
  CourseFormModal,
  DeleteCourseDialog,
} from "@/features/academic/components/CourseFormModal";
import type { Course } from "@/types";

export const CoursePage: React.FC = () => {
  const navigate = useNavigate();
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [selectedCourse, setSelectedCourse] = useState<Course | null>(null);
  const [formMode, setFormMode] = useState<"create" | "edit">("create");

  const { data: courses, isLoading, isError, error, refetch } = useCourses();

  const handleCreate = () => {
    setSelectedCourse(null);
    setFormMode("create");
    setIsFormOpen(true);
  };

  const handleEdit = (course: Course) => {
    setSelectedCourse(course);
    setFormMode("edit");
    setIsFormOpen(true);
  };

  const handleDelete = (course: Course) => {
    setSelectedCourse(course);
    setIsDeleteOpen(true);
  };

  const handleViewDetails = (courseId: string) => {
    navigate(`/admin/courses/${courseId}`);
  };

  const handleFormClose = () => {
    setIsFormOpen(false);
    setSelectedCourse(null);
  };

  const handleDeleteClose = () => {
    setIsDeleteOpen(false);
    setSelectedCourse(null);
  };

  if (isLoading) {
    return <LoadingPage message="Loading courses..." />;
  }

  if (isError) {
    return (
      <ErrorPage
        message={error?.message || "Failed to load courses"}
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
            Courses
          </h2>
          <p className="text-slate-500 dark:text-slate-400 font-medium">
            Browse and manage academic courses.
          </p>
        </div>
        <Button variant="primary" onClick={handleCreate}>
          <Plus className="w-4 h-4" />
          New Course
        </Button>
      </div>

      {/* Course Grid */}
      {!courses?.length ? (
        <div className="text-center py-12 text-slate-500 dark:text-slate-400">
          No courses found. Create your first course to get started.
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {courses.map((course) => (
            <CourseCard
              key={course.courseId}
              course={course}
              onEdit={() => handleEdit(course)}
              onDelete={() => handleDelete(course)}
              onViewDetails={() => handleViewDetails(course.courseId)}
            />
          ))}
        </div>
      )}

      {/* Form Modal */}
      <CourseFormModal
        isOpen={isFormOpen}
        onClose={handleFormClose}
        course={selectedCourse}
        mode={formMode}
      />

      {/* Delete Dialog */}
      <DeleteCourseDialog
        isOpen={isDeleteOpen}
        onClose={handleDeleteClose}
        course={selectedCourse}
      />
    </div>
  );
};
