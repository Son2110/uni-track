# Topic 06 - Software Requirement Specification v.2.3

---

# Project Management Support System (PMSS)

# Software Requirement Specification

---

**Class Code:** SWD392  
**Group Code:** [GroupCode]

**Ho Chi Minh City, 04/02/2026**

---

# Record of Change

*A - Added | M - Modified | D - Deleted*

| Effective Date | Changed Items | A / M / D | Change Description | New Version |
|----------------|----------------|-----------|--------------------|-------------|
| 04/02/2026 | Initial SRS | A | Initial SRS created from current PMSS implementation (REST + GraphQL + GitHub/Jira integrations) | 1.0 |

---

# SIGNATURE PAGE

## ORIGINATOR

| Name | Date | Role/Title |
|--------|--------|-------------|
| [Name] | 04/02/2026 | Team Leader |
| [Name] | 04/02/2026 | Team Member |
| [Name] | 04/02/2026 | Team Member |
| [Name] | 04/02/2026 | Team Member |
| [Name] | 04/02/2026 | Team Member |

---

## REVIEWERS

| Name | Date | Role |
|--------|--------|--------|
| [Name] | 04/02/2026 | Mentor/Stakeholder |

---

# TABLE OF CONTENTS

> Auto-generate this section in your Markdown editor if supported.

---

# 1. Introduction

## 1.1 Purpose

This Software Requirements Specification (SRS) document fully describes the external behavior of the **Project Management Support System (PMSS)**.

PMSS is a web-based system that supports academic project management by organizing semesters/courses/classes, managing student projects and project teams, and integrating with GitHub and Jira to track repositories, contributors, and issues.

This document defines the **functional requirements**, **non-functional requirements**, **constraints**, **business rules**, and **integration requirements** used as the primary baseline for continuing PMSS implementation.

## 1.2 Definitions, Acronyms

- **PMSS:** Project Management Support System
- **REST API:** HTTP API exposed under `/api/*`
- **GraphQL:** Query-only API endpoint exposed under `/graphql`
- **Semester:** Academic period with start/end dates
- **Course:** Academic course (code/name) 
- **Class:** An instance of a course offered in a specific semester, taught by a teacher, having enrollments and projects
- **Enrollment:** A student being enrolled in a class and/or course
- **Project:** Student project belonging to a class
- **Project Member:** Student participating in a project team
- **GitHub Repo:** Repository tracked and managed in PMSS linked to a project
- **Repo Contributor:** Link between a PMSS user and a GitHub username for a specific repo
- **Jira Config:** Jira connection configuration linked to a PMSS project
- **Access Request:** Workflow entity for requesting access to private repositories
- **UC:** Use Case
- **NFR:** Non-Functional Requirement

## 1.3 References

- IEEE Std 830-1998, IEEE Recommended Practice for Software Requirements Specifications
- Repository documentation (PMSS):
  - README
  - GitHub repository management feature guide
  - GitHub contribution dashboard guide
  - GraphQL query examples
- Vendor APIs:
  - GitHub REST API (Statistics endpoints)
  - Atlassian Jira Cloud REST API

---

# 2. Overall Description

## 2.1 Product Perspective

PMSS is a client-server web application:

- **Backend:** ASP.NET Core Web API (.NET 10), Clean Architecture (Domain/Application/Infrastructure/API)
- **Database:** SQL Server (Entity Framework Core)
- **Frontend:** React (planned/partially implemented; backend-first development)
- **Integrations:** GitHub REST API, Jira REST API

PMSS exposes:

- REST endpoints under `/api/*` for CRUD and actions
- GraphQL endpoint `/graphql` (query-only) for flexible read access (filtering/sorting/paging)

### Major Modules

1. Academic structure management
   - Semesters
   - Courses
   - Classes
   - Enrollments
2. Project management
   - Projects
   - Project members (teams)
3. GitHub integration
   - GitHub repository tracking and management
   - Repo contributor management
   - Contribution dashboard (commits/additions/deletions filtered by semester date range)
   - Access request workflow for private repositories
4. Jira integration
   - Jira configuration per project
   - Issue fetching/testing connectivity
5. Cross-cutting capabilities
   - Filtering/pagination/sorting/search
   - Error handling and logging

## 2.2 Business Process

### BP-01 — Academic Setup and Delivery

1. Admin creates semesters and courses.
2. Admin assigns teachers and creates classes for each course and semester.
3. Admin/Teacher enrolls students into classes (single/bulk).
4. Students participate in class activities and join projects.

### BP-02 — Project Team Formation and Tracking

1. Teacher or admin creates projects under a class.
2. Students are assigned to project teams (project members).
3. Project teams link one or more GitHub repositories.
4. Teachers monitor contributions (GitHub dashboard) over the semester.
5. Project teams optionally link Jira and track issues.

### BP-03 — Integration Workflows

**GitHub Repository Management:**

1. A project member creates/updates/deletes a repo record in PMSS for the project.
2. Project members manage contributors (add/remove members).
3. System stores repo metadata and optional access token for private repos.

**GitHub Contribution Dashboard:**

1. Teacher requests contribution stats for a project.
2. System fetches data from GitHub REST statistics endpoints.
3. System aggregates across multiple repos and filters by semester date range.
4. System maps GitHub usernames to PMSS users when possible.

**Jira Integration:**

1. Project member/admin creates Jira configuration for a project.
2. System validates configuration format.
3. System can test connection and fetch raw issues.

**Access Request Workflow (Private Repos):**

1. A user requests access to a project’s private repos.
2. A teacher/admin reviews and approves/rejects.
3. Status is updated and auditable.

## 2.3 User Classes

### Admin

- Goals: Maintain academic data and user accounts; ensure system integrity
- Tasks: Manage semesters/courses/classes; manage users; oversee enrollments and projects
- Technical Expertise: Intermediate to advanced

### Teacher

- Goals: Manage classes and projects; monitor student progress/contributions
- Tasks: View classes and enrollments; manage projects/teams; view GitHub contributions; manage Jira configs (as needed)
- Technical Expertise: Intermediate

### Student

- Goals: Participate in projects; connect GitHub identity; collaborate with team
- Tasks: View own enrollments/projects; join project teams; manage project repos/contributors if a team member; request access to private repos
- Technical Expertise: Beginner to intermediate

---

# 3. FUNCTIONAL REQUIREMENTS

## 3.1 Use Case Diagram

Insert Overall Use Case Diagram here.

Actors: Admin, Teacher, Student, External GitHub API, External Jira API

---

## 3.2 Use Case Specifications

> PMSS uses both REST and GraphQL. All UCs below must be accessible via REST endpoints; read-only UCs may additionally be available via GraphQL queries.

---

### USE CASE SPECIFICATION — UC-01 Manage Semesters

| Field | Value |
|--------|---------|
| Use-case No. | UC-01 |
| Use-case Version | 1.1 |
| Use-case Name | Manage Semesters |
| Author | PMSS Team |
| Date | 04/02/2026 |
| Priority | High |
| Primary Actor | Admin |
| Secondary Actors | System, Database |
| Stakeholders | Admin, Teacher (depends on semester window), Student (indirect) |
| Frequency | Medium (per term + occasional corrections) |

**Goal / Summary:** Allow Admin to create, view, search, update, and delete semester records that define valid academic time windows used by classes and (indirectly) GitHub contribution filtering.

**Triggers:**
- New academic term is opened.
- Existing semester dates must be corrected.
- Semester list is required for class creation and reporting.

**Preconditions:**
- PRE-1. Admin is authenticated and authorized to manage academic master data.
- PRE-2. Database is available.

**Post Conditions:**
- POST-1. Semester is created/updated/deleted according to request and persisted.
- POST-2. `UpdatedAt` is refreshed on updates.

---

#### Data Definitions

Semester attributes:
- `SemesterId` (GUID, system-generated, immutable)
- `Name` (string, required)
- `StartDate` (date-time, required)
- `EndDate` (date-time, required; must be ≥ `StartDate`)
- `CreatedAt` (system-generated)
- `UpdatedAt` (system-generated)

---

#### Main Success Scenarios

**MSS-1: Create a new semester**
1. Admin opens semester management.
2. Admin selects “Create Semester”.
3. Admin enters: `Name`, `StartDate`, `EndDate`.
4. System validates required fields, date range validity, and naming constraints.
5. System creates the semester, sets timestamps, and returns the created record.
6. System displays confirmation and updated list.

**MSS-2: View / search / list semesters**
1. Admin opens semester list.
2. Admin optionally supplies filters (date range) and search term (name).
3. System returns paginated, sorted list.
4. Admin can open a semester detail view.

**MSS-3: Update an existing semester**
1. Admin opens semester details.
2. Admin edits `Name` and/or `StartDate` and/or `EndDate`.
3. System validates the same rules as create.
4. System updates the semester and returns the updated record.

**MSS-4: Delete a semester**
1. Admin requests deletion.
2. System checks dependency constraints (classes exist or not).
3. If allowed, system deletes the semester.
4. System returns success and removes it from lists.

---

#### Alternative Scenarios

**A1 — Update dates but classes already exist**
- A1.1 (Strict): System rejects date changes that would invalidate existing classes under the semester.
- A1.2 (Controlled): System allows edits only with explicit admin confirmation and audit logging (recommended for future).

**A2 — Rename only**
- If only `Name` changes and date window remains valid, system proceeds.

---

#### Exceptions

**E1 — Invalid date range**
- Condition: `EndDate < StartDate`
- Result: Reject with validation error.

**E2 — Duplicate semester name**
- Condition: Another semester exists with the same name (case-insensitive), if enforced.
- Result: Reject with “Semester name already exists”.

**E3 — Delete blocked by dependencies**
- Condition: Semester has one or more classes.
- Result: Reject with “Cannot delete semester with existing classes”.

---

#### Relationships

- Required by UC-03 Manage Classes.
- Used by UC-10 View GitHub Contribution Dashboard (filters by semester date range).

---

#### REST API Contract

- `GET /api/semesters` (pagination/sorting/search + date filters)
- `GET /api/semesters/{id}`
- `POST /api/semesters`
- `PUT /api/semesters/{id}`
- `DELETE /api/semesters/{id}`

---

#### Acceptance Criteria

- AC-01: System rejects create/update when `EndDate < StartDate`.
- AC-02: System returns paginated semester list and supports searching by name.
- AC-03: Updating a semester changes `UpdatedAt`.
- AC-04: System prevents deleting a semester that has classes (unless future override policy is implemented).

---

#### Business Rules

- BR-01, BR-02, BR-03, BR-29

---

### USE CASE SPECIFICATION — UC-02 Manage Courses

| Field | Value |
|--------|---------|
| Use-case No. | UC-02 |
| Use-case Version | 1.1 |
| Use-case Name | Manage Courses |
| Author | PMSS Team |
| Date | 04/02/2026 |
| Priority | High |
| Primary Actor | Admin |
| Secondary Actors | System, Database |
| Stakeholders | Admin, Teacher (teaches classes linked to courses), Student (indirect) |
| Frequency | Medium (per curriculum + periodic adjustments) |

