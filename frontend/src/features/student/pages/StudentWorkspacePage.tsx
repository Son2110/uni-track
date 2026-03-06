import { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Info, CheckSquare, Github } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/Tabs";
import { useProject } from "../api/useProjects";
import { ProjectInfoTab } from "../components/ProjectInfoTab";
import { ProjectJiraTab } from "../components/ProjectJiraTab";
import { ProjectGithubTab } from "../components/ProjectGithubTab";

export function StudentWorkspacePage() {
  const { projectId } = useParams<{ projectId: string }>();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState("info");

  const { data: project, isLoading, error } = useProject(projectId || "");

  if (!projectId) {
    return (
      <div className="p-6">
        <div className="text-center py-12">
          <p className="text-gray-500">No project selected</p>
          <Button
            onClick={() => navigate("/student/workspace")}
            className="mt-4"
          >
            Go to Workspace
          </Button>
        </div>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="p-6">
        <div className="text-center py-12 text-gray-500">
          Loading project...
        </div>
      </div>
    );
  }

  if (error || !project) {
    return (
      <div className="p-6">
        <div className="text-center py-12">
          <p className="text-red-500 mb-4">Failed to load project</p>
          <Button onClick={() => navigate("/student/workspace")}>
            Go to Workspace
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => navigate("/student/workspace")}
        >
          <ArrowLeft className="w-4 h-4 mr-2" />
          Back
        </Button>
        <div className="flex-1">
          <h1 className="text-2xl font-bold text-gray-900">{project.name}</h1>
          <p className="text-sm text-gray-500">
            {project.courseCode} • {project.className}
          </p>
        </div>
      </div>

      {/* Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="info">
            <Info className="w-4 h-4 mr-2" />
            Info
          </TabsTrigger>
          <TabsTrigger value="jira">
            <CheckSquare className="w-4 h-4 mr-2" />
            Jira
          </TabsTrigger>
          <TabsTrigger value="github">
            <Github className="w-4 h-4 mr-2" />
            GitHub
          </TabsTrigger>
        </TabsList>

        <TabsContent value="info">
          <ProjectInfoTab project={project} />
        </TabsContent>

        <TabsContent value="jira">
          <ProjectJiraTab projectId={project.projectId} />
        </TabsContent>

        <TabsContent value="github">
          <ProjectGithubTab projectId={project.projectId} />
        </TabsContent>
      </Tabs>
    </div>
  );
}
