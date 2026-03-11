import { Outlet } from "react-router-dom";
import { StudentSidebar } from "@/components/StudentSidebar";
import { Header } from "@/components/Header";

export function StudentLayout() {
  return (
    <div className="flex min-h-screen bg-gray-50">
      <StudentSidebar />
      <div className="flex-1 flex flex-col">
        <Header />
        <main className="flex-1">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
