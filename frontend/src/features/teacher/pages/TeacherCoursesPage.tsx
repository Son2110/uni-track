import { useState } from "react";
import { Plus, Search, BookOpen, Edit, Trash2 } from "lucide-react";
import {
  useCourses,
  useCreateCourse,
  useUpdateCourse,
  useDeleteCourse,
} from "@/features/academic/api/useCourses";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Modal } from "@/components/ui/Modal";
import { Input } from "@/components/ui/Input";
import type { Course } from "@/types";

export function TeacherCoursesPage() {
  const [searchTerm, setSearchTerm] = useState("");
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [editingCourse, setEditingCourse] = useState<Course | null>(null);

  const { data: courses = [], isLoading } = useCourses();
  const createMutation = useCreateCourse();
  const updateMutation = useUpdateCourse();
  const deleteMutation = useDeleteCourse();

  const filteredCourses = courses.filter((course) =>
    [course.code, course.name, course.description]
      .join(" ")
      .toLowerCase()
      .includes(searchTerm.toLowerCase()),
  );

  const handleDelete = async (courseId: string) => {
    if (
      !confirm(
        "Are you sure you want to delete this course? All related classes will be affected.",
      )
    )
      return;

    try {
      await deleteMutation.mutateAsync(courseId);
    } catch (error) {
      console.error("Failed to delete course:", error);
    }
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Manage Courses</h1>
          <p className="text-sm text-gray-500">
            Create and manage course catalog
          </p>
        </div>
        <Button onClick={() => setShowCreateModal(true)}>
          <Plus className="w-4 h-4 mr-2" />
          Create Course
        </Button>
      </div>

      {/* Search */}
      <div className="relative">
        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
        <input
          type="text"
          placeholder="Search courses by code, name..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>

      {/* Courses List */}
      {isLoading ? (
        <div className="text-center py-8 text-gray-500">Loading...</div>
      ) : filteredCourses.length === 0 ? (
        <div className="text-center py-12">
          <BookOpen className="w-16 h-16 mx-auto mb-4 text-gray-300" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            {searchTerm ? "No courses found" : "No courses yet"}
          </h3>
          <p className="text-gray-500 mb-4">
            {searchTerm
              ? "Try a different search term"
              : "Start by creating your first course"}
          </p>
          {!searchTerm && (
            <Button onClick={() => setShowCreateModal(true)}>
              Create Course
            </Button>
          )}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredCourses.map((course) => (
            <CourseCard
              key={course.courseId}
              course={course}
              onEdit={() => setEditingCourse(course)}
              onDelete={() => handleDelete(course.courseId)}
            />
          ))}
        </div>
      )}

      {/* Create/Edit Modal */}
      {(showCreateModal || editingCourse) && (
        <CourseFormModal
          course={editingCourse}
          onClose={() => {
            setShowCreateModal(false);
            setEditingCourse(null);
          }}
          onSubmit={async (data) => {
            if (editingCourse) {
              await updateMutation.mutateAsync({
                id: editingCourse.courseId,
                data,
              });
            } else {
              await createMutation.mutateAsync(data);
            }
            setShowCreateModal(false);
            setEditingCourse(null);
          }}
        />
      )}
    </div>
  );
}

// ============================================
// COURSE CARD
// ============================================

interface CourseCardProps {
  course: Course;
  onEdit: () => void;
  onDelete: () => void;
}

function CourseCard({ course, onEdit, onDelete }: CourseCardProps) {
  return (
    <Card className="p-4 hover:shadow-lg transition-shadow">
      <div className="space-y-3">
        <div className="flex justify-between items-start">
          <div className="flex-1">
            <h3 className="font-semibold text-gray-900">{course.name}</h3>
            <p className="text-sm font-mono text-gray-500">{course.code}</p>
          </div>
          <BookOpen className="w-5 h-5 text-gray-400" />
        </div>

        {course.description && (
          <p className="text-sm text-gray-600 line-clamp-2">
            {course.description}
          </p>
        )}

        <div className="pt-3 border-t flex gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={onEdit}
            className="flex-1"
          >
            <Edit className="w-4 h-4 mr-1" />
            Edit
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={onDelete}
            className="flex-1"
          >
            <Trash2 className="w-4 h-4 mr-1" />
            Delete
          </Button>
        </div>
      </div>
    </Card>
  );
}

// ============================================
// COURSE FORM MODAL
// ============================================

interface CourseFormModalProps {
  course: Course | null;
  onClose: () => void;
  onSubmit: (data: {
    code: string;
    name: string;
    description?: string;
  }) => Promise<void>;
}

function CourseFormModal({ course, onClose, onSubmit }: CourseFormModalProps) {
  const [formData, setFormData] = useState({
    code: course?.code || "",
    name: course?.name || "",
    description: course?.description || "",
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    try {
      await onSubmit({
        code: formData.code,
        name: formData.name,
        description: formData.description || undefined,
      });
      onClose();
    } catch (error) {
      console.error("Failed to submit:", error);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={true}
      onClose={onClose}
      title={course ? "Edit Course" : "Create Course"}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <Input
          label="Course Code"
          value={formData.code}
          onChange={(e) => setFormData({ ...formData, code: e.target.value })}
          placeholder="e.g., CS101, MATH202"
          required
        />

        <Input
          label="Course Name"
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          placeholder="e.g., Introduction to Programming"
          required
        />

        <div className="space-y-1.5">
          <label className="block text-sm font-medium text-slate-700">
            Description (Optional)
          </label>
          <textarea
            value={formData.description}
            onChange={(e) =>
              setFormData({ ...formData, description: e.target.value })
            }
            placeholder="Enter course description..."
            rows={4}
            className="w-full px-4 py-2.5 bg-white border border-slate-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary placeholder:text-slate-400 text-slate-900"
          />
        </div>

        <div className="flex justify-end gap-2 pt-4">
          <Button type="button" variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" isLoading={isSubmitting}>
            {course ? "Update" : "Create"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
