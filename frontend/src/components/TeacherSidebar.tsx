import { NavLink, useNavigate } from "react-router-dom";
import {
  Calendar,
  BookOpen,
  GraduationCap,
  FolderKanban,
  Settings,
} from "lucide-react";
import { useAuth } from "@/features/auth/context/AuthContext";

export function TeacherSidebar() {
  const { user } = useAuth();
  const navigate = useNavigate();

  // Generate initials from name
  const getInitials = (name: string) => {
    return name
      .split(" ")
      .map((part) => part[0])
      .join("")
      .toUpperCase()
      .slice(0, 2);
  };

  const userInitials = user?.name ? getInitials(user.name) : "T";
  const navItems = [
    { to: "/teacher/semesters", icon: Calendar, label: "Semesters" },
    { to: "/teacher/courses", icon: BookOpen, label: "Courses" },
    { to: "/teacher/classes", icon: GraduationCap, label: "My Classes" },
    { to: "/teacher/projects", icon: FolderKanban, label: "Student Projects" },
    { to: "/teacher/settings", icon: Settings, label: "Settings" },
  ];

  return (
    <aside className="w-64 bg-white border-r border-gray-200 min-h-screen flex flex-col">
      {/* Logo */}
      <div className="p-6 border-b border-gray-200">
        <h1 className="text-xl font-bold text-primary">UniTrack</h1>
        <p className="text-xs text-gray-500 mt-1">Teacher Portal</p>
      </div>

      {/* Navigation */}
      <nav className="flex-1 p-4 space-y-1">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `flex items-center gap-3 px-4 py-3 rounded-lg transition-colors ${
                isActive
                  ? "bg-primary text-white"
                  : "text-gray-700 hover:bg-gray-100"
              }`
            }
          >
            <item.icon className="w-5 h-5" />
            <span className="font-medium">{item.label}</span>
          </NavLink>
        ))}
      </nav>

      {/* User Profile */}
      <div className="p-4 border-t border-gray-200">
        <button
          onClick={() => navigate("/teacher/settings")}
          className="w-full flex items-center gap-3 px-4 py-3 rounded-lg transition-colors cursor-pointer hover:bg-gray-50 text-left"
        >
          <div className="w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center text-primary font-bold text-sm flex-shrink-0">
            {userInitials}
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-gray-900 truncate">
              {user?.name || "Teacher"}
            </p>
            <p className="text-xs text-gray-500 truncate">
              {user?.email || ""}
            </p>
          </div>
        </button>
      </div>
    </aside>
  );
}
