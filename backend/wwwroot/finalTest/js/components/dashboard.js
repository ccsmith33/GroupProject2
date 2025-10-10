// Dashboard Component
import { filesAPI } from "../api/files.js";
import { analysisAPI } from "../api/analysis.js";
import { knowledgeAPI } from "../api/knowledge.js";
import { userAPI } from "../api/user.js";
import { store } from "../state/store.js";
import { authState } from "../state/auth-state.js";
import { notificationManager } from "../utils/notifications.js";
import { CONFIG } from "../config.js";
import { logger } from "../utils/logger.js";
import { sanitizeText } from "../utils/sanitize.js";

class Dashboard {
  constructor() {
    this.isInitialized = false;
    this.stats = {
      totalFiles: 0,
      totalStudyGuides: 0,
      totalQuizzes: 0,
      totalConversations: 0,
      recentActivity: [],
      knowledgeLevel: "Beginner",
      studyStreak: 0,
      averageScore: 0,
    };
    this.analytics = null;
    this.recentFiles = [];
    this.recentStudyGuides = [];
    this.recentQuizzes = [];
    this.currentSession = null;
    this.sessionTimer = null;
    this.sessionStartTime = null;
  }

  // Initialize dashboard
  async init() {
    if (this.isInitialized) return;

    try {
      this.setupEventListeners();
      await this.loadDashboardData();
      this.isInitialized = true;
    } catch (error) {
      logger.error("Dashboard initialization error:", error);
      notificationManager.error("Failed to initialize dashboard");
    }
  }

  // Setup event listeners
  setupEventListeners() {
    // Quick action buttons
    document.addEventListener("click", (e) => {
      if (e.target.matches("[data-dashboard-action]")) {
        this.handleDashboardAction(e.target);
      }
    });

    // Refresh button
    document.addEventListener("click", (e) => {
      if (e.target.matches("[data-refresh-dashboard]")) {
        this.refreshDashboard();
      }
    });
  }

  // Load dashboard data
  async loadDashboardData() {
    try {
      const state = store.getState();
      const userId = state.user?.id || 1;

      // Load files
      const filesResult = await filesAPI.getFiles(userId);
      if (filesResult.success) {
        this.recentFiles = (filesResult.data || []).slice(0, 5);
        this.stats.totalFiles = filesResult.data?.length || 0;
      }

      // Load study guides
      const guidesResult = await analysisAPI.getStudyGuides(userId);
      if (guidesResult.success) {
        this.recentStudyGuides = (guidesResult.data || []).slice(0, 3);
        this.stats.totalStudyGuides = guidesResult.data?.length || 0;
      }

      // Load quizzes
      const quizzesResult = await analysisAPI.getQuizzes(userId);
      if (quizzesResult.success) {
        this.recentQuizzes = (quizzesResult.data || []).slice(0, 3);
        this.stats.totalQuizzes = quizzesResult.data?.length || 0;
      }

      // Load conversations
      const conversationsResult = await analysisAPI.getConversations(userId);
      if (conversationsResult.success) {
        this.stats.totalConversations = conversationsResult.data?.length || 0;
      }

      // Load analytics
      try {
        const analyticsResult = await knowledgeAPI.getAnalytics(userId);
        if (analyticsResult.success) {
          this.analytics = analyticsResult.data;
          this.updateStatsFromAnalytics();
        }
      } catch (error) {
        logger.debug("Analytics not available:", error.message);
        this.analytics = null;
      }

      // Calculate study streak (mock data for now)
      this.stats.studyStreak = this.calculateStudyStreak();
      this.stats.knowledgeLevel = this.determineKnowledgeLevel();
    } catch (error) {
      logger.error("Error loading dashboard data:", error);
    }
  }

  // Update stats from analytics
  updateStatsFromAnalytics() {
    if (!this.analytics) return;

    this.stats.averageScore = this.analytics.averageScore || 0;
    this.stats.knowledgeLevel =
      this.analytics.overallKnowledgeLevel || "Beginner";
  }

  // Calculate study streak (mock implementation)
  calculateStudyStreak() {
    // This would calculate based on actual study activity
    // For now, return a mock value
    return Math.floor(Math.random() * 7) + 1;
  }

  // Determine knowledge level
  determineKnowledgeLevel() {
    if (this.stats.averageScore >= 90) return "Expert";
    if (this.stats.averageScore >= 80) return "Advanced";
    if (this.stats.averageScore >= 70) return "Intermediate";
    return "Beginner";
  }

