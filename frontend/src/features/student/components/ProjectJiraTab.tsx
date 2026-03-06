import { useState } from "react";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";
import { Modal } from "@/components/ui/Modal";
import { Input } from "@/components/ui/Input";
import {
  useJiraConfig,
  useCreateJiraConfig,
  useUpdateJiraConfig,
} from "../api/useProjects";
import { CheckSquare, ExternalLink, Settings } from "lucide-react";

interface ProjectJiraTabProps {
  projectId: string;
}

export function ProjectJiraTab({ projectId }: ProjectJiraTabProps) {
  const [showConfigModal, setShowConfigModal] = useState(false);
  const { data: config, isLoading, error } = useJiraConfig(projectId);

  const hasConfig = !!config && !error;

  return (
    <div className="space-y-6">
      {isLoading ? (
        <div className="text-center py-12 text-gray-500">
          Loading Jira configuration...
        </div>
      ) : !hasConfig ? (
        <Card className="p-12 text-center">
          <CheckSquare className="w-16 h-16 mx-auto mb-4 text-gray-300" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            No Jira Configuration
          </h3>
          <p className="text-gray-500 mb-4">
            This project hasn't been connected to Jira yet.
          </p>
          <Button onClick={() => setShowConfigModal(true)}>
            <Settings className="w-4 h-4 mr-2" />
            Configure Jira
          </Button>
        </Card>
      ) : (
        <>
          {/* Jira Config Details */}
          <Card className="p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">Jira Configuration</h2>
              <div className="flex items-center gap-2">
                <Badge variant={config.isActive ? "success" : "warning"}>
                  {config.isActive ? "Active" : "Inactive"}
                </Badge>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setShowConfigModal(true)}
                >
                  <Settings className="w-4 h-4 mr-2" />
                  Edit
                </Button>
              </div>
            </div>

            <div className="space-y-4">
              <div>
                <label className="text-sm font-medium text-gray-500">
                  Jira URL
                </label>
                <div className="flex items-center gap-2">
                  <p className="text-gray-900">{config.jiraUrl}</p>
                  <a
                    href={config.jiraUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-blue-600 hover:text-blue-700"
                  >
                    <ExternalLink className="w-4 h-4" />
                  </a>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-gray-500">
                    Project Key
                  </label>
                  <p className="text-gray-900 font-mono">{config.projectKey}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-500">
                    Email
                  </label>
                  <p className="text-gray-900">{config.email}</p>
                </div>
              </div>

              <div>
                <label className="text-sm font-medium text-gray-500">
                  API Token
                </label>
                <p className="text-gray-900 font-mono">
                  {config.apiTokenMasked}
                </p>
              </div>
            </div>
          </Card>

          {/* Quick Links */}
          <Card className="p-6">
            <h2 className="text-lg font-semibold mb-4">Quick Links</h2>
            <div className="space-y-2">
              <a
                href={`${config.jiraUrl}/projects/${config.projectKey}`}
                target="_blank"
                rel="noopener noreferrer"
                className="flex items-center gap-2 p-3 border rounded-lg hover:bg-gray-50"
              >
                <CheckSquare className="w-5 h-5 text-blue-600" />
                <span className="flex-1 text-gray-900">View Project Board</span>
                <ExternalLink className="w-4 h-4 text-gray-400" />
              </a>
              <a
                href={`${config.jiraUrl}/projects/${config.projectKey}/issues`}
                target="_blank"
                rel="noopener noreferrer"
                className="flex items-center gap-2 p-3 border rounded-lg hover:bg-gray-50"
              >
                <CheckSquare className="w-5 h-5 text-blue-600" />
                <span className="flex-1 text-gray-900">View Issues</span>
                <ExternalLink className="w-4 h-4 text-gray-400" />
              </a>
            </div>
          </Card>
        </>
      )}

      {/* Config Modal */}
      <JiraConfigModal
        isOpen={showConfigModal}
        onClose={() => setShowConfigModal(false)}
        projectId={projectId}
        existingConfig={hasConfig ? config : undefined}
      />
    </div>
  );
}

// ============================================
// JIRA CONFIG MODAL
// ============================================

interface JiraConfigModalProps {
  isOpen: boolean;
  onClose: () => void;
  projectId: string;
  existingConfig?: any;
}

function JiraConfigModal({
  isOpen,
  onClose,
  projectId,
  existingConfig,
}: JiraConfigModalProps) {
  const [formData, setFormData] = useState({
    jiraUrl: existingConfig?.jiraUrl || "",
    email: existingConfig?.email || "",
    apiToken: "",
    projectKey: existingConfig?.projectKey || "",
  });

  const createMutation = useCreateJiraConfig();
  const updateMutation = useUpdateJiraConfig();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      if (existingConfig) {
        await updateMutation.mutateAsync({
          projectId,
          ...formData,
        });
      } else {
        await createMutation.mutateAsync({
          projectId,
          ...formData,
        });
      }
      onClose();
    } catch (error) {
      console.error("Failed to save Jira config:", error);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={existingConfig ? "Edit Jira Configuration" : "Configure Jira"}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <Input
          label="Jira URL"
          type="url"
          required
          value={formData.jiraUrl}
          onChange={(e) =>
            setFormData({ ...formData, jiraUrl: e.target.value })
          }
          placeholder="https://your-domain.atlassian.net"
        />

        <Input
          label="Project Key"
          required
          value={formData.projectKey}
          onChange={(e) =>
            setFormData({
              ...formData,
              projectKey: e.target.value.toUpperCase(),
            })
          }
          placeholder="PROJ"
          pattern="[A-Z][A-Z0-9_]*"
          title="Must be uppercase letters, numbers, and underscores"
        />

        <Input
          label="Email"
          type="email"
          required
          value={formData.email}
          onChange={(e) => setFormData({ ...formData, email: e.target.value })}
          placeholder="your-email@example.com"
        />

        <Input
          label="API Token"
          type="password"
          required={!existingConfig}
          value={formData.apiToken}
          onChange={(e) =>
            setFormData({ ...formData, apiToken: e.target.value })
          }
          placeholder={
            existingConfig ? "Leave blank to keep existing" : "Your API token"
          }
        />

        <div className="flex justify-end gap-2 pt-4">
          <Button type="button" variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button
            type="submit"
            isLoading={createMutation.isPending || updateMutation.isPending}
          >
            {existingConfig ? "Update" : "Create"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
