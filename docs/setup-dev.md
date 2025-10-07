# Development Setup Guide

## Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Git](https://git-scm.com/downloads)
- [Cursor](https://cursor.sh/) (recommended for this project)
- Modern web browser

### 1. Clone and Setup

```bash
git clone [your-repo-url]
cd FM
```

### 2. Backend Setup

```bash
cd api
dotnet restore
dotnet build
dotnet run
```

The API will be available at `https://localhost:7000`

### 3. Frontend Setup

```bash
cd client
# Option 1: Open index.html directly in browser
# Option 2: Use VS Code Live Server extension
# Option 3: Use Python HTTP server
python -m http.server 3000
```

### 4. Verify Setup

- API: Visit `https://localhost:7000/swagger` for API documentation
- Frontend: Open `http://localhost:3000` (if using server) or `index.html` directly
- Test: Try creating a project in the frontend

## Development Workflow

### Daily Development

1. Pull latest changes: `git pull origin main`
2. Create feature branch: `git checkout -b feature/your-feature`
3. Make changes and test locally
4. Commit: `git add . && git commit -m "feat: your message"`
5. Push: `git push origin feature/your-feature`

### Testing Checklist

- [ ] Backend API starts without errors
- [ ] Frontend loads and displays correctly
- [ ] Can create/view projects
- [ ] No console errors in browser
- [ ] Responsive design works on mobile

## Common Issues & Solutions

### API Won't Start

- Check .NET 8 SDK: `dotnet --version`
- Clear NuGet cache: `dotnet nuget locals all --clear`
- Delete bin/obj folders and restore: `dotnet clean && dotnet restore`

### Frontend Can't Connect to API

- Verify API is running on port 7000
- Check browser console for CORS errors
- Ensure API URL in `client/scripts/app.js` is correct

### Database Issues

- Delete `api/database.db` and restart API (will recreate)
- Check connection string in `api/appsettings.json`

## Project Structure

```
FM/
├── api/                 # .NET 8 Web API
│   ├── Controllers/     # API endpoints
│   ├── Models/         # Data models
│   ├── Services/       # Business logic
│   └── Program.cs      # Main entry point
├── client/             # Frontend
│   ├── index.html      # Main page
│   ├── styles/         # CSS files
│   └── scripts/        # JavaScript files
└── docs/              # Documentation
```
