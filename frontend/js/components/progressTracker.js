// Progress tracking component

import { knowledgeAPI } from "../api/knowledge.js";
import { logger } from "../utils/logger.js";
import { notificationManager } from "../utils/notifications.js";
import { authState } from "../state/auth-state.js";

class ProgressTracker {
  constructor(container) {
    this.container = container;
    this.progressData = null;
    this.charts = {};
    this.init();
  }

  init() {
    // Initialize progress tracker
    logger.info("Initializing progress tracker");

    // Load progress data from API
    this.loadProgressData();

    // Set up charts and visualizations
    this.setupCharts();

    // Configure tracking options
    this.setupEventListeners();
  }

  setupEventListeners() {
    // Listen for refresh button
    this.container.addEventListener("click", (e) => {
      if (e.target.matches("[data-refresh-progress]")) {
        this.refreshProgress();
      }
    });

    // Listen for time range changes
    this.container.addEventListener("change", (e) => {
      if (e.target.matches("[data-time-range]")) {
        this.updateTimeRange(e.target.value);
      }
    });
  }

  async loadProgressData() {
    try {
      const user = authState.getState().user;
      if (!user) {
        logger.warn("No user found, skipping progress data load");
        return;
      }

      const analytics = await knowledgeAPI.getAnalytics(user.id);
      this.progressData = analytics.data;
      this.render();
    } catch (error) {
      logger.error("Error loading progress data:", error);
      notificationManager.error("Failed to load progress data");
    }
  }

  setupCharts() {
    // Initialize chart configurations
    this.charts = {
      progress: null,
      performance: null,
      subjects: null,
    };
  }

  render() {
    if (!this.progressData) {
      this.container.innerHTML = `
        <div class="progress-empty">
          <i class="bi bi-graph-up"></i>
          <p>No progress data available yet. Complete quizzes and study sessions to track your progress.</p>
        </div>
      `;
      return;
    }

    // Create progress charts and display statistics
    this.container.innerHTML = `
      <div class="progress-tracker-container">
        <div class="progress-header">
          <h3>Your Progress</h3>
          <div class="progress-actions">
            <select class="form-select form-select-sm" data-time-range>
              <option value="7d">Last 7 Days</option>
              <option value="30d" selected>Last 30 Days</option>
              <option value="90d">Last 90 Days</option>
              <option value="all">All Time</option>
            </select>
            <button class="btn btn-sm btn-outline-primary" data-refresh-progress>
              <i class="bi bi-arrow-clockwise"></i> Refresh
            </button>
          </div>
        </div>

        <div class="progress-stats">
          ${this.renderStats()}
        </div>

        <div class="progress-charts">
          ${this.renderCharts()}
        </div>

        <div class="progress-trends">
          ${this.renderTrends()}
        </div>
      </div>
    `;
  }

  renderStats() {
    const { totalFiles, totalQuizzes, averageScore, studyTime } =
      this.progressData;

    return `
      <div class="stats-grid">
        <div class="stat-card">
          <div class="stat-icon">
            <i class="bi bi-file-earmark"></i>
          </div>
          <div class="stat-content">
            <div class="stat-value">${totalFiles || 0}</div>
            <div class="stat-label">Files Uploaded</div>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-icon">
            <i class="bi bi-question-circle"></i>
          </div>
          <div class="stat-content">
            <div class="stat-value">${totalQuizzes || 0}</div>
            <div class="stat-label">Quizzes Taken</div>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-icon">
            <i class="bi bi-trophy"></i>
          </div>
          <div class="stat-content">
            <div class="stat-value">${averageScore || 0}%</div>
            <div class="stat-label">Average Score</div>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-icon">
            <i class="bi bi-clock"></i>
          </div>
          <div class="stat-content">
            <div class="stat-value">${this.formatStudyTime(
              studyTime || 0
            )}</div>
            <div class="stat-label">Study Time</div>
          </div>
        </div>
      </div>
    `;
  }

  renderCharts() {
    return `
      <div class="charts-container">
        <div class="chart-card">
          <h4>Performance Over Time</h4>
          <canvas id="performanceChart"></canvas>
        </div>

        <div class="chart-card">
          <h4>Subject Progress</h4>
          <canvas id="subjectChart"></canvas>
        </div>
      </div>
    `;
  }

  renderTrends() {
    const { subjectProfiles, recentActivity } = this.progressData;

    if (!subjectProfiles || Object.keys(subjectProfiles).length === 0) {
      return `<div class="trends-empty">No trends data available yet.</div>`;
    }

    return `
      <div class="trends-container">
        <h4>Subject Breakdown</h4>
        <div class="subject-list">
          ${Object.entries(subjectProfiles)
            .map(
              ([subject, profile]) => `
            <div class="subject-item">
              <div class="subject-name">${subject}</div>
              <div class="subject-progress">
                <div class="progress">
                  <div class="progress-bar" style="width: ${
                    profile.averageScore || 0
                  }%"></div>
                </div>
                <span class="progress-text">${profile.averageScore || 0}%</span>
              </div>
              <div class="subject-stats">
                <span>${profile.quizCount || 0} quizzes</span>
                <span>${this.formatStudyTime(profile.studyTime || 0)}</span>
              </div>
            </div>
          `
            )
            .join("")}
        </div>
      </div>
    `;
  }

  updateProgress(progressData) {
    // Update progress display
    logger.info("Updating progress display");

    this.progressData = progressData;

    // Refresh progress metrics
    this.render();

    // Update charts
    this.updateCharts();

    // Show improvements notification
    if (progressData.improvements) {
      notificationManager.success(
        `Great job! You've improved in ${progressData.improvements.length} areas!`
      );
    }
  }

  updateCharts() {
    // Update chart data
    logger.info("Updating progress charts");

    // TODO: Implement actual chart updates using Chart.js or similar
    // For now, just re-render
    this.render();
  }

  async refreshProgress() {
    logger.info("Refreshing progress data");
    await this.loadProgressData();
    notificationManager.success("Progress data refreshed");
  }

  updateTimeRange(range) {
    logger.info(`Updating time range to: ${range}`);
    // TODO: Filter progress data by time range
    this.render();
  }

  formatStudyTime(minutes) {
    if (minutes < 60) {
      return `${minutes}m`;
    }
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return mins > 0 ? `${hours}h ${mins}m` : `${hours}h`;
  }
}

export default ProgressTracker;
