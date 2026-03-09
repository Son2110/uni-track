import { useParams, useNavigate } from "react-router-dom";
import { useState } from "react";
import { ArrowLeft, BookOpen, Users } from "lucide-react";
import { useCourse } from "@/features/academic/api/useCourses";
import { useClasses } from "@/features/academic/api/useClasses";
import { Button } from "@/components/ui/Button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/Tabs";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";

export function TeacherCourseDetailPage() {
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
      <div className="p-6">
        <p className="text-red-500">Invalid course ID</p>
      </div>
    );
  }

  if (isCourseLoading || isClassesLoading) {
    return (
      <div className="p-6">
        <div className="text-center py-12 text-gray-500">
          Loading course details...
        </div>
      </div>
    );
  }

  if (courseError || !course) {
    return (
      <div className="p-6">
        <div className="text-center py-12">
          <p className="text-red-500">Failed to load course details</p>
          <Button onClick={() => navigate("/teacher/courses")} className="mt-4">
            Back to Courses
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header with Back Button */}
      <div className="flex items-center gap-4">
        <Button variant="outline" onClick={() => navigate("/teacher/courses")}>
          <ArrowLeft className="w-4 h-4 mr-2" />
          Back
        </Button>
        <div className="flex-1">
          <h1 className="text-2xl font-bold text-gray-900">
            {course.code} - {course.name}
          </h1>
          <p className="text-sm text-gray-500 mt-1">
            {courseClasses.length}{" "}
            {courseClasses.length === 1 ? "class" : "classes"} using this course
          </p>
        </div>
      </div>

      {/* Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="info">
            <BookOpen className="w-4 h-4 mr-2" />
            Course Info
          </TabsTrigger>
          <TabsTrigger value="classes">
            <Users className="w-4 h-4 mr-2" />
            Classes
          </TabsTrigger>
        </TabsList>

        {/* Course Info Tab */}
        <TabsContent value="info">
          <Card className="p-6">
            <h2 className="text-lg font-semibold mb-4">Course Details</h2>
            <div className="space-y-4">
              <div>
                <label className="text-sm font-medium text-gray-500">
                  Course Code
                </label>
                <p className="text-gray-900 font-mono">{course.code}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-500">
                  Course Name
                </label>
                <p className="text-gray-900">{course.name}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-500">
                  Description
                </label>
                <p className="text-gray-900">
                  {course.description || "No description provided"}
                </p>
              </div>
            </div>
          </Card>
        </TabsContent>

        {/* Classes Tab */}
        <TabsContent value="classes">
          <Card className="p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">Classes</h2>
              <Badge variant="secondary">
                {courseClasses.length}{" "}
                {courseClasses.length === 1 ? "class" : "classes"}
              </Badge>
            </div>

            {courseClasses.length === 0 ? (
              <div className="text-center py-8 text-gray-500">
                <Users className="w-12 h-12 mx-auto mb-2 text-gray-300" />
                <p>No classes using this course yet</p>
              </div>
            ) : (
              <div className="space-y-3">
                {courseClasses.map((classItem) => (
                  <div
                    key={classItem.classId}
                    className="flex items-center justify-between p-4 border border-gray-200 rounded-lg hover:bg-gray-50"
                  >
                    <div className="flex-1">
                      <div className="flex items-center gap-2">
                        <p className="font-medium text-gray-900">
                          {classItem.classCode}
                        </p>
                        <Badge variant="info">{classItem.semester?.name}</Badge>
                      </div>
                      <p className="text-sm text-gray-500 mt-1">
                        Teacher: {classItem.teacher?.name}
                      </p>
                    </div>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() =>
                        navigate(`/teacher/classes/${classItem.classId}`)
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