**Goal / Summary:** Allow Admin to create, view, search, update, and delete course master data (code/name/description) used for class creation and project organization.

**Triggers:**
- New course is introduced or renamed.
- Course description requires updates.
- Obsolete course must be removed (subject to dependency rules).

**Preconditions:**
- PRE-1. Admin is authenticated and authorized to manage academic master data.
- PRE-2. Database is available.

**Post Conditions:**
- POST-1. Course is created/updated/deleted according to request and persisted.
- POST-2. `UpdatedAt` is refreshed on updates.

---

#### Data Definitions

Course attributes:
- `CourseId` (GUID, system-generated, immutable)
- `Code` (string, required; unique)
- `Name` (string, required)
- `Description` (string, optional)
- `CreatedAt` (system-generated)
- `UpdatedAt` (system-generated)

---

#### Main Success Scenarios

**MSS-1: Create a new course**
1. Admin opens course management.
2. Admin selects “Create Course”.
3. Admin enters: `Code`, `Name`, optional `Description`.
4. System validates required fields and uniqueness constraints.
5. System creates the course, sets timestamps, and returns the created record.
6. System displays confirmation and updated list.

**MSS-2: View / search / list courses**
1. Admin opens course list.
2. Admin optionally uses filters (`Code`, `Name`) and/or `searchTerm`.
3. System returns paginated, sorted list.
4. Admin can open a course detail view.

**MSS-3: Update an existing course**
1. Admin opens course details.
2. Admin edits `Code` and/or `Name` and/or `Description`.
3. System validates required fields and uniqueness of `Code`.
4. System updates the course and returns the updated record.

**MSS-4: Delete a course**
1. Admin requests deletion.
2. System checks dependency constraints (classes exist or not).
3. If allowed, system deletes the course.
4. System returns success and removes it from lists.

---

#### Alternative Scenarios

**A1 — Update description only**
- Admin updates `Description` while keeping `Code` and `Name` unchanged.

**A2 — Update code**
- If `Code` changes, system must re-check uniqueness and ensure downstream references remain consistent.

---

#### Exceptions

**E1 — Duplicate course code**
- Condition: Another course exists with the same `Code` (case-insensitive).
- Result: Reject with “Course code already exists”.

**E2 — Missing required fields**
- Condition: `Code` or `Name` is empty.
- Result: Reject with validation error.

**E3 — Delete blocked by dependencies**
- Condition: Course has one or more classes.
- Result: Reject with “Cannot delete course with existing classes”.

---

#### Relationships

- Required by UC-03 Manage Classes.
- Indirectly affects UC-08 Manage Projects (projects belong to classes which belong to courses).

---

#### REST API Contract

- `GET /api/courses` (pagination/sorting/search + `code`/`name` filters)
- `GET /api/courses/{id}`
- `POST /api/courses`
- `PUT /api/courses/{id}`
- `DELETE /api/courses/{id}`

---

#### Acceptance Criteria

- AC-01: System rejects create/update when `Code` or `Name` is missing.
- AC-02: System rejects create/update when `Code` duplicates an existing course.
- AC-03: System returns paginated course list and supports filtering by `Code`/`Name` and `searchTerm`.
- AC-04: Updating a course changes `UpdatedAt`.
- AC-05: System prevents deleting a course that has classes (unless future override policy is implemented).

---

#### Business Rules

- BR-04, BR-05, BR-29

---

### USE CASE SPECIFICATION — UC-03 Manage Classes

| Field | Value |
|--------|---------|
| Use-case No. | UC-03 |
| Use-case Version | 1.1 |
| Use-case Name | Manage Classes |
| Author | PMSS Team |
| Date | 04/02/2026 |
| Priority | High |
| Primary Actor | Admin |
| Secondary Actors | System, Database |
| Stakeholders | Admin, Teacher (assigned instructor), Student (enrollment + projects) |
| Frequency | High (each semester; per course offering) |

**Goal / Summary:** Allow Admin to create, view, search, update, and delete **Class** records that represent course offerings in a specific semester and assign a teacher.

**Triggers:**
- A new semester begins and class offerings must be configured.
- A teacher assignment or class code requires correction.
- Teacher/admin needs to view classes filtered by semester/course/teacher.

**Preconditions:**
- PRE-1. Admin is authenticated and authorized.
- PRE-2. Semester exists.
- PRE-3. Course exists.
- PRE-4. Teacher user exists and has role `Teacher`.

**Post Conditions:**
- POST-1. Class is created/updated/deleted and persisted.
- POST-2. Class becomes available for enrollments (UC-04/UC-05) and projects (UC-08).
- POST-3. `UpdatedAt` is refreshed on updates.

---

#### Data Definitions

Class attributes:
- `ClassId` (GUID, system-generated, immutable)
- `SemesterId` (GUID, required)
- `CourseId` (GUID, required)
- `ClassCode` (string, required; recommended unique within the same semester)
- `TeacherId` (GUID, required; must be a `Teacher` user)
- `CreatedAt` (system-generated)
- `UpdatedAt` (system-generated)

---

#### Main Success Scenarios

**MSS-1: Create a new class**
1. Admin opens class management.
2. Admin selects “Create Class”.
3. Admin provides: `SemesterId`, `CourseId`, `ClassCode`, `TeacherId`.
4. System validates:
  - referenced semester exists
  - referenced course exists
  - referenced teacher exists and role is `Teacher`
  - `ClassCode` is not empty and does not conflict with existing classes (per policy)
5. System creates the class, sets timestamps, and returns the created record.

**MSS-2: View / search / list classes**
1. Admin (or teacher) opens class list.
2. Actor optionally filters by `TeacherId`, `SemesterId`, and/or `CourseId`.
3. Actor optionally uses `searchTerm` (e.g., class code, teacher name, course fields as supported).
4. System returns paginated, sorted list.

**MSS-3: Update an existing class**
1. Admin opens class details.
2. Admin updates `ClassCode` and/or `TeacherId`.
3. System validates teacher role and code constraints.
4. System updates the class and returns updated record.

**MSS-4: Delete a class**
1. Admin requests deletion of a class.
2. System checks dependency constraints (enrollments/projects exist or not).
3. If allowed, system deletes the class.
4. System returns success.

---

#### Alternative Scenarios

**A1 — Reassign teacher**
- Admin changes `TeacherId` to a different teacher.
- System validates the new user role is `Teacher`.

**A2 — Filter by teacher / semester / course**
- Actor lists classes using dedicated filtered endpoints or query parameters.

---

#### Exceptions

**E1 — Missing referenced entities**
- Condition: `SemesterId`, `CourseId`, or `TeacherId` does not exist.
- Result: Reject with “Semester/Course/Teacher not found”.

**E2 — Invalid teacher role**
- Condition: `TeacherId` belongs to a non-teacher user.
- Result: Reject with “Assigned teacher must have Teacher role”.

**E3 — Duplicate class code (policy-dependent)**
- Condition: `ClassCode` conflicts with another class in the same semester (recommended constraint).
- Result: Reject with “Class code already exists for this semester”.

**E4 — Delete blocked by dependencies**
- Condition: Class has enrollments and/or projects.
- Result: Reject with “Cannot delete class with existing enrollments/projects”.

---

#### Relationships

- Required before UC-04 Enroll Student into Class.
- Required before UC-08 Manage Projects (projects belong to a class).

---

#### REST API Contract

- `GET /api/classes` (pagination/sorting/search + filters)
- `GET /api/classes/{id}`
- `GET /api/classes/teacher/{teacherId}`
- `GET /api/classes/semester/{semesterId}`
- `GET /api/classes/course/{courseId}`
- `POST /api/classes`
- `PUT /api/classes/{id}`
- `DELETE /api/classes/{id}`

---

#### Acceptance Criteria

- AC-01: System rejects create/update if semester/course/teacher references are invalid.
- AC-02: System rejects create/update if assigned teacher is not role `Teacher`.
- AC-03: System lists classes with paging and supports filtering by teacher/semester/course.
- AC-04: Updating a class changes `UpdatedAt`.
- AC-05: System prevents deleting a class that has enrollments/projects (unless future override policy is implemented).

---

#### Business Rules

- BR-06, BR-07, BR-29

---

### USE CASE SPECIFICATION — UC-04 Enroll Student into Class

| Field | Value |
|--------|---------|
| Use-case No. | UC-04 |
| Use-case Version | 1.1 |
| Use-case Name | Enroll Student into Class |
| Author | PMSS Team |
| Date | 04/02/2026 |
| Priority | High |
| Primary Actor | Admin / Teacher |
| Secondary Actors | System, Database |
| Stakeholders | Admin, Teacher, Student |
| Frequency | High (each class roster update) |

**Goal / Summary:** Allow Admin/Teacher to enroll one or more students into a class while enforcing uniqueness constraints (not already enrolled in the class, and not already enrolled in the same course during the same semester).

**Triggers:**
- Student roster updates for a class.
- Import of a class list (bulk).

**Preconditions:**
- PRE-1. Acting user is authenticated and authorized to manage enrollments.
- PRE-2. Target class exists and has valid `SemesterId`, `CourseId`, and `TeacherId`.
- PRE-3. Target student user(s) exist.

**Post Conditions:**
- POST-1. For each successful enrollment, a `ClassEnrollment` record exists with `EnrolledAt` timestamp.
- POST-2. Enrollment lists/counts reflect the new enrollments.

---

#### Data Definitions

ClassEnrollment attributes:
- `ClassId` (GUID, required)
- `UserId` (GUID, required)
- `CourseId` (GUID, derived from class at enrollment time)
- `EnrolledAt` (system-generated timestamp)

---

#### Main Success Scenarios

**MSS-1: Enroll a single student**
1. Actor selects a class.
2. Actor selects a student.
3. System validates:
  - class exists
  - student exists
  - student is not already enrolled in the class
  - student is not already enrolled in the same course for the same semester (even in a different class)
4. System creates `ClassEnrollment` with `CourseId` from class and sets `EnrolledAt`.
5. System returns enrollment details and success message.

**MSS-2: Bulk enroll students**
1. Actor selects a class.
2. Actor provides a list of student IDs.
3. For each student ID, system performs the same validations as MSS-1.
4. System enrolls valid students.
5. System returns:
  - list of successful enrollments
  - summary message (e.g., X/Y enrolled)
  - failure reasons for rejected IDs (at least in message/log; recommended in a structured response later).

---

#### Alternative Scenarios

**A1 — Partial success during bulk enrollment**
- Some students are enrolled successfully; some fail validation.
- System still completes the operation and reports a summary.

**A2 — Unenroll student (related action)**
- Actor removes a student enrollment from a class.
- System deletes the `ClassEnrollment` record (subject to policy; e.g., cannot remove after grading in a future enhancement).

---

#### Exceptions

**E1 — Class not found**
- Condition: `ClassId` does not exist.
- Result: Reject with “Class not found”.

**E2 — Student not found**
- Condition: `UserId` does not exist.
- Result: Reject with “User not found”.

**E3 — Duplicate enrollment in same class**
- Condition: Enrollment already exists for (`ClassId`, `UserId`).
- Result: Reject with “Student is already enrolled in this class”.

