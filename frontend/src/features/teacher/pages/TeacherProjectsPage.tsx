import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Search, FolderKanban, ArrowRight, Users } from "lucide-react";
import { useProjects } from "@/features/student/api/useProjects";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import type { ProjectDto } from "@/types";

export function TeacherProjectsPage() {
  const [searchTerm, setSearchTerm] = useState("");
  const navigate = useNavigate();

  // Get all projects - can be filtered by class if needed
  const { data: projects = [], isLoading } = useProjects();

  const filteredProjects = projects.filter((project) =>
    [project.name, project.courseName, project.courseCode, project.className]
      .join(" ")
      .toLowerCase()
      .includes(searchTerm.toLowerCase()),
  );

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Student Projects</h1>
        <p className="text-sm text-gray-500">
          View and monitor projects from your classes
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

      {/* Projects List */}
      {isLoading ? (
        <div className="text-center py-8 text-gray-500">Loading...</div>
      ) : filteredProjects.length === 0 ? (
        <div className="text-center py-12">
          <FolderKanban className="w-16 h-16 mx-auto mb-4 text-gray-300" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            {searchTerm ? "No projects found" : "No student projects yet"}
          </h3>
          <p className="text-gray-500">
            {searchTerm
              ? "Try a different search term"
              : "Students haven't created any projects in your classes yet"}
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredProjects.map((project) => (
            <ProjectCard
              key={project.projectId}
              project={project}
              onClick={() => navigate(`/teacher/projects/${project.projectId}`)}
            />
          ))}
        </div>
      )}
    </div>
  );
}

// ============================================
// PROJECT CARD
// ============================================

interface ProjectCardProps {
  project: ProjectDto;
  onClick: () => void;
}

function ProjectCard({ project, onClick }: ProjectCardProps) {
  return (
    <Card
      onClick={onClick}
      className="p-4 hover:shadow-lg transition-shadow cursor-pointer"
    >
      <div className="space-y-3">
        <div className="flex justify-between items-start">
          <div className="flex-1">
            <h3 className="font-semibold text-gray-900">{project.name}</h3>
            <p className="text-sm text-gray-500">
              {project.courseCode} - {project.courseName}
            </p>
          </div>
          <FolderKanban className="w-5 h-5 text-gray-400" />
        </div>

        {project.description && (
          <p className="text-sm text-gray-600 line-clamp-2">
            {project.description}
          </p>
        )}

        <div className="space-y-2">
          <div className="flex items-center gap-2">
            <Badge variant="info">{project.className}</Badge>
          </div>
          <div className="text-xs text-gray-500">
            Created: {new Date(project.createdAt).toLocaleDateString()}
          </div>
        </div>

        <div className="pt-3 border-t flex items-center justify-between text-sm">
          <span className="text-gray-600 flex items-center gap-1">
            <Users className="w-4 h-4" />
            View Details
          </span>
          <ArrowRight className="w-4 h-4 text-gray-400" />
        </div>
      </div>
    </Card>
  );
}
