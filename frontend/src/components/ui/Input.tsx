import React from "react";
import { cn } from "@/lib/utils";
import type { LucideIcon } from "lucide-react";

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  icon?: LucideIcon;
  containerClassName?: string;
  label?: string;
  error?: string;
  helpText?: string;
}

export const Input = React.forwardRef<HTMLInputElement, InputProps>(
  (
    {
      className,
      icon: Icon,
      containerClassName,
      label,
      error,
      helpText,
      ...props
    },
    ref,
  ) => {
    const inputClasses = cn(
      "w-full px-4 py-2.5 bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-600 rounded-lg text-sm",
      "focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary",
      "placeholder:text-slate-400 text-slate-900 dark:text-white",
      "disabled:opacity-50 disabled:cursor-not-allowed",
      error && "border-red-500 focus:ring-red-500/50 focus:border-red-500",
      Icon && "pl-10",
      className,
    );

    const inputElement = Icon ? (
      <div className={cn("relative", containerClassName)}>
        <Icon className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 w-5 h-5" />
        <input ref={ref} className={inputClasses} {...props} />
      </div>
    ) : (
      <input ref={ref} className={inputClasses} {...props} />
    );

    if (label || error || helpText) {
      return (
        <div className="space-y-1.5">
          {label && (
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">
              {label}
            </label>
          )}
          {inputElement}
          {error && <p className="text-sm text-red-500">{error}</p>}
          {!error && helpText && (
            <p className="text-sm text-slate-500">{helpText}</p>
          )}
        </div>
      );
    }

    return inputElement;
  },
);

Input.displayName = "Input";
