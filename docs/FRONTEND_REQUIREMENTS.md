# Frontend Requirements - Student Study AI Platform

## Overview

This document provides comprehensive requirements for frontend developers to build the UI for the Student Study AI Platform. The frontend is built using vanilla JavaScript, HTML5, and CSS3 with no frameworks, communicating with a .NET 8 backend API.

## Technology Stack

- **Frontend**: Vanilla JavaScript (ES6+), HTML5, CSS3
- **API Base URL**: `http://localhost:5000/api`
- **Backend Server**: .NET 8 running on port 5000
- **Authentication**: JWT tokens with Bearer authentication
- **File Storage**: Local file system via backend API
- **Database**: MySQL 8.0.37 (backend managed)

## Project Structure

```
frontend/
├── index.html              # Main application entry point
├── css/
│   ├── main.css           # Global styles and layout
│   └── components.css     # Component-specific styles
├── js/
│   ├── main.js            # Application initialization
│   ├── api/               # API communication modules
│   │   ├── auth.js        # Authentication API calls
│   │   ├── files.js       # File management API calls
│   │   ├── analysis.js    # AI analysis API calls
│   │   ├── knowledge.js   # Knowledge tracking API calls
│   │   └── user.js        # User profile API calls
│   ├── components/        # UI components
│   │   ├── fileUpload.js  # File upload component
│   │   ├── dashboard.js   # Dashboard component
│   │   ├── quiz.js        # Quiz interface component
│   │   ├── studyGuide.js  # Study guide component
│   │   └── knowledge.js   # Knowledge tracking component
│   ├── utils/             # Utility functions
│   │   ├── auth.js        # Authentication utilities
│   │   ├── api.js         # API helper functions
│   │   └── ui.js          # UI helper functions
│   └── workers/           # Web workers for heavy processing
│       └── fileProcessor.js
└── assets/                # Static assets
    ├── images/
    └── icons/
```

## Authentication System

### Authentication Flow

1. **Login/Register** → Get JWT token
2. **Store token** in localStorage
3. **Include token** in all API requests
4. **Handle token expiration** with refresh logic
5. **Logout** clears token and redirects

### API Endpoints

#### POST `/api/auth/register`

Register a new user account.

**Request:**

```json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "password123",
  "confirmPassword": "password123"
}
```

**Response:**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "guid-guid-guid-guid",
  "expiresAt": "2024-01-01T01:00:00Z",
  "user": {
    "id": 1,
    "username": "john_doe",
    "email": "john@example.com",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "lastLoginAt": "2024-01-01T00:00:00Z"
  }
}
```

#### POST `/api/auth/login`

Login with email and password.

**Request:**

```json
{
  "email": "john@example.com",
  "password": "password123"
}
```

**Response:** Same as register response.

#### GET `/api/auth/me`

Get current user information (requires authentication).

**Headers:**

```
Authorization: Bearer <token>
```

**Response:**

```json
{
  "id": 1,
  "username": "john_doe",
  "email": "john@example.com",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "lastLoginAt": "2024-01-01T00:00:00Z"
}
```

#### POST `/api/auth/change-password`

Change user password (requires authentication).

**Request:**

```json
{
  "currentPassword": "oldpassword",
  "newPassword": "newpassword123",
  "confirmPassword": "newpassword123"
}
```

#### POST `/api/auth/logout`

Logout user (requires authentication).

**Response:**

```json
{
  "message": "Logged out successfully"
}
```

### Authentication Implementation

```javascript
// utils/auth.js
class AuthManager {
  constructor() {
    this.token = localStorage.getItem("authToken");
    this.user = JSON.parse(localStorage.getItem("user") || "null");
  }

  async login(email, password) {
    const response = await fetch("/api/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password }),
    });