**E4 — Duplicate course enrollment in same semester**
- Condition: Student already enrolled in the same `CourseId` for the same `SemesterId` via another class.
- Result: Reject with “Student is already enrolled in this course for this semester”.

---

#### Relationships

- Depends on UC-03 Manage Classes and UC-06 Manage Users.
- Enables UC-05 View Enrollment Lists and Counts.
- Recommended constraint for UC-09 Manage Project Members: only enrolled students can be added to projects (BR-16).

---

#### REST API Contract

- `POST /api/classenrollments` (single enrollment)
- `POST /api/classenrollments/bulk` (bulk enrollment)
- `DELETE /api/classenrollments/class/{classId}/user/{userId}` (unenroll)

---

#### Acceptance Criteria

- AC-01: System rejects enrollment if class does not exist.
- AC-02: System rejects enrollment if student does not exist.
- AC-03: System rejects enrollment if student already enrolled in the same class.
- AC-04: System rejects enrollment if student already enrolled in the same course within the same semester.
- AC-05: Bulk enrollment supports partial success and returns an accurate enrolled count.

---

#### Business Rules

- BR-08, BR-08a, BR-09, BR-29

---

### USE CASE SPECIFICATION — UC-05 View Enrollment Lists and Counts

| Field | Value |
|--------|---------|
| Use-case No. | UC-05 |
| Use-case Version | 1.1 |
| Use-case Name | View Enrollment Lists and Counts |
| Author | PMSS Team |
| Date | 04/02/2026 |
| Priority | Medium |
| Primary Actors | Admin, Teacher, Student |
| Secondary Actors | System, Database |
| Stakeholders | Admin, Teacher, Student |
| Frequency | High (roster checks, dashboards, student self-checks) |

**Goal / Summary:** Provide read-only access to enrollment data, including roster lists, student enrollment lists, and enrollment counts, with consistent filtering/paging/sorting/search behavior.

**Triggers:**
- Teacher checks class roster before/after bulk enrollment.
- Admin audits enrollments across classes/semesters.
- Student checks their enrolled classes.

**Preconditions:**
- PRE-1. Acting user is authenticated.
- PRE-2. Acting user is authorized:
  - Admin/Teacher can view enrollments within their management scope.
  - Student can view only their own enrollments.

**Post Conditions:**
- POST-1. No data is modified.
- POST-2. Enrollment list/count is returned consistently according to filters.

---

#### Data Definitions

ClassEnrollmentDto fields returned by the API include:
- `ClassId`, `ClassCode`, `ClassName`
- `CourseId`, `CourseCode`, `CourseName`
- `SemesterName`
- `TeacherName`
- `UserId`, `StudentName`, `StudentEmail`
- `EnrolledAt`

---

#### Main Success Scenarios

**MSS-1: View class roster (enrollments by class)**
1. Actor selects a class.
2. System retrieves enrollments for the class.
3. System returns list of students with enrollment metadata.
4. Actor may optionally request the roster size (count).

**MSS-2: View enrollments for a user (student self-check or admin view)**
1. Actor selects a student (or student selects “My enrollments”).
2. System retrieves enrollments for the user.
3. System returns the list including class/course/semester details.

**MSS-3: View enrollment count for a class**
1. Actor requests count for a class.
2. System calculates and returns the number of enrollments.
3. The count matches the roster list size when queried at the same time.

**MSS-4: Query enrollments with filters + paging + sorting + search**
1. Actor requests enrollment list with optional filters:
  - `ClassId`, `UserId`, `CourseId`, `SemesterId`
2. Actor may provide paging/sorting/search parameters:
  - `pageNumber`, `pageSize`, `searchTerm`, `sortBy`, `sortDescending`
3. System returns a paged result containing:
  - `Items`, `TotalCount`, `PageNumber`, `PageSize`
4. Default sorting is by `EnrolledAt` descending when `sortBy` is not specified.

---

#### Alternative Scenarios

**A1 — Search within enrollments**
- Actor provides `searchTerm`.
- System searches across: student name/email and course code/name.

**A2 — Get a single enrollment record**
- Actor requests the enrollment for a specific (`ClassId`, `UserId`) pair.
- System returns the enrollment if it exists.

---

#### Exceptions

**E1 — Enrollment not found (single enrollment lookup)**
- Condition: No enrollment exists for (`ClassId`, `UserId`).
- Result: System returns “Enrollment not found”.

**E2 — Unauthorized access**
- Condition: Student attempts to query another user’s enrollments; or teacher attempts to view rosters outside permitted scope.
- Result: System rejects the request.

**E3 — Empty result set**
- Condition: No enrollments match the provided filters.
- Result: System returns an empty list and `TotalCount = 0` (or count = 0).

---

#### REST API Contract

- `GET /api/classenrollments` (paged query)
  - Query params: `classId`, `userId`, `courseId`, `semesterId`, `pageNumber`, `pageSize`, `searchTerm`, `sortBy`, `sortDescending`
  - `sortBy` supported: `enrolledAt`, `studentName`, `courseCode`
- `GET /api/classenrollments/class/{classId}` (roster list)
- `GET /api/classenrollments/class/{classId}/count` (roster count)
- `GET /api/classenrollments/user/{userId}` (enrollments for a user)
- `GET /api/classenrollments/class/{classId}/user/{userId}` (single enrollment)

---

#### Acceptance Criteria

- AC-01: System returns roster list for a class with student identity fields.
- AC-02: System returns enrollment list for a user with class/course/semester details.
- AC-03: System returns class enrollment count and it equals the roster list size for the same class.
- AC-04: System supports filtering by `classId`, `userId`, `courseId`, `semesterId` and returns correct `TotalCount`.
- AC-05: System supports paging and returns a stable sorted order for a given `sortBy`.
- AC-06: System supports search by student name/email and course code/name.
- AC-07: Student can only view their own enrollments.

---

#### Business Rules

- BR-10, BR-29

---

### USE CASE SPECIFICATION — UC-06 Manage Users

| Field | Value |
|--------|---------|
| Use-case No. | UC-06 |
| Use-case Version | 1.1 |
| Use-case Name | Manage Users |
| Author | PMSS Team |
| Date | 04/02/2026 |
| Priority | High |
| Primary Actor | Admin |
| Secondary Actors | System, Database |
| Stakeholders | Admin, Teacher, Student |
| Frequency | Medium (initial setup + ongoing maintenance) |

**Goal / Summary:** Allow Admin to create, view, search, update, and delete user accounts; assign roles (Admin/Teacher/Student); and maintain GitHub identity fields used for GitHub contribution mapping.

**Triggers:**
- New students/teachers/admin staff are added.
- User profile information changes (name/email/GitHub identity).
- Admin needs to audit users by role.

**Preconditions:**
- PRE-1. Admin is authenticated and authorized to manage users.
- PRE-2. Database is available.

**Post Conditions:**
- POST-1. User record is created/updated/deleted and persisted.
- POST-2. Password is stored as a hash (never stored/returned in plaintext).
- POST-3. `UpdatedAt` is refreshed on update operations.

---

#### Data Definitions

User attributes:
- `UserId` (GUID, system-generated)
- `Name` (string, required)
- `Email` (string, required, unique)
- `HashedPassword` (string, system-managed)
- `Role` (enum: Admin/Teacher/Student)
- `GithubUsername` (string, optional, unique when present)
- `GithubEmail` (string, optional)
- `CreatedAt`, `UpdatedAt` (system-generated)

---

#### Main Success Scenarios

**MSS-1: Create a user**
1. Admin provides user details: `Name`, `Email`, `Password`, `Role` (optionally `GithubUsername`, `GithubEmail`).
2. System validates uniqueness constraints (email; GitHub username if provided).
3. System hashes the provided password and persists the user.
4. System returns the created user profile (excluding password/hash).

**MSS-2: Update a user profile and role**
1. Admin selects a user by `UserId`.
2. Admin updates profile fields (name/email/GitHub identity) and/or role.
3. System validates uniqueness constraints (email; GitHub username if provided).
4. System persists changes and returns updated user profile.

**MSS-3: Delete a user**
1. Admin selects a user by `UserId`.
2. System deletes the user record.
3. System returns success.

**MSS-4: List/search users with paging/filtering/sorting**
1. Admin requests the user list with optional filters:
  - `role`, `githubUsername`
2. Admin may specify common list parameters:
  - `pageNumber`, `pageSize`, `searchTerm`, `sortBy`, `sortDescending`
3. System returns a paged result containing:
  - `Items`, `TotalCount`, `PageNumber`, `PageSize`
4. Default sorting is by `Name` ascending when `sortBy` is not specified.

---

#### Alternative Scenarios

**A1 — Search users by name/email**
- Admin provides `searchTerm`.
- System searches by user name and email.

**A2 — Maintain GitHub identity for contribution mapping**
- Admin sets `GithubUsername` and optional `GithubEmail`.
- System enforces uniqueness on `GithubUsername` when provided.

---

#### Exceptions

**E1 — User not found**
- Condition: `UserId` does not exist for get/update/delete.
- Result: System returns “User not found”.

**E2 — Duplicate email**
- Condition: Another user already has the requested email.
- Result: System rejects with “User with this email already exists”.

**E3 — Duplicate GitHub username**
- Condition: Another user already has the requested GitHub username.
- Result: System rejects with “User with this GitHub username already exists”.

**E4 — Invalid role value**
- Condition: Role is not one of Admin/Teacher/Student.
- Result: System rejects the request.

---

#### REST API Contract

- `GET /api/users` (paged list)
  - Query params: `role`, `githubUsername`, `pageNumber`, `pageSize`, `searchTerm`, `sortBy`, `sortDescending`
  - `sortBy` supported: `name`, `email`, `role`, `createdAt`
- `GET /api/users/{id}` (get user by id)
- `POST /api/users` (create user)
- `PUT /api/users/{id}` (update user)
- `DELETE /api/users/{id}` (delete user)

---

#### Acceptance Criteria

- AC-01: Creating a user stores password as a hash and never returns it.
- AC-02: Creating/updating a user rejects duplicate emails.
- AC-03: Creating/updating a user rejects duplicate GitHub usernames when provided.
- AC-04: Listing users supports paging and returns correct `TotalCount`.
- AC-05: Listing users supports filtering by role and exact `githubUsername`.
- AC-06: Listing users supports search by name/email.
- AC-07: Sorting works for `name`, `email`, `role`, and `createdAt`.

---

#### Business Rules

- BR-11, BR-11a, BR-12, BR-13, BR-29

---

### USE CASE SPECIFICATION — UC-07 Change Password

| Field | Value |
|--------|---------|
| Use-case No. | UC-07 |
| Use-case Version | 1.1 |
| Use-case Name | Change Password |
| Author | PMSS Team |
| Date | 04/02/2026 |
| Priority | High |
| Primary Actors | Admin, Teacher, Student |
| Secondary Actors | System, Database |
| Stakeholders | Admin, Teacher, Student |
| Frequency | Medium (security updates, account recovery flows in future) |

