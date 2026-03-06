import React, { useState } from "react";
import { Upload, UserPlus } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { UserFilters } from "@/features/users/components/UserFilters";
import { UserTable } from "@/features/users/components/UserTable";
import { LoadingPage, ErrorPage } from "@/components/ui/Loading";
import { useUsers } from "@/features/users/api/useUsers";
import {
  UserFormModal,
  DeleteUserDialog,
} from "@/features/users/components/UserFormModal";
import type { User } from "@/types";

export const UserPage: React.FC = () => {
  const { data: users, isLoading, isError, error, refetch } = useUsers();

  // Modal states
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [formMode, setFormMode] = useState<"create" | "edit">("create");

  const handleCreate = () => {
    setSelectedUser(null);
    setFormMode("create");
    setIsFormOpen(true);
  };

  const handleEdit = (user: User) => {
    setSelectedUser(user);
    setFormMode("edit");
    setIsFormOpen(true);
  };

  const handleDelete = (user: User) => {
    setSelectedUser(user);
    setIsDeleteOpen(true);
  };

  if (isLoading) {
    return <LoadingPage message="Loading users..." />;
  }

  if (isError) {
    return (
      <ErrorPage
        message={error?.message || "Failed to load users"}
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
            User Management
          </h2>
          <p className="text-slate-500 dark:text-slate-400 font-medium">
            Control user access and roles.
          </p>
        </div>
        <div className="flex gap-3">
          <Button variant="success">
            <Upload className="w-4 h-4" />
            Import Excel
          </Button>
          <Button variant="primary" onClick={handleCreate}>
            <UserPlus className="w-4 h-4" />
            Add User
          </Button>
        </div>
      </div>

      {/* Filters */}
      <UserFilters />

      {/* User Table */}
      <UserTable
        users={users || []}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />

      {/* Create/Edit Modal */}
      <UserFormModal
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        user={selectedUser}
        mode={formMode}
      />

      {/* Delete Confirmation Dialog */}
      <DeleteUserDialog
        isOpen={isDeleteOpen}
        onClose={() => setIsDeleteOpen(false)}
        user={selectedUser}
      />
    </div>
  );
};
