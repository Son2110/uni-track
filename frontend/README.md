# PMSS Frontend - React Application

## Overview

React-based frontend for the Project Management Support System (PMSS). This will provide a modern, responsive UI for managing academic projects, GitHub integration, and Jira tracking.

## Tech Stack (Recommended)

- **React 18+** - UI Framework
- **TypeScript** - Type Safety
- **Vite** - Build Tool & Dev Server
- **React Router** - Routing
- **Axios** - HTTP Client
- **TanStack Query (React Query)** - Server State Management
- **Zustand** or **Redux Toolkit** - Client State Management
- **Tailwind CSS** or **Material-UI** - Styling
- **React Hook Form** - Form Handling
- **Zod** - Schema Validation

## Prerequisites

- [Node.js 18+](https://nodejs.org/) (LTS version recommended)
- [npm](https://www.npmjs.com/) or [yarn](https://yarnpkg.com/) or [pnpm](https://pnpm.io/)
- Code editor ([VS Code](https://code.visualstudio.com/) recommended)

## Project Setup (Not Yet Created)

When your team creates the React app, use one of these commands:

### Option 1: Vite (Recommended - Faster)

```bash
cd frontend
npm create vite@latest . -- --template react-ts
npm install
```

### Option 2: Create React App

```bash
cd frontend
npx create-react-app . --template typescript
```

## Quick Start (After Setup)

### 1. Install Dependencies

```bash
cd frontend
npm install
```

### 2. Configure Environment

Create `.env.local` file:

```env
VITE_API_BASE_URL=http://localhost:5000/api
VITE_API_TIMEOUT=30000
```

### 3. Start Development Server

```bash
npm run dev
```

The app will be available at `http://localhost:5173` (Vite) or `http://localhost:3000` (CRA)

## Recommended Project Structure

```
frontend/
├── public/
│   ├── favicon.ico
│   └── assets/
├── src/
│   ├── api/                    # API client & endpoints
│   │   ├── client.ts           # Axios instance
│   │   ├── endpoints/
│   │   │   ├── projects.ts
│   │   │   ├── users.ts
│   │   │   └── courses.ts
│   │   └── types/              # API response types
│   ├── components/             # Reusable components
│   │   ├── common/
│   │   │   ├── Button.tsx
│   │   │   ├── Input.tsx
│   │   │   └── Modal.tsx
│   │   ├── layout/
│   │   │   ├── Header.tsx
│   │   │   ├── Sidebar.tsx
│   │   │   └── Footer.tsx
│   │   └── features/
│   │       ├── ProjectCard.tsx
│   │       └── UserProfile.tsx
│   ├── pages/                  # Page components
│   │   ├── Dashboard.tsx
│   │   ├── Projects/
│   │   │   ├── ProjectList.tsx
│   │   │   ├── ProjectDetail.tsx
│   │   │   └── CreateProject.tsx
│   │   ├── Users/
│   │   └── Courses/
│   ├── hooks/                  # Custom React hooks
│   │   ├── useAuth.ts
│   │   ├── useProjects.ts
│   │   └── usePagination.ts
│   ├── store/                  # State management
│   │   ├── authStore.ts
│   │   └── uiStore.ts
│   ├── utils/                  # Utility functions
│   │   ├── formatters.ts
│   │   └── validators.ts
│   ├── types/                  # TypeScript types
│   │   └── index.ts
│   ├── router/                 # Routing configuration
│   │   └── index.tsx
│   ├── styles/                 # Global styles
│   │   └── globals.css
│   ├── App.tsx
│   ├── main.tsx
│   └── vite-env.d.ts
├── .env.example
├── .env.local
├── package.json
├── tsconfig.json
├── vite.config.ts
└── README.md
```

## Recommended Dependencies

```bash
# Core dependencies
npm install react-router-dom
npm install axios
npm install @tanstack/react-query
npm install zustand  # or @reduxjs/toolkit
npm install react-hook-form
npm install zod

# UI Framework (choose one)
npm install @mui/material @mui/icons-material @emotion/react @emotion/styled  # Material-UI
# OR
npm install -D tailwindcss postcss autoprefixer  # Tailwind CSS

# Dev dependencies
npm install -D @types/node
npm install -D eslint @typescript-eslint/parser @typescript-eslint/eslint-plugin
npm install -D prettier eslint-config-prettier eslint-plugin-prettier
```

## API Integration

### Example API Client Setup

```typescript
// src/api/client.ts
import axios from 'axios';

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor for auth token
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    // Handle errors globally
    return Promise.reject(error);
  }
);

export default apiClient;
```

### Example API Endpoint

```typescript
// src/api/endpoints/projects.ts
import apiClient from '../client';
import { Project, ApiResponse, PagedResult } from '../types';

export const projectsApi = {
  getAll: (params?: {
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
  }) =>
    apiClient.get<ApiResponse<PagedResult<Project>>>('/projects', { params }),

  getById: (id: number) =>
    apiClient.get<ApiResponse<Project>>(`/projects/${id}`),

  create: (data: Partial<Project>) =>
    apiClient.post<ApiResponse<Project>>('/projects', data),

  update: (id: number, data: Partial<Project>) =>
    apiClient.put<ApiResponse<Project>>(`/projects/${id}`, data),

  delete: (id: number) =>
    apiClient.delete<ApiResponse<void>>(`/projects/${id}`),
};
```

## Development Guidelines

### Component Best Practices

1. **Use Functional Components** with hooks
2. **TypeScript** for type safety
3. **Props Interface** for every component
4. **Composition** over inheritance
5. **Custom Hooks** for reusable logic

### Example Component

```typescript
import React from 'react';

interface ProjectCardProps {
  id: number;
  name: string;
  description: string;
  onEdit: (id: number) => void;
}

export const ProjectCard: React.FC<ProjectCardProps> = ({
  id,
  name,
  description,
  onEdit,
}) => {
  return (
    <div className="project-card">
      <h3>{name}</h3>
      <p>{description}</p>
      <button onClick={() => onEdit(id)}>Edit</button>
    </div>
  );
};
```

## Available Scripts

```bash
# Start development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Run linting
npm run lint

# Run tests (when configured)
npm run test

# Format code
npm run format
```

## Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `VITE_API_BASE_URL` | Backend API URL | `http://localhost:5000/api` |
| `VITE_API_TIMEOUT` | Request timeout (ms) | `30000` |

## Features to Implement

### Phase 1: Core Features
- [ ] User authentication (login/register)
- [ ] Dashboard with statistics
- [ ] Project CRUD operations
- [ ] User management
- [ ] Course management

### Phase 2: Advanced Features
- [ ] GitHub repository integration
- [ ] Jira configuration
- [ ] Team member management
- [ ] Access request workflow
- [ ] Contribution statistics

### Phase 3: Polish
- [ ] Dark mode
- [ ] Responsive design
- [ ] Loading states
- [ ] Error boundaries
- [ ] Toast notifications

## Styling Recommendations

### Tailwind CSS Setup

```bash
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
```

### Material-UI Setup

```bash
npm install @mui/material @mui/icons-material @emotion/react @emotion/styled
```

## Testing

```bash
# Install testing libraries
npm install -D vitest @testing-library/react @testing-library/jest-dom @testing-library/user-event

# Run tests
npm run test
```

## Deployment

### Build for Production

```bash
npm run build
```

Output will be in the `dist/` folder (Vite) or `build/` folder (CRA).

### Deploy Options

- **Vercel** - Automatic deployment from GitHub
- **Netlify** - Simple drag & drop or GitHub integration
- **GitHub Pages** - Free hosting for static sites
- **AWS S3 + CloudFront** - Scalable solution

## Troubleshooting

### CORS Issues

If you encounter CORS errors, ensure the backend has CORS configured for your frontend URL.

### API Connection Issues

1. Verify backend is running (`http://localhost:5000`)
2. Check `.env.local` has correct `VITE_API_BASE_URL`
3. Use browser DevTools Network tab to inspect requests

## Resources

- [React Documentation](https://react.dev/)
- [TypeScript Documentation](https://www.typescriptlang.org/)
- [Vite Documentation](https://vitejs.dev/)
- [TanStack Query](https://tanstack.com/query/latest)
- [React Router](https://reactrouter.com/)

## Contributing

See [CONTRIBUTING.md](../CONTRIBUTING.md) in the root directory.

## License

[Your License]