  // Render dashboard
  renderDashboard() {
    const dashboardView = document.getElementById("dashboardView");
    if (!dashboardView) return;

    const state = store.getState();
    const isGuest = state.isGuest;

    dashboardView.innerHTML = `
      <div class="row">
        <div class="col-12">
          <div class="d-flex justify-content-between align-items-center mb-4">
            <div>
              <h1 class="page-title mb-1">Welcome back${
                state.user ? `, ${state.user.name || state.user.email}` : ""
              }!</h1>
              <p class="page-subtitle mb-0">Here's your learning overview</p>
            </div>
            <button class="btn btn-outline-primary" data-refresh-dashboard>
              <i class="bi bi-arrow-clockwise me-2"></i>Refresh
            </button>
          </div>
        </div>
      </div>

      ${isGuest ? this.renderGuestWelcome() : ""}

      <!-- Stats Cards -->
      <div class="row mb-4">
        <div class="col-md-3 col-sm-6 mb-3">
          <div class="stat-card">
            <div class="stat-icon">
              <i class="bi bi-folder"></i>
            </div>
            <div class="stat-value">${this.stats.totalFiles}</div>
            <div class="stat-label">Files Uploaded</div>
          </div>
        </div>
        <div class="col-md-3 col-sm-6 mb-3">
          <div class="stat-card">
            <div class="stat-icon">
              <i class="bi bi-book"></i>
            </div>
            <div class="stat-value">${this.stats.totalStudyGuides}</div>
            <div class="stat-label">Study Guides</div>
          </div>
        </div>
        <div class="col-md-3 col-sm-6 mb-3">
          <div class="stat-card">
            <div class="stat-icon">
              <i class="bi bi-question-circle"></i>
            </div>
            <div class="stat-value">${this.stats.totalQuizzes}</div>
            <div class="stat-label">Quizzes Taken</div>
          </div>
        </div>
        <div class="col-md-3 col-sm-6 mb-3">
          <div class="stat-card">
            <div class="stat-icon">
              <i class="bi bi-graph-up"></i>
            </div>
            <div class="stat-value">${this.stats.averageScore}%</div>
            <div class="stat-label">Average Score</div>
          </div>
        </div>
      </div>

      <!-- Study Session Status -->
      <div class="row mb-4">
        <div class="col-12">
          ${this.renderSessionStatus()}
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="row mb-4">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">
                <i class="bi bi-lightning me-2"></i>Quick Actions
              </h5>
            </div>
            <div class="card-body">
              <div class="row">
                <div class="col-md-3 col-sm-6 mb-3">
                  <button class="btn btn-primary w-100 h-100 py-3" data-dashboard-action="upload-files">
                    <i class="bi bi-cloud-upload display-6 d-block mb-2"></i>
                    <div class="fw-medium">Upload Files</div>
                    <small class="text-white-50">Add study materials</small>
                  </button>
                </div>
                <div class="col-md-3 col-sm-6 mb-3">
                  <button class="btn btn-success w-100 h-100 py-3" data-dashboard-action="generate-guide">
                    <i class="bi bi-magic display-6 d-block mb-2"></i>
                    <div class="fw-medium">Study Guide</div>
                    <small class="text-white-50">Generate from files</small>
                  </button>
                </div>
                <div class="col-md-3 col-sm-6 mb-3">
                  <button class="btn btn-warning w-100 h-100 py-3" data-dashboard-action="take-quiz">
                    <i class="bi bi-question-circle display-6 d-block mb-2"></i>
                    <div class="fw-medium">Take Quiz</div>
                    <small class="text-white-50">Test your knowledge</small>
                  </button>
                </div>
                <div class="col-md-3 col-sm-6 mb-3">
                  <button class="btn btn-info w-100 h-100 py-3" data-dashboard-action="chat-ai">
                    <i class="bi bi-chat-dots display-6 d-block mb-2"></i>
                    <div class="fw-medium">Chat with AI</div>
                    <small class="text-white-50">Ask questions</small>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Recent Activity -->
      <div class="row">
        <div class="col-md-8">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">
                <i class="bi bi-clock me-2"></i>Recent Files
              </h5>
            </div>
            <div class="card-body">
              ${this.renderRecentFiles()}
            </div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">
                <i class="bi bi-trophy me-2"></i>Learning Progress
              </h5>
            </div>
            <div class="card-body">
              ${this.renderLearningProgress()}
            </div>
          </div>
        </div>
      </div>

      <!-- Study Content -->
      <div class="row mt-4">
        <div class="col-md-6">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">
                <i class="bi bi-book me-2"></i>Recent Study Guides
              </h5>
            </div>
            <div class="card-body">
              ${this.renderRecentStudyGuides()}
            </div>
          </div>
        </div>
        <div class="col-md-6">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">
                <i class="bi bi-question-circle me-2"></i>Recent Quizzes
              </h5>
            </div>
            <div class="card-body">
              ${this.renderRecentQuizzes()}
            </div>
          </div>
        </div>
      </div>
    `;
  }

