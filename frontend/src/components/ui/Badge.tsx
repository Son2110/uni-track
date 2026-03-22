import React from "react";
import { cn } from "@/lib/utils";

export type BadgeVariant =
  | "active"
  | "upcoming"
  | "archived"
  | "scheduled"
  | "full"
  | "info"
  | "secondary"
  | "success"
  | "warning";

export interface BadgeProps {
  variant: BadgeVariant;
  children: React.ReactNode;
  className?: string;
}

export const Badge: React.FC<BadgeProps> = ({
  variant,
  children,
  className,
}) => {
  const baseStyles =
    "inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium";

  const variantStyles: Record<BadgeVariant, string> = {
    active: "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200",
    upcoming: "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200",
    archived: "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300",
    scheduled:
      "bg-yellow-100 text-yellow-700 dark:bg-yellow-900 dark:text-yellow-300",
    full: "bg-gray-100 text-gray-600 dark:bg-gray-700 dark:text-gray-300",
    info: "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200",
    secondary: "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300",
    success:
      "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200",
    warning:
      "bg-yellow-100 text-yellow-700 dark:bg-yellow-900 dark:text-yellow-300",
  };

  return (
    <span className={cn(baseStyles, variantStyles[variant], className)}>
      {children}
    </span>
  );
};
