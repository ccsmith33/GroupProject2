# Cursor Collaboration Guide

## Cursor-Specific Features for Team Development

Since your team is using Cursor, here are some specific features and workflows that will enhance collaboration:

## Cursor AI Features

### 1. Cursor Tab (Cmd/Ctrl + K)

- **Code Generation**: Use for generating boilerplate code, API endpoints, or UI components
- **Code Explanation**: Ask Cursor to explain complex code sections
- **Refactoring**: Use to improve code structure and readability

### 2. Cursor Chat (Cmd/Ctrl + L)

- **Code Review**: Ask Cursor to review code before committing
- **Debugging**: Get help with error messages and debugging
- **Architecture Questions**: Ask about best practices for .NET and frontend development

### 3. Cursor Composer (Cmd/Ctrl + I)

- **Multi-file Changes**: Make coordinated changes across multiple files
- **Feature Implementation**: Implement entire features with proper file structure
- **Refactoring**: Rename variables/functions across the entire codebase

## Team Workflow with Cursor

### Code Review Process

1. **Before Creating PR**: Use Cursor Chat to review your code

   ```
   "Review this C# method for best practices and potential issues"
   "Check this JavaScript function for bugs and improvements"
   ```

2. **During Code Review**: Use Cursor to understand changes
   - Ask Cursor to explain complex changes
   - Use "Show me the differences" for better understanding

### Pair Programming with Cursor

1. **Screen Share + Cursor**: Share your Cursor screen during pair programming
2. **Cursor Composer**: Use for real-time collaborative coding
3. **AI-Assisted Debugging**: Use Cursor to quickly identify and fix issues

### Feature Development

1. **Planning**: Use Cursor Chat to plan feature architecture
2. **Implementation**: Use Cursor Tab for code generation
3. **Testing**: Use Cursor to generate test cases
4. **Documentation**: Use Cursor to generate documentation

## Cursor-Specific Settings

### Recommended Cursor Settings

Add to your Cursor settings.json:

```json
{
  "cursor.ai.enableCodeActions": true,
  "cursor.ai.enableInlineChat": true,
  "cursor.ai.enableComposer": true,
  "cursor.ai.model": "gpt-4",
  "editor.formatOnSave": true,
  "editor.codeActionsOnSave": {
    "source.fixAll": true
  }
}
```

### Cursor Rules for Team

1. **Always use Cursor's AI features** for code generation and review
2. **Document AI-generated code** with comments explaining the approach
3. **Use Cursor Chat** for architectural decisions and best practices
4. **Leverage Cursor Composer** for complex multi-file changes

## Collaboration Best Practices

### 1. Code Generation

- Use Cursor Tab for consistent code patterns
- Generate API endpoints with proper error handling
- Create UI components with Bootstrap 5 structure

### 2. Code Review

- Use Cursor Chat to review code before committing
- Ask Cursor to check for security vulnerabilities
- Use Cursor to ensure code follows project standards

### 3. Debugging

- Use Cursor Chat to debug complex issues
- Ask Cursor to explain error messages
- Use Cursor to suggest fixes for bugs

### 4. Documentation

- Use Cursor to generate API documentation
- Ask Cursor to create README sections
- Use Cursor to generate code comments

## Cursor Prompts for This Project

### Backend Development

```
"Generate a C# API controller for [feature] following .NET 8 best practices"
"Create a SQLite database model for [entity] with proper relationships"
"Add error handling and logging to this API endpoint"
"Split this large Program.cs into modular services and controllers"
"Create a service class for [entity] with proper dependency injection"
```

### Frontend Development

```
"Create a Bootstrap 5 form component for [feature]"
"Generate JavaScript code to handle API calls with proper error handling"
"Create a responsive navigation component for mobile devices"
"Split this large JavaScript file into smaller, focused modules"
"Create separate files for API calls, UI manipulation, and business logic"
```

### Full Stack Integration

```
"Connect this frontend form to the backend API endpoint"
"Add CORS configuration for frontend-backend communication"
"Implement proper error handling for API failures"
```

## Team Communication with Cursor

### Daily Standups

- Use Cursor to generate progress reports
- Ask Cursor to identify blockers and suggest solutions
- Use Cursor to plan daily tasks

### Code Reviews

- Use Cursor Chat to discuss code changes
- Ask Cursor to suggest improvements
- Use Cursor to ensure code quality

### Problem Solving

- Use Cursor to brainstorm solutions
- Ask Cursor to research best practices
- Use Cursor to implement complex features

## AIS Challenge Specific Prompts

### Problem Definition

```
"Help me identify a real-world problem that can be solved with information systems for the AIS Innovation Challenge"
"Suggest UN Sustainable Development Goals that would be good for our project"
```

### Solution Design

```
"Design an innovative information system solution for [chosen SDG]"
"Create a system architecture that addresses [specific problem]"
```

### Implementation

```
"Implement a prototype that demonstrates our solution to [chosen SDG]"
"Create a user interface that showcases the societal impact of our solution"
```

## Cursor Workspace Configuration

### .cursorrules Enhancement

Your existing `.cursorrules` file is perfect for Cursor. The AI will follow these guidelines when generating code.

### Cursor Composer Rules

- Always follow the project's technology stack
- Use Bootstrap 5 for all UI components
- Follow .NET 8 best practices for backend code
- Maintain consistent code style across the project
- **Create modular, small files instead of large ones**
- **Maximum file sizes**: 200 lines for JS, 300 lines for C#
- **Split large functions** into smaller, focused functions
- **Separate concerns** into different files (API, UI, business logic)
- **Preserve existing patterns** when making changes
- **Suggest safe approaches** for new features

## Troubleshooting with Cursor

### Common Issues

1. **API Connection Problems**: Ask Cursor to debug CORS and connection issues
2. **Database Errors**: Use Cursor to troubleshoot SQLite problems
3. **Frontend Issues**: Ask Cursor to debug JavaScript and CSS problems
4. **Build Errors**: Use Cursor to resolve .NET build issues

### Getting Help

- Use Cursor Chat for immediate help
- Ask Cursor to explain error messages
- Use Cursor to suggest alternative approaches

---

**Remember**: Cursor's AI is your team's additional member. Use it to enhance productivity, maintain code quality, and solve complex problems efficiently.