**Goal / Summary:** Allow a user to change their password securely by providing the current password, verifying it server-side, and storing only a hashed new password.

**Triggers:**
- Password rotation / security hygiene.
- User suspects their password is compromised.

**Preconditions:**
- PRE-1. User account exists.
- PRE-2. Acting user is authenticated.
- PRE-3. Authorization: users can change their own password; admin password resets (without current password) are a future enhancement.

**Post Conditions:**
- POST-1. User password hash is updated.
- POST-2. `UpdatedAt` is refreshed.
- POST-3. No plaintext password is stored or returned.

---

#### Data Definitions

UpdatePassword request fields:
- `CurrentPassword` (string, required)
- `NewPassword` (string, required)

---

#### Main Success Scenario

**MSS-1: Change own password**
1. User submits `CurrentPassword` and `NewPassword`.
2. System verifies the user exists.
3. System verifies `CurrentPassword` matches the stored password hash.
4. System validates `NewPassword` against password policy.
5. System hashes `NewPassword`, stores it, updates `UpdatedAt`.
6. System returns success.

---

#### Alternative Scenarios

**A1 — Reuse of old password (policy-dependent)**
- If the system enforces “no reuse” in future, the request is rejected.

---

#### Exceptions

**E1 — User not found**
- Condition: `UserId` does not exist.
- Result: Reject with “User not found”.

**E2 — Wrong current password**
- Condition: `CurrentPassword` verification fails.
- Result: Reject with “Current password is incorrect”; do not change password.

**E3 — New password violates policy**
- Condition: `NewPassword` fails BR-12.
- Result: Reject with a validation error; do not change password.

---

#### REST API Contract

- `PUT /api/users/{id}/password`
  - Body: `UpdatePasswordDto` (`currentPassword`, `newPassword`)
  - Result: success flag + message

---

#### Acceptance Criteria

- AC-01: System verifies current password before updating.
- AC-02: System rejects with a clear message when current password is incorrect.
- AC-03: System hashes the new password and never returns it.
- AC-04: System updates `UpdatedAt` after password change.
- AC-05: System enforces the password policy for the new password.

---

#### Business Rules

- BR-12

---

### USE CASE SPECIFICATION — UC-08 Manage Projects

| Field | Value |
|--------|---------|
| Use-case No. | UC-08 |
| Use-case Version | 1.1 |
| Use-case Name | Manage Projects |
| Author | PMSS Team |
| Date | 04/02/2026 |
| Priority | High |
| Primary Actors | Admin, Teacher |
| Secondary Actors | System, Database |
| Stakeholders | Admin, Teacher, Student |
| Frequency | High (per class; iterative updates) |

**Goal / Summary:** Allow Admin/Teacher to create, view, search, update, and delete project records under a class. Projects are the parent container for team membership, GitHub repositories, Jira configuration, and (optional) access requests.

**Triggers:**
- A class begins project work and teams must be tracked.
- Teacher needs to create/update project metadata.
- Admin audits or corrects project data.

**Preconditions:**
- PRE-1. Acting user is authenticated and authorized to manage projects.
- PRE-2. Target class exists.

**Post Conditions:**
- POST-1. Project is created/updated/deleted and persisted.
- POST-2. `CreatedAt`/`UpdatedAt` timestamps are system-managed.
- POST-3. A created project can later have members (UC-09), GitHub repos (UC-11), and Jira config (UC-13).

---

#### Data Definitions

Project attributes:
- `ProjectId` (GUID, system-generated)
- `ClassId` (GUID, required)
- `Name` (string, required)
- `Description` (string, optional)
- `CreatedAt`, `UpdatedAt` (system-generated)

Project list/read DTOs also include context fields:
- `ClassName`, `CourseCode`, `CourseName`

---

#### Main Success Scenarios

**MSS-1: Create a project**
1. Actor selects a class.
2. Actor provides project `Name` and optional `Description`.
3. System validates the class exists.
4. System creates the project and sets `CreatedAt`/`UpdatedAt`.
5. System returns the created project.

**MSS-2: Update a project**
1. Actor selects a project.
2. Actor updates `Name` and/or `Description`.
3. System persists changes and updates `UpdatedAt`.
4. System returns the updated project.

**MSS-3: Delete a project**
1. Actor selects a project.
2. System deletes the project.
3. System returns success.

**MSS-4: List/search projects with paging/filtering/sorting**
1. Actor requests the project list with optional filters:
  - `classId`, `courseId`, `teacherId`
2. Actor may specify common list parameters:
  - `pageNumber`, `pageSize`, `searchTerm`, `sortBy`, `sortDescending`
3. System returns a paged result containing:
  - `Items`, `TotalCount`, `PageNumber`, `PageSize`
4. Default sorting is by `CreatedAt` descending when `sortBy` is not specified.

---

#### Alternative Scenarios

**A1 — Search by name/description**
- Actor provides `searchTerm`.
- System searches in project name and description.

**A2 — View a single project**
- Actor retrieves a project by `ProjectId`.
- System returns project details.

---

#### Exceptions

**E1 — Class not found (create)**
- Condition: `ClassId` does not exist.
- Result: Reject with “Class not found”.

**E2 — Project not found (get/update/delete)**
- Condition: `ProjectId` does not exist.
- Result: Reject with “Project not found”.

**E3 — Invalid input**
- Condition: Missing required fields (e.g., empty name) or invalid request body.
- Result: Reject with validation error.

---

#### Relationships

- Enables UC-09 Manage Project Members (teams).
- Enables UC-11 Manage GitHub Repos and UC-12 Manage Repo Contributors.
- Enables UC-13 Jira Integration.
- Supports UC-10 View GitHub Contribution Dashboard via `GET /api/projects/{id}/github-contributions`.

---

#### REST API Contract

- `GET /api/projects` (paged list)
  - Query params: `classId`, `courseId`, `teacherId`, `pageNumber`, `pageSize`, `searchTerm`, `sortBy`, `sortDescending`
  - `sortBy` supported: `name`, `createdAt`, `updatedAt`
- `GET /api/projects/{id}` (get by id)
- `POST /api/projects` (create)
- `PUT /api/projects/{id}` (update)
- `DELETE /api/projects/{id}` (delete)

---

#### Acceptance Criteria

- AC-01: Creating a project requires an existing class.
- AC-02: Updating a project refreshes `UpdatedAt`.
- AC-03: Listing projects supports filtering by `classId`, `courseId`, and `teacherId`.
- AC-04: Listing projects supports paging and returns correct `TotalCount`.
- AC-05: Listing projects supports search by name/description.
- AC-06: Sorting works for `name`, `createdAt`, and `updatedAt`.

---

#### Business Rules

- BR-14, BR-29

---

### USE CASE SPECIFICATION — UC-09 Manage Project Members (Teams)

| Field | Value |
|--------|---------|
| Use-case No. | UC-09 |
| Use-case Version | 1.1 |
| Use-case Name | Manage Project Members (Teams) |
| Author | PMSS Team |
| Date | 04/02/2026 |
| Priority | High |
| Primary Actors | Admin, Teacher |
| Secondary Actors | System, Database |
| Stakeholders | Admin, Teacher, Student |
| Frequency | High (during team formation and adjustments) |

**Goal / Summary:** Allow Admin/Teacher to add/remove users as members of a project team and to view team membership lists. Membership is a prerequisite for managing GitHub repos/contributors under the project.

**Triggers:**
- Teacher assigns students into project teams.
- Team changes are required (add/drop student).
- Admin audits team composition.

**Preconditions:**
- PRE-1. Acting user is authenticated and authorized to manage project teams.
- PRE-2. Target project exists.
- PRE-3. Target user exists.
- PRE-4 (recommended). User is enrolled in the project’s class (BR-16).

**Post Conditions:**
- POST-1. If added, membership record exists with `JoinedAt`.
- POST-2. If removed, membership record no longer exists.
- POST-3. Membership lists reflect changes.

---

#### Data Definitions

ProjectMember attributes:
- `ProjectId` (GUID, required)
- `UserId` (GUID, required)
- `JoinedAt` (system-generated timestamp)

ProjectMemberDto returned by the API includes:
- `ProjectId`, `ProjectName`
- `UserId`, `UserName`, `UserEmail`, `GithubUsername`
- `JoinedAt`

---

#### Main Success Scenarios

**MSS-1: Add a member to a project**
1. Actor selects a project.
2. Actor selects a user to add.
3. System validates:
   - project exists
   - user exists
   - user is not already a member of the project
   - (recommended) user is enrolled in the class that owns the project
4. System creates membership and sets `JoinedAt`.
5. System returns the created membership.

**MSS-2: Remove a member from a project**
1. Actor selects a project.
2. Actor selects a member to remove.
3. System verifies membership exists.
4. System deletes the membership and returns success.

**MSS-3: View members of a project (paged)**
1. Actor requests members for a project.
2. System returns a paged list of memberships.
3. System sorts by `JoinedAt` (ascending by default; descending when requested).

**MSS-4: View all projects a user belongs to (paged)**
1. Actor requests memberships for a user.
2. System returns a paged list of memberships for that user.

---

#### Alternative Scenarios

**A1 — Lookup a specific membership**
- Actor requests membership by (`ProjectId`, `UserId`).
- System returns the membership if it exists.

**A2 — Filter membership list**
- Actor requests membership list with optional filters (`projectId`, `userId`) and pagination.

---

#### Exceptions

**E1 — Project not found**
- Condition: `ProjectId` does not exist.
- Result: Reject with “Project not found”.

**E2 — User not found**
- Condition: `UserId` does not exist.
- Result: Reject with “User not found”.

**E3 — Duplicate membership**
- Condition: Membership already exists for (`ProjectId`, `UserId`).
- Result: Reject with “User is already a member of this project”.

**E4 — Membership not found (remove/lookup)**
- Condition: Membership does not exist.
- Result: Reject with “Membership not found”.

**E5 — Not enrolled in class (recommended rule)**
- Condition: User is not enrolled in the project’s class.
- Result: Reject with “User must be enrolled in the class to join the project”.

---

#### REST API Contract

- `GET /api/projectmembers` (paged list)
  - Query params: `projectId`, `userId`, `pageNumber`, `pageSize`, `sortDescending`
  - Sorting: `JoinedAt` ascending by default, descending when `sortDescending=true`
- `GET /api/projectmembers/project/{projectId}?pageNumber=&pageSize=` (members by project)
- `GET /api/projectmembers/user/{userId}?pageNumber=&pageSize=` (projects by user)
- `GET /api/projectmembers/{projectId}/{userId}` (get membership)
- `POST /api/projectmembers` (add member)
- `DELETE /api/projectmembers/{projectId}/{userId}` (remove member)

---

#### Acceptance Criteria

- AC-01: System rejects adding a member if the project does not exist.
- AC-02: System rejects adding a member if the user does not exist.
- AC-03: System rejects adding a member if membership already exists.
- AC-04: System returns a paged member list for a project.
- AC-05: System returns a paged list of projects for a user.
- AC-06: Sorting by `JoinedAt` works via `sortDescending`.

---

#### Business Rules

- BR-15, BR-16, BR-29

