# Contributing to PMSS

Thank you for your interest in contributing to the Project Management Support System! This document provides guidelines and instructions for contributing to the project.

## 🤝 Code of Conduct

- Be respectful and inclusive
- Welcome newcomers and help them get started
- Focus on constructive feedback
- Respect differing viewpoints and experiences

## 🚀 Getting Started

### 1. Fork & Clone

```bash
# Fork the repository on GitHub
# Then clone your fork
git clone https://github.com/YOUR_USERNAME/PMSS.git
cd PMSS
```

### 2. Set Up Development Environment

#### Backend Setup
```bash
cd backend
dotnet restore
cd PMSS.API
dotnet ef database update --project ../PMSS.Infrastructure
dotnet run
```

#### Frontend Setup (when available)
```bash
cd frontend
npm install
npm run dev
```

### 3. Create a Branch

```bash
git checkout -b feature/your-feature-name
```

## 📝 Commit Message Guidelines

We follow the [Conventional Commits](https://www.conventionalcommits.org/) specification.

### Format

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

### Types

- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, semicolons, etc.)
- `refactor`: Code refactoring without changing functionality
- `perf`: Performance improvements
- `test`: Adding or updating tests
- `chore`: Maintenance tasks, dependencies, etc.
- `ci`: CI/CD configuration changes

### Examples

```bash
feat(api): add endpoint for project statistics
fix(db): resolve foreign key constraint issue
docs(readme): update installation instructions
style(backend): format code with dotnet format
refactor(services): simplify project service logic
test(controllers): add unit tests for ProjectsController
chore(deps): update Entity Framework to 10.0.1
```

### Scope Examples

**Backend:**
- `api`, `controllers`, `services`, `repositories`, `db`, `models`, `dto`

**Frontend:**
- `ui`, `components`, `pages`, `hooks`, `api`, `store`, `utils`

## 🌿 Branch Naming

### Format

```
<type>/<short-description>
```

### Examples

```
feature/github-integration
fix/user-login-validation
docs/api-documentation
refactor/project-service
test/user-controller
chore/update-dependencies
```

## 🔀 Pull Request Process

### 1. Before Creating PR

- [ ] Code follows project style guidelines
- [ ] All tests pass locally
- [ ] Added/updated tests for new features
- [ ] Updated documentation if needed
- [ ] Commits follow commit message guidelines
- [ ] No merge conflicts with main branch

### 2. Creating the PR

1. Push your branch to GitHub
```bash
git push origin feature/your-feature-name
```

2. Go to the repository on GitHub
3. Click "New Pull Request"
4. Fill out the PR template

### 3. PR Title Format

Use the same format as commit messages:
```
feat(api): add project statistics endpoint
```

### 4. PR Description Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Changes Made
- Change 1
- Change 2
- Change 3

## Testing Done
- Test case 1
- Test case 2

## Screenshots (if applicable)
[Add screenshots here]

## Related Issues
Closes #123
Relates to #456
```

### 5. Review Process

- At least one approval required
- All CI checks must pass
- Address reviewer feedback
- Keep discussions focused and respectful

## 🎨 Code Style Guidelines

### Backend (.NET/C#)

#### Naming Conventions

```csharp
// PascalCase for classes, methods, properties
public class ProjectService { }
public async Task<Project> GetProjectAsync(int id) { }
public string ProjectName { get; set; }

// camelCase for local variables, parameters
var projectList = new List<Project>();
public void UpdateProject(int projectId) { }

// UPPER_CASE for constants
public const string DEFAULT_CONNECTION = "DefaultConnection";

// Prefix interfaces with 'I'
public interface IProjectService { }
```

#### File Organization

```csharp
// 1. Using statements
using System;
using System.Collections.Generic;

// 2. Namespace
namespace PMSS.Application.Services
{
    // 3. Class
    public class ProjectService
    {
        // 4. Private fields
        private readonly IProjectRepository _repository;
        
        // 5. Constructor
        public ProjectService(IProjectRepository repository)
        {
            _repository = repository;
        }
        
        // 6. Public methods
        public async Task<Project> GetAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
        
        // 7. Private methods
        private bool ValidateProject(Project project)
        {
            return project != null;
        }
    }
}
```

#### Best Practices

- Use `async/await` for I/O operations
- Prefer dependency injection over static classes
- Follow SOLID principles
- Use meaningful variable names
- Add XML comments for public APIs
- Keep methods small and focused

### Frontend (React/TypeScript)

#### Naming Conventions

```typescript
// PascalCase for components
export const ProjectCard: React.FC = () => { };

