// Progress Analytics Component
import { knowledgeAPI } from "../api/knowledge.js";
import { analysisAPI } from "../api/analysis.js";
import { filesAPI } from "../api/files.js";
import { store } from "../state/store.js";
import { authState } from "../state/auth-state.js";
import { notificationManager } from "../utils/notifications.js";
import { CONFIG } from "../config.js";
import { logger } from "../utils/logger.js";

class ProgressAnalytics {
  constructor() {
    this.isInitialized = false;
    this.analytics = null;
    this.preferences = null;
    this.charts = {};
    this.timeRange = "30d"; // 7d, 30d, 90d, 1y
    this.selectedSubject = "all";
    this.isEditingPreferences = false;
  }

  // Initialize progress analytics
  async init() {
    if (this.isInitialized) return;

    try {
      this.setupEventListeners();
      await this.loadAnalyticsData();
      this.isInitialized = true;
    } catch (error) {
      logger.error("Progress analytics initialization error:", error);
      notificationManager.error("Failed to initialize progress analytics");
    }
  }

  // Setup event listeners
  setupEventListeners() {
    // Time range selector
    document.addEventListener("change", (e) => {
      if (e.target.matches("[data-time-range]")) {
        this.timeRange = e.target.value;
        this.refreshAnalytics();
      }
    });

    // Subject filter
    document.addEventListener("change", (e) => {
      if (e.target.matches("[data-subject-filter]")) {
        this.selectedSubject = e.target.value;
        this.updateCharts();
      }
    });

    // Refresh button
    document.addEventListener("click", (e) => {
      if (e.target.matches("[data-refresh-analytics]")) {
        this.refreshAnalytics();
      }
    });

    // Export buttons
    document.addEventListener("click", (e) => {
      if (e.target.matches("[data-export-analytics]")) {
        this.exportAnalytics(e.target.dataset.exportAnalytics);
      }
    });

    // Progress actions
    document.addEventListener("click", (e) => {
      if (e.target.matches("[data-progress-action]")) {
        this.handleProgressAction(e.target);
      }
    });
  }

  // Handle progress actions
  handleProgressAction(button) {
    const action = button.dataset.progressAction;

    switch (action) {
      case "show-auth-modal":
        window.app?.showAuthModal();
        break;
      case "edit-preferences":
        this.isEditingPreferences = true;
        this.renderProgressView();
        break;
      case "cancel-preferences":
        this.isEditingPreferences = false;
        this.renderProgressView();
        break;
      case "save-preferences":
        this.savePreferences();
        break;
    }
  }

  // Load analytics data
  async loadAnalyticsData() {
    try {
      const state = store.getState();
      const userId = state.user?.id || 1;

      // Load analytics and preferences in parallel
      const [analyticsResult, preferencesResult] = await Promise.all([
        knowledgeAPI.getAnalytics(userId),
        knowledgeAPI.getPreferences(userId),
      ]);

      if (analyticsResult.success) {
        this.analytics = analyticsResult.data;
      } else {
        // Create mock data for demo purposes
        this.analytics = this.createMockAnalytics();
      }

      if (preferencesResult.success) {
        this.preferences = preferencesResult.data;
      } else {
        this.preferences = this.getDefaultPreferences();
      }

      this.renderProgressView();
      this.initializeCharts();
    } catch (error) {
      logger.error("Error loading analytics data:", error);
      // Create mock data for demo purposes
      this.analytics = this.createMockAnalytics();
      this.preferences = this.getDefaultPreferences();
      this.renderProgressView();
      this.initializeCharts();
    }
  }

