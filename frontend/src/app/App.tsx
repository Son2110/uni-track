import { RouterProvider } from "react-router-dom";
import { AuthProvider } from "@/features/auth/context/AuthContext";
import { router } from "./router";

/**
 * App Component
 * Wraps the router with AuthProvider
 * This allows AuthProvider to use useNavigate hook
 */

export function App() {
  return (
    <AuthProvider>
      <RouterProvider router={router} />
    </AuthProvider>
  );
}
