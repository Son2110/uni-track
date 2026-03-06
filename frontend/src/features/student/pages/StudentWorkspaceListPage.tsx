import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Search, FolderKanban, ArrowRight } from "lucide-react";
import { useUserProjects } from "../api/useProjects";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { useAuth } from "@/features/auth/context/AuthContext";
import type { ProjectMemberDto } from "@/types";

export function StudentWorkspaceListPage() {
  const { user } = useAuth();
  const [searchTerm, setSearchTerm] = useState("");
  const navigate = useNavigate();

  const { data: projectMemberships = [], isLoading } = useUserProjects(
    user?.userId || "",
  );

  const filteredProjects = projectMemberships.filter((membership) =>
    membership.projectName.toLowerCase().includes(searchTerm.toLowerCase()),
  );

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">My Workspace</h1>
        <p className="text-sm text-gray-500">
          View and manage your project workspaces
        </p>
      </div>

      {/* Search */}
      <div className="relative">
        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
        <input
          type="text"
          placeholder="Search projects..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>

      {/* Projects */}
      {isLoading ? (
        <div className="text-center py-12 text-gray-500">
          Loading workspace...
        </div>
      ) : filteredProjects.length === 0 ? (
        <div className="text-center py-12">
          <FolderKanban className="w-16 h-16 mx-auto mb-4 text-gray-300" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            {searchTerm ? "No projects found" : "No projects yet"}
          </h3>
          <p className="text-gray-500">
            {searchTerm
              ? "Try a different search term"
              : "You haven't joined any projects yet"}
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredProjects.map((membership) => (
            <ProjectWorkspaceCard
              key={membership.projectId}
              membership={membership}
              onClick={() =>
                navigate(`/student/workspace/${membership.projectId}`)
              }
            />
          ))}
        </div>
      )}
    </div>
  );
}

// ============================================
// PROJECT WORKSPACE CARD
// ============================================

interface ProjectWorkspaceCardProps {
  membership: ProjectMemberDto;
  onClick: () => void;
}

function ProjectWorkspaceCard({
  membership,
  onClick,
}: ProjectWorkspaceCardProps) {
  return (
    <Card
      className="p-4 hover:shadow-lg transition-shadow cursor-pointer group"
      onClick={onClick}
    >
      <div className="space-y-3">
        <div className="flex items-start justify-between">
          <div className="flex-1">
            <h3 className="font-semibold text-gray-900 group-hover:text-blue-600">
              {membership.projectName}
            </h3>
            <p className="text-xs text-gray-500 mt-1">
              Joined {new Date(membership.joinedAt).toLocaleDateString()}
            </p>
          </div>
          <ArrowRight className="w-5 h-5 text-gray-400 group-hover:text-blue-600 group-hover:translate-x-1 transition-all" />
        </div>

        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded-full bg-blue-100 flex items-center justify-center">
            <span className="text-blue-600 font-medium text-sm">
              {membership.projectName.charAt(0).toUpperCase()}
            </span>
          </div>
          <div className="flex-1">
            <p className="text-sm text-gray-600">{membership.userName}</p>
          </div>
        </div>

        {membership.githubUsername && (
          <Badge variant="secondary">@{membership.githubUsername}</Badge>
        )}
      </div>
    </Card>
  );
}
