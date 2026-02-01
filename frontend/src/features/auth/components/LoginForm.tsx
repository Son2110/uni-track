import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ChevronDown, KeyRound, Info } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { GoogleIcon } from '@/components/icons/GoogleIcon';
import type { LoginCredentials } from '@/types';

const CAMPUSES = [
    { id: 'hn', name: 'FPT University Hanoi', code: 'HN' },
    { id: 'hcm', name: 'FPT University Ho Chi Minh', code: 'HCM' },
    { id: 'dn', name: 'FPT University Da Nang', code: 'DN' },
    { id: 'ct', name: 'FPT University Can Tho', code: 'CT' },
    { id: 'qn', name: 'FPT University Quy Nhon', code: 'QN' },
];

export const LoginForm: React.FC = () => {
    const navigate = useNavigate();
    const [credentials, setCredentials] = useState<LoginCredentials>({
        campus: '',
        rememberMe: false,
    });

    const handleLogin = (method: 'google' | 'feid') => {
        // Mock login - in production, this would call authentication API
        console.log(`Logging in with ${method}`, credentials);

        // Navigate to admin dashboard
        navigate('/admin');
    };

    return (
        <div className="w-full max-w-md flex-grow flex flex-col justify-center space-y-8">
            {/* Header */}
            <div className="text-center">
                <h2 className="text-4xl font-bold text-gray-900 dark:text-white">
                    Welcome Back
                </h2>
                <p className="mt-2 text-gray-600 dark:text-gray-400">
                    Log in to UniTrack Academic Portal
                </p>
            </div>

            {/* Form Card */}
            <div className="bg-white dark:bg-gray-800 p-10 rounded-2xl shadow-xl shadow-slate-200/50 dark:shadow-none border border-slate-100 dark:border-gray-700">
                <form className="space-y-6" onSubmit={(e) => e.preventDefault()}>
                    {/* Campus Selection */}
                    <div>
                        <label
                            htmlFor="campus"
                            className="block text-sm font-semibold text-gray-700 dark:text-gray-300 mb-2"
                        >
                            Select Your Campus
                        </label>
                        <div className="relative">
                            <select
                                id="campus"
                                name="campus"
                                value={credentials.campus}
                                onChange={(e) => setCredentials({ ...credentials, campus: e.target.value })}
                                className="block w-full pl-4 pr-10 py-3 text-base border-gray-300 focus:outline-none focus:ring-primary focus:border-primary rounded-lg bg-gray-50 dark:bg-gray-900 dark:border-gray-600 dark:text-white appearance-none transition"
                            >
                                <option value="" disabled>
                                    Choose a campus
                                </option>
                                {CAMPUSES.map((campus) => (
                                    <option key={campus.id} value={campus.id}>
                                        {campus.name}
                                    </option>
                                ))}
                            </select>
                            <div className="pointer-events-none absolute inset-y-0 right-0 flex items-center px-2 text-gray-500">
                                <ChevronDown className="w-5 h-5" />
                            </div>
                        </div>
                    </div>

                    {/* Login Buttons */}
                    <div className="space-y-4 pt-2">
                        {/* Google Login */}
                        <button
                            type="button"
                            onClick={() => handleLogin('google')}
                            className="w-full flex items-center justify-center gap-3 px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg shadow-sm bg-white dark:bg-gray-700 text-gray-700 dark:text-white hover:bg-gray-50 dark:hover:bg-gray-600 transition-all font-medium"
                        >
                            <GoogleIcon />
                            Login with Google
                        </button>

                        {/* OR Divider */}
                        <div className="relative flex items-center py-2">
                            <div className="flex-grow border-t border-gray-200 dark:border-gray-700" />
                            <span className="flex-shrink mx-4 text-gray-400 text-xs font-semibold uppercase tracking-wider">
                                or
                            </span>
                            <div className="flex-grow border-t border-gray-200 dark:border-gray-700" />
                        </div>

                        {/* FeID Login */}
                        <Button
                            type="button"
                            variant="primary"
                            onClick={() => handleLogin('feid')}
                            className="w-full group"
                        >
                            <KeyRound className="w-5 h-5 group-hover:scale-110 transition" />
                            Login with FeID
                        </Button>
                    </div>

                    {/* Info Note */}
                    <div className="flex items-start gap-2 p-4 bg-blue-50 dark:bg-blue-900/20 rounded-lg">
                        <Info className="text-primary w-4 h-4 mt-0.5 flex-shrink-0" />
                        <p className="text-xs text-blue-800 dark:text-blue-300 leading-relaxed">
                            <strong>Note:</strong> For K19+ students, please ensure you use your{' '}
                            <strong>FeID</strong> account for authentication.
                        </p>
                    </div>
                </form>
            </div>

            {/* Footer */}
            <footer className="w-full mt-auto py-8">
                <div className="flex flex-col md:flex-row items-center justify-center gap-x-6 gap-y-4 text-xs text-gray-400 font-medium">
                    <span className="text-gray-500 dark:text-gray-400">
                        © 2026 UniTrack
                    </span>
                    <div className="flex gap-4">
                        <a
                            href="#"
                            className="hover:text-primary transition underline decoration-gray-200 dark:decoration-gray-700 underline-offset-4"
                        >
                            Support Center
                        </a>
                        <a
                            href="#"
                            className="hover:text-primary transition underline decoration-gray-200 dark:decoration-gray-700 underline-offset-4"
                        >
                            Privacy Policy
                        </a>
                        <a
                            href="#"
                            className="hover:text-primary transition underline decoration-gray-200 dark:decoration-gray-700 underline-offset-4"
                        >
                            Terms of Service
                        </a>
                    </div>
                </div>
            </footer>
        </div>
    );
};
