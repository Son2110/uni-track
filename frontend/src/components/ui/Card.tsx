import React from "react";
import { cn } from "@/lib/utils";

export interface CardProps {
  children: React.ReactNode;
  className?: string;
  onClick?: () => void;
}

export const Card: React.FC<CardProps> = ({ children, className, onClick }) => {
  return (
    <div
      className={cn(
        "bg-card-light dark:bg-card-dark rounded-xl shadow-sm border border-gray-100 dark:border-gray-800",
        onClick && "cursor-pointer transition-all hover:shadow-md",
        className,
      )}
      onClick={onClick}
    >
      {children}
    </div>
  );
};

export const CardHeader: React.FC<CardProps> = ({ children, className }) => {
  return <div className={cn("p-6", className)}>{children}</div>;
};

export const CardContent: React.FC<CardProps> = ({ children, className }) => {
  return <div className={cn("p-6", className)}>{children}</div>;
};

export const CardFooter: React.FC<CardProps> = ({ children, className }) => {
  return <div className={cn("p-6 pt-0", className)}>{children}</div>;
};
