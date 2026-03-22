import { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Plus,
  Search,
  Users as UsersIcon,
  Edit,
  Trash2,
  GraduationCap,
  ArrowRight,
} from "lucide-react";
import {
  useClasses,
  useCreateClass,
  useUpdateClass,
  useDeleteClass,
} from "@/features/academic/api/useClasses";
import { useSemesters } from "@/features/academic/api/useSemesters";
import { useCourses } from "@/features/academic/api/useCourses";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";
import { Modal } from "@/components/ui/Modal";
import { Input } from "@/components/ui/Input";
import type { ClassWithRelations } from "@/features/academic/api/useClasses";
import { useAuth } from "@/features/auth/context/AuthContext";

export function TeacherClassesPage() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [searchTerm, setSearchTerm] = useState("");
  const [filterSemester, setFilterSemester] = useState("");
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [editingClass, setEditingClass] = useState<ClassWithRelations | null>(
    null,
  );

  const { data: classes = [], isLoading } = useClasses();
  const { data: semesters = [] } = useSemesters();
  const createMutation = useCreateClass();
  const updateMutation = useUpdateClass();
  const deleteMutation = useDeleteClass();

  // Filter classes taught by current teacher
  const myClasses = classes.filter((cls) => cls.teacherId === user?.userId);

  const filteredClasses = myClasses.filter((cls) => {
    const matchesSearch = [
      cls.classCode,
      cls.course?.name,
      cls.course?.code,
      cls.semester?.name,
    ]
      .join(" ")
      .toLowerCase()
      .includes(searchTerm.toLowerCase());

    const matchesSemester = filterSemester
      ? cls.semesterId === filterSemester
      : true;

    return matchesSearch && matchesSemester;
  });

  const handleDelete = async (classId: string) => {
    if (
      !confirm(
        "Are you sure you want to delete this class? All enrollments will be removed.",
      )
    )
      return;

    try {
      await deleteMutation.mutateAsync(classId);
    } catch (error) {
      console.error("Failed to delete class:", error);
    }
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">My Classes</h1>
          <p className="text-sm text-gray-500">Manage your teaching classes</p>
        </div>
        <Button onClick={() => setShowCreateModal(true)}>
          <Plus className="w-4 h-4 mr-2" />
          Create Class
        </Button>
      </div>

      {/* Filters */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
          <input
            type="text"
            placeholder="Search classes..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
        </div>

        <select
          value={filterSemester}
          onChange={(e) => setFilterSemester(e.target.value)}
          className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        >
          <option value="">All Semesters</option>
          {semesters.map((semester) => (
            <option key={semester.semesterId} value={semester.semesterId}>
              {semester.name}
            </option>
          ))}
        </select>
      </div>

      {/* Classes List */}
      {isLoading ? (
        <div className="text-center py-8 text-gray-500">Loading...</div>
      ) : filteredClasses.length === 0 ? (
        <div className="text-center py-12">
          <GraduationCap className="w-16 h-16 mx-auto mb-4 text-gray-300" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            {searchTerm || filterSemester
              ? "No classes found"
              : "No classes yet"}
          </h3>
          <p className="text-gray-500 mb-4">
            {searchTerm || filterSemester
              ? "Try adjusting your filters"
              : "Start by creating your first class"}
          </p>
          {!searchTerm && !filterSemester && (
            <Button onClick={() => setShowCreateModal(true)}>
              Create Class
            </Button>
          )}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredClasses.map((classItem) => (
            <ClassCard
              key={classItem.classId}
              classItem={classItem}
              onEdit={() => setEditingClass(classItem)}
              onDelete={() => handleDelete(classItem.classId)}
              onViewDetails={() =>
                navigate(`/teacher/classes/${classItem.classId}`)
              }
            />
          ))}
        </div>
      )}

      {/* Create/Edit Modal */}
      {(showCreateModal || editingClass) && (
        <ClassFormModal
          classItem={editingClass}
          onClose={() => {
            setShowCreateModal(false);
            setEditingClass(null);
          }}
          onSubmit={async (data) => {
            if (editingClass) {
              await updateMutation.mutateAsync({
                id: editingClass.classId,
                data,
              });
            } else {
              await createMutation.mutateAsync(data);
            }
            setShowCreateModal(false);
            setEditingClass(null);
          }}
        />
      )}
    </div>
  );
}

// ============================================
// CLASS CARD
// ============================================

interface ClassCardProps {
  classItem: ClassWithRelations;
  onEdit: () => void;
  onDelete: () => void;
  onViewDetails: () => void;
}

