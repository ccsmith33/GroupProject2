# Student Study AI Platform

An AI-powered study platform that analyzes uploaded files and provides personalized feedback and study recommendations.

## 🚀 Quick Start

### Prerequisites

- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **MySQL 8.0.37** - [Download here](https://dev.mysql.com/downloads/mysql/)
- **MySQL Workbench** (Optional) - [Download here](https://dev.mysql.com/downloads/workbench/)

> **Note**: Docker is not required for this project. We use direct MySQL installation for simplicity.

### Setup Instructions

#### 1. Clone the Repository

```bash
git clone <your-repo-url>
cd "MIS 321 Group Project 2"
```

#### 2. Install MySQL

**Option A: Using winget (Recommended)**

```powershell
winget install Oracle.MySQL
```

**Option B: Manual Installation**

- Download MySQL 8.0.37 for Windows from [mysql.com](https://dev.mysql.com/downloads/mysql/)
- Run installer as Administrator
- Set root password during installation (remember this!)
- MySQL service starts automatically

**Verify Installation:**

```powershell
# Check if MySQL is running
Get-Service -Name "MySQL*"

# Test connection
mysql -u root -p
```

#### 3. Configure Database

Update the connection string in `backend/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StudentStudyAI;Uid=root;Pwd=YOUR_PASSWORD_HERE;Port=3306;SslMode=Required;"
  }
}
```

#### 4. Run the Backend

```bash
cd backend
dotnet restore
dotnet build
dotnet run
```

#### 5. Initialize Database

In a new terminal:

```bash
# PowerShell
Invoke-WebRequest -Uri http://localhost:5000/init-db -Method POST

# Or open in browser
# http://localhost:5000/init-db
```

#### 6. Run the Frontend

Open `frontend/index.html` in your browser or use a local server:

```bash
# Using Python (if installed)
cd frontend
python -m http.server 3000

# Then open: http://localhost:3000
```

## 🏗️ Project Structure

```
MIS 321 Group Project 2/
├── backend/                 # .NET 8 Web API
│   ├── Controllers/         # API endpoints
│   ├── Services/           # Business logic
│   ├── Models/             # Data models
│   └── Program.cs          # Application entry point
├── frontend/               # Vanilla JavaScript + HTML
│   ├── index.html          # Main page
│   ├── js/                 # JavaScript modules
│   └── css/                # Styling
├── docs/                   # Documentation
└── tests/                  # Unit tests
```

## 🔧 Development

### Backend Development

```bash
cd backend
dotnet run --watch  # Auto-reload on changes
```

### Frontend Development

- Edit files in `frontend/` folder
- Refresh browser to see changes
- Use browser dev tools for debugging

### Database Management

- **MySQL Workbench**: Connect to `localhost:3306`
- **Command Line**: `mysql -u root -p`
- **API Endpoint**: `POST http://localhost:5000/init-db`

## 📡 API Endpoints

| Endpoint                | Method | Description         |
| ----------------------- | ------ | ------------------- |
| `/health`               | GET    | Health check        |
| `/init-db`              | POST   | Initialize database |
| `/api/files/upload`     | POST   | Upload file         |
| `/api/analysis/analyze` | POST   | Analyze content     |

## 🛠️ Tech Stack

- **Backend**: C# .NET 8, MySQL 8.0.37
- **Frontend**: Vanilla JavaScript, HTML5, CSS3
- **Database**: MySQL with raw ADO.NET (no ORM)
- **AI**: GPT-4o integration (mock data by default)

## 🐛 Troubleshooting

### Common Issues

#### MySQL Connection Failed

```
Error: Unable to connect to any of the specified MySQL hosts
```

**Solution**:

1. Check if MySQL service is running:

   ```powershell
   Get-Service -Name "MySQL*"
   # If not running: Start-Service -Name "MySQL80"
   ```

2. Verify MySQL is listening on port 3306:

   ```powershell
   netstat -an | findstr :3306
   ```

3. Check password in `appsettings.json`
4. Test connection manually:
   ```powershell
   mysql -u root -p
   ```

#### Database Not Found

```
Error: Unknown database 'StudentStudyAI'
```

**Solution**: Run the init endpoint: `POST http://localhost:5000/init-db`

#### Build Errors

```
Error: CS0234: The type or namespace name 'Data' does not exist
```

**Solution**: Run `dotnet clean` then `dotnet build`

### Getting Help

1. Check the logs in the terminal
2. Verify MySQL is running
3. Check connection string in `appsettings.json`
4. Ensure all dependencies are installed

## 📝 Features

- ✅ **File Upload**: Support for PDFs, images, documents
- ✅ **AI Analysis**: Mock analysis with realistic responses
- ✅ **Database Storage**: MySQL for persistent data
- ✅ **RESTful API**: Clean API design
- ✅ **Responsive UI**: Works on desktop and mobile

## 🚧 Roadmap

- [ ] Real OpenAI integration
- [ ] User authentication
- [ ] File processing improvements
- [ ] Study session tracking
- [ ] Progress analytics

## 👥 Team Development

- **Frontend**: Work in `frontend/` folder
- **Backend**: Work in `backend/` folder
- **Database**: Use `docs/mysql-setup.md` for reference
- **API**: Test with provided endpoints

## 📄 License

This project is for educational purposes as part of MIS 321 Group Project 2.
