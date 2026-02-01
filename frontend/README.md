# UniTrack - Academic Project Management System

> A modern, full-stack academic project management platform for FPT University, built with React, TypeScript, and Vite.

![TypeScript](https://img.shields.io/badge/TypeScript-5.8-blue)
![React](https://img.shields.io/badge/React-19.1-61dafb)
![Vite](https://img.shields.io/badge/Vite-6.3-646cff)
![TailwindCSS](https://img.shields.io/badge/TailwindCSS-3.4-38bdf8)

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Available Scripts](#available-scripts)
- [Development Guidelines](#development-guidelines)
- [Environment Setup](#environment-setup)

---

## 🎯 Overview

**UniTrack** is a comprehensive academic project management system designed for FPT University. It provides a centralized platform for managing:

- **Students & Teachers** - User management with role-based access control
- **Semesters** - Academic term scheduling and management
- **Courses** - Course catalog and curriculum management
- **Classes** - Class scheduling, enrollment tracking
- **Projects** - Student project tracking with GitHub and Jira integration

The system features a modern, responsive UI with support for both light and dark modes, seamlessly integrating with FPT University's authentication system (FeID and Google OAuth).

---

## 🛠️ Tech Stack

### Core
- **React 19.1** - UI library
- **TypeScript 5.8** - Type safety
- **Vite 6.3** - Build tool & dev server

### Styling
- **Tailwind CSS 3.4** - Utility-first CSS framework
- **clsx + tailwind-merge** - Dynamic class management

### Routing & State
- **React Router DOM 7.13** - Client-side routing
- **Zustand 5.0** - Global state management

### UI Components
- **Lucide React** - Modern icon library
- Custom UI component library (Button, Card, Table, Badge, Input)

### Development Tools
- **ESLint** - Code linting
- **PostCSS** - CSS processing
- **Autoprefixer** - CSS vendor prefixing

---

## 🏗️ Architecture

UniTrack follows a **Feature-Based Architecture** for maintainability and scalability:

```
src/
├── app/                    # Global application setup
│   ├── router.tsx         # Route configuration
│   └── store.ts           # Global state (Zustand)
│
├── components/            # Shared UI components
│   ├── ui/               # Base components (Button, Card, Table, etc.)
│   ├── icons/            # Custom icon components
│   ├── Header.tsx        # Top navigation header
│   └── Sidebar.tsx       # Main navigation sidebar
│
├── features/             # Feature modules
│   ├── auth/            # Authentication (Login, OAuth)
│   ├── dashboard/       # Dashboard statistics & charts
│   ├── academic/        # Semesters, Courses, Classes
│   └── users/           # User management
│
├── layouts/              # Layout wrappers
│   ├── AdminLayout.tsx  # Admin dashboard layout
│   └── AuthLayout.tsx   # Authentication pages layout
│
├── data/                 # Mock data (to be replaced with API)
│   └── mockData.ts      # Centralized mock data
│
├── types/                # TypeScript type definitions
│   └── index.ts         # Shared interfaces
│
├── lib/                  # Utility functions
│   └── utils.ts         # Helper functions (cn, etc.)
│
└── main.tsx             # Application entry point
```

### Key Architectural Principles

1. **Feature-Based Organization**: Each feature (auth, dashboard, academic, users) is self-contained with its own pages, components, and logic.

2. **Shared UI Components**: Reusable components in `components/ui/` ensure consistency across the app.

3. **Type Safety**: Strict TypeScript interfaces in `types/index.ts` matching the database schema.

4. **Separation of Concerns**: 
   - **Layouts** handle page structure
   - **Features** contain business logic
   - **Components** are pure & reusable

5. **Data Flow**:
   - Mock data centralized in `data/mockData.ts`
   - Global state managed via Zustand (`app/store.ts`)
   - Component-level state using React hooks

---

## 🚀 Getting Started

### Prerequisites

- **Node.js** v18+ (recommended v20+)
- **npm** v9+ or **yarn** v1.22+

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd PMSS/frontend
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Start development server**
   ```bash
   npm run dev
   ```

4. **Open in browser**
   ```
   http://localhost:5173
   ```

---

## 📂 Project Structure

```
frontend/
├── public/               # Static assets
│   └── favicon.svg      # Custom GraduationCap favicon
│
├── src/
│   ├── app/             # Router & global state
│   ├── components/      # Shared components
│   ├── data/            # Mock data
│   ├── features/        # Feature modules
│   ├── layouts/         # Page layouts
│   ├── lib/             # Utilities
│   ├── types/           # TypeScript definitions
│   ├── index.css        # Global styles
│   └── main.tsx         # Entry point
│
├── index.html           # HTML template
├── package.json         # Dependencies
├── tailwind.config.js   # Tailwind configuration
├── tsconfig.app.json    # TypeScript config
├── vite.config.ts       # Vite configuration
└── README.md            # This file
```

---

## 🎨 Available Scripts

| Script | Command | Description |
|--------|---------|-------------|
| **Dev Server** | `npm run dev` | Start development server with HMR |
| **Build** | `npm run build` | Build for production (outputs to `dist/`) |
| **Preview** | `npm run preview` | Preview production build locally |
| **Lint** | `npm run lint` | Run ESLint to check code quality |

---

## 📘 Development Guidelines

### Code Style

- **TypeScript**: Use strict typing, avoid `any`
- **Components**: Functional components with TypeScript
- **Naming**: PascalCase for components, camelCase for functions/variables
- **File Naming**: 
  - Components: `ComponentName.tsx`
  - Utilities: `utilityName.ts`
  - Types: `index.ts`

### Component Structure

```tsx
import React from 'react';
import type { PropsInterface } from '@/types';

interface ComponentProps {
  // Props definition
}

export const Component: React.FC<ComponentProps> = ({ props }) => {
  // Component logic
  
  return (
    // JSX
  );
};
```

### Styling Guidelines

- Use Tailwind utility classes
- Use `cn()` utility for conditional classes
- Follow color scheme: `primary` (#1E5BB8), `fpt-orange` (#F37021)
- Support dark mode with `dark:` variants

### Adding New Features

1. Create feature directory in `src/features/`
2. Add `pages/` and `components/` subdirectories
3. Define routes in `app/router.tsx`
4. Add types to `types/index.ts`
5. Update mock data in `data/mockData.ts` (until API is connected)

---

## 🔧 Environment Setup

### Path Aliases

The project uses `@/` as an alias for `src/`:

```typescript
import { Button } from '@/components/ui/Button';
import { mockUsers } from '@/data/mockData';
```

Configured in:
- `tsconfig.app.json` - TypeScript path resolution
- `vite.config.ts` - Vite module resolution

### Custom Tailwind Theme

Colors defined in `tailwind.config.js`:

```javascript
colors: {
  primary: '#1E5BB8',          // FPT Blue
  'fpt-orange': '#F37021',     // FPT Orange
  'background-light': '#F9FAFB',
  'background-dark': '#111827',
  'card-light': '#ffffff',
  'card-dark': '#111827',
}
```

---

## 🗄️ Database Schema

Types in `src/types/index.ts` map directly to the database schema:

| Entity | Key Fields |
|--------|-----------|
| **User** | userId, name, email, role, studentOrEmployeeId |
| **Semester** | semesterId, name, startDate, endDate |
| **Course** | courseId, code, name, description |
| **Class** | classId, classCode, teacherId, courseId, semesterId |
| **Project** | projectId, classId, name, description |
| **ClassEnrollment** | classId, userId, enrolledAt |

---

## 🔮 Future Enhancements

- [ ] Connect to backend API (replace mock data)
- [ ] Implement real authentication (OAuth, FeID)
- [ ] Add project GitHub integration
- [ ] Add project Jira integration
- [ ] Implement file upload functionality
- [ ] Add real-time notifications
- [ ] Export data to Excel/PDF
- [ ] Advanced filtering and search
- [ ] User profile management
- [ ] Project activity tracking

---

