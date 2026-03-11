import React, { useState, useEffect } from "react";
import { Search, Filter } from "lucide-react";
import { Card } from "@/components/ui/Card";
import { Input } from "@/components/ui/Input";
import type { UserFilterParams } from "@/features/users/api/queries";

interface UserFiltersProps {
  filters: UserFilterParams;
  onFiltersChange: (filters: UserFilterParams) => void;
}

export const UserFilters: React.FC<UserFiltersProps> = ({
  filters,
  onFiltersChange,
}) => {
  // Local state for search input with debounce
  const [searchInput, setSearchInput] = useState(filters.search || "");

  // Debounce search input - only update filters after 500ms of no typing
  useEffect(() => {
    const timer = setTimeout(() => {
      onFiltersChange({
        ...filters,
        search: searchInput || undefined,
      });
    }, 500);

    return () => clearTimeout(timer);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchInput]);

  // Sync local state when filters change externally (e.g., clear filters)
  useEffect(() => {
    setSearchInput(filters.search || "");
  }, [filters.search]);

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchInput(e.target.value);
  };

  const handleRoleChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const value = e.target.value;
    onFiltersChange({
      ...filters,
      role:
        value === "all"
          ? undefined
          : (value as "Admin" | "Teacher" | "Student"),
    });
  };

  const handleClearFilters = () => {
    onFiltersChange({});
  };
  return (
    <Card className="p-4 flex flex-col sm:flex-row gap-4 items-center justify-between">
      {/* Search Input */}
      <div className="w-full sm:w-72">
        <Input
          icon={Search}
          placeholder="Search by name, email..."
          type="text"
          value={searchInput}
          onChange={handleSearchChange}
        />
      </div>

      {/* Filter Controls */}
      <div className="flex items-center gap-3 w-full sm:w-auto">
        <select
          className="form-select bg-gray-50 dark:bg-gray-800 border-none rounded-lg text-sm py-2 px-4 focus:ring-2 focus:ring-primary/50 text-slate-700 dark:text-white cursor-pointer"
          value={filters.role?.toLowerCase() || "all"}
          onChange={handleRoleChange}
        >
          <option value="all">All Roles</option>
          <option value="admin">Admin</option>
          <option value="teacher">Teacher</option>
          <option value="student">Student</option>
        </select>
        <button
          onClick={handleClearFilters}
          className="p-2 bg-gray-100 dark:bg-gray-700 rounded-lg text-slate-600 dark:text-slate-300 hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors"
          title="Clear filters"
        >
          <Filter className="w-5 h-5" />
        </button>
      </div>
    </Card>
  );
};
