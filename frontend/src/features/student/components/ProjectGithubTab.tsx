import { useState } from "react";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";
import { Modal } from "@/components/ui/Modal";
import { Input } from "@/components/ui/Input";
import {
  useGithubRepos,
  useProjectGithubContributions,
  useCreateGithubRepo,
  useUpdateGithubRepo,
  useDeleteGithubRepo,
  useSyncProjectGithub,
} from "../api/useProjects";
import {
  Github,
  ExternalLink,
  Plus,
  Trash2,
  GitBranch,
  RefreshCw,
  Pencil,
} from "lucide-react";
import { useAuth } from "@/features/auth/context/AuthContext";
import type { GithubRepoDto } from "@/types";
import { CommitsOverTimeChart } from "./CommitsOverTimeChart";
import { ContributorCard } from "./ContributorCard";

interface ProjectGithubTabProps {
  projectId: string;
  readOnly?: boolean;
}

export function ProjectGithubTab({
  projectId,
  readOnly = false,
}: ProjectGithubTabProps) {
  const { user } = useAuth();
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [editingRepo, setEditingRepo] = useState<GithubRepoDto | null>(null);

  const { data: repos = [], isLoading } = useGithubRepos({ projectId });
  // Only fetch contributions if there are repos
  const { data: contributions } = useProjectGithubContributions(
    projectId,
    repos.length > 0,
  );
  const syncMutation = useSyncProjectGithub();
  const deleteMutation = useDeleteGithubRepo();

  const handleEdit = (repo: GithubRepoDto) => {
    setEditingRepo(repo);
  };

  const handleDelete = async (repoId: string) => {
    if (!user) return;
    if (!confirm("Are you sure you want to delete this repository connection?"))
      return;

    try {
      await deleteMutation.mutateAsync({ repoId, userId: user.userId });
    } catch (error) {
      console.error("Failed to delete repo:", error);
    }
  };
  const handleSync = async () => {
    try {
      const result = await syncMutation.mutateAsync(projectId);
      console.log("Sync result:", result);
      alert(
        `Sync completed! ${result?.successfulSyncs || 0}/${result?.totalRepositories || 0} repositories synced successfully.`,
      );
    } catch (error: any) {
      console.error("Failed to sync GitHub data:", error);
      alert(
        `Failed to sync GitHub data: ${error.response?.data?.message || error.message || "Unknown error"}`,
      );
    }
  };

  return (
    <div className="space-y-6">
      {/* Overall Commits Over Time Chart */}
      {contributions &&
        contributions.overallCommitsOverTime &&
        contributions.overallCommitsOverTime.length > 0 && (
          <CommitsOverTimeChart
            data={contributions.overallCommitsOverTime}
            title="Commits over time"
            subtitle={`Weekly from ${new Date(contributions.semesterStartDate).toLocaleDateString()} to ${new Date(contributions.semesterEndDate).toLocaleDateString()}`}
          />
        )}

      {/* Summary Stats Cards */}
      {contributions && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <Card className="p-4 bg-white border-gray-200">
            <div className="flex items-center gap-3">
              <div className="w-12 h-12 rounded-lg bg-blue-100 flex items-center justify-center">
                <GitBranch className="w-6 h-6 text-blue-600" />
              </div>
              <div>
                <p className="text-sm text-gray-500">Total Commits</p>
                <p className="text-2xl font-bold text-gray-900">
                  {contributions.totalCommitsInSemester}
                </p>
              </div>
            </div>
          </Card>

          <Card className="p-4 bg-white border-gray-200">
            <div className="flex items-center gap-3">
              <div className="w-12 h-12 rounded-lg bg-green-100 flex items-center justify-center">
                <span className="text-green-600 font-bold text-lg">+</span>
              </div>
              <div>
                <p className="text-sm text-gray-500">Additions</p>
                <p className="text-2xl font-bold text-green-600">
                  {contributions.totalAdditionsInSemester.toLocaleString()}
                </p>
              </div>
            </div>
          </Card>

          <Card className="p-4 bg-white border-gray-200">
            <div className="flex items-center gap-3">
              <div className="w-12 h-12 rounded-lg bg-red-100 flex items-center justify-center">
                <span className="text-red-600 font-bold text-lg">-</span>
              </div>
              <div>
                <p className="text-sm text-gray-500">Deletions</p>
                <p className="text-2xl font-bold text-red-600">
                  {contributions.totalDeletionsInSemester.toLocaleString()}
                </p>
              </div>
            </div>
          </Card>
        </div>
      )}

      {/* GitHub Repositories */}
      <Card className="p-6 bg-white border-gray-200">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold text-gray-900">
            GitHub Repositories
          </h2>
          <div className="flex gap-2">
            {repos.length > 0 && (
              <Button
                variant="outline"
                onClick={handleSync}
                isLoading={syncMutation.isPending}
              >
                <RefreshCw className="w-4 h-4 mr-2" />
                Sync Data
              </Button>
            )}
            {!readOnly && (
              <Button onClick={() => setShowCreateModal(true)}>
                <Plus className="w-4 h-4 mr-2" />
                Add Repository
              </Button>
            )}
          </div>
        </div>

        {isLoading ? (
          <div className="text-center py-8 text-gray-500">
            Loading repositories...
          </div>
        ) : repos.length === 0 ? (
          <div className="text-center py-12">
            <Github className="w-16 h-16 mx-auto mb-4 text-gray-300" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">
              No Repositories Connected
            </h3>
            <p className="text-gray-500 mb-4">
              Connect a GitHub repository to track your project's development.
            </p>
            {!readOnly && (
              <Button onClick={() => setShowCreateModal(true)}>
                <Plus className="w-4 h-4 mr-2" />
                Add Repository
              </Button>
            )}
          </div>
        ) : (
          <div className="space-y-3">
            {repos.map((repo) => (
              <GithubRepoCard
                key={repo.githubRepoId}
                repo={repo}
                onEdit={handleEdit}
                onDelete={handleDelete}
                readOnly={readOnly}
              />
            ))}
          </div>
        )}
      </Card>

      {/* Contributors */}
      {contributions &&
        contributions.contributors &&
        contributions.contributors.length > 0 && (
          <div className="space-y-3">
            {contributions.contributors.map((contributor, index) => (
              <ContributorCard
                key={contributor.githubUsername}
                contributor={contributor}
                rank={index + 1}
              />
            ))}
          </div>
        )}

      {/* Create Repo Modal */}
      <CreateRepoModal
        isOpen={showCreateModal}
        onClose={() => setShowCreateModal(false)}
        projectId={projectId}
      />

      {/* Edit Repo Modal */}
      {editingRepo && (
        <EditRepoModal
          isOpen={true}
          onClose={() => setEditingRepo(null)}
          repo={editingRepo}
        />
      )}
    </div>
  );
}

