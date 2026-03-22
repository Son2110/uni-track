import { Outlet } from "react-router-dom";
import { TeacherSidebar } from "@/components/TeacherSidebar";
import { Header } from "@/components/Header";

export function TeacherLayout() {
  return (
    <div className="flex min-h-screen bg-gray-50">
      <TeacherSidebar />
      <div className="flex-1 flex flex-col">
        <Header />
        <main className="flex-1">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
