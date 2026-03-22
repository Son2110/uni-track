import React from "react";
import { cn } from "@/lib/utils";

interface TextareaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string;
  error?: string;
}

export const Textarea = React.forwardRef<HTMLTextAreaElement, TextareaProps>(
  ({ label, error, className, ...props }, ref) => {
    return (
      <div className="space-y-1.5">
        {label && (
          <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">
            {label}
          </label>
        )}
        <textarea
          ref={ref}
          className={cn(
            "w-full px-4 py-2.5 text-sm rounded-lg resize-none",
            "bg-white dark:bg-slate-800",
            "border border-slate-300 dark:border-slate-600",
            "text-slate-900 dark:text-white placeholder:text-slate-400",
            "focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary",
            "disabled:opacity-50 disabled:cursor-not-allowed",
            error &&
              "border-red-500 focus:ring-red-500/50 focus:border-red-500",
            className,
          )}
          {...props}
        />
        {error && <p className="text-sm text-red-500">{error}</p>}
      </div>
    );
  },
);

Textarea.displayName = "Textarea";
