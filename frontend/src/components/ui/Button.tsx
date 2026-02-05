import React from 'react';
import { cn } from '@/lib/utils';

export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    variant?: 'primary' | 'secondary' | 'success' | 'ghost';
    size?: 'sm' | 'md' | 'lg';
    children: React.ReactNode;
}

export const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
    ({ className, variant = 'primary', size = 'md', children, ...props }, ref) => {
        const baseStyles = 'inline-flex items-center justify-center gap-2 rounded-lg font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-primary/50 disabled:opacity-50 disabled:pointer-events-none';

        const variantStyles = {
            primary: 'bg-primary hover:bg-primary-dark text-white shadow-sm shadow-primary/30',
            secondary: 'bg-white dark:bg-card-dark border border-gray-200 dark:border-gray-700 text-slate-700 dark:text-slate-200 hover:bg-gray-50 dark:hover:bg-gray-800 shadow-sm',
            success: 'bg-green-600 hover:bg-green-700 text-white shadow-sm',
            ghost: 'bg-gray-100 dark:bg-gray-700 text-slate-600 dark:text-slate-300 hover:bg-gray-200 dark:hover:bg-gray-600',
        };

        const sizeStyles = {
            sm: 'px-3 py-1.5 text-xs',
            md: 'px-4 py-2 text-sm',
            lg: 'px-6 py-3 text-base',
        };

        return (
            <button
                ref={ref}
                className={cn(baseStyles, variantStyles[variant], sizeStyles[size], className)}
                {...props}
            >
                {children}
            </button>
        );
    }
);

Button.displayName = 'Button';