// ============================================
// GITHUB REPO CARD
// ============================================

interface GithubRepoCardProps {
  repo: GithubRepoDto;
  onEdit: (repo: GithubRepoDto) => void;
  onDelete: (repoId: string) => void;
  readOnly?: boolean;
}

function GithubRepoCard({
  repo,
  onEdit,
  onDelete,
  readOnly = false,
}: GithubRepoCardProps) {
  return (
    <div className="flex items-center gap-4 p-4 border border-gray-200 bg-white rounded-lg hover:bg-gray-50">
      <div className="w-12 h-12 rounded-lg bg-gray-100 flex items-center justify-center">
        <Github className="w-6 h-6 text-gray-700" />
      </div>

      <div className="flex-1">
        <div className="flex items-center gap-2">
          <p className="font-medium text-gray-900">
            {repo.repoOwnerName}/{repo.repoName}
          </p>
          <Badge variant={repo.isPrivate ? "warning" : "secondary"}>
            {repo.isPrivate ? "Private" : "Public"}
          </Badge>
        </div>
        <div className="flex items-center gap-4 mt-1 text-sm text-gray-500">
          <span>Added {new Date(repo.createdAt).toLocaleDateString()}</span>
        </div>
      </div>

      <div className="flex items-center gap-2">
        <a
          href={repo.repoUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="text-gray-500 hover:text-gray-700"
        >
          <ExternalLink className="w-5 h-5" />
        </a>
        {!readOnly && (
          <>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onEdit(repo)}
              className="hover:bg-blue-50"
            >
              <Pencil className="w-4 h-4 text-blue-500" />
            </Button>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onDelete(repo.githubRepoId)}
              className="hover:bg-red-50"
            >
              <Trash2 className="w-4 h-4 text-red-500" />
            </Button>
          </>
        )}
      </div>
    </div>
  );
}

// ============================================
// CREATE REPO MODAL
// ============================================

interface CreateRepoModalProps {
  isOpen: boolean;
  onClose: () => void;
  projectId: string;
}

