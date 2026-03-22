import React from "react";
import { User, Users, Edit, Trash2 } from "lucide-react";
import { Card } from "@/components/ui/Card";

// Extended class interface for display
export interface ClassCardProps {
  classItem: {
    classId: string;
    classCode: string;
    courseName?: string;
    lecturer?: string;
    studentCount?: number;
    category?: string;
  };
  onEdit?: () => void;
  onDelete?: () => void;
  onViewDetails?: () => void;
}

export const ClassCard: React.FC<ClassCardProps> = ({
  classItem,
  onEdit,
  onDelete,
  onViewDetails,
}) => {
  return (
    <Card className="p-5 flex flex-col gap-4 hover:shadow-md transition-shadow">
      {/* Header - Class Code with Edit/Delete */}
      <div className="flex justify-between items-center">
        <h3 className="font-bold text-lg text-slate-900 dark:text-white">
          {classItem.classCode}
        </h3>
        <div className="flex items-center gap-2">
          <button
            onClick={onEdit}
            className="p-1.5 text-slate-400 hover:text-primary hover:bg-primary/10 rounded-md transition-colors"
            title="Edit class"
          >
            <Edit className="w-3.5 h-3.5" />
          </button>
          <button
            onClick={onDelete}
            className="p-1.5 text-slate-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-md transition-colors"
            title="Delete class"
          >
            <Trash2 className="w-3.5 h-3.5" />
          </button>
        </div>
      </div>

      {/* Class Info */}
      <div>
        <p className="text-sm text-slate-500 dark:text-slate-400">
          {classItem.courseName || "Course Name"}
        </p>
      </div>

      {/* Details */}
      <div className="mt-auto pt-4 border-t border-gray-100 dark:border-gray-800 flex flex-col gap-2">
        <div className="flex items-center justify-between text-sm">
          <span className="text-slate-400 flex items-center gap-1">
            <User className="w-4 h-4" />
            Lecturer
          </span>
          <span className="font-medium text-slate-700 dark:text-slate-200">
            {classItem.lecturer || "TBA"}
          </span>
        </div>
        <div className="flex items-center justify-between text-sm">
          <span className="text-slate-400 flex items-center gap-1">
            <Users className="w-4 h-4" />
            Students
          </span>
          <span className="font-medium text-slate-700 dark:text-slate-200">
            {classItem.studentCount || 0}
          </span>
        </div>
      </div>

      {/* Footer */}
      {onViewDetails && (
        <div className="pt-2">
          <button
            onClick={onViewDetails}
            className="text-primary hover:text-primary-dark text-sm font-medium transition-colors"
          >
            View Details →
          </button>
        </div>
      )}
    </Card>
  );
};