  // Render guest welcome
  renderGuestWelcome() {
    return `
      <div class="alert alert-info d-flex align-items-center mb-4">
        <i class="bi bi-info-circle me-2"></i>
        <div class="flex-grow-1">
          <strong>Guest Mode:</strong> You're using limited features. 
          <a href="#" class="alert-link" data-dashboard-action="show-auth-modal">Create an account</a> for full access to all features and to save your progress.
        </div>
      </div>
    `;
  }

  // Render recent files
  renderRecentFiles() {
    if (this.recentFiles.length === 0) {
      return `
        <div class="empty-state-small text-center py-3">
          <i class="bi bi-folder-x text-muted"></i>
          <p class="text-muted mb-0">No files uploaded yet</p>
          <button class="btn btn-primary btn-sm mt-2" data-dashboard-action="upload-files">
            <i class="bi bi-plus me-1"></i>Upload Files
          </button>
        </div>
      `;
    }

    return `
      <div class="recent-files">
        ${this.recentFiles.map((file) => this.renderFileItem(file)).join("")}
      </div>
    `;
  }

  // Render file item
  renderFileItem(file) {
    const statusBadge = this.getFileStatusBadge(file);
    const subjectColor = CONFIG.getSubjectColor(
      file.autoDetectedSubject || file.subject || "General"
    );

    return `
      <div class="file-item">
        <div class="file-icon" style="background-color: ${subjectColor};">
          <i class="bi ${CONFIG.getFileIcon(file.fileType)}"></i>
        </div>
        <div class="file-details">
          <div class="file-name">${file.fileName}</div>
          <div class="file-meta">
            <span class="file-type">${file.fileType}</span>
            <span class="file-size">${CONFIG.formatFileSize(
              file.fileSize
            )}</span>
            <span class="file-date">${CONFIG.formatDate(file.uploadedAt)}</span>
          </div>
        </div>
        <div class="file-status">
          ${statusBadge}
        </div>
      </div>
    `;
  }

  // Get file status badge
  getFileStatusBadge(file) {
    if (file.isProcessed) {
      return '<span class="badge bg-success">Processed</span>';
    } else if (file.processingStatus === "processing") {
      return '<span class="badge bg-warning">Processing...</span>';
    } else if (file.processingStatus === "failed") {
      return '<span class="badge bg-danger">Failed</span>';
    } else {
      return '<span class="badge bg-secondary">Pending</span>';
    }
  }

  // Render learning progress
  renderLearningProgress() {
    const progressPercentage = Math.min(this.stats.averageScore || 0, 100);

    return `
      <div class="learning-progress">
        <div class="progress-item">
          <div class="progress-label">
            <span>Knowledge Level</span>
            <span class="progress-value">${this.stats.knowledgeLevel}</span>
          </div>
          <div class="progress progress-lg">
            <div class="progress-bar" style="width: ${this.getKnowledgeLevelPercentage()}%"></div>
          </div>
        </div>
        
        <div class="progress-item">
          <div class="progress-label">
            <span>Study Streak</span>
            <span class="progress-value">${this.stats.studyStreak} days</span>
          </div>
          <div class="progress progress-lg">
            <div class="progress-bar bg-warning" style="width: ${Math.min(
              (this.stats.studyStreak / 7) * 100,
              100
            )}%"></div>
          </div>
        </div>
        
        <div class="progress-item">
          <div class="progress-label">
            <span>Average Score</span>
            <span class="progress-value">${this.stats.averageScore}%</span>
          </div>
          <div class="progress progress-lg">
            <div class="progress-bar ${
              progressPercentage >= 70
                ? "bg-success"
                : progressPercentage >= 50
                ? "bg-warning"
                : "bg-danger"
            }" 
                 style="width: ${progressPercentage}%"></div>
          </div>
        </div>
      </div>
    `;
  }

