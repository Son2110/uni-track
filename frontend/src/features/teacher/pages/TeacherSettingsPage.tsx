import { useState, useEffect } from "react";
import { User, Mail, Github, Lock } from "lucide-react";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { Modal } from "@/components/ui/Modal";
import { useUser, useUpdateUser } from "@/features/users/api/useUsers";
import { useAuth } from "@/features/auth/context/AuthContext";

export function TeacherSettingsPage() {
  const { user: authUser } = useAuth();
  const { data: user, isLoading } = useUser(authUser?.userId || "");
  const updateMutation = useUpdateUser();

  const [formData, setFormData] = useState({
    name: "",
    email: "",
    githubUsername: "",
    githubEmail: "",
  });
  const [showPasswordModal, setShowPasswordModal] = useState(false);

  // Update form data when user data loads
  useEffect(() => {
    if (user) {
      setFormData({
        name: user.name,
        email: user.email,
        githubUsername: user.githubUsername || "",
        githubEmail: user.githubEmail || "",
      });
    }
  }, [user]);

  const handleSaveProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!authUser) return;

    try {
      await updateMutation.mutateAsync({
        id: authUser.userId,
        data: formData,
      });
      alert("Profile updated successfully!");
    } catch (error) {
      console.error("Failed to update profile:", error);
      alert("Failed to update profile");
    }
  };

  if (isLoading) {
    return (
      <div className="p-6">
        <p>Loading...</p>
      </div>
    );
  }

  if (!user) {
    return (
      <div className="p-6">
        <p>User not found</p>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6 max-w-4xl">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Settings</h1>
        <p className="text-sm text-gray-500">Manage your account settings</p>
      </div>

      {/* Profile Information */}
      <Card className="p-6">
        <h2 className="text-lg font-semibold mb-4">Profile Information</h2>
        <form onSubmit={handleSaveProfile} className="space-y-4">
          <div className="flex items-center gap-4 mb-6">
            <div className="w-16 h-16 rounded-full bg-primary/10 flex items-center justify-center">
              <User className="w-8 h-8 text-primary" />
            </div>
            <div>
              <p className="font-medium text-gray-900">{user.name}</p>
              <p className="text-sm text-gray-500">{user.role}</p>
            </div>
          </div>

          <Input
            label="Full Name"
            icon={User}
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            required
          />

          <Input
            label="Email"
            type="email"
            icon={Mail}
            value={formData.email}
            onChange={(e) =>
              setFormData({ ...formData, email: e.target.value })
            }
            required
          />

          <Input
            label="GitHub Username"
            icon={Github}
            value={formData.githubUsername}
            onChange={(e) =>
              setFormData({ ...formData, githubUsername: e.target.value })
            }
            placeholder="your-username"
          />

          <Input
            label="GitHub Email"
            type="email"
            icon={Mail}
            value={formData.githubEmail}
            onChange={(e) =>
              setFormData({ ...formData, githubEmail: e.target.value })
            }
            placeholder="github@example.com"
          />

          <div className="flex justify-end gap-2 pt-4">
            <Button type="submit">Save Changes</Button>
          </div>
        </form>
      </Card>

      {/* Security */}
      <Card className="p-6">
        <h2 className="text-lg font-semibold mb-4">Security</h2>
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="font-medium text-gray-900">Password</p>
              <p className="text-sm text-gray-500">
                Change your password to keep your account secure
              </p>
            </div>
            <Button
              variant="outline"
              onClick={() => setShowPasswordModal(true)}
            >
              <Lock className="w-4 h-4 mr-2" />
              Change Password
            </Button>
          </div>
        </div>
      </Card>

      {/* Password Change Modal */}
      {showPasswordModal && (
        <PasswordChangeModal onClose={() => setShowPasswordModal(false)} />
      )}
    </div>
  );
}

// ============================================
// PASSWORD CHANGE MODAL
// ============================================

interface PasswordChangeModalProps {
  onClose: () => void;
}

function PasswordChangeModal({ onClose }: PasswordChangeModalProps) {
  const [formData, setFormData] = useState({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (formData.newPassword !== formData.confirmPassword) {
      alert("New passwords do not match!");
      return;
    }

    setIsSubmitting(true);

    try {
      // TODO: Implement password change API call
      console.log("Changing password");
      await new Promise((resolve) => setTimeout(resolve, 1000));
      alert("Password changed successfully!");
      onClose();
    } catch (error) {
      console.error("Failed to change password:", error);
      alert("Failed to change password");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal isOpen={true} onClose={onClose} title="Change Password">
      <form onSubmit={handleSubmit} className="space-y-4">
        <Input
          label="Current Password"
          type="password"
          value={formData.currentPassword}
          onChange={(e) =>
            setFormData({ ...formData, currentPassword: e.target.value })
          }
          required
        />

        <Input
          label="New Password"
          type="password"
          value={formData.newPassword}
          onChange={(e) =>
            setFormData({ ...formData, newPassword: e.target.value })
          }
          required
        />

        <Input
          label="Confirm New Password"
          type="password"
          value={formData.confirmPassword}
          onChange={(e) =>
            setFormData({ ...formData, confirmPassword: e.target.value })
          }
          required
        />

        <div className="flex justify-end gap-2 pt-4">
          <Button type="button" variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" isLoading={isSubmitting}>
            Change Password
          </Button>
        </div>
      </form>
    </Modal>
  );
}
