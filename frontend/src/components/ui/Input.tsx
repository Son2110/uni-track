import React from 'react';
import { cn } from '@/lib/utils';
import type { LucideIcon } from 'lucide-react';

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
    icon?: LucideIcon;
    containerClassName?: string;
}

export const Input = React.forwardRef<HTMLInputElement, InputProps>(
    ({ className, icon: Icon, containerClassName, ...props }, ref) => {
        if (Icon) {
            return (
                <div className={cn('relative', containerClassName)}>
                    <Icon className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5" />
                    <input
                        ref={ref}
                        className={cn(
                            'w-full pl-10 pr-4 py-2 bg-gray-50 dark:bg-gray-800 border-none rounded-lg text-sm focus:ring-2 focus:ring-primary/50 placeholder-gray-400',
                            className
                        )}
                        {...props}
                    />
                </div>
            );
        }

        return (
            <input
                ref={ref}
                className={cn(
                    'w-full px-4 py-2 bg-gray-50 dark:bg-gray-800 border-none rounded-lg text-sm focus:ring-2 focus:ring-primary/50 placeholder-gray-400',
                    className
                )}
                {...props}
            />
        );
    }
);

Input.displayName = 'Input';
