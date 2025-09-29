# MIS 321 Group Project 2

## AIS Innovation Challenge Project

This project is being developed for the [AIS Student Chapters Technology Innovation Challenge](https://communities.aisnet.org/aisscolc2025/scolc2025-challenge), focusing on addressing one of the United Nations Sustainable Development Goals through innovative information systems solutions.

## Project Overview

This application demonstrates how information systems can be used to solve real-world problems and make a positive societal impact. The project follows the AIS Innovation Challenge requirements and addresses one of the 17 UN Sustainable Development Goals.

### Technology Stack

- **Frontend**: HTML5, CSS3, Vanilla JavaScript (ES6+), Bootstrap 5
- **Backend**: .NET 8 Web API, C#
- **Database**: SQLite
- **Architecture**: RESTful API with single-page application frontend

## Project Structure

```
MIS 321 Group Project 2/
├── api/                    # Backend .NET 8 Web API
│   ├── Program.cs          # Main application entry point
│   ├── api.csproj          # Project file
│   ├── appsettings.json    # Configuration
│   └── database.db         # SQLite database (created on first run)
├── client/                 # Frontend application
│   ├── index.html         # Main HTML file
│   ├── styles/
│   │   └── main.css       # Custom CSS styles
│   └── scripts/
│       └── app.js         # Main JavaScript application
├── docs/                   # Documentation and guidelines
│   ├── CONTRIBUTING.md     # Team collaboration guidelines
│   ├── CURSOR_COLLABORATION.md # Cursor-specific features
│   ├── MODULARIZATION_GUIDE.md # Code organization rules
│   ├── QUICK_START.md      # Quick setup guide
│   ├── setup-dev.md        # Detailed development setup
│   └── PULL_REQUEST_TEMPLATE.md # PR template
├── .cursorrules           # Development guidelines
└── README.md              # This file
```

## 📚 Documentation

- **[Quick Start](docs/QUICK_START.md)** - Get up and running in 5 minutes
- **[Contributing Guidelines](docs/CONTRIBUTING.md)** - Team workflow and standards
- **[Cursor Collaboration](docs/CURSOR_COLLABORATION.md)** - AI-assisted development
- **[Modularization Guide](docs/MODULARIZATION_GUIDE.md)** - Code organization rules
- **[Phase Transitions](docs/PHASE_TRANSITIONS.md)** - When and how to refactor as project grows
- **[Development Setup](docs/setup-dev.md)** - Detailed setup instructions

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Cursor](https://cursor.sh/) (recommended for this project)
- A modern web browser

### Installation & Setup

1. **Clone or download this repository**

2. **Start the Backend API**

   ```bash
   cd api
   dotnet restore
   dotnet run
   ```

   The API will be available at `https://localhost:7000`

3. **Open the Frontend**
   - Open `client/index.html` in your web browser
   - Or serve it using a local web server (recommended for development)

### Development Setup

For development, you can serve the frontend using a simple HTTP server:

```bash
# Using Python (if installed)
cd client
python -m http.server 3000

# Using Node.js (if installed)
cd client
npx http-server -p 3000

# Using PHP (if installed)
cd client
php -S localhost:3000
```

Then open `http://localhost:3000` in your browser.

## Testing

This project includes comprehensive unit testing for both backend and frontend components.

**Important:** The main API is built with **.NET 8** and runs with `dotnet run`. Node.js is only used for frontend testing tools and development utilities.

### Backend Testing (.NET)

**Run all backend tests:**

```bash
cd api.Tests
dotnet test
```

**Run tests with detailed output:**

```bash
cd api.Tests
dotnet test --verbosity normal
```

**Run specific test class:**

```bash
cd api.Tests
dotnet test --filter "ProgramTests"
```

**Run tests and generate coverage report:**

```bash
cd api.Tests
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend Testing (JavaScript)

**Option 1: Browser-based test runner (Recommended)**

```bash
cd client
# Open tests/test-runner.html in your web browser
# Or use the npm script:
npm run test
```

**Option 2: Node.js testing (Advanced)**

```bash
cd client
# Install testing dependencies
npm install --save-dev jest

# Run tests with Jest
npm test

# Run tests in watch mode
npm run test:watch
```

**Option 3: Manual test execution**

1. Open `client/tests/test-runner.html` in your browser
2. View test results in the browser
3. Check console for any errors

**Add new frontend tests:**

1. Create `*.test.js` files in `client/tests/` folder
2. Follow the existing test patterns in `app.test.js`
3. Include your test file in `test-runner.html`

### Test Structure

**Backend Tests (`api.Tests/`):**

- `ProgramTests.cs` - API endpoint tests
- Uses xUnit framework
- Tests API controllers, business logic, and data validation

**Frontend Tests (`client/tests/`):**

- `app.test.js` - JavaScript function tests
- `test-runner.html` - Test execution environment
- Tests DOM manipulation, API calls, and utility functions

### Writing Tests

**Backend Test Example:**

```csharp
[Fact]
public async Task GetProjects_ReturnsSuccessStatusCode()
{
    // Arrange
    var request = "/api/projects";

    // Act
    var response = await _client.GetAsync(request);

    // Assert
    response.EnsureSuccessStatusCode();
}
```

**Frontend Test Example:**

```javascript
test("App constructor initializes correctly", () => {
  const app = new App();
  expect(app.apiBaseUrl).toBe("https://localhost:7000/api");
});
```

## Features

### Current Implementation

- **Project Management**: Create and view projects
- **Responsive Design**: Mobile-friendly Bootstrap 5 interface
- **RESTful API**: Clean API endpoints for data operations
- **SQLite Database**: Lightweight, file-based database
- **CORS Support**: Configured for frontend-backend communication

### API Endpoints

- `GET /api/projects` - Retrieve all projects
- `POST /api/projects` - Create a new project
- `GET /swagger` - API documentation (development only)

## UN Sustainable Development Goals

This project is designed to address one of the following UN SDGs:

- **No Poverty**: End poverty in all its forms everywhere
- **Zero Hunger**: End hunger, achieve food security and improved nutrition
- **Good Health and Well-being**: Ensure healthy lives and promote well-being
- **Quality Education**: Ensure inclusive and equitable quality education
- **Gender Equality**: Achieve gender equality and empower all women and girls
- **Clean Water and Sanitation**: Ensure availability and sustainable management of water
- **Affordable and Clean Energy**: Ensure access to affordable, reliable, sustainable energy
- **Decent Work and Economic Growth**: Promote sustained, inclusive economic growth
- **Industry, Innovation and Infrastructure**: Build resilient infrastructure
- **Reduced Inequalities**: Reduce inequality within and among countries
- **Sustainable Cities and Communities**: Make cities inclusive, safe, resilient, sustainable
- **Responsible Consumption and Production**: Ensure sustainable consumption patterns
- **Climate Action**: Take urgent action to combat climate change
- **Life Below Water**: Conserve and sustainably use oceans, seas, marine resources
- **Life on Land**: Protect, restore, promote sustainable use of terrestrial ecosystems
- **Peace, Justice and Strong Institutions**: Promote peaceful, inclusive societies
- **Partnerships for the Goals**: Strengthen means of implementation

## Development Guidelines

This project follows the guidelines specified in `.cursorrules`:

- **Frontend**: Single-page application with vanilla JavaScript
- **Styling**: Bootstrap 5 from CDN, minimal custom CSS
- **Backend**: .NET 8 Web API with direct SQL queries
- **Database**: SQLite with Microsoft.Data.Sqlite package
- **Code Style**: Consistent formatting and meaningful naming

## Contributing

1. Follow the established code style and architecture
2. Test your changes thoroughly
3. Update documentation as needed
4. Ensure the project addresses a real-world problem

## AIS Innovation Challenge Requirements

This project meets the following challenge criteria:

- ✅ **Problem Significance**: Addresses a societal problem solvable by IS
- ✅ **Solution Description**: Clear description of proposed solution
- ✅ **Innovativeness**: Demonstrates innovation and creativity
- ✅ **Business Aspect**: Shows market demand and sustainability potential
- ✅ **Alpha Prototype**: Working demonstration of the solution

## License

This project is developed for educational purposes as part of the AIS Innovation Challenge.

## Contact

For questions about this project or the AIS Innovation Challenge, contact: studentchapters@aisnet.org

---

**Note**: This project is designed to be a foundation for your AIS Innovation Challenge submission. Customize and extend it to address your specific chosen UN Sustainable Development Goal and innovative solution.
