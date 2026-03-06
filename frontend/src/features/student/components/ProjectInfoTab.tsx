import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { useProjectMembersList } from "../api/useProjects";
import type { ProjectDto } from "@/types";
import { Users } from "lucide-react";

interface ProjectInfoTabProps {
  project: ProjectDto;
}

export function ProjectInfoTab({ project }: ProjectInfoTabProps) {
  const { data: members = [], isLoading } = useProjectMembersList(
    project.projectId,
  );

  return (
    <div className="space-y-6">
      {/* Project Details */}
      <Card className="p-6">
        <h2 className="text-lg font-semibold mb-4">Project Details</h2>
        <div className="space-y-4">
          <div>
            <label className="text-sm font-medium text-gray-500">
              Project Name
            </label>
            <p className="text-gray-900">{project.name}</p>
          </div>
          <div>
            <label className="text-sm font-medium text-gray-500">
              Description
            </label>
            <p className="text-gray-900">
              {project.description || "No description"}
            </p>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="text-sm font-medium text-gray-500">
                Course
              </label>
              <p className="text-gray-900">
                {project.courseCode} - {project.courseName}
              </p>
            </div>
            <div>
              <label className="text-sm font-medium text-gray-500">Class</label>
              <p className="text-gray-900">{project.className}</p>
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="text-sm font-medium text-gray-500">
                Created At
              </label>
              <p className="text-gray-900">
                {new Date(project.createdAt).toLocaleDateString()}
              </p>
            </div>
            <div>
              <label className="text-sm font-medium text-gray-500">
                Updated At
              </label>
              <p className="text-gray-900">
                {new Date(project.updatedAt).toLocaleDateString()}
              </p>
            </div>
          </div>
        </div>
      </Card>

      {/* Team Members */}
      <Card className="p-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold">Team Members</h2>
          <Badge variant="secondary">{members.length} members</Badge>
        </div>
        {isLoading ? (
          <div className="text-center py-4 text-gray-500">
            Loading members...
          </div>
        ) : members.length === 0 ? (
          <div className="text-center py-8 text-gray-500">
            <Users className="w-12 h-12 mx-auto mb-2 text-gray-300" />
            <p>No team members yet</p>
          </div>
        ) : (
          <div className="space-y-3">
            {members.map((member) => (
              <div
                key={member.userId}
                className="flex items-center gap-3 p-3 border rounded-lg hover:bg-gray-50"
              >
                <div className="w-10 h-10 rounded-full bg-blue-100 flex items-center justify-center">
                  <span className="text-blue-600 font-medium">
                    {member.userName.charAt(0).toUpperCase()}
                  </span>
                </div>
                <div className="flex-1">
                  <p className="font-medium text-gray-900">{member.userName}</p>
                  <p className="text-sm text-gray-500">{member.userEmail}</p>
                </div>
                {member.githubUsername && (
                  <Badge variant="secondary">@{member.githubUsername}</Badge>
                )}
              </div>
            ))}
          </div>
        )}
      </Card>
    </div>
  );
}
