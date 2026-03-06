import { gql } from "graphql-request";
import type { ProjectMemberDto, ProjectDto, GithubRepoDto } from "@/types";

/**
 * GraphQL Queries for Projects, Project Members, and GitHub Repos
 * Replacing REST GET endpoints with GraphQL for better performance
 */

// ============================================
// PROJECT MEMBERS
// ============================================

// Interface for dynamic queries with optional filters (used in useProjectMembers hook)
export interface GetProjectMembersResponse {
  projectMembers: {
    nodes: Array<{
      projectId: string;
      userId: string;
      joinedAt: string;
      project: {
        projectId: string;
        name: string;
        description: string;
        classId: string;
        class: {
          classCode: string;
          courseId: string;
          course: {
            code: string;
            name: string;
          };
        };
      };
      user: {
        userId: string;
        name: string;
        email: string;
        role: number;
      };
    }>;
  };
}

export const GET_USER_PROJECTS = gql`
  query GetUserProjects($userId: UUID!) {
    projectMembers(where: { userId: { eq: $userId } }) {
      nodes {
        projectId
        userId
        joinedAt
        project {
          projectId
          name
          description
          classId
          class {
            classCode
            courseId
            course {
              code
              name
            }
          }
        }
        user {
          userId
          name
          email
          role
        }
      }
    }
  }
`;

export interface GetUserProjectsResponse {
  projectMembers: {
    nodes: Array<{
      projectId: string;
      userId: string;
      joinedAt: string;
      project: {
        projectId: string;
        name: string;
        description: string;
        classId: string;
        class: {
          classCode: string;
          courseId: string;
          course: {
            code: string;
            name: string;
          };
        };
      };
      user: {
        userId: string;
        name: string;
        email: string;
        role: number;
      };
    }>;
  };
}

export const GET_PROJECT_MEMBERS_LIST = gql`
  query GetProjectMembersList($projectId: UUID!) {
    projectMembers(where: { projectId: { eq: $projectId } }) {
      nodes {
        projectId
        userId
        joinedAt
        project {
          projectId
          name
          description
          classId
          class {
            classCode
            courseId
            course {
              code
              name
            }
          }
        }
        user {
          userId
          name
          email
          role
        }
      }
    }
  }
`;

export interface GetProjectMembersListResponse {
  projectMembers: {
    nodes: Array<{
      projectId: string;
      userId: string;
      joinedAt: string;
      project: {
        projectId: string;
        name: string;
        description: string;
        classId: string;
        class: {
          classCode: string;
          courseId: string;
          course: {
            code: string;
            name: string;
          };
        };
      };
      user: {
        userId: string;
        name: string;
        email: string;
        role: number;
      };
    }>;
  };
}

// ============================================
// PROJECTS
// ============================================

// Interface for dynamic queries with optional filters (used in useProjects hook)
export interface GetProjectsResponse {
  projects: {
    nodes: Array<{
      projectId: string;
      name: string;
      description: string;
      classId: string;
      createdAt: string;
      updatedAt: string;
      class: {
        classId: string;
        classCode: string;
        courseId: string;
        semesterId: string;
        course: {
          courseId: string;
          code: string;
          name: string;
        };
        semester: {
          semesterId: string;
          name: string;
        };
        teacher: {
          userId: string;
          name: string;
        };
      };
    }>;
  };
}

export const GET_PROJECT = gql`
  query GetProject($projectId: UUID!) {
    projects(where: { projectId: { eq: $projectId } }) {
      nodes {
        projectId
        name
        description
        classId
        createdAt
        updatedAt
        class {
          classId
          classCode
          courseId
          semesterId
          course {
            courseId
            code
            name
          }
          semester {
            semesterId
            name
          }
          teacher {
            userId
            name
          }
        }
      }
    }
  }
`;

export interface GetProjectResponse {
  projects: {
    nodes: Array<{
      projectId: string;
      name: string;
      description: string;
      classId: string;
      createdAt: string;
      updatedAt: string;
      class: {
        classId: string;
        classCode: string;
        courseId: string;
        semesterId: string;
        course: {
          courseId: string;
          code: string;
          name: string;
        };
        semester: {
          semesterId: string;
          name: string;
        };
        teacher: {
          userId: string;
          name: string;
        };
      };
    }>;
  };
}

// ============================================
// GITHUB REPOS
// ============================================

// Interface for dynamic queries with optional filters (used in useGithubRepos hook)
export interface GetGithubReposResponse {
  githubRepos: {
    nodes: Array<{
      githubRepoId: string;
      projectId: string;
      repoOwnerName: string;
      repoName: string;
      isPrivate: boolean;
      apiToken?: string;
      totalCommits: number;
      totalAdditions: number;
      totalDeletions: number;
      lastSyncedAt?: string;
      createdAt: string;
      updatedAt: string;
      project: {
        projectId: string;
        name: string;
        classId: string;
        class: {
          classCode: string;
          courseId: string;
          course: {
            code: string;
            name: string;
          };
        };
      };
    }>;
  };
}

export const GET_GITHUB_REPO = gql`
  query GetGithubRepo($githubRepoId: UUID!) {
    githubRepos(where: { githubRepoId: { eq: $githubRepoId } }) {
      nodes {
        githubRepoId
        projectId
        repoOwnerName
        repoName
        isPrivate
        apiToken
        totalCommits
        totalAdditions
        totalDeletions
        lastSyncedAt
        createdAt
        updatedAt
        project {
          projectId
          name
        }
      }
    }
  }
`;

export interface GetGithubRepoResponse {
  githubRepos: {
    nodes: Array<{
      githubRepoId: string;
      projectId: string;
      repoOwnerName: string;
      repoName: string;
      isPrivate: boolean;
      apiToken?: string;
      totalCommits: number;
      totalAdditions: number;
      totalDeletions: number;
      lastSyncedAt?: string;
      createdAt: string;
      updatedAt: string;
      project: {
        projectId: string;
        name: string;
      };
    }>;
  };
}

// ============================================
// HELPER FUNCTIONS TO TRANSFORM RESPONSES
// ============================================

export const transformToProjectMemberDto = (
  node: GetProjectMembersResponse["projectMembers"]["nodes"][0],
): ProjectMemberDto => {
  return {
    projectId: node.projectId,
    projectName: node.project.name,
    userId: node.userId,
    userName: node.user.name,
    userEmail: node.user.email,
    githubUsername: null, // Not available in this query
    joinedAt: node.joinedAt,
  };
};

export const transformToProjectDto = (
  node: GetProjectsResponse["projects"]["nodes"][0],
): ProjectDto => {
  return {
    projectId: node.projectId,
    classId: node.classId,
    className: node.class.classCode,
    courseCode: node.class.course.code,
    courseName: node.class.course.name,
    name: node.name,
    description: node.description,
    createdAt: node.createdAt,
    updatedAt: node.updatedAt,
  };
};

export const transformToGithubRepoDto = (
  node: GetGithubReposResponse["githubRepos"]["nodes"][0],
): GithubRepoDto => {
  return {
    githubRepoId: node.githubRepoId,
    projectId: node.projectId,
    projectName: node.project.name,
    courseId: node.project.class.courseId,
    courseName: node.project.class.course.name,
    courseCode: node.project.class.course.code,
    repoOwnerName: node.repoOwnerName,
    repoName: node.repoName,
    repoUrl: `https://github.com/${node.repoOwnerName}/${node.repoName}`,
    isPrivate: node.isPrivate,
    contributorCount: 0, // Not available in GraphQL query
    createdAt: node.createdAt,
    updatedAt: node.updatedAt,
    contributors: [], // Not available in GraphQL query
  };
};
