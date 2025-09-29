# Modularization Guide

## 🎯 Core Principle: Small, Focused Files

**ALWAYS choose multiple small files over one large file. This makes code more maintainable, testable, and easier for teams to collaborate on.**

## 📏 File Size Limits

- **JavaScript**: Maximum 200 lines per file
- **C#**: Maximum 300 lines per file
- **Functions**: Maximum 20 lines per function
- **Classes**: Maximum 200 lines per class

## 🏗️ Frontend Modularization

### Current Structure (Good Start)

```
client/
├── index.html
├── scripts/
│   └── app.js          # 200+ lines - needs splitting!
└── styles/
    └── main.css
```

### Recommended Structure

```
client/
├── index.html
├── scripts/
│   ├── app.js          # Main application controller (50-100 lines)
│   ├── api.js          # API communication functions
│   ├── ui.js           # UI manipulation functions
│   ├── utils.js        # Utility functions
│   ├── auth.js         # Authentication logic
│   ├── projects.js     # Project management features
│   └── components/     # Reusable UI components
│       ├── modal.js
│       ├── form.js
│       └── table.js
└── styles/
    ├── main.css        # Global styles
    ├── components.css  # Reusable component styles
    ├── layout.css      # Layout-specific styles
    └── features/       # Feature-specific styles
        ├── projects.css
        └── auth.css
```

### JavaScript Module Examples

#### `api.js` - API Communication

```javascript
class ApiClient {
  constructor(baseUrl) {
    this.baseUrl = baseUrl;
  }

  async get(endpoint) {
    // GET request logic
  }

  async post(endpoint, data) {
    // POST request logic
  }
}

export default ApiClient;
```

#### `ui.js` - UI Manipulation

```javascript
class UIManager {
  static showAlert(message, type = "info") {
    // Alert display logic
  }

  static hideElement(elementId) {
    // Hide element logic
  }
}

export default UIManager;
```

#### `projects.js` - Project Features

```javascript
class ProjectManager {
  constructor(apiClient) {
    this.api = apiClient;
  }

  async loadProjects() {
    // Load projects logic
  }

  async createProject(projectData) {
    // Create project logic
  }
}

export default ProjectManager;
```

## 🏗️ Backend Modularization

### Current Structure (Needs Improvement)

```
api/
├── Program.cs          # 100+ lines - needs splitting!
├── appsettings.json
└── api.csproj
```

### Recommended Structure

```
api/
├── Controllers/
│   ├── ProjectsController.cs
│   ├── UsersController.cs
│   └── AuthController.cs
├── Services/
│   ├── ProjectService.cs
│   ├── UserService.cs
│   └── DatabaseService.cs
├── Models/
│   ├── Project.cs
│   ├── User.cs
│   └── ApiResponse.cs
├── Middleware/
│   ├── ErrorHandlingMiddleware.cs
│   └── LoggingMiddleware.cs
├── Utils/
│   ├── DatabaseHelper.cs
│   └── ValidationHelper.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Program.cs          # Minimal startup code
└── appsettings.json
```

### C# Module Examples

#### `ProjectService.cs` - Business Logic

```csharp
public class ProjectService
{
    private readonly IDatabaseService _database;

    public ProjectService(IDatabaseService database)
    {
        _database = database;
    }

    public async Task<List<Project>> GetAllProjectsAsync()
    {
        // Business logic here
    }

    public async Task<Project> CreateProjectAsync(CreateProjectRequest request)
    {
        // Create logic here
    }
}
```

#### `ProjectsController.cs` - API Endpoints

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _projectService;

    public ProjectsController(ProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects()
    {
        // Controller logic here
    }
}
```

## 🔧 Cursor AI Prompts for Modularization

### When Creating New Features

```
"Create a modular structure for [feature] with separate files for API, UI, and business logic"
"Split this large function into smaller, focused functions"
"Create a service class for [entity] with proper separation of concerns"
```

### When Refactoring

```
"Refactor this large file into smaller, focused modules"
"Extract the API logic into a separate service class"
"Split this CSS file into component-specific stylesheets"
```

### When Adding Features

```
"Add [feature] following the modular structure with separate files for each concern"
"Create a new controller for [entity] with proper dependency injection"
"Add validation logic in a separate utility class"
```

## 📋 Modularization Checklist

### Before Committing Code

- [ ] No file exceeds size limits (200 JS, 300 C#)
- [ ] Each file has a single, clear responsibility
- [ ] Functions are under 20 lines
- [ ] Related functionality is grouped together
- [ ] Dependencies are properly injected
- [ ] Code is easily testable

### When Adding New Features

- [ ] Create separate files for different concerns
- [ ] Use proper naming conventions
- [ ] Follow the established folder structure
- [ ] Add proper imports/exports
- [ ] Include error handling

## 🎯 Benefits of Modularization

1. **Easier Collaboration** - Team members can work on different files
2. **Better Testing** - Small, focused functions are easier to test
3. **Improved Maintainability** - Changes are isolated to specific files
4. **Code Reusability** - Modules can be reused across features
5. **Better Performance** - Smaller files load faster
6. **Easier Debugging** - Issues are easier to locate and fix

## 🚀 Getting Started

1. **Start with the current `app.js`** - Split it into modules
2. **Create service classes** for business logic
3. **Separate API calls** into dedicated files
4. **Split CSS** into component-specific files
5. **Add new features** using the modular structure

Remember: **Small files are better than large files. Always choose modularity over convenience.**