function ClassCard({
  classItem,
  onEdit,
  onDelete,
  onViewDetails,
}: ClassCardProps) {
  return (
    <Card className="p-4 hover:shadow-lg transition-shadow">
      <div className="space-y-3">
        <div className="flex justify-between items-start">
          <div className="flex-1">
            <h3 className="font-semibold text-gray-900">
              {classItem.course?.name}
            </h3>
            <p className="text-sm text-gray-500">{classItem.course?.code}</p>
          </div>
          <Badge variant="info">{classItem.classCode}</Badge>
        </div>

        <div className="space-y-2 text-sm">
          <div className="flex items-center gap-2 text-gray-600">
            <GraduationCap className="w-4 h-4" />
            <span>{classItem.semester?.name}</span>
          </div>
          <div className="flex items-center gap-2 text-gray-600">
            <UsersIcon className="w-4 h-4" />
            <span>Teacher: {classItem.teacher?.name}</span>
          </div>
        </div>

        <div className="pt-3 border-t space-y-2">
          <Button
            variant="outline"
            size="sm"
            onClick={onViewDetails}
            className="w-full"
          >
            <ArrowRight className="w-4 h-4 mr-1" />
            View Details
          </Button>
          <div className="flex gap-2">
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
      </div>
    </Card>
  );
}

// ============================================
// CLASS FORM MODAL
// ============================================

interface ClassFormModalProps {
  classItem: ClassWithRelations | null;
  onClose: () => void;
  onSubmit: (data: any) => Promise<void>;
}

function ClassFormModal({ classItem, onClose, onSubmit }: ClassFormModalProps) {
  const { data: semesters = [] } = useSemesters();
  const { data: courses = [] } = useCourses();

  const { user } = useAuth();

  const [formData, setFormData] = useState({
    classCode: classItem?.classCode || "",
    semesterId: classItem?.semesterId || "",
    courseId: classItem?.courseId || "",
    teacherId: classItem?.teacherId || user?.userId || "",
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    try {
      if (classItem) {
        // Update: teacher is locked in UI but backend requires it
        await onSubmit({
          classCode: formData.classCode,
          teacherId: formData.teacherId, // Required by backend UpdateClassDto
        });
      } else {
        // Create requires all fields
        await onSubmit({
          semesterId: formData.semesterId,
          courseId: formData.courseId,
          classCode: formData.classCode,
          teacherId: formData.teacherId,
        });
      }
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
      title={classItem ? "Edit Class" : "Create Class"}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <Input
          label="Class Code"
          value={formData.classCode}
          onChange={(e) =>
            setFormData({ ...formData, classCode: e.target.value })
          }
          placeholder="e.g., L01, L02, K01"
          required
        />

        {!classItem && (
          <>
            <div className="space-y-1.5">
              <label className="block text-sm font-medium text-slate-700">
                Semester
              </label>
              <select
                value={formData.semesterId}
                onChange={(e) =>
                  setFormData({ ...formData, semesterId: e.target.value })
                }
                className="w-full px-4 py-2.5 bg-white border border-slate-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary"
                required
              >
                <option value="">Select Semester</option>
                {semesters.map((semester) => (
                  <option key={semester.semesterId} value={semester.semesterId}>
                    {semester.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="space-y-1.5">
              <label className="block text-sm font-medium text-slate-700">
                Course
              </label>
              <select
                value={formData.courseId}
                onChange={(e) =>
                  setFormData({ ...formData, courseId: e.target.value })
                }
                className="w-full px-4 py-2.5 bg-white border border-slate-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary"
                required
              >
                <option value="">Select Course</option>
                {courses.map((course) => (
                  <option key={course.courseId} value={course.courseId}>
                    {course.code} - {course.name}
                  </option>
                ))}
              </select>
            </div>
          </>
        )}

        <div className="space-y-1.5">
          <label className="block text-sm font-medium text-slate-700">
            Teacher
          </label>
          <div className="w-full px-4 py-2.5 bg-gray-50 border border-slate-300 rounded-lg text-sm text-gray-700">
            {user?.name} ({user?.email})
          </div>
          <p className="text-xs text-gray-500">
            You will be assigned as the teacher for this class
          </p>
        </div>

        {classItem && (
          <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-3 text-sm text-yellow-800">
            Note: Semester and Course cannot be changed after creation.
          </div>
        )}

        <div className="flex justify-end gap-2 pt-4">
          <Button type="button" variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" isLoading={isSubmitting}>
            {classItem ? "Update" : "Create"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
