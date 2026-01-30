# GraphQL Query Examples for Frontend

This file contains ready-to-use GraphQL queries for common scenarios in the PMSS application.

## Basic Queries

### 1. Get All Users
```graphql
query GetAllUsers {
  users {
    nodes {
      userId
      name
      email
      role
      githubUsername
      createdAt
    }
  }
}
```

### 2. Get User by ID
```graphql
query GetUserById($userId: UUID!) {
  users(where: { userId: { eq: $userId } }) {
    nodes {
      userId
      name
      email
      role
      githubUsername
      githubEmail
      projectMembers {
        project {
          projectId
          name
        }
      }
    }
  }
}
```

### 3. Get All Projects
```graphql
query GetAllProjects {
  projects {
    nodes {
      projectId
      name
      description
      createdAt
      class {
        classCode
        course {
          name
        }
      }
    }
  }
}
```

### 4. Get Project with Full Details
```graphql
query GetProjectDetails($projectId: UUID!) {
  projects(where: { projectId: { eq: $projectId } }) {
    nodes {
      projectId
      name
      description
      createdAt
      updatedAt
      class {
        classId
        classCode
        teacher {
          name
          email
        }
        course {
          name
          code
        }
        semester {
          name
          startDate
          endDate
        }
      }
      projectMembers {
        userId
        joinedAt
        user {
          userId
          name
          email
          githubUsername
        }
      }
      githubRepos {
        githubRepoId
        repoName
        repoOwnerName
        isPrivate
        repoContributors {
          githubUsername
          githubEmail
          user {
            name
          }
        }
      }
      jiraConfig {
        jiraConfigId
        jiraUrl
        isActive
      }
      accessRequests {
        requestId
        status
        requestedAt
        requester {
          name
          email
        }
      }
    }
  }
}
```

### 5. Get Projects by Class
```graphql
query GetProjectsByClass($classId: UUID!) {
  projects(where: { classId: { eq: $classId } }) {
    nodes {
      projectId
      name
      description
      projectMembers {
        user {
          name
          email
        }
      }
    }
  }
}
```

## Class Queries

### 6. Get All Classes
```graphql
query GetAllClasses {
  classes {
    nodes {
      classId
      classCode
      createdAt
      teacher {
        name
        email
      }
      course {
        name
        code
      }
      semester {
        name
      }
    }
  }
}
```

### 7. Get Class with Students
```graphql
query GetClassWithStudents($classId: UUID!) {
  classes(where: { classId: { eq: $classId } }) {
    nodes {
      classId
      classCode
      teacher {
        name
        email
      }
      course {
        name
        description
      }
      semester {
        name
        startDate
        endDate
      }
      classEnrollments {
        userId
        enrolledAt
        user {
          userId
          name
          email
          githubUsername
        }
      }
      projects {
        projectId
        name
        projectMembers {
          user {
            name
          }
        }
      }
    }
  }
}
```

### 8. Get Classes by Teacher
```graphql
query GetClassesByTeacher($teacherId: UUID!) {
  classes(where: { teacherId: { eq: $teacherId } }) {
    nodes {
      classId
      classCode
      course {
        name
      }
      semester {
        name
      }
      classEnrollments {
        user {
          name
        }
      }
    }
  }
}
```

## Student/Enrollment Queries

### 9. Get Student's Classes
```graphql
query GetStudentClasses($userId: UUID!) {
  classEnrollments(where: { userId: { eq: $userId } }) {
    nodes {
      enrolledAt
      class {
        classId
        classCode
        teacher {
          name
        }
        course {
          name
          code
        }
        semester {
          name
        }
      }
    }
  }
}
```

### 10. Get Student's Projects
```graphql
query GetStudentProjects($userId: UUID!) {
  projectMembers(where: { userId: { eq: $userId } }) {
    nodes {
      joinedAt
      project {
        projectId
        name
        description
        class {
          classCode
          course {
            name
          }
        }
        githubRepos {
          repoName
          repoOwnerName
        }
      }
    }
  }
}
```

## GitHub & Repository Queries

### 11. Get All GitHub Repos
```graphql
query GetAllGithubRepos {
  githubRepos {
    nodes {
      githubRepoId
      repoName
      repoOwnerName
      isPrivate
      createdAt
      project {
        name
      }
    }
  }
}
```

### 12. Get Repo Contributors
```graphql
query GetRepoContributors($repoId: UUID!) {
  repoContributors(where: { githubRepoId: { eq: $repoId } }) {
    nodes {
      githubUsername
      githubEmail
      addedAt
      user {
        userId
        name
        email
      }
      githubRepo {
        repoName
        repoOwnerName
      }
    }
  }
}
```

## Access Request Queries

### 13. Get Pending Access Requests
```graphql
query GetPendingAccessRequests {
  accessRequests(where: { status: { eq: PENDING } }) {
    nodes {
      requestId
      requestedAt
      requester {
        userId
        name
        email
      }
      project {
        projectId
        name
        class {
          classCode
        }
      }
    }
  }
}
```

### 14. Get User's Access Requests
```graphql
query GetUserAccessRequests($userId: UUID!) {
  accessRequests(where: { requesterId: { eq: $userId } }) {
    nodes {
      requestId
      status
      requestedAt
      resolvedAt
      project {
        projectId
        name
      }
    }
  }
}
```