function CreateRepoModal({ isOpen, onClose, projectId }: CreateRepoModalProps) {
  const { user } = useAuth();
  const [formData, setFormData] = useState({
    repoOwnerName: "",
    repoName: "",
    isPrivate: false,
    apiToken: "",
  });

  const createMutation = useCreateGithubRepo();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user) return;

    try {
      await createMutation.mutateAsync({
        projectId,
        userId: user.userId,
        ...formData,
      });
      setFormData({
        repoOwnerName: "",
        repoName: "",
        isPrivate: false,
        apiToken: "",
      });
      onClose();
    } catch (error) {
      console.error("Failed to create repo:", error);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Add GitHub Repository">
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="p-3 bg-blue-50 border border-blue-200 rounded-lg text-sm">
          <p className="font-medium text-blue-900 mb-1">
            📌 GitHub Token Required
          </p>
          <p className="text-blue-800 mb-2">
            Get your token at:{" "}
            <a
              href="https://github.com/settings/tokens/new?description=PMSS&scopes=repo"
              target="_blank"
              rel="noopener noreferrer"
              className="underline font-medium hover:text-blue-900"
            >
              github.com/settings/tokens
            </a>
          </p>
          <p className="text-xs text-blue-700">
            Generate token → Select{" "}
            <code className="px-1 bg-blue-100 rounded">repo</code> scope → Copy
            token (starts with{" "}
            <code className="px-1 bg-blue-100 rounded">ghp_</code>)
          </p>
        </div>

        <Input
          label="Repository Owner"
          required
          value={formData.repoOwnerName}
          onChange={(e) =>
            setFormData({ ...formData, repoOwnerName: e.target.value })
          }
          placeholder="username or organization"
        />

        <Input
          label="Repository Name"
          required
          value={formData.repoName}
          onChange={(e) =>
            setFormData({ ...formData, repoName: e.target.value })
          }
          placeholder="repository-name"
        />

        <div className="flex items-center gap-2">
          <input
            type="checkbox"
            id="isPrivate"
            checked={formData.isPrivate}
            onChange={(e) =>
              setFormData({ ...formData, isPrivate: e.target.checked })
            }
            className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
          />
          <label htmlFor="isPrivate" className="text-sm text-gray-700">
            Private Repository
          </label>
        </div>

        <Input
          label="GitHub API Token"
          type="password"
          required
          value={formData.apiToken}
          onChange={(e) =>
            setFormData({ ...formData, apiToken: e.target.value })
          }
          placeholder="ghp_..."
          helpText="Required for syncing data. Get your token at: github.com/settings/tokens"
        />

        <div className="flex justify-end gap-2 pt-4">
          <Button type="button" variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" isLoading={createMutation.isPending}>
            Add Repository
          </Button>
        </div>
      </form>
    </Modal>
  );
}

// ============================================
// EDIT REPO MODAL
// ============================================

interface EditRepoModalProps {
  isOpen: boolean;
  onClose: () => void;
  repo: GithubRepoDto;
}

function EditRepoModal({ isOpen, onClose, repo }: EditRepoModalProps) {
  const { user } = useAuth();
  const [formData, setFormData] = useState({
    repoOwnerName: repo.repoOwnerName,
    repoName: repo.repoName,
    isPrivate: repo.isPrivate,
    apiToken: "",
  });

  const updateMutation = useUpdateGithubRepo();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user) return;

    try {
      await updateMutation.mutateAsync({
        repoId: repo.githubRepoId,
        userId: user.userId,
        ...formData,
      });
      onClose();
    } catch (error) {
      console.error("Failed to update repo:", error);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Edit GitHub Repository">
      <form onSubmit={handleSubmit} className="space-y-4">
        <Input
          label="Repository Owner"
          required
          value={formData.repoOwnerName}
          onChange={(e) =>
            setFormData({ ...formData, repoOwnerName: e.target.value })
          }
          placeholder="username or organization"
        />

        <Input
          label="Repository Name"
          required
          value={formData.repoName}
          onChange={(e) =>
            setFormData({ ...formData, repoName: e.target.value })
          }
          placeholder="repository-name"
        />

        <div className="flex items-center gap-2">
          <input
            type="checkbox"
            id="editIsPrivate"
            checked={formData.isPrivate}
            onChange={(e) =>
              setFormData({ ...formData, isPrivate: e.target.checked })
            }
            className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
          />
          <label htmlFor="editIsPrivate" className="text-sm text-gray-700">
            Private Repository
          </label>
        </div>

        <Input
          label="GitHub API Token (Optional)"
          type="password"
          value={formData.apiToken}
          onChange={(e) =>
            setFormData({ ...formData, apiToken: e.target.value })
          }
          placeholder="ghp_..."
          helpText="Leave empty to keep current token"
        />

        <div className="flex justify-end gap-2 pt-4">
          <Button type="button" variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" isLoading={updateMutation.isPending}>
            Save Changes
          </Button>
        </div>
      </form>
    </Modal>
  );
}
