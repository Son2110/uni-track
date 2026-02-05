import React from 'react';
import { Outlet } from 'react-router-dom';
import { GraduationCap } from 'lucide-react';

export const AuthLayout: React.FC = () => {
    return (
        <div className="min-h-screen flex flex-col md:flex-row">
            {/* Left Side - Blue Decorative Panel */}
            <div className="relative w-full md:w-[40%] bg-primary flex flex-col justify-center items-center p-12 text-white overflow-hidden">
                {/* Geometric Overlay Pattern */}
                <div
                    className="absolute inset-0"
                    style={{
                        backgroundImage: 'radial-gradient(circle at 2px 2px, rgba(255,255,255,0.05) 1px, transparent 0)',
                        backgroundSize: '40px 40px'
                    }}
                />

                {/* Blur Blobs for Visual Interest */}
                <div
                    className="absolute -top-20 -left-20 w-[300px] h-[300px] bg-white/10 rounded-full"
                    style={{ filter: 'blur(80px)' }}
                />
                <div
                    className="absolute -bottom-20 -right-20 w-[300px] h-[300px] bg-white/10 rounded-full"
                    style={{ filter: 'blur(80px)' }}
                />

                {/* Logo and Branding */}
                <div className="relative z-10 flex flex-col items-center text-center space-y-6">
                    <div className="flex items-center space-x-3">
                        <div className="w-12 h-12 bg-white rounded-xl flex items-center justify-center">
                            <GraduationCap className="text-primary w-8 h-8" />
                        </div>
                        <h1 className="text-4xl font-bold tracking-tight">UniTrack</h1>
                    </div>
                    <div className="h-1.5 w-16 bg-fpt-orange rounded-full" />
                    <p className="text-xl font-medium text-blue-100 max-w-xs">
                        Empowering Education Through Innovation
                    </p>
                </div>
            </div>

            {/* Right Side - Auth Content */}
            <div className="w-full md:w-[60%] bg-background-light dark:bg-background-dark flex flex-col justify-center items-center p-8 md:p-12">
                <Outlet />
            </div>
        </div>
    );
};