  // Get knowledge level percentage
  getKnowledgeLevelPercentage() {
    const levels = {
      Beginner: 25,
      Intermediate: 50,
      Advanced: 75,
      Expert: 100,
    };
    return levels[this.stats.knowledgeLevel] || 25;
  }

  // Render recent study guides
  renderRecentStudyGuides() {
    if (this.recentStudyGuides.length === 0) {
      return `
        <div class="empty-state-small text-center py-3">
          <i class="bi bi-book text-muted"></i>
          <p class="text-muted mb-0">No study guides yet</p>
          <button class="btn btn-success btn-sm mt-2" data-dashboard-action="generate-guide">
            <i class="bi bi-plus me-1"></i>Generate Guide
          </button>
        </div>
      `;
    }

    return `
      <div class="content-list">
        ${this.recentStudyGuides
          .map((guide) => this.renderStudyGuideItem(guide))
          .join("")}
      </div>
    `;
  }

  // Render study guide item
  renderStudyGuideItem(guide) {
    return `
      <div class="content-item">
        <div class="content-header">
          <h6 class="content-title">${
            guide.title || "Untitled Study Guide"
          }</h6>
          <span class="content-date">${CONFIG.formatDate(
            guide.createdAt
          )}</span>
        </div>
        <div class="content-body">
          <p class="content-preview">${CONFIG.truncateText(
            guide.content || "",
            80
          )}</p>
          <div class="content-actions">
            <button class="btn btn-sm btn-outline-primary" data-dashboard-action="view-guide" data-guide-id="${
              guide.id
            }">
              <i class="bi bi-eye me-1"></i>View
            </button>
          </div>
        </div>
      </div>
    `;
  }

  // Render recent quizzes
  renderRecentQuizzes() {
    if (this.recentQuizzes.length === 0) {
      return `
        <div class="empty-state-small text-center py-3">
          <i class="bi bi-question-circle text-muted"></i>
          <p class="text-muted mb-0">No quizzes yet</p>
          <button class="btn btn-warning btn-sm mt-2" data-dashboard-action="take-quiz">
            <i class="bi bi-plus me-1"></i>Take Quiz
          </button>
        </div>
      `;
    }

    return `
      <div class="content-list">
        ${this.recentQuizzes.map((quiz) => this.renderQuizItem(quiz)).join("")}
      </div>
    `;
  }

  // Render quiz item
  renderQuizItem(quiz) {
    const questionCount = quiz.questions
      ? JSON.parse(quiz.questions).length
      : 0;

    return `
      <div class="content-item">
        <div class="content-header">
          <h6 class="content-title">${quiz.title || "Untitled Quiz"}</h6>
          <span class="content-date">${CONFIG.formatDate(quiz.createdAt)}</span>
        </div>
        <div class="content-body">
          <p class="content-preview">
            ${questionCount} questions • ${quiz.subject || "General"} • ${
      quiz.level || "Beginner"
    }
          </p>
          <div class="content-actions">
            <button class="btn btn-sm btn-outline-primary" data-dashboard-action="view-quiz" data-quiz-id="${
              quiz.id
            }">
              <i class="bi bi-play me-1"></i>Start
            </button>
          </div>
        </div>
      </div>
    `;
  }

  // Handle dashboard actions
  async handleDashboardAction(button) {
    const action = button.dataset.dashboardAction;

    switch (action) {
      case "upload-files":
        app.showView("files");
        break;
      case "generate-guide":
        app.showView("study");
        break;
      case "take-quiz":
        app.showView("study");
        break;
      case "chat-ai":
        app.showView("study");
        break;
      case "view-guide":
        const guideId = parseInt(button.dataset.guideId);
        this.viewStudyGuide(guideId);
        break;
      case "show-auth-modal":
        window.app?.showAuthModal();
        break;
      case "start-session":
        this.startStudySession();
        break;
      case "end-session":
        this.endStudySession();
        break;
      case "view-quiz":
        const quizId = parseInt(button.dataset.quizId);
        this.viewQuiz(quizId);
        break;
    }
  }