  // Create mock analytics data
  createMockAnalytics() {
    return {
      totalQuizzes: 12,
      averageScore: 78,
      totalStudyTime: 24.5,
      knowledgeLevel: "Intermediate",
      subjectProgress: [
        { subject: "Mathematics", progress: 85, quizzes: 5, averageScore: 82 },
        { subject: "Science", progress: 72, quizzes: 4, averageScore: 75 },
        { subject: "History", progress: 68, quizzes: 3, averageScore: 71 },
      ],
      weeklyProgress: [
        { week: "Week 1", quizzes: 3, averageScore: 65 },
        { week: "Week 2", quizzes: 4, averageScore: 72 },
        { week: "Week 3", quizzes: 3, averageScore: 78 },
        { week: "Week 4", quizzes: 2, averageScore: 85 },
      ],
      recentActivity: [
        { type: "quiz", subject: "Mathematics", score: 85, date: "2024-01-15" },
        {
          type: "study_guide",
          subject: "Science",
          title: "Biology Basics",
          date: "2024-01-14",
        },
        { type: "quiz", subject: "History", score: 71, date: "2024-01-13" },
        {
          type: "file_upload",
          subject: "Mathematics",
          title: "Calculus Notes",
          date: "2024-01-12",
        },
      ],
      achievements: [
        {
          name: "First Quiz",
          description: "Completed your first quiz",
          earned: true,
          date: "2024-01-10",
        },
        {
          name: "Study Streak",
          description: "7 days in a row",
          earned: true,
          date: "2024-01-15",
        },
        {
          name: "High Scorer",
          description: "Scored 90% or higher",
          earned: false,
          date: null,
        },
        {
          name: "Subject Master",
          description: "Mastered 5 subjects",
          earned: false,
          date: null,
        },
      ],
    };
  }

  // Get default preferences
  getDefaultPreferences() {
    return {
      knowledgeLevel: "highSchool",
      preferredSubjects: ["mathematics", "science", "english"],
      studyGoals: {
        dailyStudyTime: 60, // minutes
        weeklyQuizzes: 5,
        targetScore: 80,
      },
      notifications: {
        studyReminders: true,
        quizResults: true,
        progressUpdates: true,
        email: false,
      },
      display: {
        theme: "light",
        chartType: "line",
        showAnimations: true,
      },
      learning: {
        difficulty: "medium",
        focusAreas: [],
        studyMethods: ["quizzes", "study_guides", "practice"],
      },
    };
  }

  // Save preferences
  async savePreferences() {
    try {
      const state = store.getState();
      const userId = state.user?.id || 1;

      // Get form data
      const form = document.getElementById("preferencesForm");
      if (!form) return;

      const formData = new FormData(form);
      const preferences = {
        knowledgeLevel: formData.get("knowledgeLevel"),
        preferredSubjects: Array.from(formData.getAll("preferredSubjects")),
        studyGoals: {
          dailyStudyTime: parseInt(formData.get("dailyStudyTime")),
          weeklyQuizzes: parseInt(formData.get("weeklyQuizzes")),
          targetScore: parseInt(formData.get("targetScore")),
        },
        notifications: {
          studyReminders: formData.get("studyReminders") === "on",
          quizResults: formData.get("quizResults") === "on",
          progressUpdates: formData.get("progressUpdates") === "on",
          email: formData.get("email") === "on",
        },
        display: {
          theme: formData.get("theme"),
          chartType: formData.get("chartType"),
          showAnimations: formData.get("showAnimations") === "on",
        },
        learning: {
          difficulty: formData.get("difficulty"),
          focusAreas: Array.from(formData.getAll("focusAreas")),
          studyMethods: Array.from(formData.getAll("studyMethods")),
        },
      };

      const result = await knowledgeAPI.updatePreferences(userId, preferences);
      if (result.success) {
        this.preferences = preferences;
        this.isEditingPreferences = false;
        this.renderProgressView();
        notificationManager.success("Preferences saved successfully!");
      } else {
        notificationManager.error(
          result.message || "Failed to save preferences"
        );
      }
    } catch (error) {
      logger.error("Error saving preferences:", error);
      notificationManager.error("Failed to save preferences");
    }
  }