## Semester & Course Queries

### 15. Get Current Semester Classes
```graphql
query GetCurrentSemesterClasses($semesterId: UUID!) {
  semesters(where: { semesterId: { eq: $semesterId } }) {
    nodes {
      semesterId
      name
      startDate
      endDate
      classes {
        classId
        classCode
        course {
          name
        }
        teacher {
          name
        }
      }
    }
  }
}
```

### 16. Get All Courses with Classes
```graphql
query GetCoursesWithClasses {
  courses {
    nodes {
      courseId
      code
      name
      description
      classes {
        classId
        classCode
        semester {
          name
        }
        teacher {
          name
        }
      }
    }
  }
}
```

## Pagination Examples

### 17. Paginated Users (First 10)
```graphql
query GetPaginatedUsers($first: Int!, $after: String) {
  users(first: $first, after: $after) {
    pageInfo {
      hasNextPage
      hasPreviousPage
      startCursor
      endCursor
    }
    nodes {
      userId
      name
      email
    }
    totalCount
  }
}
```

Variables:
```json
{
  "first": 10,
  "after": null
}
```

### 18. Paginated Projects with Sorting
```graphql
query GetPaginatedProjects($first: Int!, $after: String) {
  projects(
    first: $first
    after: $after
    order: { createdAt: DESC }
  ) {
    pageInfo {
      hasNextPage
      endCursor
    }
    nodes {
      projectId
      name
      createdAt
    }
  }
}
```

## Advanced Filtering Examples

### 19. Filter Users by Role
```graphql
query GetStudents {
  users(where: { role: { eq: STUDENT } }) {
    nodes {
      userId
      name
      email
      classEnrollments {
        class {
          classCode
        }
      }
    }
  }
}
```

### 20. Filter Projects by Date Range
```graphql
query GetRecentProjects($startDate: DateTime!) {
  projects(where: { createdAt: { gte: $startDate } }) {
    nodes {
      projectId
      name
      createdAt
      class {
        classCode
      }
    }
  }
}
```

Variables:
```json
{
  "startDate": "2024-01-01T00:00:00Z"
}
```

### 21. Complex Filtering Example
```graphql
query GetActiveProjectsWithMembers {
  projects(
    where: {
      and: [
        { projectMembers: { some: {} } }
        { githubRepos: { some: {} } }
      ]
    }
    order: { name: ASC }
  ) {
    nodes {
      projectId
      name
      projectMembers {
        user {
          name
        }
      }
      githubRepos {
        repoName
      }
    }
  }
}
```

## Search Examples

### 22. Search Projects by Name
```graphql
query SearchProjects($searchTerm: String!) {
  projects(where: { name: { contains: $searchTerm } }) {
    nodes {
      projectId
      name
      description
    }
  }
}
```

Variables:
```json
{
  "searchTerm": "Mobile"
}
```

### 23. Search Users by Name or Email
```graphql
query SearchUsers($searchTerm: String!) {
  users(
    where: {
      or: [
        { name: { contains: $searchTerm } }
        { email: { contains: $searchTerm } }
      ]
    }
  ) {
    nodes {
      userId
      name
      email
    }
  }
}
```

## Dashboard Queries

### 24. Teacher Dashboard Data
```graphql
query GetTeacherDashboard($teacherId: UUID!) {
  users(where: { userId: { eq: $teacherId } }) {
    nodes {
      name
      email
      taughtClasses {
        classId
        classCode
        course {
          name
        }
        semester {
          name
        }
        classEnrollments {
          user {
            name
          }
        }
        projects {
          projectId
          name
          projectMembers {
            user {
              name
            }
          }
        }
      }
    }
  }
}
```

### 25. Student Dashboard Data
```graphql
query GetStudentDashboard($userId: UUID!) {
  users(where: { userId: { eq: $userId } }) {
    nodes {
      userId
      name
      email
      githubUsername
      classEnrollments {
        class {
          classCode
          course {
            name
          }
          teacher {
            name
          }
        }
      }
      projectMembers {
        project {
          projectId
          name
          description
          githubRepos {
            repoName
            repoOwnerName
          }
        }
      }
      accessRequests {
        requestId
        status
        project {
          name
        }
      }
    }
  }
}
```

## Tips for Frontend Integration

1. **Use Variables**: Always use variables for dynamic values instead of string concatenation
2. **Request Only What You Need**: GraphQL allows you to request specific fields - use this to optimize performance
3. **Leverage Nested Queries**: Fetch related data in a single request instead of multiple REST calls
4. **Use Pagination**: For large datasets, always use pagination to improve performance
5. **Handle Errors**: GraphQL returns errors in a structured format - handle them appropriately
6. **Cache Queries**: Use client-side caching (like Apollo Client cache) for better performance
7. **Use Fragments**: For repeated field selections, use GraphQL fragments

## GraphQL Fragment Example

```graphql
fragment UserBasicInfo on User {
  userId
  name
  email
  githubUsername
}

fragment ProjectBasicInfo on Project {
  projectId
  name
  description
  createdAt
}

query GetProjectWithMembers($projectId: UUID!) {
  projects(where: { projectId: { eq: $projectId } }) {
    nodes {
      ...ProjectBasicInfo
      projectMembers {
        joinedAt
        user {
          ...UserBasicInfo
        }
      }
    }
  }
}
```

