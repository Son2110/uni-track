import React from "react";
import { Edit, Trash2 } from "lucide-react";
import { Card } from "@/components/ui/Card";
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
} from "@/components/ui/Table";
import type { Semester } from "@/types";

export interface SemesterTableProps {
  semesters: Semester[];
  onEdit?: (semester: Semester) => void;
  onDelete?: (semester: Semester) => void;
}

// Format ISO date to display format
const formatDate = (isoDate: string): string => {
  return new Date(isoDate).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
};

export const SemesterTable: React.FC<SemesterTableProps> = ({
  semesters,
  onEdit,
  onDelete,
}) => {
  return (
    <Card>
      <Table>
        <TableHeader>
          <tr>
            <TableHead>Name</TableHead>
            <TableHead>Start Date</TableHead>
            <TableHead>End Date</TableHead>
            <TableHead className="text-right">Actions</TableHead>
          </tr>
        </TableHeader>
        <TableBody>
          {semesters.map((semester) => (
            <TableRow key={semester.semesterId}>
              <TableCell className="font-medium text-slate-900 dark:text-white">
                {semester.name}
              </TableCell>
              <TableCell className="text-slate-600 dark:text-slate-300">
                {formatDate(semester.startDate)}
              </TableCell>
              <TableCell className="text-slate-600 dark:text-slate-300">
                {formatDate(semester.endDate)}
              </TableCell>
              <TableCell className="text-right">
                <div className="flex justify-end gap-2">
                  <button
                    onClick={() => onEdit?.(semester)}
                    className="p-1.5 text-slate-400 hover:text-primary hover:bg-primary/10 rounded-md transition-colors"
                    title="Edit semester"
                  >
                    <Edit className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => onDelete?.(semester)}
                    className="p-1.5 text-slate-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-md transition-colors"
                    title="Delete semester"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Card>
  );
};
