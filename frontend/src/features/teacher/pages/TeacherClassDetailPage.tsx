import { useParams, useNavigate } from "react-router-dom";
import { useState } from "react";
import { ArrowLeft, BookOpen, Users, FolderKanban } from "lucide-react";
import { useClass } from "@/features/academic/api/useClasses";
import { useClassEnrollments } from "@/features/student/api/useEnrollments";
import { useProjects } from "@/features/student/api/useProjects";
import { Button } from "@/components/ui/Button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/Tabs";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";

export function TeacherClassDetailPage() {
  const { classId } = useParams<{ classId: string }>();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState("info");

  const {
    data: classData,
    isLoading: isClassLoading,
    error: classError,
  } = useClass(classId || "");
  const { data: enrollments, isLoading: isEnrollmentsLoading } =
    useClassEnrollments(classId || "");
  const { data: allProjects = [] } = useProjects({ classId });

  if (!classId) {
    return (
      <div className="p-6">
        <p className="text-red-500">Invalid class ID</p>
      </div>
    );
  }

  if (isClassLoading || isEnrollmentsLoading) {
    return (
      <div className="p-6">
        <div className="text-center py-12 text-gray-500">
          Loading class details...
        </div>
      </div>
    );
  }

  if (classError || !classData) {
    return (
      <div className="p-6">
        <div className="text-center py-12">
          <p className="text-red-500">Failed to load class details</p>
          <Button onClick={() => navigate("/teacher/classes")} className="mt-4">
            Back to Classes
          </Button>
        </div>
      </div>
    );
  }

  const studentCount = enrollments?.length || 0;

  return (
    <div className="p-6 space-y-6">
      {/* Header with Back Button */}
      <div className="flex items-center gap-4">
        <Button variant="outline" onClick={() => navigate("/teacher/classes")}>
          <ArrowLeft className="w-4 h-4 mr-2" />
          Back
        </Button>
        <div className="flex-1">
          <h1 className="text-2xl font-bold text-gray-900">
            {classData.classCode}
          </h1>
          <div className="flex items-center gap-4 text-sm text-gray-500 mt-1">
            {classData.course && (
              <span>
                {classData.course.code} - {classData.course.name}
              </span>
            )}
            {classData.semester && <span>• {classData.semester.name}</span>}
            <span>
              • {studentCount} {studentCount === 1 ? "student" : "students"}
            </span>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="info">
            <BookOpen className="w-4 h-4 mr-2" />
            Class Info
          </TabsTrigger>
          <TabsTrigger value="students">
            <Users className="w-4 h-4 mr-2" />
            Students
          </TabsTrigger>
          <TabsTrigger value="projects">
            <FolderKanban className="w-4 h-4 mr-2" />
            Projects
          </TabsTrigger>
        </TabsList>

        {/* Class Info Tab */}
        <TabsContent value="info">
          <Card className="p-6">
            <h2 className="text-lg font-semibold mb-4">Class Details</h2>
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-gray-500">
                    Class Code
                  </label>
                  <p className="text-gray-900 font-mono">
                    {classData.classCode}
                  </p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-500">
                    Semester
                  </label>
                  <p className="text-gray-900">{classData.semester?.name}</p>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-gray-500">
                    Course
                  </label>
                  <p className="text-gray-900">
                    {classData.course?.code} - {classData.course?.name}
                  </p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-500">
                    Teacher
                  </label>
                  <p className="text-gray-900">{classData.teacher?.name}</p>
                </div>
              </div>
            </div>
          </Card>
        </TabsContent>

        {/* Students Tab */}
        <TabsContent value="students">
          <Card className="p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">Enrolled Students</h2>
              <Badge variant="secondary">
                {studentCount} {studentCount === 1 ? "student" : "students"}
              </Badge>
            </div>

            {!enrollments || enrollments.length === 0 ? (
              <div className="text-center py-8 text-gray-500">
                <Users className="w-12 h-12 mx-auto mb-2 text-gray-300" />
                <p>No students enrolled yet</p>
              </div>
            ) : (
              <div className="space-y-2">
                {enrollments.map((enrollment) => (
                  <div
                    key={enrollment.userId}
                    className="flex items-center gap-3 p-3 border border-gray-200 rounded-lg hover:bg-gray-50"
                  >
                    <div className="w-10 h-10 rounded-full bg-blue-100 flex items-center justify-center">
                      <span className="text-blue-600 font-medium">
                        {enrollment.studentName?.charAt(0).toUpperCase()}
                      </span>
                    </div>
                    <div className="flex-1">
                      <p className="font-medium text-gray-900">
                        {enrollment.studentName}
                      </p>
                      <p className="text-sm text-gray-500">
                        {enrollment.studentEmail}
                      </p>
                    </div>
                    <Badge variant="success">
                      Enrolled:{" "}
                      {new Date(enrollment.enrolledAt).toLocaleDateString()}
                    </Badge>
                  </div>
                ))}
              </div>
            )}
          </Card>
        </TabsContent>

        {/* Projects Tab */}
        <TabsContent value="projects">
          <Card className="p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">Class Projects</h2>
              <Badge variant="secondary">
                {allProjects.length}{" "}
                {allProjects.length === 1 ? "project" : "projects"}
              </Badge>
            </div>

            {allProjects.length === 0 ? (
              <div className="text-center py-8 text-gray-500">
                <FolderKanban className="w-12 h-12 mx-auto mb-2 text-gray-300" />
                <p>No projects in this class yet</p>
              </div>
            ) : (
              <div className="space-y-3">
                {allProjects.map((project) => (
                  <div
                    key={project.projectId}
                    className="flex items-center justify-between p-4 border border-gray-200 rounded-lg hover:bg-gray-50"
                  >
                    <div className="flex-1">
                      <p className="font-medium text-gray-900">
                        {project.name}
                      </p>
                      {project.description && (
                        <p className="text-sm text-gray-500 mt-1 line-clamp-1">
                          {project.description}
                        </p>
                      )}
                    </div>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() =>
                        navigate(`/teacher/projects/${project.projectId}`)
                      }
                    >
                      View Details
                    </Button>
                  </div>
                ))}
              </div>
            )}
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
