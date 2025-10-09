# API Test Page

This is a bare-bones HTML/JavaScript test page for testing all Student Study AI API endpoints.

## How to Use

1. **Start the backend server:**

   ```bash
   cd backend
   dotnet run
   ```

2. **Open the test page:**

   - Navigate to: `https://localhost:5001/test`
   - Or directly: `https://localhost:5001/test-api.html`

3. **Initialize the database:**

   - Click the "Initialize Database" button (green button in auth section)
   - This creates all necessary tables

4. **Test Authentication:**

   - Register a new user or login with existing credentials
   - Token will be stored and displayed

5. **Test All Endpoints:**
   - Use the various sections to test different API functionality
   - All responses will be displayed in the response area at the bottom

## Features Tested

### Authentication

- ✅ User Registration
- ✅ User Login
- ✅ User Logout
- ✅ Get Current User

### User Management

- ✅ Get User Profile
- ✅ Update User Profile
- ✅ Get User Statistics
- ✅ Study Session CRUD operations

### File Management

- ✅ File Upload (supports PDF, Word, images)
- ✅ Get File List
- ✅ Get File Status
- ✅ Delete File

### AI Analysis

- ✅ File Analysis
- ✅ Study Guide Generation
- ✅ Quiz Generation
- ✅ Chat/Conversation

### Data Retrieval

- ✅ Get Study Guides
- ✅ Get Quizzes
- ✅ Get Conversations
- ✅ Submit Quiz Attempts

## Notes

- **No styling** - This is intentionally bare-bones for testing only
- **CORS enabled** - Works with the backend CORS configuration
- **Token persistence** - Login tokens are stored in localStorage
- **Error handling** - All API errors are displayed in the response area
- **File upload** - Supports drag-and-drop file selection

## Troubleshooting

- **CORS errors**: Make sure the backend is running on https://localhost:5001
- **Database errors**: Click "Initialize Database" first
- **Authentication errors**: Register/login first before testing protected endpoints
- **File upload errors**: Make sure to select a file before uploading

## API Base URL

The test page uses: `https://localhost:5001/api`

All endpoints are relative to this base URL.