---

### USE CASE SPECIFICATION — UC-10 View GitHub Contribution Dashboard

| Field | Value |
|--------|---------|
| Use-case No. | UC-10 |
| Use-case Version | 1.1 |
| Use-case Name | View GitHub Contribution Dashboard |
| Author | PMSS Team |
| Date | 04/02/2026 |
| Priority | High |
| Primary Actor | Teacher |
| Secondary Actors | System, Database, GitHub REST API |
| Stakeholders | Teacher, Admin, Student (indirect) |
| Frequency | High (weekly / during semester) |

**Goal / Summary:** Allow Teacher to view GitHub contribution statistics (overall + per-contributor) for a specific project, filtered to the semester date range of the project’s class.

**Description:** The system aggregates GitHub statistics across all repositories linked to the project, filters weekly activity within the semester start/end dates, and attempts to map GitHub contributors to PMSS users.

**Triggers:** Teacher wants to assess student contributions.

**Preconditions:**
- PRE-1. Actor is authenticated (Teacher role).
- PRE-2. `ProjectId` exists.
- PRE-3. Project belongs to a class that belongs to a semester with `StartDate` and `EndDate`.
- PRE-4. Project has at least one GitHub repository configured in PMSS (UC-11).

**Post Conditions:**
- POST-1. System returns contribution data filtered by semester date range.
- POST-2. No PMSS data is modified (read-only UC).

---

#### Data Definitions

Contribution dashboard response (`ProjectGithubContributionDto`) includes:
- `ProjectId` (GUID)
- `ProjectName` (string)
- `SemesterStartDate` (date-time)
- `SemesterEndDate` (date-time)
- `TotalCommitsInSemester` (int)
- `TotalAdditionsInSemester` (int)
- `TotalDeletionsInSemester` (int)
- `Repositories` (list of `RepoContributionDto`)
- `OverallCommitsOverTime` (list of `WeeklyCommitDto`)
- `Contributors` (list of `ContributorStatsDto`)

Repository contribution (`RepoContributionDto`):
- `GithubRepoId` (GUID)
- `RepoOwnerName` (string)
- `RepoName` (string)
- `RepoUrl` (string; derived as `https://github.com/{owner}/{repo}`)

Overall weekly commits (`WeeklyCommitDto`):
- `WeekStart` (date-time; week bucket start as provided by GitHub stats timestamp)
- `WeekEnd` (date-time; `WeekStart + 7 days`)
- `CommitCount` (int)

Contributor stats (`ContributorStatsDto`):
- `GithubUsername` (string)
- `GithubEmail` (string; optional)
- `UserId` (GUID; optional, null if not mapped)
- `UserFullName` (string; optional)
- `TotalCommits` (int; within semester)
- `TotalAdditions` (int; within semester)
- `TotalDeletions` (int; within semester)
- `WeeklyActivity` (list of `WeeklyContributorActivityDto`)

Weekly contributor activity (`WeeklyContributorActivityDto`):
- `WeekStart` (date-time)
- `WeekEnd` (date-time; `WeekStart + 7 days`)
- `Commits` (int)
- `Additions` (int)
- `Deletions` (int)

Notes:
- GitHub statistics APIs typically return up to the last ~52 weeks of data. If the semester is outside that window, returned activity may be empty or partial.
- The system aggregates by GitHub username (case-insensitive) across multiple repositories.

---

#### Main Success Scenarios

**MSS-1: View GitHub contribution dashboard for a project**

1. Teacher requests dashboard for a project.
2. System validates `ProjectId` and loads the project.
3. System loads the project’s class and semester date range (`StartDate`, `EndDate`).
4. System loads all GitHub repositories linked to the project.
5. For each repository, system requests GitHub statistics endpoints:
  - `/stats/commit_activity` (overall weekly commits)
  - `/stats/contributors` (per-contributor weekly additions/deletions/commits)
6. If GitHub returns `202 Accepted` for statistics computation, system retries a limited number of times with increasing delay.
7. System aggregates data across repositories:
  - merges weekly commit totals by week timestamp
  - merges contributor weekly stats and totals by GitHub username
8. System filters weekly activity to only include week buckets within the semester start/end date range.
9. System attempts to map GitHub contributors to PMSS users:
  - by existing `RepoContributor.UserId` when set, and/or
  - by matching `User.GithubUsername` to the GitHub contributor username.
10. System sorts:
  - contributors by `TotalCommits` descending
  - each contributor’s `WeeklyActivity` by `WeekStart` ascending
11. System returns totals, overall weekly timeline, and contributor breakdown.

---

#### Alternative Scenarios

**A1 — Multiple repositories linked to the project**
- System aggregates contributions across all linked repositories (not per-repo dashboards).

**A2 — Repository is private**
- System uses the stored per-repository access token (if present) to authenticate GitHub API calls.

**A3 — Contributor cannot be mapped to a PMSS user**
- System returns contributor stats with `UserId` and `UserFullName` as null.

**A4 — Partial GitHub data**
- If one repository fails (invalid repo, token revoked, transient failure), system continues with remaining repositories and returns partial aggregated results.

---

#### Exceptions

**E1 — Project not found**
- Condition: `ProjectId` does not exist.
- Result: Reject with “Project not found”.

**E2 — Class not found**
- Condition: Project references a class that does not exist.
- Result: Reject with “Class not found”.

**E3 — Semester not found**
- Condition: Class references a semester that does not exist.
- Result: Reject with “Semester not found”.

**E4 — No GitHub repositories found for the project**
- Condition: Project has zero repos configured.
- Result: Reject with “No GitHub repositories found for this project”.

**E5 — GitHub statistics still computing (202 Accepted)**
- Condition: GitHub returns 202 for the stats endpoint beyond the allowed retry attempts.
- Result: System treats that repository’s stats as unavailable for this request and returns partial results if other repos succeed; otherwise returns an integration error.

**E6 — GitHub rate limited / auth failed**
- Condition: GitHub returns rate limit response, 401/403 due to missing/invalid token, or insufficient permissions.
- Result: Return an integration error indicating authentication/rate-limit issue and recommend using a valid token for private repos.

---

#### REST API Contract

- `GET /api/projects/{id}/github-contributions` (dashboard by project)
  - Path params: `id` (GUID)
  - Response body: `ApiResponse<ProjectGithubContributionDto>`
  - Notes:
   - Uses GitHub REST statistics endpoints (`/stats/commit_activity`, `/stats/contributors`).
   - May return empty/partial results depending on GitHub data availability and the last-52-weeks constraint.

---

#### Acceptance Criteria

- AC-01: System rejects the request when `ProjectId` does not exist.
- AC-02: System rejects the request when the project has no GitHub repositories configured.
- AC-03: System aggregates contributions across multiple repositories.
- AC-04: System filters returned weekly activity to the project’s semester start/end dates.
- AC-05: System returns contributors sorted by total commits (descending).
- AC-06: System returns each contributor’s weekly activity sorted by week start (ascending).
- AC-07: When a `RepoContributor` is linked to a PMSS user, the response includes `UserId` and `UserFullName`.
- AC-08: If one repository fails to fetch stats, the system still returns data for remaining repositories.

### Business Rules

- BR-17, BR-18, BR-19

---

### USE CASE SPECIFICATION — UC-11 Manage GitHub Repositories (PMSS Tracking)

| Field | Value |
|--------|---------|
| Use-case No. | UC-11 |
| Use-case Version | 1.1 |
| Use-case Name | Manage GitHub Repositories |
| Author | PMSS Team |
| Date | 04/02/2026 |
| Priority | High |
| Primary Actor | Student (Project Member) |
| Secondary Actors | System, Database |
| Stakeholders | Teacher, Admin, Student |
| Frequency | Medium (per team setup and adjustments) |

**Goal / Summary:** Project members can register and maintain GitHub repository records used by PMSS for contribution analytics.

**Description:** Any project member may create, update, or delete GitHub repo records linked to their project. For private repositories, an optional access token may be stored to enable dashboard retrieval; tokens are never exposed by the API.

**Triggers:** Team links repos to PMSS.

**Preconditions:**
- PRE-1. Project exists.
- PRE-2. Acting user is a project member.

**Post Conditions:**
- POST-1. Repo metadata is stored; URL is derived from owner/name.
- POST-2. API token (if provided) is stored securely and never returned.

---

#### Data Definitions

Repository (`GithubRepoDto`):
- `GithubRepoId` (GUID)
- `ProjectId` (GUID)
- `ProjectName` (string)
- `CourseId` (GUID)
- `CourseName` (string)
- `CourseCode` (string)
- `RepoOwnerName` (string)
- `RepoName` (string)
- `RepoUrl` (string; derived `https://github.com/{owner}/{repo}`)
- `IsPrivate` (bool)
- `ContributorCount` (int)
- `CreatedAt`, `UpdatedAt` (date-time)
- `Contributors` (list of `RepoContributorDto`)

Create (`CreateGithubRepoDto`):
- `ProjectId` (GUID, required)
- `RepoOwnerName` (string, required)
- `RepoName` (string, required)
- `IsPrivate` (bool)
- `ApiToken` (string; optional; never returned)

Update (`UpdateGithubRepoDto`):
- `RepoOwnerName` (string)
- `RepoName` (string)
- `IsPrivate` (bool)
- `ApiToken` (string; optional)

Filter (`GithubRepoFilterParams`):
- Common paging: `pageNumber`, `pageSize`, `sortBy`, `sortDescending`
- Filters: `projectId`, `courseId`, `userId`, `repoOwnerName`, `isPrivate`

#### Main Success Scenarios

**MSS-1: Create a repository record**
1. Project member submits `CreateGithubRepoDto` (owner/name, privacy, optional token).
2. System validates acting user is a member of the target project.
3. System validates required fields and formats (non-empty; basic owner/name format).
4. System creates the repo record, derives `RepoUrl`, sets `CreatedAt` and `UpdatedAt`.
5. System returns `GithubRepoDto` (excluding token).

**MSS-2: Update a repository record**
1. Project member submits `UpdateGithubRepoDto`.
2. System validates acting user is a member of the repo’s project.
3. System validates input (owner/name non-empty; privacy boolean).
4. System updates repo fields, refreshes `UpdatedAt`.
5. System returns updated `GithubRepoDto` (excluding token).

**MSS-3: Delete a repository record**
1. Project member requests delete by `GithubRepoId`.
2. System validates acting user is a member of the repo’s project.
3. System deletes the repo record.
4. System returns success.

**MSS-4: List/search repositories (paged)**
1. Actor requests repo list with optional filters (`projectId`, `courseId`, `userId`, `repoOwnerName`, `isPrivate`) and common params.
2. System returns a paged result with items, total count, page number, page size.
3. Sorting controlled by `sortBy` and `sortDescending` (default by `CreatedAt` descending when unspecified).

**MSS-5: Get repository by ID**
1. Actor requests repo by `GithubRepoId`.
2. System returns `GithubRepoDto`.

---

#### Alternative Scenarios

**A1 — Private repository without token**
- System accepts creation/update. Contribution dashboard calls may later fail or be rate-limited; token can be provided subsequently.

