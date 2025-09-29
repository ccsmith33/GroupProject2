# Contributing to MIS 321 Group Project 2

## Team Collaboration Guidelines

This document outlines how our 4-person team should collaborate effectively on this AIS Innovation Challenge project.

## Team Members

- [Your Name] - Project Lead
- [Team Member 2] - Backend Developer
- [Team Member 3] - Frontend Developer
- [Team Member 4] - Full Stack Developer

## Getting Started

### Initial Setup

1. **Clone the repository**

   ```bash
   git clone https://github.com/ccsmith33/GroupProject2.git
   cd GroupProject2
   ```

2. **Set up your development environment**

   - Install [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Use a modern code editor (VS Code, Visual Studio, or Rider)
   - Install recommended extensions (see below)

3. **Run the project locally**

   ```bash
   # Backend
   cd api
   dotnet restore
   dotnet run

   # Frontend (in another terminal)
   cd client
   # Open index.html in browser or use a local server
   ```

## Git Workflow

### Branch Strategy

- **main**: Production-ready code
- **develop**: Integration branch for features
- **feature/feature-name**: Individual feature branches
- **hotfix/issue-name**: Critical bug fixes

### Commit Guidelines

Use conventional commit messages:

```
type(scope): description

Examples:
feat(api): add user authentication endpoint
fix(frontend): resolve mobile navigation issue
docs(readme): update installation instructions
refactor(api): improve database connection handling
```

**Types**: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

### Pull Request Process

1. **Create a feature branch**

   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**

   - Write clean, documented code
   - Test your changes locally
   - Follow the coding standards below

3. **Commit and push**

   ```bash
   git add .
   git commit -m "feat(scope): your commit message"
   git push origin feature/your-feature-name
   ```

4. **Create a Pull Request**

   - Use the GitHub PR template
   - Assign reviewers (at least 2 team members)
   - Link any related issues
   - Include screenshots for UI changes

5. **Code Review**
   - Reviewers should check code quality, functionality, and adherence to standards
   - Address all feedback before merging
   - Merge only after approval from at least 2 reviewers

## Coding Standards

### General

- Use meaningful variable and function names
- Write comments for complex logic
- Keep functions small and focused
- Follow the existing code style

### C# (.NET API)

- Use PascalCase for classes, methods, properties
- Use camelCase for variables and parameters
- Use async/await for database operations
- Include XML documentation for public methods

### JavaScript

- Use camelCase for variables and functions
- Use const/let instead of var
- Use arrow functions where appropriate
- Include JSDoc comments for functions

### HTML/CSS

- Use semantic HTML elements
- Follow Bootstrap 5 conventions
- Use consistent indentation (2 spaces)
- Keep CSS organized and commented

## File Organization

### Backend (api/)

```
api/
├── Controllers/          # API controllers
├── Models/              # Data models
├── Services/            # Business logic
├── Data/               # Database context
├── Middleware/         # Custom middleware
└── Program.cs          # Application entry point
```

### Frontend (client/)

```
client/
├── index.html          # Main HTML file
├── styles/
│   └── main.css        # Custom styles
└── scripts/
    └── app.js          # Main JavaScript
```

## Communication

### Daily Standups

- **When**: [Specify time, e.g., 9:00 AM daily]
- **Duration**: 15 minutes
- **Format**: What did you do yesterday? What will you do today? Any blockers?

### Weekly Reviews

- **When**: [Specify day/time, e.g., Fridays 2:00 PM]
- **Purpose**: Review progress, plan next week, address issues

### Communication Channels

- **GitHub Issues**: Bug reports, feature requests, questions
- **GitHub Discussions**: General project discussion
- **Pull Request Comments**: Code-specific discussions

## Development Environment

### Recommended Cursor Extensions

- C# Dev Kit
- C# Extensions
- Prettier - Code formatter
- GitLens
- Live Server (for frontend development)
- SQLite Viewer

**Note**: Since the team is using Cursor, you get built-in AI features that enhance development. See `CURSOR_COLLABORATION.md` for Cursor-specific workflows.

### Project Setup Scripts

```bash
# Backend setup
cd api
dotnet restore
dotnet build

# Frontend setup (if using a local server)
cd client
# Use Live Server extension or:
python -m http.server 3000
```

## Testing

### Backend Testing

- Write unit tests for business logic
- Test API endpoints with Postman or Swagger
- Use the built-in test project structure

### Frontend Testing

- Test in multiple browsers (Chrome, Firefox, Safari)
- Test responsive design on different screen sizes
- Validate form inputs and error handling

## Deployment

### Development

- Local development with SQLite database
- Frontend served from file system or local server

### Production (if needed)

- Deploy API to Azure/AWS/Heroku
- Use production database (PostgreSQL/SQL Server)
- Configure CORS for production domain

## Troubleshooting

### Common Issues

**API not starting:**

- Check if .NET 8 SDK is installed
- Verify database connection string
- Check for port conflicts (7000)

**Frontend not connecting to API:**

- Verify API is running
- Check CORS configuration
- Ensure correct API URL in JavaScript

**Database issues:**

- Delete database file and restart API (it will recreate)
- Check connection string in appsettings.json

## AIS Challenge Requirements

Remember our project must address:

- **Problem Significance**: Real societal problem solvable by IS
- **Solution Description**: Clear, innovative solution
- **Business Aspect**: Market demand and sustainability
- **Prototype Demo**: Working demonstration

## Resources

- [AIS Innovation Challenge](https://communities.aisnet.org/aisscolc2025/scolc2025-challenge)
- [UN Sustainable Development Goals](https://sdgs.un.org/goals)
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Bootstrap 5 Documentation](https://getbootstrap.com/docs/5.3/)

---

**Questions?** Create an issue or start a discussion in the GitHub repository.
