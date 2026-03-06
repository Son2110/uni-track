import { useState, useRef, useEffect } from "react";
import { LogOut, User, ChevronDown } from "lucide-react";
import { useAuth } from "@/features/auth/context/AuthContext";

/**
 * User Menu Component
 * Dropdown menu with user info and logout button
 */

export function UserMenu() {
  const [isOpen, setIsOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
  const { user, logout } = useAuth();
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  // Close menu when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [isOpen]);

  const handleLogout = () => {
    setIsLoggingOut(true);
    logout();
  };

  if (!user) return null;

  return (
    <div className="relative" ref={menuRef}>
      {/* User Menu Trigger Button */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
      >
        {/* User Avatar */}
        <div className="w-8 h-8 rounded-full bg-primary text-white flex items-center justify-center font-semibold text-sm">
          {user.name.charAt(0).toUpperCase()}
        </div>

        {/* User Info - Hidden on mobile */}
        <div className="hidden md:block text-left">
          <div className="text-sm font-semibold text-gray-900 dark:text-white">
            {user.name}
          </div>
          <div className="text-xs text-gray-500 dark:text-gray-400">
            {user.role}
          </div>
        </div>

        {/* Chevron Icon */}
        <ChevronDown
          className={`w-4 h-4 text-gray-500 transition-transform ${
            isOpen ? "rotate-180" : ""
          }`}
        />
      </button>

      {/* Dropdown Menu */}
      {isOpen && (
        <div className="absolute right-0 mt-2 w-56 bg-white dark:bg-gray-800 rounded-lg shadow-lg border border-gray-200 dark:border-gray-700 py-2 z-50">
          {/* User Info (mobile) */}
          <div className="md:hidden px-4 py-3 border-b border-gray-200 dark:border-gray-700">
            <div className="text-sm font-semibold text-gray-900 dark:text-white">
              {user.name}
            </div>
            <div className="text-xs text-gray-500 dark:text-gray-400">
              {user.email}
            </div>
            <div className="mt-1 inline-block px-2 py-0.5 bg-primary/10 text-primary text-xs rounded">
              {user.role}
            </div>
          </div>

          {/* User Info (desktop) */}
          <div className="hidden md:block px-4 py-3 border-b border-gray-200 dark:border-gray-700">
            <div className="text-xs text-gray-500 dark:text-gray-400">
              {user.email}
            </div>
          </div>

          {/* Menu Items */}
          <div className="py-1">
            {/* Profile Link */}
            <button
              onClick={() => {
                setIsOpen(false);
                // Navigate to profile
              }}
              className="w-full flex items-center gap-3 px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
            >
              <User className="w-4 h-4" />
              <span>Profile</span>
            </button>

            {/* Logout Button */}
            <button
              onClick={handleLogout}
              disabled={isLoggingOut}
              className="w-full flex items-center gap-3 px-4 py-2 text-sm text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <LogOut className="w-4 h-4" />
              <span>{isLoggingOut ? "Logging out..." : "Logout"}</span>
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