  // View study guide
  viewStudyGuide(guideId) {
    // This would open a modal or navigate to study guide view
    notificationManager.info("Viewing study guide...");
  }

  // View quiz
  viewQuiz(quizId) {
    // This would open a modal or navigate to quiz view
    notificationManager.info("Starting quiz...");
  }

  // Refresh dashboard
  async refreshDashboard() {
    try {
      notificationManager.info("Refreshing dashboard...");
      await this.loadDashboardData();
      this.renderDashboard();
      notificationManager.success("Dashboard refreshed!");
    } catch (error) {
      logger.error("Error refreshing dashboard:", error);
      notificationManager.error("Failed to refresh dashboard");
    }
  }

  // Start study session
  async startStudySession() {
    try {
      const state = store.getState();
      if (state.isGuest) {
        notificationManager.info(
          "Please create an account to track study sessions"
        );
        return;
      }

      const sessionName = prompt("Enter session name (optional):");
      if (sessionName === null) return; // User cancelled

      const sessionData = {
        sessionName: sanitizeText(sessionName || "Study Session"),
        notes: "",
        fileIds: [],
        isActive: true,
      };

      const result = await userAPI.createStudySession(sessionData);
      if (result.success) {
        this.currentSession = result.data;
        this.sessionStartTime = new Date();
        this.startSessionTimer();
        this.renderDashboard();
        notificationManager.success("Study session started!");
      } else {
        notificationManager.error("Failed to start study session");
      }
    } catch (error) {
      logger.error("Error starting study session:", error);
      notificationManager.error("Failed to start study session");
    }
  }

  // End study session
  async endStudySession() {
    try {
      if (!this.currentSession) return;

      const notes = prompt("Add session notes (optional):");
      const sessionData = {
        isActive: false,
        endSession: true,
        notes: sanitizeText(notes || ""),
        endTime: new Date().toISOString(),
      };

      const result = await userAPI.updateStudySession(
        this.currentSession.id,
        sessionData
      );
      if (result.success) {
        this.stopSessionTimer();
        this.currentSession = null;
        this.sessionStartTime = null;
        this.renderDashboard();
        notificationManager.success("Study session ended!");
        await this.loadDashboardData(); // Refresh stats
      } else {
        notificationManager.error("Failed to end study session");
      }
    } catch (error) {
      logger.error("Error ending study session:", error);
      notificationManager.error("Failed to end study session");
    }
  }

  // Start session timer
  startSessionTimer() {
    this.sessionTimer = setInterval(() => {
      this.updateSessionDisplay();
    }, 1000);
  }

  // Stop session timer
  stopSessionTimer() {
    if (this.sessionTimer) {
      clearInterval(this.sessionTimer);
      this.sessionTimer = null;
    }
  }

  // Update session display
  updateSessionDisplay() {
    const sessionDisplay = document.getElementById("sessionDisplay");
    if (!sessionDisplay || !this.sessionStartTime) return;

    const now = new Date();
    const elapsed = Math.floor((now - this.sessionStartTime) / 1000);
    const hours = Math.floor(elapsed / 3600);
    const minutes = Math.floor((elapsed % 3600) / 60);
    const seconds = elapsed % 60;

    const timeString = `${hours.toString().padStart(2, "0")}:${minutes
      .toString()
      .padStart(2, "0")}:${seconds.toString().padStart(2, "0")}`;
    sessionDisplay.textContent = timeString;
  }

  // Render session status
  renderSessionStatus() {
    if (!this.currentSession) {
      return `
        <div class="session-status">
          <div class="alert alert-info">
            <i class="bi bi-play-circle me-2"></i>
            <strong>No active session</strong>
            <button class="btn btn-sm btn-primary ms-2" data-dashboard-action="start-session">
              Start Study Session
            </button>
          </div>
        </div>
      `;
    }

    return `
      <div class="session-status">
        <div class="alert alert-success">
          <i class="bi bi-clock me-2"></i>
          <strong>Active Session:</strong> ${sanitizeText(
            this.currentSession.sessionName
          )}
          <span class="ms-2" id="sessionDisplay">00:00:00</span>
          <button class="btn btn-sm btn-outline-danger ms-2" data-dashboard-action="end-session">
            End Session
          </button>
        </div>
      </div>
    `;
  }
}

// Create global dashboard instance
const dashboard = new Dashboard();

// Export for global access
window.dashboard = dashboard;

export { dashboard };