    if (response.ok) {
      const data = await response.json();
      this.setAuthData(data);
      return data;
    }
    throw new Error("Login failed");
  }

  setAuthData(authData) {
    this.token = authData.accessToken;
    this.user = authData.user;
    localStorage.setItem("authToken", this.token);
    localStorage.setItem("user", JSON.stringify(this.user));
  }

  getAuthHeaders() {
    return {
      Authorization: `Bearer ${this.token}`,
      "Content-Type": "application/json",
    };
  }

  logout() {
    this.token = null;
    this.user = null;
    localStorage.removeItem("authToken");
    localStorage.removeItem("user");
  }
}
```

## File Management System

### File Upload Interface

#### POST `/api/files/upload`

Upload a file with metadata.

**Request:** Multipart form data

- `file`: File object
- `userId`: string (user ID)
- `subject`: string (optional)
- `studentLevel`: string (optional)

**Response:**

```json
{
  "fileId": 123,
  "fileName": "document.pdf",
  "fileType": ".pdf",
  "fileSize": 1024000,
  "uploadedAt": "2024-01-01T00:00:00Z",
  "status": "uploaded",
  "message": "File uploaded successfully. Processing will begin shortly."
}
```

#### GET `/api/files/list?userId={id}`

Get list of user's files.

**Response:**

```json
{
  "files": [
    {
      "fileId": 123,
      "fileName": "document.pdf",
      "fileType": ".pdf",
      "fileSize": 1024000,
      "uploadedAt": "2024-01-01T00:00:00Z",
      "status": "completed",
      "processedAt": "2024-01-01T00:01:00Z",
      "hasContent": true
    }
  ],
  "count": 1
}
```

#### GET `/api/files/{fileId}`

Get specific file details.

#### GET `/api/files/{fileId}/status`

Check file processing status.

#### POST `/api/files/process/{fileId}`

Manually trigger file processing.

### File Upload Component

```javascript
// components/fileUpload.js
class FileUploadComponent {
  constructor(containerId) {
    this.container = document.getElementById(containerId);
    this.setupDropZone();
  }

  setupDropZone() {
    const dropZone = this.container.querySelector(".drop-zone");

    dropZone.addEventListener("dragover", (e) => {
      e.preventDefault();
      dropZone.classList.add("drag-over");
    });

    dropZone.addEventListener("dragleave", () => {
      dropZone.classList.remove("drag-over");
    });

    dropZone.addEventListener("drop", (e) => {
      e.preventDefault();
      dropZone.classList.remove("drag-over");
      this.handleFiles(e.dataTransfer.files);
    });

    dropZone.addEventListener("click", () => {
      this.container.querySelector('input[type="file"]').click();
    });
  }

  async handleFiles(files) {
    for (const file of files) {
      await this.uploadFile(file);
    }
  }

  async uploadFile(file) {
    const formData = new FormData();
    formData.append("file", file);
    formData.append("userId", this.getCurrentUserId());

    try {
      const response = await fetch("/api/files/upload", {
        method: "POST",
        body: formData,
      });

      const result = await response.json();
      this.showUploadSuccess(result);
    } catch (error) {
      this.showUploadError(error);
    }
  }
}
```

## Subject Grouping System

### API Endpoints

#### GET `/api/files/{userId}/grouped`

Get files grouped by subject and custom groups.

**Response:**

```json
{
  "success": true,
  "data": {
    "filesBySubject": {
      "Mathematics": [
        {
          "id": 1,
          "fileName": "calculus_notes.pdf",
          "fileType": ".pdf",
          "subject": "Mathematics",
          "topic": "Calculus"
        }
      ]
    },
    "customGroups": [
      {
        "id": 1,
        "groupName": "Final Exam Prep",
        "description": "Files for final exam preparation",
        "color": "#3498db",
        "createdAt": "2024-01-01T00:00:00Z"
      }
    ],
    "ungroupedFiles": []
  }
}
```

#### POST `/api/files/groups`

Create a new subject group.

**Request:**

```json
{
  "groupName": "Final Exam Prep",
  "description": "Files for final exam preparation",
  "color": "#3498db"
}
```

#### PUT `/api/files/{fileId}/subject`

Update file subject and topic.

**Request:**

```json
{
  "fileId": 123,
  "userDefinedSubject": "Mathematics",
  "userDefinedTopic": "Calculus",
  "subjectGroupId": 1
}
```

#### POST `/api/files/bulk-update`

Bulk update multiple files.

**Request:**

```json
{
  "fileIds": [1, 2, 3],
  "userDefinedSubject": "Mathematics",
  "userDefinedTopic": "Algebra",
  "subjectGroupId": 2
}
```

### Subject Grouping UI

```javascript
// components/subjectGrouping.js
class SubjectGroupingComponent {
  constructor(containerId) {
    this.container = document.getElementById(containerId);
    this.groups = [];
    this.files = [];
    this.loadData();
  }