// camelCase for functions, variables
const handleSubmit = () => { };
const projectList = [];

// PascalCase for types/interfaces
interface ProjectProps {
  id: number;
  name: string;
}

// UPPER_SNAKE_CASE for constants
const API_BASE_URL = 'http://localhost:5000';
```

#### Component Structure

```typescript
import React from 'react';
import { ProjectProps } from '../types';

// 1. Types/Interfaces
interface ProjectCardProps {
  project: ProjectProps;
  onEdit: (id: number) => void;
}

// 2. Component
export const ProjectCard: React.FC<ProjectCardProps> = ({ 
  project, 
  onEdit 
}) => {
  // 3. Hooks
  const [isEditing, setIsEditing] = React.useState(false);
  
  // 4. Event handlers
  const handleEdit = () => {
    onEdit(project.id);
  };
  
  // 5. Render
  return (
    <div className="project-card">
      <h3>{project.name}</h3>
      <button onClick={handleEdit}>Edit</button>
    </div>
  );
};
```

#### Best Practices

- Use functional components with hooks
- Destructure props
- Use TypeScript for type safety
- Extract reusable logic to custom hooks
- Keep components small and focused
- Use meaningful component names

## 🧪 Testing Guidelines

### Backend Tests

```csharp
[Fact]
public async Task GetProjectAsync_ValidId_ReturnsProject()
{
    // Arrange
    var projectId = 1;
    var expected = new Project { Id = projectId, Name = "Test" };
    
    // Act
    var result = await _service.GetProjectAsync(projectId);
    
    // Assert
    Assert.Equal(expected.Id, result.Id);
}
```

### Frontend Tests

```typescript
describe('ProjectCard', () => {
  it('should render project name', () => {
    const project = { id: 1, name: 'Test Project' };
    render(<ProjectCard project={project} />);
    expect(screen.getByText('Test Project')).toBeInTheDocument();
  });
});
```

## 📂 File Structure

### Backend

```
PMSS.API/Controllers/
├── ProjectsController.cs        # NOT ProjectController.cs
├── UsersController.cs
└── CoursesController.cs

PMSS.Application/DTOs/
├── Project/
│   ├── ProjectDto.cs
│   ├── CreateProjectDto.cs
│   └── UpdateProjectDto.cs
```

### Frontend

```
src/
├── components/
│   ├── ProjectCard.tsx          # NOT projectCard.tsx
│   └── UserProfile.tsx
├── pages/
│   ├── Dashboard.tsx
│   └── Projects/
│       ├── ProjectList.tsx
│       └── ProjectDetail.tsx
```

## 🐛 Reporting Bugs

### Before Reporting

1. Check if the bug already exists in Issues
2. Try to reproduce the bug consistently
3. Gather relevant information

### Bug Report Template

```markdown
**Description:**
Clear description of the bug

**Steps to Reproduce:**
1. Step 1
2. Step 2
3. Step 3

**Expected Behavior:**
What should happen

**Actual Behavior:**
What actually happens

**Environment:**
- OS: Windows 11
- .NET Version: 10.0
- Browser: Chrome 120

**Screenshots:**
[Add screenshots if applicable]
```

## 💡 Requesting Features

### Feature Request Template

```markdown
**Feature Description:**
Clear description of the feature

**Problem it Solves:**
What problem does this solve?

**Proposed Solution:**
How would you implement this?

**Alternatives Considered:**
What other solutions did you think about?

**Additional Context:**
Any other relevant information
```

## 📚 Documentation

### When to Update Documentation

- Adding new features
- Changing existing behavior
- Adding new dependencies
- Modifying setup process

### Documentation Locations

- **Code comments**: Complex logic explanation
- **README files**: Setup and usage
- **API docs**: Endpoint documentation
- **Architecture docs**: System design

## ✅ Definition of Done

A task is considered done when:

- [ ] Code is written and follows style guidelines
- [ ] Tests are written and passing
- [ ] Documentation is updated
- [ ] Code is reviewed and approved
- [ ] PR is merged to main branch

## 🎓 Learning Resources

### Backend
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)

### Frontend
- [React Documentation](https://react.dev/)
- [TypeScript Documentation](https://www.typescriptlang.org/)
- [React Best Practices](https://react.dev/learn/thinking-in-react)

## 🤔 Questions?

- Create a GitHub Discussion
- Ask in team meetings
- Reach out to project maintainers

## 📜 License

By contributing, you agree that your contributions will be licensed under the project's license.

---

Thank you for contributing to PMSS! 🎉