  // Render preferences section
  renderPreferencesSection() {
    if (this.isEditingPreferences) {
      return this.renderPreferencesForm();
    }

    return `
      <div class="row mb-4">
        <div class="col-12">
          <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
              <h5 class="card-title mb-0">
                <i class="bi bi-gear me-2"></i>Learning Preferences
              </h5>
              <button class="btn btn-outline-primary btn-sm" data-progress-action="edit-preferences">
                <i class="bi bi-pencil me-1"></i>Edit
              </button>
            </div>
            <div class="card-body">
              <div class="row">
                <div class="col-md-6">
                  <h6>Study Goals</h6>
                  <ul class="list-unstyled">
                    <li><strong>Daily Study Time:</strong> ${
                      this.preferences.studyGoals.dailyStudyTime
                    } minutes</li>
                    <li><strong>Weekly Quizzes:</strong> ${
                      this.preferences.studyGoals.weeklyQuizzes
                    }</li>
                    <li><strong>Target Score:</strong> ${
                      this.preferences.studyGoals.targetScore
                    }%</li>
                  </ul>
                </div>
                <div class="col-md-6">
                  <h6>Learning Style</h6>
                  <ul class="list-unstyled">
                    <li><strong>Knowledge Level:</strong> ${
                      this.preferences.knowledgeLevel
                    }</li>
                    <li><strong>Difficulty:</strong> ${
                      this.preferences.learning.difficulty
                    }</li>
                    <li><strong>Preferred Subjects:</strong> ${this.preferences.preferredSubjects.join(
                      ", "
                    )}</li>
                  </ul>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  // Render preferences form
  renderPreferencesForm() {
    return `
      <div class="row mb-4">
        <div class="col-12">
          <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
              <h5 class="card-title mb-0">
                <i class="bi bi-gear me-2"></i>Edit Learning Preferences
              </h5>
              <div class="btn-group">
                <button class="btn btn-primary btn-sm" data-progress-action="save-preferences">
                  <i class="bi bi-check me-1"></i>Save
                </button>
                <button class="btn btn-secondary btn-sm" data-progress-action="cancel-preferences">
                  <i class="bi bi-x me-1"></i>Cancel
                </button>
              </div>
            </div>
            <div class="card-body">
              <form id="preferencesForm">
                <div class="row">
                  <div class="col-md-6">
                    <h6>Study Goals</h6>
                    <div class="mb-3">
                      <label for="dailyStudyTime" class="form-label">Daily Study Time (minutes)</label>
                      <input type="number" class="form-control" id="dailyStudyTime" name="dailyStudyTime" 
                             value="${
                               this.preferences.studyGoals.dailyStudyTime
                             }" min="15" max="480">
                    </div>
                    <div class="mb-3">
                      <label for="weeklyQuizzes" class="form-label">Weekly Quizzes</label>
                      <input type="number" class="form-control" id="weeklyQuizzes" name="weeklyQuizzes" 
                             value="${
                               this.preferences.studyGoals.weeklyQuizzes
                             }" min="1" max="20">
                    </div>
                    <div class="mb-3">
                      <label for="targetScore" class="form-label">Target Score (%)</label>
                      <input type="number" class="form-control" id="targetScore" name="targetScore" 
                             value="${
                               this.preferences.studyGoals.targetScore
                             }" min="50" max="100">
                    </div>
                  </div>
                  <div class="col-md-6">
                    <h6>Learning Style</h6>
                    <div class="mb-3">
                      <label for="knowledgeLevel" class="form-label">Knowledge Level</label>
                      <select class="form-select" id="knowledgeLevel" name="knowledgeLevel">
                        <option value="elementary" ${
                          this.preferences.knowledgeLevel === "elementary"
                            ? "selected"
                            : ""
                        }>Elementary</option>
                        <option value="middleSchool" ${
                          this.preferences.knowledgeLevel === "middleSchool"
                            ? "selected"
                            : ""
                        }>Middle School</option>
                        <option value="highSchool" ${
                          this.preferences.knowledgeLevel === "highSchool"
                            ? "selected"
                            : ""
                        }>High School</option>
                        <option value="college" ${
                          this.preferences.knowledgeLevel === "college"
                            ? "selected"
                            : ""
                        }>College</option>
                      </select>
                    </div>
                    <div class="mb-3">
                      <label for="difficulty" class="form-label">Difficulty</label>
                      <select class="form-select" id="difficulty" name="difficulty">
                        <option value="easy" ${
                          this.preferences.learning.difficulty === "easy"
                            ? "selected"
                            : ""
                        }>Easy</option>
                        <option value="medium" ${
                          this.preferences.learning.difficulty === "medium"
                            ? "selected"
                            : ""
                        }>Medium</option>
                        <option value="hard" ${
                          this.preferences.learning.difficulty === "hard"
                            ? "selected"
                            : ""
                        }>Hard</option>
                      </select>
                    </div>
                    <div class="mb-3">
                      <label class="form-label">Preferred Subjects</label>
                      <div class="form-check">
                        <input class="form-check-input" type="checkbox" name="preferredSubjects" value="mathematics" 
                               ${
                                 this.preferences.preferredSubjects.includes(
                                   "mathematics"
                                 )
                                   ? "checked"
                                   : ""
                               }>
                        <label class="form-check-label">Mathematics</label>
                      </div>
                      <div class="form-check">
                        <input class="form-check-input" type="checkbox" name="preferredSubjects" value="science" 
                               ${
                                 this.preferences.preferredSubjects.includes(
                                   "science"
                                 )
                                   ? "checked"
                                   : ""
                               }>
                        <label class="form-check-label">Science</label>
                      </div>
                      <div class="form-check">
                        <input class="form-check-input" type="checkbox" name="preferredSubjects" value="english" 
                               ${
                                 this.preferences.preferredSubjects.includes(
                                   "english"
                                 )
                                   ? "checked"
                                   : ""
                               }>
                        <label class="form-check-label">English</label>
                      </div>
                    </div>
                  </div>
                </div>
                <div class="row">
                  <div class="col-md-6">
                    <h6>Notifications</h6>
                    <div class="form-check">
                      <input class="form-check-input" type="checkbox" name="studyReminders" 
                             ${
                               this.preferences.notifications.studyReminders
                                 ? "checked"
                                 : ""
                             }>
                      <label class="form-check-label">Study Reminders</label>
                    </div>
                    <div class="form-check">
                      <input class="form-check-input" type="checkbox" name="quizResults" 
                             ${
                               this.preferences.notifications.quizResults
                                 ? "checked"
                                 : ""
                             }>
                      <label class="form-check-label">Quiz Results</label>
                    </div>
                    <div class="form-check">
                      <input class="form-check-input" type="checkbox" name="progressUpdates" 
                             ${
                               this.preferences.notifications.progressUpdates
                                 ? "checked"
                                 : ""
                             }>
                      <label class="form-check-label">Progress Updates</label>
                    </div>
                  </div>
                  <div class="col-md-6">
                    <h6>Display</h6>
                    <div class="mb-3">
                      <label for="theme" class="form-label">Theme</label>
                      <select class="form-select" id="theme" name="theme">
                        <option value="light" ${
                          this.preferences.display.theme === "light"
                            ? "selected"
                            : ""
                        }>Light</option>
                        <option value="dark" ${
                          this.preferences.display.theme === "dark"
                            ? "selected"
                            : ""
                        }>Dark</option>
                      </select>
                    </div>
                    <div class="mb-3">
                      <label for="chartType" class="form-label">Chart Type</label>
                      <select class="form-select" id="chartType" name="chartType">
                        <option value="line" ${
                          this.preferences.display.chartType === "line"
                            ? "selected"
                            : ""
                        }>Line</option>
                        <option value="bar" ${
                          this.preferences.display.chartType === "bar"
                            ? "selected"
                            : ""
                        }>Bar</option>
                        <option value="pie" ${
                          this.preferences.display.chartType === "pie"
                            ? "selected"
                            : ""
                        }>Pie</option>
                      </select>
                    </div>
                  </div>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  // Render progress view
  renderProgressView() {
    const progressView = document.getElementById("progressView");
    if (!progressView) return;

    const state = store.getState();
    const isGuest = state.isGuest;

    progressView.innerHTML = `
      <div class="row">
        <div class="col-12">
          <div class="d-flex justify-content-between align-items-center mb-4">
            <div>
              <h1 class="page-title mb-1">Progress & Analytics</h1>
              <p class="page-subtitle mb-0">Track your learning journey and achievements</p>
            </div>
            <div class="d-flex gap-2">
              <select class="form-select" data-time-range>
                <option value="7d">Last 7 days</option>
                <option value="30d" selected>Last 30 days</option>
                <option value="90d">Last 90 days</option>
                <option value="1y">Last year</option>
              </select>
              <button class="btn btn-outline-primary" data-refresh-analytics>
                <i class="bi bi-arrow-clockwise me-2"></i>Refresh
              </button>
            </div>
          </div>
        </div>
      </div>

      ${isGuest ? this.renderGuestNotice() : ""}

      <!-- Overview Stats -->
      <div class="row mb-4">
        <div class="col-md-3 col-sm-6 mb-3">
          <div class="stat-card">
            <div class="stat-icon">
              <i class="bi bi-graph-up"></i>
            </div>
            <div class="stat-value">${this.analytics.averageScore}%</div>
            <div class="stat-label">Average Score</div>
          </div>
        </div>
        <div class="col-md-3 col-sm-6 mb-3">
          <div class="stat-card">
            <div class="stat-icon">
              <i class="bi bi-question-circle"></i>
            </div>
            <div class="stat-value">${this.analytics.totalQuizzes}</div>
            <div class="stat-label">Quizzes Taken</div>
          </div>
        </div>
        <div class="col-md-3 col-sm-6 mb-3">
          <div class="stat-card">
            <div class="stat-icon">
              <i class="bi bi-clock"></i>
            </div>
            <div class="stat-value">${this.analytics.totalStudyTime}h</div>
            <div class="stat-label">Study Time</div>
          </div>
        </div>
        <div class="col-md-3 col-sm-6 mb-3">
          <div class="stat-card">
            <div class="stat-icon">
              <i class="bi bi-trophy"></i>
            </div>
            <div class="stat-value">${this.analytics.knowledgeLevel}</div>
            <div class="stat-label">Knowledge Level</div>
          </div>
        </div>
      </div>

      <!-- Charts Row -->
      <div class="row mb-4">
        <div class="col-md-8">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">
                <i class="bi bi-graph-up me-2"></i>Progress Over Time
              </h5>
            </div>
            <div class="card-body">
              <canvas id="progressChart" width="400" height="200"></canvas>
            </div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">
                <i class="bi bi-pie-chart me-2"></i>Subject Distribution
              </h5>
            </div>
            <div class="card-body">
              <canvas id="subjectChart" width="300" height="200"></canvas>
            </div>
          </div>
        </div>
      </div>

      <!-- Preferences Section -->
      ${!isGuest ? this.renderPreferencesSection() : ""}

      <!-- Subject Progress -->
      <div class="row mb-4">
        <div class="col-12">
          <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
              <h5 class="card-title mb-0">
                <i class="bi bi-bar-chart me-2"></i>Subject Progress
              </h5>
              <select class="form-select" style="width: auto;" data-subject-filter>
                <option value="all">All Subjects</option>
                ${this.analytics.subjectProgress
                  .map(
                    (subject) =>
                      `<option value="${subject.subject}">${subject.subject}</option>`
                  )
                  .join("")}
              </select>
            </div>
            <div class="card-body">
              ${this.renderSubjectProgress()}
            </div>
          </div>
        </div>
      </div>

      <!-- Achievements and Activity -->
      <div class="row">
        <div class="col-md-6">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">
                <i class="bi bi-trophy me-2"></i>Achievements
              </h5>
            </div>
            <div class="card-body">
              ${this.renderAchievements()}
            </div>
          </div>
        </div>
        <div class="col-md-6">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">
                <i class="bi bi-clock me-2"></i>Recent Activity
              </h5>
            </div>
            <div class="card-body">
              ${this.renderRecentActivity()}
            </div>
          </div>
        </div>
      </div>

      <!-- Export Options -->
      <div class="row mt-4">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">
                <i class="bi bi-download me-2"></i>Export Data
              </h5>
            </div>
            <div class="card-body">
              <div class="d-flex gap-2 flex-wrap">
                <button class="btn btn-outline-primary" data-export-analytics="pdf">
                  <i class="bi bi-file-pdf me-2"></i>Export as PDF
                </button>
                <button class="btn btn-outline-success" data-export-analytics="csv">
                  <i class="bi bi-file-spreadsheet me-2"></i>Export as CSV
                </button>
                <button class="btn btn-outline-info" data-export-analytics="json">
                  <i class="bi bi-file-code me-2"></i>Export as JSON
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  // Render guest notice
  renderGuestNotice() {
    return `
      <div class="alert alert-warning d-flex align-items-center mb-4">
        <i class="bi bi-exclamation-triangle me-2"></i>
        <div>
          <strong>Guest Mode:</strong> Analytics are limited. 
          <a href="#" class="alert-link" data-progress-action="show-auth-modal">Create an account</a> for detailed progress tracking.
        </div>
      </div>
    `;
  }

  // Render subject progress
  renderSubjectProgress() {
    const filteredSubjects =
      this.selectedSubject === "all"
        ? this.analytics.subjectProgress
        : this.analytics.subjectProgress.filter(
            (s) => s.subject === this.selectedSubject
          );

    return `
      <div class="subject-progress-list">
        ${filteredSubjects
          .map((subject) => this.renderSubjectProgressItem(subject))
          .join("")}
      </div>
    `;
  }

  // Render subject progress item
  renderSubjectProgressItem(subject) {
    const progressColor =
      subject.progress >= 80
        ? "success"
        : subject.progress >= 60
        ? "warning"
        : "danger";

    return `
      <div class="subject-progress-item">
        <div class="subject-info">
          <h6 class="subject-name">${subject.subject}</h6>
          <div class="subject-stats">
            <span class="stat-item">
              <i class="bi bi-question-circle me-1"></i>${subject.quizzes} quizzes
            </span>
            <span class="stat-item">
              <i class="bi bi-graph-up me-1"></i>${subject.averageScore}% avg
            </span>
          </div>
        </div>
        <div class="subject-progress">
          <div class="progress progress-lg">
            <div class="progress-bar bg-${progressColor}" style="width: ${subject.progress}%"></div>
          </div>
          <div class="progress-label">
            <span>Progress</span>
            <span class="progress-value">${subject.progress}%</span>
          </div>
        </div>
      </div>
    `;
  }

  // Render achievements
  renderAchievements() {
    return `
      <div class="achievements-list">
        ${this.analytics.achievements
          .map((achievement) => this.renderAchievementItem(achievement))
          .join("")}
      </div>
    `;
  }

  // Render achievement item
  renderAchievementItem(achievement) {
    const earnedClass = achievement.earned ? "earned" : "not-earned";
    const iconClass = achievement.earned ? "bi-trophy-fill" : "bi-trophy";

    return `
      <div class="achievement-item ${earnedClass}">
        <div class="achievement-icon">
          <i class="bi ${iconClass}"></i>
        </div>
        <div class="achievement-content">
          <h6 class="achievement-name">${achievement.name}</h6>
          <p class="achievement-description">${achievement.description}</p>
          ${
            achievement.earned
              ? `<small class="achievement-date">Earned on ${CONFIG.formatDate(
                  achievement.date
                )}</small>`
              : ""
          }
        </div>
      </div>
    `;
  }

  // Render recent activity
  renderRecentActivity() {
    return `
      <div class="activity-feed">
        ${this.analytics.recentActivity
          .map((activity) => this.renderActivityItem(activity))
          .join("")}
      </div>
    `;
  }

  // Render activity item
  renderActivityItem(activity) {
    const iconClass = this.getActivityIcon(activity.type);
    const iconColor = this.getActivityColor(activity.type);

    return `
      <div class="activity-item">
        <div class="activity-icon ${iconColor}">
          <i class="bi ${iconClass}"></i>
        </div>
        <div class="activity-content">
          <div class="activity-title">${this.getActivityTitle(activity)}</div>
          <div class="activity-description">${this.getActivityDescription(
            activity
          )}</div>
          <div class="activity-time">${CONFIG.formatDate(activity.date)}</div>
        </div>
      </div>
    `;
  }

  // Get activity icon
  getActivityIcon(type) {
    const icons = {
      quiz: "bi-question-circle",
      study_guide: "bi-book",
      file_upload: "bi-cloud-upload",
      chat: "bi-chat-dots",
    };
    return icons[type] || "bi-circle";
  }

  // Get activity color
  getActivityColor(type) {
    const colors = {
      quiz: "quiz",
      study_guide: "study",
      file_upload: "upload",
      chat: "chat",
    };
    return colors[type] || "upload";
  }

  // Get activity title
  getActivityTitle(activity) {
    switch (activity.type) {
      case "quiz":
        return `Quiz: ${activity.subject}`;
      case "study_guide":
        return `Study Guide: ${activity.title}`;
      case "file_upload":
        return `Uploaded: ${activity.title}`;
      case "chat":
        return `AI Chat: ${activity.subject}`;
      default:
        return "Activity";
    }
  }

  // Get activity description
  getActivityDescription(activity) {
    switch (activity.type) {
      case "quiz":
        return `Scored ${activity.score}%`;
      case "study_guide":
        return `Created study guide for ${activity.subject}`;
      case "file_upload":
        return `Uploaded file for ${activity.subject}`;
      case "chat":
        return `Had a conversation about ${activity.subject}`;
      default:
        return "Activity completed";
    }
  }

  // Initialize charts
  initializeCharts() {
    this.createProgressChart();
    this.createSubjectChart();
  }

  // Create progress chart
  createProgressChart() {
    const ctx = document.getElementById("progressChart");
    if (!ctx) return;

    // Check if Chart.js is available
    if (typeof Chart === "undefined") {
      logger.error("Chart.js not loaded");
      throw new Error("Chart.js library required but not loaded");
    }

    const labels = this.analytics.weeklyProgress.map((week) => week.week);
    const scores = this.analytics.weeklyProgress.map(
      (week) => week.averageScore
    );
    const quizzes = this.analytics.weeklyProgress.map((week) => week.quizzes);

    this.charts.progress = new Chart(ctx, {
      type: "line",
      data: {
        labels: labels,
        datasets: [
          {
            label: "Average Score (%)",
            data: scores,
            borderColor: "rgb(44, 90, 160)",
            backgroundColor: "rgba(44, 90, 160, 0.1)",
            tension: 0.4,
            yAxisID: "y",
          },
          {
            label: "Quizzes Taken",
            data: quizzes,
            borderColor: "rgb(40, 167, 69)",
            backgroundColor: "rgba(40, 167, 69, 0.1)",
            tension: 0.4,
            yAxisID: "y1",
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          y: {
            type: "linear",
            display: true,
            position: "left",
            title: {
              display: true,
              text: "Score (%)",
            },
          },
          y1: {
            type: "linear",
            display: true,
            position: "right",
            title: {
              display: true,
              text: "Quizzes",
            },
            grid: {
              drawOnChartArea: false,
            },
          },
        },
        plugins: {
          legend: {
            position: "top",
          },
          title: {
            display: false,
          },
        },
      },
    });
  }

  // Create subject chart
  createSubjectChart() {
    const ctx = document.getElementById("subjectChart");
    if (!ctx) return;

    // Check if Chart.js is available
    if (typeof Chart === "undefined") {
      logger.error("Chart.js not loaded");
      throw new Error("Chart.js library required but not loaded");
    }

    const labels = this.analytics.subjectProgress.map(
      (subject) => subject.subject
    );
    const data = this.analytics.subjectProgress.map(
      (subject) => subject.progress
    );
    const colors = this.analytics.subjectProgress.map((subject) =>
      subject.progress >= 80
        ? "#28a745"
        : subject.progress >= 60
        ? "#ffc107"
        : "#dc3545"
    );

    this.charts.subject = new Chart(ctx, {
      type: "doughnut",
      data: {
        labels: labels,
        datasets: [
          {
            data: data,
            backgroundColor: colors,
            borderWidth: 2,
            borderColor: "#fff",
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: "bottom",
          },
          title: {
            display: false,
          },
        },
      },
    });
  }

  // Update charts
  updateCharts() {
    if (this.charts.progress) {
      this.charts.progress.destroy();
    }
    if (this.charts.subject) {
      this.charts.subject.destroy();
    }
    this.initializeCharts();
  }

  // Refresh analytics
  async refreshAnalytics() {
    try {
      notificationManager.info("Refreshing analytics...");
      await this.loadAnalyticsData();
      notificationManager.success("Analytics refreshed!");
    } catch (error) {
      logger.error("Error refreshing analytics:", error);
      notificationManager.error("Failed to refresh analytics");
    }
  }

  // Export analytics
  exportAnalytics(format) {
    try {
      let data, filename, mimeType;

      switch (format) {
        case "pdf":
          // For PDF export, we would need a PDF library
          notificationManager.info("PDF export coming soon!");
          return;
        case "csv":
          data = this.convertToCSV();
          filename = "analytics.csv";
          mimeType = "text/csv";
          break;
        case "json":
          data = JSON.stringify(this.analytics, null, 2);
          filename = "analytics.json";
          mimeType = "application/json";
          break;
        default:
          notificationManager.error("Unsupported export format");
          return;
      }

      const blob = new Blob([data], { type: mimeType });
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = filename;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);

      notificationManager.success(
        `Analytics exported as ${format.toUpperCase()}`
      );
    } catch (error) {
      logger.error("Error exporting analytics:", error);
      notificationManager.error("Failed to export analytics");
    }
  }

  // Convert analytics to CSV
  convertToCSV() {
    const headers = ["Subject", "Progress", "Quizzes", "Average Score"];
    const rows = this.analytics.subjectProgress.map((subject) => [
      subject.subject,
      subject.progress,
      subject.quizzes,
      subject.averageScore,
    ]);

    return [headers, ...rows].map((row) => row.join(",")).join("\n");
  }
}

// Create global progress analytics instance
const progressAnalytics = new ProgressAnalytics();

// Export for global access
window.progressAnalytics = progressAnalytics;

export { progressAnalytics };
