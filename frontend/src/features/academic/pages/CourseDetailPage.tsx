import { useParams, useNavigate } from "react-router-dom";
import { useState } from "react";
import { ArrowLeft, BookOpen, Users } from "lucide-react";
import { useCourse } from "../api/useCourses";
import { useClasses } from "../api/useClasses";
import { Button } from "@/components/ui/Button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/Tabs";
import { Card } from "@/components/ui/Card";

export function CourseDetailPage() {
  const { courseId } = useParams<{ courseId: string }>();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState("info");

  const {
    data: course,
    isLoading: isCourseLoading,
    error: courseError,
  } = useCourse(courseId || "");
  const { data: allClasses, isLoading: isClassesLoading } = useClasses();

  // Filter classes by courseId
  const courseClasses =
    allClasses?.filter((c) => c.courseId === courseId) || [];

  if (!courseId) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <p className="text-red-500">Invalid course ID</p>
      </div>
    );
  }

  if (isCourseLoading || isClassesLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto mb-4"></div>
          <p className="text-gray-600">Loading course details...</p>
        </div>
      </div>
    );
  }

  if (courseError || !course) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <p className="text-red-500">Failed to load course details</p>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header with Back Button */}
      <div className="flex items-center gap-4">
        <Button
          variant="outline"
          onClick={() => navigate("/admin/courses")}
          className="flex items-center gap-2"
        >
          <ArrowLeft className="w-4 h-4" />
          Back
        </Button>
        <div className="flex-1">
          <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100">
            {course.code} - {course.name}
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
            {courseClasses.length}{" "}
            {courseClasses.length === 1 ? "class" : "classes"} using this course
          </p>
        </div>
      </div>

      {/* Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="info" className="flex items-center gap-2">
            <BookOpen className="w-4 h-4" />
            Course Info
          </TabsTrigger>
          <TabsTrigger value="classes" className="flex items-center gap-2">
            <Users className="w-4 h-4" />
            Classes ({courseClasses.length})
          </TabsTrigger>
        </TabsList>

        {/* Course Info Tab */}
        <TabsContent value="info" className="space-y-4">
          <Card className="p-6">
            <div className="space-y-4">
              <div>
                <label className="text-sm font-medium text-gray-500 dark:text-gray-400">
                  Course Code
                </label>
                <p className="text-base text-gray-900 dark:text-gray-100 mt-1">
                  {course.code}
                </p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-500 dark:text-gray-400">
                  Course Name
                </label>
                <p className="text-base text-gray-900 dark:text-gray-100 mt-1">
                  {course.name}
                </p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-500 dark:text-gray-400">
                  Description
                </label>
                <p className="text-base text-gray-900 dark:text-gray-100 mt-1">
                  {course.description || "No description available"}
                </p>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-gray-500 dark:text-gray-400">
                    Created At
                  </label>
                  <p className="text-base text-gray-900 dark:text-gray-100 mt-1">
                    {new Date(course.createdAt).toLocaleDateString()}
                  </p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-500 dark:text-gray-400">
                    Updated At
                  </label>
                  <p className="text-base text-gray-900 dark:text-gray-100 mt-1">
                    {new Date(course.updatedAt).toLocaleDateString()}
                  </p>
                </div>
              </div>
            </div>
          </Card>
        </TabsContent>

        {/* Classes Tab */}
        <TabsContent value="classes" className="space-y-4">
          {courseClasses.length === 0 ? (
            <Card className="p-6">
              <p className="text-gray-500 text-center">
                No classes are using this course yet
              </p>
            </Card>
          ) : (
            <div className="grid gap-4">
              {courseClasses.map((classItem) => (
                <Card key={classItem.classId} className="p-6">
                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
                        {classItem.classCode}
                      </h3>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() =>
                          navigate(`/admin/classes/${classItem.classId}`)
                        }
                      >
                        View Details
                      </Button>
                    </div>
                    {classItem.semester && (
                      <p className="text-sm text-gray-600 dark:text-gray-400">
                        Semester: {classItem.semester.name}
                      </p>
                    )}
                    {classItem.teacher && (
                      <p className="text-sm text-gray-600 dark:text-gray-400">
                        Teacher: {classItem.teacher.name} (
                        {classItem.teacher.email})
                      </p>
                    )}
                  </div>
                </Card>
              ))}
            </div>
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}