**A2 — Change privacy from public to private**
- System updates `IsPrivate`. If `ApiToken` is omitted, dashboard may require a token to access private repo stats.

**A3 — Course/User views**
- Actor lists repos by course (`courseId`) or by user (`userId`) using provided endpoints.

---

#### Exceptions

**E1 — Project not found (create)**
- Condition: `ProjectId` does not exist.
- Result: Reject with “Project not found”.

**E2 — Repo not found (get/update/delete)**
- Condition: `GithubRepoId` does not exist.
- Result: Reject with “Repository not found”.

**E3 — Not a project member**
- Condition: Acting user is not a member of the repo’s project.
- Result: Reject with “User is not a project member”.

**E4 — Invalid input**
- Condition: Missing required fields or invalid owner/name format.
- Result: Reject with validation error details.

**E5 — Token policy**
- Condition: Attempt to retrieve token via API.
- Result: Token is never returned; reject or omit field as per API contract.

---

#### REST API Contract

- `GET /api/githubrepos` (paged list)
  - Query: `projectId`, `courseId`, `userId`, `repoOwnerName`, `isPrivate`, `pageNumber`, `pageSize`, `sortBy`, `sortDescending`
- `GET /api/githubrepos/{id}` (get by id)
- `POST /api/githubrepos` (create)
  - Headers: `X-User-Id` (acting user)
  - Body: `CreateGithubRepoDto`
- `PUT /api/githubrepos/{id}` (update)
  - Headers: `X-User-Id` (acting user)
  - Body: `UpdateGithubRepoDto`
- `DELETE /api/githubrepos/{id}` (delete)
  - Headers: `X-User-Id` (acting user)
- `GET /api/githubrepos/course/{courseId}`
- `GET /api/githubrepos/user/{userId}`

---

#### Acceptance Criteria

- AC-01: Only project members can create, update, and delete repo records.
- AC-02: Creating a repo requires `ProjectId`, `RepoOwnerName`, `RepoName`.
- AC-03: `RepoUrl` is derived from `RepoOwnerName` and `RepoName`.
- AC-04: Updating a repo refreshes `UpdatedAt` and returns updated metadata.
- AC-05: API never returns `ApiToken` in responses.
- AC-06: Listing supports filters, paging, and sorting per common parameters (BR-29).
- AC-07: Course/user-specific endpoints return repos scoped appropriately.

### Business Rules

- BR-20, BR-21, BR-29

---

### USE CASE SPECIFICATION — UC-12 Manage Repo Contributors

| Field | Value |
|--------|---------|
| Use-case No. | UC-12 |
| Use-case Version | 1.1 |
| Use-case Name | Manage Repo Contributors |
| Author | PMSS Team |
| Date | 04/02/2026 |
| Priority | High |
| Primary Actor | Student (Project Member) |
| Secondary Actors | System, Database |
| Stakeholders | Teacher, Admin, Student |
| Frequency | Medium (per team setup and adjustments) |

**Goal / Summary:** Allow project members to manage the set of PMSS users linked as contributors to a GitHub repository. These mappings enable contributor analytics and user association in UC-10.

**Description:** Any project member may add or remove contributors for a repository under their project. A contributor must be a PMSS user who is already a member of the same project. The system stores contributor mappings (GitHub username/email if available) and exposes them in repo details.

**Triggers:** Team aligns PMSS project members with GitHub identities for contribution tracking.

**Preconditions:**
- PRE-1. Repository exists under a project.
- PRE-2. Acting user is a project member of the repository’s project.
- PRE-3. Target user to be added is a project member of the same project (BR-23).

**Post Conditions:**
- POST-1. A `RepoContributor` record exists linking the repository to the target user.
- POST-2. Repository details expose the `Contributors` list; analytics (UC-10) can map stats to users.

---

#### Data Definitions

Repo contributor (`RepoContributorDto`):
- `GithubUsername` (string)
- `GithubEmail` (string; optional)
- `UserId` (GUID)
- `UserFullName` (string)
- `AddedAt` (date-time)

Notes:
- `GithubUsername`/`GithubEmail` may be copied from the user profile when set; otherwise may remain empty or be provided later.

---

#### Main Success Scenarios

**MSS-1: Add a contributor to a repository**
1. Project member selects repository `repoId` and target `userId` to add.
2. System validates:
   - repository exists and belongs to a project
   - acting user is a member of that project
   - target user is a member of that project (BR-23)
   - no existing contributor entry for (`repoId`, `userId`)
3. System creates `RepoContributor` and sets `AddedAt`; optionally copies `GithubUsername`/`GithubEmail` from the user profile.
4. System returns success; repository contributors include the new entry.

**MSS-2: Remove a contributor from a repository**
1. Project member selects repository `repoId` and target `userId` to remove.
2. System validates acting user is a member of the repo’s project.
3. System verifies contributor entry exists for (`repoId`, `userId`).
4. System removes the entry and returns success.

**MSS-3: View contributors for a repository**
1. Actor requests repository details.
2. System returns `GithubRepoDto` including `Contributors` list with `GithubUsername`, `UserId`, `UserFullName`, `AddedAt`.

**MSS-4: Lookup a contributor** (optional)
1. Actor requests a specific contributor mapping by (`repoId`, `githubUsername`).
2. System returns the contributor mapping if available.

---

#### Alternative Scenarios

**A1 — Self-add / self-remove**
- A project member can add/remove themselves as a contributor to a repo in the same project.

**A2 — Missing GitHub identity**
- If the user has no `GithubUsername`, the contributor mapping still succeeds. Analytics may show unmapped GitHub contributors until identity is set.

**A3 — Multiple repositories in the same project**
- Contributors are managed per repository. A project member may be a contributor to some repos and not others.

---

#### Exceptions

**E1 — Repository not found**
- Condition: `repoId` does not exist.
- Result: Reject with “Repository not found”.

**E2 — Not a project member (actor)**
- Condition: Acting user is not a member of the repository’s project.
- Result: Reject with “User is not a project member”.

**E3 — Target user not a project member**
- Condition: Target `userId` is not a member of the repository’s project.
- Result: Reject with “Contributor must be a project member of the same project”.

**E4 — Contributor already exists**
- Condition: A mapping already exists for (`repoId`, `userId`).
- Result: Reject with “User is already a contributor of this repository”.

**E5 — Contributor not found (remove/lookup)**
- Condition: No mapping exists for (`repoId`, `userId`) or (`repoId`, `githubUsername`).
- Result: Reject with “Contributor not found”.

---

#### REST API Contract

- `POST /api/githubrepos/{repoId}/contributors/{userId}` (add contributor)
  - Headers: `X-User-Id` (acting user)
- `DELETE /api/githubrepos/{repoId}/contributors/{userId}` (remove contributor)
  - Headers: `X-User-Id` (acting user)
- `GET /api/githubrepos/{id}` (repo details include `Contributors`)
- Optional: `GET /api/repocontributors?githubRepoId=&githubUsername=&pageNumber=&pageSize=` (if exposed; returns paged mappings)

---

#### Acceptance Criteria

- AC-01: Only project members can add/remove contributors for repositories in the project.
- AC-02: Adding a contributor requires the target user to be a member of the same project.
- AC-03: System rejects duplicate contributor mappings for (`repoId`, `userId`).
- AC-04: Removing a contributor requires the mapping to exist and returns success.
- AC-05: Repository details include `Contributors` list with `GithubUsername`, `UserId`, `UserFullName`, and `AddedAt`.
- AC-06: API respects security: no GitHub tokens are exposed and only authorized actors can manage contributors.
- AC-07: If a dedicated list endpoint is used, it supports paging per BR-29.

### Business Rules

- BR-22, BR-23, BR-29

---

### USE CASE SPECIFICATION — UC-13 Jira Configuration Management

| Field | Value |
|--------|---------|
| Use-case No. | UC-13 |
| Use-case Version | 1.1 |
| Use-case Name | Manage Jira Configuration |
| Author | PMSS Team |
| Date | 04/02/2026 |
| Priority | Medium |
| Primary Actor | Admin / Teacher / Project Member |
| Secondary Actors | System, Database |
| Stakeholders | Teacher, Admin, Student (indirect) |
| Frequency | Medium (per project setup + occasional maintenance) |

**Goal / Summary:** Create, view, update, delete, and test a Jira configuration linked to a project so the system can securely fetch issues for analytics and tracking.

**Description:** One Jira configuration per project stores Jira Cloud connection details (URL, email, API token, project key). API token is never exposed and is masked in responses. Connectivity can be tested, and issues can be fetched when an active configuration exists (UC-14).

**Triggers:** Project uses Jira for issue tracking and needs connectivity.

**Preconditions:**
- PRE-1. PMSS project exists.
- PRE-2. Actor is authenticated and authorized to manage the project’s integrations.

**Post Conditions:**
- POST-1. Jira configuration is persisted/updated and can be used for fetching issues.
- POST-2. API token remains confidential; responses include a masked token only.

---

#### Data Definitions

Jira configuration (`JiraConfigDto`):
- `JiraConfigId` (GUID)
- `ProjectId` (GUID)
- `ProjectName` (string)
- `JiraUrl` (string)
- `Email` (string)
- `ApiTokenMasked` (string; masked view only)
- `ProjectKey` (string; uppercase letters/numbers/underscores)
- `IsActive` (bool)
- `CreatedAt`, `UpdatedAt` (date-time)

Create (`CreateJiraConfigDto`):
- `ProjectId` (GUID, required)
- `JiraUrl` (string, required, valid URL)
- `Email` (string, required, valid email)
- `ApiToken` (string, required)
- `ProjectKey` (string, required, matches `^[A-Z][A-Z0-9_]*$`)

Update (`UpdateJiraConfigDto`):
- `JiraUrl` (string, optional, valid URL)
- `Email` (string, optional, valid email)
- `ApiToken` (string, optional)
- `ProjectKey` (string, optional, matches `^[A-Z][A-Z0-9_]*$`)
- `IsActive` (bool?, optional)

Notes:
- Only one configuration per project is allowed; creating a second config is rejected with guidance to update.
- `ApiToken` is stored but never returned; `ApiTokenMasked` is included for user feedback.

---

#### Main Success Scenarios

**MSS-1: Create Jira configuration**
1. Actor submits `CreateJiraConfigDto` for a `ProjectId`.
2. System validates project existence and that no config exists yet for the project.
3. System validates `JiraUrl`, `Email`, `ProjectKey` format, and presence of `ApiToken`.
4. System persists the configuration with `IsActive = true`, sets `CreatedAt`/`UpdatedAt`, and returns `JiraConfigDto` (masked token).

**MSS-2: View Jira configuration**
1. Actor requests the configuration by `ProjectId`.
2. System returns `JiraConfigDto` with `ApiTokenMasked`.

**MSS-3: Update Jira configuration**
1. Actor submits `UpdateJiraConfigDto` with fields to change.
2. System validates provided fields and updates only those fields.
3. System refreshes `UpdatedAt` and returns success.

**MSS-4: Delete Jira configuration**
1. Actor requests delete by `ProjectId`.
2. System removes the configuration and returns no content.

