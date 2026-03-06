import { useState } from "react";
import { Plus, Search, Calendar, Edit, Trash2 } from "lucide-react";
import {
  useSemesters,
  useCreateSemester,
  useUpdateSemester,
  useDeleteSemester,
} from "@/features/academic/api/useSemesters";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";
import { Modal } from "@/components/ui/Modal";
import { Input } from "@/components/ui/Input";
import type { Semester } from "@/types";

export function TeacherSemestersPage() {
  const [searchTerm, setSearchTerm] = useState("");
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [editingSemester, setEditingSemester] = useState<Semester | null>(null);

  const { data: semesters = [], isLoading } = useSemesters();
  const createMutation = useCreateSemester();
  const updateMutation = useUpdateSemester();
  const deleteMutation = useDeleteSemester();

  const filteredSemesters = semesters.filter((semester) =>
    semester.name.toLowerCase().includes(searchTerm.toLowerCase()),
  );

  const handleDelete = async (semesterId: string) => {
    if (
      !confirm(
        "Are you sure you want to delete this semester? All related classes will be affected.",
      )
    )
      return;

    try {
      await deleteMutation.mutateAsync(semesterId);
    } catch (error) {
      console.error("Failed to delete semester:", error);
    }
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Manage Semesters</h1>
          <p className="text-sm text-gray-500">
            Create and manage academic semesters
          </p>
        </div>
        <Button onClick={() => setShowCreateModal(true)}>
          <Plus className="w-4 h-4 mr-2" />
          Create Semester
        </Button>
      </div>

      {/* Search */}
      <div className="relative">
        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
        <input
          type="text"
          placeholder="Search semesters..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>

      {/* Semesters List */}
      {isLoading ? (
        <div className="text-center py-8 text-gray-500">Loading...</div>
      ) : filteredSemesters.length === 0 ? (
        <div className="text-center py-12">
          <Calendar className="w-16 h-16 mx-auto mb-4 text-gray-300" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            {searchTerm ? "No semesters found" : "No semesters yet"}
          </h3>
          <p className="text-gray-500 mb-4">
            {searchTerm
              ? "Try a different search term"
              : "Start by creating your first semester"}
          </p>
          {!searchTerm && (
            <Button onClick={() => setShowCreateModal(true)}>
              Create Semester
            </Button>
          )}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredSemesters.map((semester) => (
            <SemesterCard
              key={semester.semesterId}
              semester={semester}
              onEdit={() => setEditingSemester(semester)}
              onDelete={() => handleDelete(semester.semesterId)}
            />
          ))}
        </div>
      )}

      {/* Create/Edit Modal */}
      {(showCreateModal || editingSemester) && (
        <SemesterFormModal
          semester={editingSemester}
          onClose={() => {
            setShowCreateModal(false);
            setEditingSemester(null);
          }}
          onSubmit={async (data) => {
            if (editingSemester) {
              await updateMutation.mutateAsync({
                id: editingSemester.semesterId,
                data,
              });
            } else {
              await createMutation.mutateAsync(data);
            }
            setShowCreateModal(false);
            setEditingSemester(null);
          }}
        />
      )}
    </div>
  );
}

// ============================================
// SEMESTER CARD
// ============================================

interface SemesterCardProps {
  semester: Semester;
  onEdit: () => void;
  onDelete: () => void;
}

function SemesterCard({ semester, onEdit, onDelete }: SemesterCardProps) {
  const now = new Date();
  const start = new Date(semester.startDate);
  const end = new Date(semester.endDate);

  let status: "active" | "upcoming" | "archived" = "archived";
  if (now >= start && now <= end) status = "active";
  else if (now < start) status = "upcoming";

  const statusLabels = {
    active: "Active",
    upcoming: "Upcoming",
    archived: "Archived",
  };

  return (
    <Card className="p-4 hover:shadow-lg transition-shadow">
      <div className="space-y-3">
        <div className="flex justify-between items-start">
          <div className="flex-1">
            <h3 className="font-semibold text-gray-900">{semester.name}</h3>
          </div>
          <Badge variant={status}>{statusLabels[status]}</Badge>
        </div>

        <div className="space-y-2 text-sm">
          <div className="flex items-center gap-2 text-gray-600">
            <Calendar className="w-4 h-4" />
            <span>
              {new Date(semester.startDate).toLocaleDateString()} -{" "}
              {new Date(semester.endDate).toLocaleDateString()}
            </span>
          </div>
        </div>

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
// SEMESTER FORM MODAL
// ============================================

interface SemesterFormModalProps {
  semester: Semester | null;
  onClose: () => void;
  onSubmit: (data: {
    name: string;
    startDate: string;
    endDate: string;
  }) => Promise<void>;
}

function SemesterFormModal({
  semester,
  onClose,
  onSubmit,
}: SemesterFormModalProps) {
  const [formData, setFormData] = useState({
    name: semester?.name || "",
    startDate: semester?.startDate
      ? new Date(semester.startDate).toISOString().split("T")[0]
      : "",
    endDate: semester?.endDate
      ? new Date(semester.endDate).toISOString().split("T")[0]
      : "",
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    try {
      await onSubmit({
        name: formData.name,
        startDate: new Date(formData.startDate).toISOString(),
        endDate: new Date(formData.endDate).toISOString(),
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
      title={semester ? "Edit Semester" : "Create Semester"}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <Input
          label="Semester Name"
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          placeholder="e.g., Fall 2024, Spring 2025"
          required
        />

        <Input
          label="Start Date"
          type="date"
          value={formData.startDate}
          onChange={(e) =>
            setFormData({ ...formData, startDate: e.target.value })
          }
          required
        />

        <Input
          label="End Date"
          type="date"
          value={formData.endDate}
          onChange={(e) =>
            setFormData({ ...formData, endDate: e.target.value })
          }
          required
        />

        <div className="flex justify-end gap-2 pt-4">
          <Button type="button" variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" isLoading={isSubmitting}>
            {semester ? "Update" : "Create"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