  async loadData() {
    const response = await fetch(
      `/api/files/${this.getCurrentUserId()}/grouped`
    );
    const data = await response.json();
    this.groups = data.data.customGroups;
    this.files = data.data.filesBySubject;
    this.render();
  }

  render() {
    this.container.innerHTML = `
      <div class="subject-groups">
        <h3>Subject Groups</h3>
        <button class="btn btn-primary" onclick="this.showCreateGroupModal()">
          Create New Group
        </button>
        <div class="groups-list">
          ${this.groups.map((group) => this.renderGroup(group)).join("")}
        </div>
      </div>
      <div class="files-by-subject">
        <h3>Files by Subject</h3>
        ${Object.entries(this.files)
          .map(([subject, files]) => this.renderSubjectSection(subject, files))
          .join("")}
      </div>
    `;
  }

  renderGroup(group) {
    return `
      <div class="group-item" style="border-left: 4px solid ${group.color}">
        <h4>${group.groupName}</h4>
        <p>${group.description || ""}</p>
        <div class="group-actions">
          <button onclick="this.editGroup(${group.id})">Edit</button>
          <button onclick="this.deleteGroup(${group.id})">Delete</button>
        </div>
      </div>
    `;
  }
}
```

## AI Analysis System

### Study Guide Generation

#### POST `/api/analysis/generate-study-guide`

Generate a study guide from content.

**Request:**

```json
{
  "userPrompt": "Create a study guide for calculus derivatives",
  "fileId": 123,
  "subject": "Mathematics",
  "topic": "Calculus"
}
```

**Response:**

```json
{
  "id": 1,
  "title": "Calculus Derivatives Study Guide",
  "content": "## Introduction to Derivatives...",
  "subject": "Mathematics",
  "topic": "Calculus",
  "level": "College",
  "keyPoints": ["Definition of derivative", "Power rule", "Chain rule"],
  "summary": "This study guide covers the fundamental concepts...",
  "status": "completed",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Quiz Generation

#### POST `/api/analysis/generate-quiz`

Generate a quiz from content.

**Request:**

```json
{
  "userPrompt": "Create a quiz about calculus derivatives",
  "fileId": 123,
  "subject": "Mathematics",
  "topic": "Calculus"
}
```

**Response:**

```json
{
  "id": 1,
  "title": "Calculus Derivatives Quiz",
  "subject": "Mathematics",
  "topic": "Calculus",
  "level": "College",
  "status": "completed",
  "questions": [
    {
      "id": 1,
      "question": "What is the derivative of x²?",
      "options": ["2x", "x", "2", "x²"],
      "correctAnswerIndex": 0,
      "explanation": "Using the power rule, d/dx(x²) = 2x"
    }
  ],
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### AI Chat

#### POST `/api/analysis/chat`

Chat with AI about content.

**Request:**

```json
{
  "message": "Can you explain the chain rule?",
  "fileId": 123,
  "subject": "Mathematics",
  "topic": "Calculus"
}
```

**Response:**

```json
{
  "response": "The chain rule is a fundamental rule in calculus..."
}
```

### Study Guide Component

```javascript
// components/studyGuide.js
class StudyGuideComponent {
  constructor(containerId) {
    this.container = document.getElementById(containerId);
    this.guides = [];
    this.loadGuides();
  }

  async loadGuides() {
    const response = await fetch(
      `/api/analysis/study-guides/${this.getCurrentUserId()}`
    );
    this.guides = await response.json();
    this.render();
  }

  async generateStudyGuide(prompt, fileId, subject, topic) {
    const response = await fetch("/api/analysis/generate-study-guide", {
      method: "POST",
      headers: this.getAuthHeaders(),
      body: JSON.stringify({
        userPrompt: prompt,
        fileId: fileId,
        subject: subject,
        topic: topic,
      }),
    });

    const guide = await response.json();
    this.guides.unshift(guide);
    this.render();
  }

  render() {
    this.container.innerHTML = `
      <div class="study-guide-generator">
        <h3>Generate Study Guide</h3>
        <form onsubmit="this.handleGenerate(event)">
          <textarea placeholder="Describe what you want to study..." required></textarea>
          <select name="fileId">
            <option value="">Select a file (optional)</option>
            ${this.getFiles()
              .map(
                (file) => `<option value="${file.id}">${file.fileName}</option>`
              )
              .join("")}
          </select>
          <input type="text" placeholder="Subject (optional)" name="subject">
          <input type="text" placeholder="Topic (optional)" name="topic">
          <button type="submit">Generate Study Guide</button>
        </form>
      </div>
      <div class="study-guides-list">
        <h3>Your Study Guides</h3>
        ${this.guides.map((guide) => this.renderGuide(guide)).join("")}
      </div>
    `;
  }

  renderGuide(guide) {
    return `
      <div class="study-guide-item">
        <h4>${guide.title}</h4>
        <p class="guide-meta">${guide.subject} - ${guide.topic} (${guide.level})</p>
        <div class="guide-content">${guide.content}</div>
        <div class="guide-actions">
          <button onclick="this.downloadGuide(${guide.id})">Download</button>
          <button onclick="this.shareGuide(${guide.id})">Share</button>
        </div>
      </div>
    `;
  }
}
```

## Knowledge Tracking System

### API Endpoints

#### GET `/api/knowledge/{userId}/analytics`

Get user's knowledge analytics.

**Response:**

```json
{
  "success": true,
  "data": {
    "subjectProfiles": {
      "Mathematics": {
        "id": 1,
        "userId": 1,
        "subject": "Mathematics",
        "knowledgeLevel": 4,
        "confidenceScore": 0.85,
        "lastUpdated": "2024-01-01T00:00:00Z"
      }
    },
    "recentPerformance": [
      {
        "id": 1,
        "userId": 1,
        "quizId": 1,
        "score": 85.5,
        "knowledgeLevel": 4,
        "difficulty": 3,
        "timeSpent": 1200,
        "completedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "progressionHistory": [
      {
        "id": 1,
        "userId": 1,
        "subject": "Mathematics",
        "previousLevel": 3,
        "newLevel": 4,
        "changeReason": "quiz_performance",
        "confidenceScore": 0.85,
        "createdAt": "2024-01-01T00:00:00Z"
      }
    ],
    "preferences": {
      "id": 1,
      "userId": 1,
      "preferredQuizLength": "standard",
      "customQuestionMultiplier": 1.0,
      "preferredDifficulty": "adaptive",
      "timeAvailable": 30,
      "studyStyle": "balanced"
    },
    "subjectMastery": {
      "Mathematics": 0.85,
      "Physics": 0.72,
      "Chemistry": 0.68
    },
    "recommendedSubjects": ["Advanced Mathematics", "Statistics"]
  }
}
```

#### POST `/api/knowledge/{userId}/level`

Update knowledge level for a subject.

**Request:**

```json
{
  "userId": 1,
  "subject": "Mathematics",
  "newLevel": 5,
  "changeReason": "manual_adjustment",
  "confidenceScore": 0.9
}
```

#### GET/PUT `/api/knowledge/{userId}/preferences`

Get or update learning preferences.

**Request (PUT):**

```json
{
  "preferredQuizLength": "comprehensive",
  "customQuestionMultiplier": 1.5,
  "preferredDifficulty": "challenging",
  "timeAvailable": 45,
  "studyStyle": "intensive"
}
```

### Knowledge Tracking Component

```javascript
// components/knowledgeTracking.js
class KnowledgeTrackingComponent {
  constructor(containerId) {
    this.container = document.getElementById(containerId);
    this.analytics = null;
    this.loadAnalytics();
  }

  async loadAnalytics() {
    const response = await fetch(
      `/api/knowledge/${this.getCurrentUserId()}/analytics`
    );
    const data = await response.json();
    this.analytics = data.data;
    this.render();
  }

  render() {
    this.container.innerHTML = `
      <div class="knowledge-dashboard">
        <h2>Knowledge Dashboard</h2>
        
        <div class="subject-mastery">
          <h3>Subject Mastery</h3>
          <div class="mastery-chart">
            ${Object.entries(this.analytics.subjectMastery)
              .map(([subject, mastery]) =>
                this.renderMasteryBar(subject, mastery)
              )
              .join("")}
          </div>
        </div>

        <div class="knowledge-levels">
          <h3>Knowledge Levels</h3>
          <div class="levels-grid">
            ${Object.entries(this.analytics.subjectProfiles)
              .map(([subject, profile]) =>
                this.renderKnowledgeLevel(subject, profile)
              )
              .join("")}
          </div>
        </div>

        <div class="recent-performance">
          <h3>Recent Performance</h3>
          <div class="performance-list">
            ${this.analytics.recentPerformance
              .map((performance) => this.renderPerformance(performance))
              .join("")}
          </div>
        </div>

        <div class="learning-preferences">
          <h3>Learning Preferences</h3>
          <button onclick="this.showPreferencesModal()">Edit Preferences</button>
          <div class="preferences-display">
            ${this.renderPreferences(this.analytics.preferences)}
          </div>
        </div>
      </div>
    `;
  }

  renderMasteryBar(subject, mastery) {
    const percentage = Math.round(mastery * 100);
    return `
      <div class="mastery-item">
        <div class="subject-name">${subject}</div>
        <div class="mastery-bar">
          <div class="mastery-fill" style="width: ${percentage}%"></div>
        </div>
        <div class="mastery-percentage">${percentage}%</div>
      </div>
    `;
  }
}
```

## User Profile System

### API Endpoints

#### GET `/api/user/profile`

Get user profile information.

**Response:**

```json
{
  "id": 1,
  "username": "john_doe",
  "email": "john@example.com",
  "createdAt": "2024-01-01T00:00:00Z",
  "lastLoginAt": "2024-01-01T00:00:00Z",
  "isActive": true,
  "isAdmin": false
}
```

#### PUT `/api/user/profile`

Update user profile.

**Request:**

```json
{
  "username": "john_doe_updated",
  "email": "john.updated@example.com"
}
```

#### GET `/api/user/stats`

Get user statistics.

**Response:**

```json
{
  "totalFiles": 15,
  "totalStudySessions": 8,
  "totalStudyTimeMinutes": 240,
  "activeStudySessions": 1,
  "studyGuidesCreated": 5,
  "quizzesTaken": 12,
  "analysisResults": 20,
  "filesProcessed": 12,
  "lastActivity": "2024-01-01T00:00:00Z"
}
```

### Study Sessions

#### GET `/api/user/study-sessions`

Get user's study sessions.

**Query Parameters:**

- `limit`: number (default: 20)
- `offset`: number (default: 0)

**Response:**

```json
{
  "sessions": [
    {
      "id": 1,
      "sessionName": "Calculus Review",
      "startTime": "2024-01-01T00:00:00Z",
      "endTime": "2024-01-01T01:30:00Z",
      "duration": "01:30:00",
      "notes": "Focused on derivatives and integrals",
      "fileIds": [1, 2, 3],
      "isActive": false
    }
  ],
  "total": 8,
  "limit": 20,
  "offset": 0
}
```

#### POST `/api/user/study-sessions`

Create a new study session.

**Request:**

```json
{
  "sessionName": "Calculus Review",
  "notes": "Focused on derivatives and integrals",
  "fileIds": [1, 2, 3]
}
```

#### PUT `/api/user/study-sessions/{id}`

Update a study session.

**Request:**

```json
{
  "sessionName": "Calculus Review - Updated",
  "notes": "Added more practice problems",
  "fileIds": [1, 2, 3, 4],
  "isActive": false,
  "endSession": true
}
```

## Dashboard Component

```javascript
// components/dashboard.js
class DashboardComponent {
  constructor(containerId) {
    this.container = document.getElementById(containerId);
    this.stats = null;
    this.recentActivity = [];
    this.loadDashboardData();
  }

  async loadDashboardData() {
    try {
      const [statsResponse, sessionsResponse] = await Promise.all([
        fetch("/api/user/stats", { headers: this.getAuthHeaders() }),
        fetch("/api/user/study-sessions?limit=5", {
          headers: this.getAuthHeaders(),
        }),
      ]);

      this.stats = await statsResponse.json();
      const sessionsData = await sessionsResponse.json();
      this.recentActivity = sessionsData.sessions;

      this.render();
    } catch (error) {
      console.error("Error loading dashboard data:", error);
    }
  }

  render() {
    this.container.innerHTML = `
      <div class="dashboard">
        <h1>Study Dashboard</h1>
        
        <div class="stats-grid">
          <div class="stat-card">
            <h3>Total Files</h3>
            <div class="stat-value">${this.stats.totalFiles}</div>
          </div>
          <div class="stat-card">
            <h3>Study Time</h3>
            <div class="stat-value">${Math.round(
              this.stats.totalStudyTimeMinutes / 60
            )}h</div>
          </div>
          <div class="stat-card">
            <h3>Quizzes Taken</h3>
            <div class="stat-value">${this.stats.quizzesTaken}</div>
          </div>
          <div class="stat-card">
            <h3>Study Guides</h3>
            <div class="stat-value">${this.stats.studyGuidesCreated}</div>
          </div>
        </div>

        <div class="quick-actions">
          <h3>Quick Actions</h3>
          <div class="action-buttons">
            <button class="btn btn-primary" onclick="this.showFileUpload()">
              Upload File
            </button>
            <button class="btn btn-secondary" onclick="this.startStudySession()">
              Start Study Session
            </button>
            <button class="btn btn-success" onclick="this.generateQuiz()">
              Generate Quiz
            </button>
            <button class="btn btn-info" onclick="this.createStudyGuide()">
              Create Study Guide
            </button>
          </div>
        </div>

        <div class="recent-activity">
          <h3>Recent Activity</h3>
          <div class="activity-list">
            ${this.recentActivity
              .map((activity) => this.renderActivity(activity))
              .join("")}
          </div>
        </div>
      </div>
    `;
  }

  renderActivity(activity) {
    const duration = this.formatDuration(activity.duration);
    const status = activity.isActive ? "Active" : "Completed";

    return `
      <div class="activity-item">
        <div class="activity-title">${activity.sessionName}</div>
        <div class="activity-meta">
          ${status} • ${duration} • ${activity.fileIds.length} files
        </div>
        ${
          activity.notes
            ? `<div class="activity-notes">${activity.notes}</div>`
            : ""
        }
      </div>
    `;
  }
}
```

## Error Handling

### Global Error Handler

```javascript
// utils/errorHandler.js
class ErrorHandler {
  static handleApiError(error, response) {
    if (response?.status === 401) {
      // Token expired or invalid
      this.handleUnauthorized();
      return;
    }

    if (response?.status === 403) {
      this.showError(
        "Access denied. You do not have permission to perform this action."
      );
      return;
    }

    if (response?.status === 404) {
      this.showError("The requested resource was not found.");
      return;
    }

    if (response?.status >= 500) {
      this.showError("Server error. Please try again later.");
      return;
    }

    this.showError(error.message || "An unexpected error occurred.");
  }

  static handleUnauthorized() {
    // Clear auth data and redirect to login
    const authManager = new AuthManager();
    authManager.logout();
    window.location.href = "/login.html";
  }

  static showError(message) {
    // Show toast notification
    const toast = document.createElement("div");
    toast.className = "toast toast-error";
    toast.textContent = message;
    document.body.appendChild(toast);

    setTimeout(() => {
      toast.remove();
    }, 5000);
  }

  static showSuccess(message) {
    const toast = document.createElement("div");
    toast.className = "toast toast-success";
    toast.textContent = message;
    document.body.appendChild(toast);

    setTimeout(() => {
      toast.remove();
    }, 3000);
  }
}
```

## State Management

### Application State

```javascript
// utils/stateManager.js
class StateManager {
  constructor() {
    this.state = {
      user: null,
      files: [],
      studyGuides: [],
      quizzes: [],
      knowledgeAnalytics: null,
      activeStudySession: null,
      notifications: [],
    };
    this.listeners = {};
  }

  setState(newState) {
    this.state = { ...this.state, ...newState };
    this.notifyListeners();
  }

  getState() {
    return this.state;
  }

  subscribe(key, callback) {
    if (!this.listeners[key]) {
      this.listeners[key] = [];
    }
    this.listeners[key].push(callback);
  }

  notifyListeners() {
    Object.values(this.listeners).forEach((callbacks) => {
      callbacks.forEach((callback) => callback(this.state));
    });
  }
}
```

## UI Components

### Modal Component

```javascript
// components/modal.js
class Modal {
  constructor(id, title) {
    this.id = id;
    this.title = title;
    this.isOpen = false;
    this.createModal();
  }

  createModal() {
    const modal = document.createElement("div");
    modal.id = this.id;
    modal.className = "modal";
    modal.innerHTML = `
      <div class="modal-content">
        <div class="modal-header">
          <h2>${this.title}</h2>
          <button class="modal-close" onclick="this.close()">&times;</button>
        </div>
        <div class="modal-body"></div>
        <div class="modal-footer"></div>
      </div>
    `;
    document.body.appendChild(modal);
  }

  open() {
    document.getElementById(this.id).style.display = "block";
    this.isOpen = true;
  }

  close() {
    document.getElementById(this.id).style.display = "none";
    this.isOpen = false;
  }

  setContent(content) {
    document.querySelector(`#${this.id} .modal-body`).innerHTML = content;
  }
}
```

## CSS Framework

### Base Styles

```css
/* css/main.css */
:root {
  --primary-color: #3498db;
  --secondary-color: #2ecc71;
  --danger-color: #e74c3c;
  --warning-color: #f39c12;
  --info-color: #17a2b8;
  --light-color: #f8f9fa;
  --dark-color: #343a40;
  --border-radius: 8px;
  --box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
  line-height: 1.6;
  color: #333;
  background-color: #f5f5f5;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

.btn {
  display: inline-block;
  padding: 10px 20px;
  border: none;
  border-radius: var(--border-radius);
  cursor: pointer;
  text-decoration: none;
  font-size: 14px;
  transition: all 0.3s ease;
}

.btn-primary {
  background-color: var(--primary-color);
  color: white;
}

.btn-primary:hover {
  background-color: #2980b9;
}

.card {
  background: white;
  border-radius: var(--border-radius);
  box-shadow: var(--box-shadow);
  padding: 20px;
  margin-bottom: 20px;
}

.toast {
  position: fixed;
  top: 20px;
  right: 20px;
  padding: 15px 20px;
  border-radius: var(--border-radius);
  color: white;
  z-index: 1000;
}

.toast-success {
  background-color: var(--secondary-color);
}

.toast-error {
  background-color: var(--danger-color);
}

.modal {
  display: none;
  position: fixed;
  z-index: 1000;
  left: 0;
  top: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
}

.modal-content {
  background-color: white;
  margin: 5% auto;
  padding: 0;
  border-radius: var(--border-radius);
  width: 90%;
  max-width: 600px;
}

.modal-header {
  padding: 20px;
  border-bottom: 1px solid #eee;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-body {
  padding: 20px;
}

.modal-footer {
  padding: 20px;
  border-top: 1px solid #eee;
  text-align: right;
}
```

## File Upload Specifications

### Supported File Types

- **Documents**: PDF (.pdf), Word (.docx), PowerPoint (.pptx)
- **Images**: JPEG (.jpg, .jpeg), PNG (.png)
- **Text**: Plain text (.txt)

### File Size Limits

- **Maximum file size**: 10MB per file
- **Maximum files per upload**: 10 files
- **Total upload size limit**: 50MB per batch

### Upload Process

1. **File Selection**: Drag-and-drop or click to select
2. **Validation**: Check file type and size
3. **Upload**: Send to `/api/files/upload` endpoint
4. **Processing**: Backend processes file asynchronously
5. **Status Updates**: Poll `/api/files/{fileId}/status` for progress
6. **Completion**: File appears in file list when processed

## Data Models

### File Upload Model

```javascript
const FileUpload = {
  id: Number,
  userId: Number,
  fileName: String,
  filePath: String,
  fileType: String,
  fileSize: Number,
  uploadedAt: Date,
  isProcessed: Boolean,
  processingStatus: String,
  studentLevel: String,
  status: String, // 'uploaded', 'processing', 'completed', 'error'
  processedAt: Date,
  extractedContent: String,
  subject: String,
  topic: String,
  autoDetectedSubject: String,
  autoDetectedTopic: String,
  userDefinedSubject: String,
  userDefinedTopic: String,
  isUserModified: Boolean,
  subjectGroupId: Number,
};
```

### Quiz Model

```javascript
const Quiz = {
  id: Number,
  userId: Number,
  title: String,
  subject: String,
  topic: String,
  level: String,
  status: String,
  createdAt: Date,
  isActive: Boolean,
  questions: [
    {
      id: Number,
      question: String,
      options: [String],
      correctAnswerIndex: Number,
      correctAnswer: String,
      explanation: String,
    },
  ],
  sourceFileIds: [Number],
  sourceStudyGuideIds: [Number],
};
```

### Study Guide Model

```javascript
const StudyGuide = {
  id: Number,
  userId: Number,
  title: String,
  content: String,
  subject: String,
  topic: String,
  level: String,
  keyPoints: [String],
  summary: String,
  status: String,
  createdAt: Date,
  updatedAt: Date,
  isActive: Boolean,
  sourceFileIds: [Number],
};
```

## User Flow Diagrams

### Authentication Flow

```
[Login Page] → [Enter Credentials] → [API Call] → [Store Token] → [Redirect to Dashboard]
     ↓
[Register Page] → [Enter Details] → [API Call] → [Store Token] → [Redirect to Dashboard]
```

### File Upload Flow

```
[Dashboard] → [Click Upload] → [Select Files] → [Validate] → [Upload] → [Processing] → [Complete]
     ↓
[File List] → [View Details] → [Edit Subject] → [Group Files] → [Generate Content]
```

### Study Session Flow

```
[Dashboard] → [Start Session] → [Select Files] → [Add Notes] → [Study] → [End Session] → [Save]
```

## Implementation Checklist

### Phase 1: Core Infrastructure

- [ ] Set up project structure
- [ ] Implement authentication system
- [ ] Create API utility functions
- [ ] Set up error handling
- [ ] Create base UI components

### Phase 2: File Management

- [ ] File upload component
- [ ] File list display
- [ ] File details modal
- [ ] Subject grouping interface
- [ ] Bulk operations

### Phase 3: AI Features

- [ ] Study guide generator
- [ ] Quiz generator and interface
- [ ] AI chat component
- [ ] Content analysis display

### Phase 4: Knowledge Tracking

- [ ] Knowledge dashboard
- [ ] Subject mastery visualization
- [ ] Learning preferences
- [ ] Performance analytics

### Phase 5: Study Sessions

- [ ] Session management
- [ ] Timer functionality
- [ ] Session history
- [ ] Progress tracking

### Phase 6: Polish & Testing

- [ ] Responsive design
- [ ] Error handling improvements
- [ ] Performance optimization
- [ ] User testing

## Development Guidelines

### Code Standards

- Use ES6+ features (arrow functions, async/await, destructuring)
- Follow consistent naming conventions (camelCase for variables, PascalCase for classes)
- Use meaningful variable and function names
- Add comments for complex logic
- Handle all error cases

### API Integration

- Always use try-catch blocks for API calls
- Implement proper loading states
- Show user-friendly error messages
- Handle network timeouts gracefully

### UI/UX Principles

- Keep the interface clean and intuitive
- Use consistent spacing and typography
- Provide visual feedback for all user actions
- Ensure accessibility (keyboard navigation, screen readers)
- Test on multiple screen sizes

This document provides a comprehensive guide for frontend developers to implement the Student Study AI Platform UI. All backend APIs are documented with request/response examples, and the component structure provides a clear path for implementation.