**MSS-5: Test Jira connection**
1. Actor triggers a connection test for `ProjectId`.
2. System attempts to call Jira using stored config.
3. System returns connection status (success or failure with details).

---

#### Alternative Scenarios

**A1 — Deactivate configuration**
- Actor sets `IsActive = false`; system retains config but UC-14 fetching requires an active config.

**A2 — Partial update**
- Actor provides only certain fields (e.g., rotates `ApiToken`); system updates those fields and retains others.

---

#### Exceptions

**E1 — Project not found**
- Condition: `ProjectId` does not exist.
- Result: Reject with “Project not found”.

**E2 — Configuration already exists (create)**
- Condition: A configuration already exists for `ProjectId`.
- Result: Reject with “Jira configuration already exists. Use PUT to update.”

**E3 — Configuration not found (get/update/delete/test)**
- Condition: No configuration exists for `ProjectId`.
- Result: Return not found.

**E4 — Validation errors**
- Condition: Invalid `JiraUrl`, `Email`, or `ProjectKey` format.
- Result: Reject with validation error details.

**E5 — Connection failed (test/fetch)**
- Condition: Jira API request fails (network/auth/endpoint issues).
- Result: Return 400 or 502 with diagnostics.

**E6 — No active configuration (test/fetch)**
- Condition: Configuration exists but `IsActive = false`.
- Result: Return not found or bad request indicating inactive configuration.

---

#### REST API Contract

- `POST /api/jira/config` (create)
  - Body: `CreateJiraConfigDto`
- `GET /api/jira/config/{projectId}` (get by project)
- `PUT /api/jira/config/{projectId}` (update)
  - Body: `UpdateJiraConfigDto`
- `DELETE /api/jira/config/{projectId}` (delete)
- `POST /api/jira/config/{projectId}/test` (test connection)
- Related (UC-14): `GET /api/jira/fetch/{projectId}` (fetch issues)

---

#### Acceptance Criteria

- AC-01: Creating a config requires `ProjectId`, valid `JiraUrl`, valid `Email`, `ApiToken`, and `ProjectKey` that matches the pattern.
- AC-02: Only one Jira configuration can exist per project; second create attempt is rejected.
- AC-03: `ApiToken` is never returned by the API; responses include `ApiTokenMasked`.
- AC-04: Updating a configuration refreshes `UpdatedAt` and updates only provided fields.
- AC-05: Deleting a configuration removes it and returns 204.
- AC-06: Test connection returns success when Jira is reachable; otherwise returns an error with details.
- AC-07: Fetching issues requires an active configuration (UC-14 dependency).

### Business Rules

- BR-24, BR-25, BR-29

---

### USE CASE SPECIFICATION — UC-14 Fetch Jira Issues

| Field | Value |
|--------|---------|
| Use-case No. | UC-14 |
| Use-case Version | 1.1 |
| Use-case Name | Fetch Jira Issues |
| Author | PMSS Team |
| Date | 04/02/2026 |
| Priority | Medium |
| Primary Actors | Teacher, Admin |
| Secondary Actors | System, Jira REST API |
| Stakeholders | Teacher, Admin, Student (indirect) |
| Frequency | Medium (per sprint or milestone review) |

**Goal / Summary:** Retrieve raw Jira issues for a project using its active Jira configuration to support visibility, analysis, and reporting.

**Description:** The endpoint returns a pass-through JSON payload from Jira (e.g., the Jira Search API response), including issue metadata such as key, summary, status, assignee, priority, labels, and timestamps. No sensitive credentials are returned.

**Triggers:** A teacher/admin wants to view current Jira issues for a project.

**Preconditions:**
- PRE-1. A Jira configuration exists for the project and is `IsActive = true` (UC-13).
- PRE-2. Actor is authenticated and authorized to view the project’s integrations.

**Post Conditions:**
- POST-1. System returns the raw JSON Jira issues payload.
- POST-2. No PMSS data is modified (read-only UC); credentials remain confidential.

---

#### Data Definitions

Raw Jira issues JSON (pass-through):
- Top-level fields commonly include: `issues` (array), `startAt`, `maxResults`, `total`.
- Each issue commonly contains: `key`, `fields.summary`, `fields.status.name`, `fields.assignee.displayName` (optional), `fields.priority.name`, `fields.labels[]`, `fields.created`, `fields.updated`.

Notes:
- The exact shape depends on Jira Cloud’s REST response; PMSS does not transform the payload.
- API tokens are never included in responses; configuration values (email/token) remain server-side only.

---

#### Main Success Scenario

1. Actor requests Jira issues for a `ProjectId`.
2. System validates that a Jira configuration exists and is active for the project.
3. System uses stored `JiraUrl`, `Email`, `ApiToken`, and `ProjectKey` to call Jira and fetch issues.
4. System returns the raw JSON payload with `Content-Type: application/json`.

---

#### Alternative Scenarios

**A1 — Inactive configuration**
- Configuration exists but `IsActive = false`; system rejects the fetch and indicates the configuration is inactive.

**A2 — Partial/unexpected Jira response**
- Jira returns a different shape or partial fields; system still returns the raw payload for the frontend to handle.

---

#### Exceptions

**E1 — No active Jira configuration**
- Condition: No configuration exists or it is inactive.
- Result: 404 Not Found with message.

**E2 — Invalid configuration**
- Condition: Configuration is incomplete/invalid (e.g., bad URL or missing key).
- Result: 400 Bad Request with message.

**E3 — Jira request failure**
- Condition: Network/authentication/remote error when calling Jira.
- Result: 502 Bad Gateway with diagnostics (message and details).

---

#### REST API Contract

- `GET /api/jira/fetch/{projectId}` (fetch issues)
  - Path params: `projectId` (GUID)
  - Response: Raw JSON (pass-through from Jira), `Content-Type: application/json`
  - Errors: 404 when no active configuration; 400 for invalid config; 502 for Jira failures

---

#### Acceptance Criteria

- AC-01: Endpoint requires an active Jira configuration for the project; otherwise returns 404.
- AC-02: Response is raw JSON with `Content-Type: application/json` and contains `issues` when Jira returns them.
- AC-03: No sensitive credentials are included in responses (email/token never exposed).
- AC-04: Invalid/incomplete configuration returns 400 with a clear error.
- AC-05: Jira communication failures return 502 with an error and details.
- AC-06: Endpoint is read-only and does not modify PMSS data.

### Business Rules

- BR-25

---

### USE CASE SPECIFICATION — UC-15 Access Request Workflow (Private Repos)

| Field | Value |
|--------|---------|
| Use-case No. | UC-15 |
| Use-case Version | 1.1 |
| Use-case Name | Request Access to Private Repositories |
| Author | PMSS Team |
| Date | 04/02/2026 |
| Priority | Medium |
| Primary Actors | Student (Requester), Teacher/Admin (Reviewer) |
| Secondary Actors | System, Database |
| Stakeholders | Student, Teacher, Admin |
| Frequency | Medium (per private repo collaboration need) |

**Goal / Summary:** Allow students to request access to private GitHub repositories linked to their project, and enable teachers/admins to approve or reject these requests with auditable status and timestamps.

**Description:** An `AccessRequest` captures the requester, project, status (`Pending` → `Approved`/`Rejected`), and timestamps. The system prevents duplicate pending requests for the same requester/project and sets `ResolvedAt` on terminal statuses.

**Triggers:** Student needs access to private repos for collaboration or evaluation.

**Preconditions:**
- PRE-1. Target `ProjectId` exists.
- PRE-2. Requester is a valid user; ideally a member of the project’s team (recommended policy).
- PRE-3. Project has at least one private repository (recommended policy to enable meaningful access).

**Post Conditions:**
- POST-1. `AccessRequest` is created/updated with correct status and timestamps.
- POST-2. Review decisions are auditable via status and `ResolvedAt`.

---

#### Data Definitions

Access request (`AccessRequestDto`):
- `RequestId` (GUID)
- `RequesterId` (GUID)
- `RequesterName` (string)
- `ProjectId` (GUID)
- `ProjectName` (string)
- `Status` (enum: `Pending`, `Approved`, `Rejected`)
- `RequestedAt` (date-time)
- `ResolvedAt` (date-time?; set when status becomes terminal)

Create (`CreateAccessRequestDto`):
- `RequesterId` (GUID, required)
- `ProjectId` (GUID, required)

Update status (`UpdateAccessRequestStatusDto`):
- `Status` (enum: `Approved` or `Rejected`)

Filter (`AccessRequestFilterParams`):
- Common paging: `pageNumber`, `pageSize`
- Filters: `requesterId?`, `projectId?`, `status?`

Notes:
- At most one `Pending` request per (`RequesterId`, `ProjectId`) exists.
- `ResolvedAt` is set when status changes from `Pending` to a terminal value (`Approved`/`Rejected`).

---

#### Main Success Scenarios

**MSS-1: Create access request**
1. Student submits `CreateAccessRequestDto` for a project.
2. System validates project and requester exist; verifies no `Pending` request already exists for this pair.
3. System persists the request with `Status = Pending` and `RequestedAt = now`.
4. System returns `AccessRequestDto`.

**MSS-2: List and review pending requests**
1. Teacher/Admin lists access requests filtered by `status = Pending` (with paging).
2. System returns a paged list including `RequesterName`, `ProjectName`, and timestamps.

**MSS-3: Approve or reject a request**
1. Teacher/Admin selects a `RequestId` and submits `UpdateAccessRequestStatusDto` with `Approved` or `Rejected`.
2. System validates role (only Teacher/Admin can update status) and that current status is `Pending`.
3. System updates `Status`, sets `ResolvedAt = now`, and returns success.

**MSS-4: View requests by requester/project**
1. Actor lists requests by `RequesterId` or by `ProjectId` with optional `status` filter and paging.
2. System returns a paged list.

---

#### Alternative Scenarios

**A1 — Withdraw or delete pending request (optional policy)**
- Student deletes their own `Pending` request before review; system removes the record.

**A2 — Duplicate request while pending**
- Student attempts to create a new request for the same (`RequesterId`, `ProjectId`) while one is `Pending`; system rejects duplicate pending.

---

#### Exceptions

**E1 — Project not found**
- Condition: `ProjectId` does not exist.
- Result: Reject with “Project not found”.

**E2 — Requester not found**
- Condition: `RequesterId` does not exist.
- Result: Reject with “Requester not found”.

**E3 — Duplicate pending request**
- Condition: A `Pending` request already exists for (`RequesterId`, `ProjectId`).
- Result: Reject with “A pending access request already exists for this project”.

**E4 — Unauthorized status update**
- Condition: Actor is not Teacher/Admin.
- Result: Reject with “Only teacher/admin can approve/reject access requests”.

**E5 — Invalid status transition**
- Condition: Attempt to change status from terminal (`Approved`/`Rejected`) or to an invalid value.
- Result: Reject with “Invalid status transition”.

**E6 — Access request not found**
- Condition: `RequestId` does not exist.
- Result: Reject with “Access request not found”.

---

#### REST API Contract

