import React from "react";
import { Edit, Trash2 } from "lucide-react";
import { Card } from "@/components/ui/Card";

// Extended Course interface with display fields
export interface CourseCardProps {
  course: {
    courseId: string;
    code: string;
    name: string;
    description: string;
  };
  onEdit?: () => void;
  onDelete?: () => void;
  onViewDetails?: () => void;
}

export const CourseCard: React.FC<CourseCardProps> = ({
  course,
  onEdit,
  onDelete,
  onViewDetails,
}) => {
  return (
    <Card className="overflow-hidden group hover:shadow-md transition-all">
      {/* Course Header */}
      <div className="h-32 bg-sidebar-blue relative p-6 flex flex-col justify-between">
        <div className="absolute top-4 right-4 flex items-center gap-2">
          <span className="bg-white/20 text-white text-xs px-2 py-1 rounded font-medium">
            {course.code}
          </span>
          <button
            onClick={onEdit}
            className="p-1.5 bg-white/20 hover:bg-white/30 text-white rounded-md transition-colors opacity-0 group-hover:opacity-100"
            title="Edit course"
          >
            <Edit className="w-3.5 h-3.5" />
          </button>
          <button
            onClick={onDelete}
            className="p-1.5 bg-white/20 hover:bg-red-500/80 text-white rounded-md transition-colors opacity-0 group-hover:opacity-100"
            title="Delete course"
          >
            <Trash2 className="w-3.5 h-3.5" />
          </button>
        </div>
        <h3 className="text-white font-bold text-xl mt-auto">{course.name}</h3>
      </div>

      {/* Course Details */}
      <div className="p-6">
        <p className="text-sm text-slate-600 dark:text-slate-300 line-clamp-3">
          {course.description || "No description available"}
        </p>
      </div>

      {/* Footer */}
      {onViewDetails && (
        <div className="px-6 pb-6 pt-0">
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
