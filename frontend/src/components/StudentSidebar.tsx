import { NavLink } from "react-router-dom";
import { BookOpen, FolderKanban, Settings, User } from "lucide-react";

export function StudentSidebar() {
  const navItems = [
    { to: "/student/classes", icon: BookOpen, label: "My Classes" },
    { to: "/student/workspace", icon: FolderKanban, label: "Workspace" },
    { to: "/student/settings", icon: Settings, label: "Settings" },
  ];

  return (
    <aside className="w-64 bg-white border-r border-gray-200 min-h-screen flex flex-col">
      {/* Logo */}
      <div className="p-6 border-b border-gray-200">
        <h1 className="text-xl font-bold text-primary">UniTrack</h1>
        <p className="text-xs text-gray-500 mt-1">Student Portal</p>
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
        <div className="flex items-center gap-3 px-4 py-3">
          <div className="w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center">
            <User className="w-5 h-5 text-primary" />
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-gray-900 truncate">
              Student Name
            </p>
            <p className="text-xs text-gray-500">Student</p>
          </div>
        </div>
      </div>
    </aside>
  );
}