- `POST /api/accessrequests` (create)
  - Body: `CreateAccessRequestDto`
- `GET /api/accessrequests` (paged list)
  - Query: `requesterId?`, `projectId?`, `status?`, `pageNumber`, `pageSize`
- `GET /api/accessrequests/{id}` (get by id)
- `GET /api/accessrequests/requester/{requesterId}` (list by requester)
- `GET /api/accessrequests/project/{projectId}` (list by project)
- `PUT /api/accessrequests/{id}/status` (approve/reject)
  - Body: `UpdateAccessRequestStatusDto`
- `DELETE /api/accessrequests/{id}` (delete; optional policy: allow only when `Pending`)

GraphQL (read-only):
- `query { accessRequests { nodes { requestId requesterId projectId status requestedAt resolvedAt } } }`
  - Supports filtering/sorting/paging via GraphQL middleware.

---

#### Acceptance Criteria

- AC-01: Creating a request requires valid `RequesterId` and `ProjectId` and sets `Status = Pending`, `RequestedAt`.
- AC-02: System prevents duplicate `Pending` requests per (`RequesterId`, `ProjectId`).
- AC-03: Only Teacher/Admin can set status to `Approved` or `Rejected` for a `Pending` request.
- AC-04: Changing status to a terminal value sets `ResolvedAt`.
- AC-05: Terminal requests cannot be changed back to `Pending`.
- AC-06: Listing endpoints support paging and filtering by requester/project/status (BR-29).
- AC-07: DTOs include `RequesterName` and `ProjectName` for review context.

### Business Rules

- BR-26, BR-27, BR-28, BR-29

---

### USE CASE SPECIFICATION — UC-16 GraphQL Read Access

| Field | Value |
|--------|---------|
| Use-case No. | UC-16 |
| Use-case Version | 1.0 |
| Use-case Name | Query Data via GraphQL |
| Author | PMSS Team |
| Date | 04/02/2026 |
| Priority | Medium |
| Primary Actor | Frontend |
| Secondary Actor | System |

**Description:** Frontend queries users/classes/projects/repos/configs via GraphQL with paging/filtering/sorting.

**Triggers:** Frontend needs flexible read models.

**Preconditions:**
- PRE-1. GraphQL endpoint is available.

**Post Conditions:**
- POST-1. Query results reflect latest database state.

### Business Rules

- BR-29

---

## 3.3 State Diagrams

Insert state diagrams for entities with lifecycle complexity.

### SD-01 Access Request Status

States:
- Pending
- Approved
- Rejected

Transitions:
- Pending → Approved (reviewer approves)
- Pending → Rejected (reviewer rejects)
- Approved/Rejected → (terminal; optional future: re-open)

### SD-02 Jira Config Activity

States:
- Active
- Inactive

Transitions:
- Active ↔ Inactive (toggle)

---

## 3.4 Data Flow Diagrams

Insert DFDs for critical processes.

Recommended DFDs:

- DFD-01 Academic setup (Semester/Course/Class)
- DFD-02 Enrollment (single/bulk)
- DFD-03 Project creation and team assignment
- DFD-04 GitHub contribution dashboard (project → repos → GitHub API → aggregation)
- DFD-05 Jira issue fetch (project → config → Jira API)
- DFD-06 Access request workflow

---

## 3.5 Logical Data Model

### Core Entities (from Domain)

- **User** (`UserId`, `Name`, `Email`, `HashedPassword`, `Role`, `GithubUsername`, `GithubEmail`, timestamps)
- **Semester** (`SemesterId`, `Name`, `StartDate`, `EndDate`, timestamps)
- **Course** (`CourseId`, `Code`, `Name`, `Description`, timestamps)
- **Class** (`ClassId`, `SemesterId`, `CourseId`, `ClassCode`, `TeacherId`, timestamps)
- **ClassEnrollment** (`ClassId`, `UserId`, `CourseId`, `EnrolledAt`) — composite key (`ClassId`,`UserId`)
- **Project** (`ProjectId`, `ClassId`, `Name`, `Description`, timestamps)
- **ProjectMember** (`ProjectId`, `UserId`, `JoinedAt`) — composite key (`ProjectId`,`UserId`)
- **GithubRepo** (`GithubRepoId`, `ProjectId`, `RepoOwnerName`, `RepoName`, `IsPrivate`, `ApiToken?`, timestamps)
- **RepoContributor** (`GithubRepoId`, `GithubUsername`, `GithubEmail?`, `UserId?`, `AddedAt`) — composite key (`GithubRepoId`,`GithubUsername`)
- **JiraConfig** (`JiraConfigId`, `ProjectId`, `JiraUrl`, `Email`, `ApiToken`, `ProjectKey`, `IsActive`, timestamps)
- **AccessRequest** (`RequestId`, `RequesterId`, `ProjectId`, `Status`, `RequestedAt`, `ResolvedAt?`)

### Key Relationships

- Semester 1—N Class
- Course 1—N Class
- User(Teacher) 1—N Class
- Class 1—N Project
- Class M—N User via ClassEnrollment
- Project M—N User via ProjectMember
- Project 1—N GithubRepo
- GithubRepo 1—N RepoContributor
- Project 1—0..1 JiraConfig
- Project 1—N AccessRequest
- User 1—N AccessRequest (as requester)

### Data Constraints (minimum)

- Course `Code` must be unique.
- Class `ClassCode` should be unique within a semester.
- Semester `StartDate <= EndDate`.
- Repo URL is derived as `https://github.com/{RepoOwnerName}/{RepoName}`.
- API tokens must not be returned in API responses.

---

# 4. NON-FUNCTIONAL REQUIREMENTS

## 4.1 Usability

- Usability Requirement 1: The frontend shall provide consistent CRUD screens for all core resources (semester, course, class, users, projects) with validation messages.
- Usability Requirement 2: The system shall support search, filtering, sorting, and pagination for list pages consistent with backend query parameters.
- Usability Requirement 3: The system shall expose Swagger/OpenAPI documentation for REST endpoints.

## 4.2 Reliability

- Reliability Requirement 1: The backend shall return consistent `ApiResponse<T>` structure for REST endpoints.
- Reliability Requirement 2: The system shall implement centralized exception handling and produce non-sensitive error messages.
- Reliability Requirement 3: For GitHub stats endpoints returning HTTP 202, the system shall retry with backoff and return a clear error if still unavailable.

## 4.3 Performance

- Performance Requirement 1: Standard CRUD endpoints shall respond within 2 seconds for up to 50 concurrent users with a dataset of 10,000 records per entity.
- Performance Requirement 2: GitHub contribution endpoint shall respond within 10 seconds for up to 5 repositories per project (excluding third-party downtime), with caching recommended for production.
- Performance Requirement 3: GraphQL queries shall support server-side paging/filtering/sorting and must not return unbounded results.

## 4.4 Reusability

- Reusability Requirement 1: Backend shall maintain Clean Architecture boundaries (API → Application → Infrastructure → Domain).
- Reusability Requirement 2: DTOs shall be stable contracts for the frontend and integration clients.

## 4.5 Scalability

- Scalability Requirement 1: The system shall support scaling to 500 concurrent users by enabling stateless API instances and database connection pooling.
- Scalability Requirement 2: GitHub/Jira calls shall support caching and/or background processing for large classes/projects.

## 4.6 Security (Additional)

- Security Requirement 1: Passwords shall be stored as salted hashes (never plaintext).
- Security Requirement 2: Sensitive tokens (GitHub/Jira) shall be stored securely and masked/not returned.
- Security Requirement 3: Role-based authorization shall restrict admin-only operations (user creation, semester/course/class management) and teacher-only views (contribution dashboards across classes).
- Security Requirement 4: The current `X-User-Id` header authorization mechanism shall be replaced with JWT/OAuth2 before production.
- Security Requirement 5: Audit logging shall record administrative actions and status transitions (access requests, Jira config changes).

---

# 5. Supporting Information

## 5.1 Appendices

---

## Appendix A — Business Rules Reference

### Academic Structure

- **BR-01:** Semester start date must be before or equal to end date.
- **BR-02:** Semester name should be unique within the system.
- **BR-03:** Semesters cannot be deleted if classes exist under them (or must require admin override).
- **BR-04:** Course code is required and must be unique.
- **BR-05:** Course name is required.
- **BR-06:** Class must reference exactly one semester and one course.
- **BR-07:** Teacher assigned to a class must be a user with role `Teacher`.

### Enrollment

- **BR-08:** A student cannot be enrolled in the same class more than once.
- **BR-08a:** A student cannot be enrolled in the same course more than once within the same semester (even across different classes).
- **BR-09:** Bulk enrollment must return partial success details for failed student IDs.
- **BR-10:** Enrollment count must match enrollment list results for a class.

### Users and Roles

- **BR-11:** Email must be unique per user.
- **BR-11a:** GitHub username must be unique when set.
- **BR-12:** Password policy: minimum 8 characters; must include upper, lower, number, special character.
- **BR-13:** User role must be one of: Admin, Teacher, Student.

### Projects and Teams

- **BR-14:** Project must belong to exactly one class.
- **BR-15:** A user cannot join the same project more than once.
- **BR-16:** Only users enrolled in the project’s class can become project members (recommended).

### GitHub Repositories and Contributors

- **BR-17:** GitHub contribution dashboard must filter data by the semester date range of the project’s class.
- **BR-18:** If project has multiple repos, contributions must be aggregated across repos.
- **BR-19:** Repo contributions must be mapped to PMSS users when `RepoContributor.UserId` exists or when `User.GithubUsername` matches.
- **BR-20:** Only project members can create/update/delete GitHub repo records.
- **BR-21:** GitHub API token must never be returned via API.
- **BR-22:** Only project members can add/remove contributors.
- **BR-23:** A contributor user must be a member of the same project.

### Jira

- **BR-24:** Jira project key must match pattern `^[A-Z][A-Z0-9_]*$`.
- **BR-25:** Jira issues can be fetched only when an active Jira config exists for the project.

### Access Requests (Private Repos)

- **BR-26:** Access request status values: Pending, Approved, Rejected.
- **BR-27:** Only teacher/admin can approve/reject access requests.
- **BR-28:** Access request must store `ResolvedAt` when status moves from Pending to terminal.

### API Contract

- **BR-29:** All list endpoints must support pagination, sorting, and search using common parameters.

---

## Appendix B — Integration Requirements

- **GitHub REST API**
  - Must support authenticated calls using per-repo personal access tokens for private repos.
  - Must handle rate limiting and 202 responses.
  - Must use statistics endpoints (`/stats/contributors`, `/stats/commit_activity`).

- **Jira REST API**
  - Must support basic authentication with email + API token (Jira Cloud).
  - Must fetch issues for a configured project key.

- **GraphQL**
  - Must support query-only operations for core entities.
  - Must support filtering/sorting/paging.

---

## Appendix C — Security Requirements

- Authentication and authorization (JWT/OAuth2 required for production)
- TLS required for all network traffic
- Secrets management (no tokens in source control)
- Audit logging for admin/teacher operations

---

**Project Management Support System (PMSS)**
