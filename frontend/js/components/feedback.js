// Feedback component for displaying AI analysis results

import { analysisAPI } from "../api/analysis.js";
import { logger } from "../utils/logger.js";
import { notificationManager } from "../utils/notifications.js";

class Feedback {
  constructor(container) {
    this.container = container;
    this.feedbackData = null;
    this.init();
  }

  init() {
    // Initialize feedback component
    logger.info("Initializing feedback component");

    // Set up event listeners for feedback interactions
    this.setupEventListeners();

    // Load any saved feedback data
    this.loadFeedbackData();
  }

  setupEventListeners() {
    // Listen for feedback actions
    this.container.addEventListener("click", (e) => {
      if (e.target.matches("[data-feedback-action]")) {
        this.handleFeedbackAction(e.target);
      }
    });
  }

  handleFeedbackAction(button) {
    const action = button.dataset.feedbackAction;
    const feedbackId = button.dataset.feedbackId;

    switch (action) {
      case "expand":
        this.expandFeedback(feedbackId);
        break;
      case "collapse":
        this.collapseFeedback(feedbackId);
        break;
      case "copy":
        this.copyFeedback(feedbackId);
        break;
      case "export":
        this.exportFeedback(feedbackId);
        break;
    }
  }

  render(feedbackData) {
    // Render feedback UI
    if (!feedbackData) {
      this.container.innerHTML = `
        <div class="feedback-empty">
          <i class="bi bi-chat-left-text"></i>
          <p>No feedback available yet. Generate a study guide or quiz to see AI feedback.</p>
        </div>
      `;
      return;
    }

    this.feedbackData = feedbackData;

    // Display AI feedback
    this.container.innerHTML = `
      <div class="feedback-container">
        <div class="feedback-header">
          <h3>AI Feedback</h3>
          <div class="feedback-actions">
            <button class="btn btn-sm btn-outline-primary" data-feedback-action="copy">
              <i class="bi bi-clipboard"></i> Copy
            </button>
            <button class="btn btn-sm btn-outline-primary" data-feedback-action="export">
              <i class="bi bi-download"></i> Export
            </button>
          </div>
        </div>
        
        ${this.renderFeedbackContent(feedbackData)}
        ${this.renderRecommendations(feedbackData.recommendations)}
        ${this.renderAnalysisResults(feedbackData.analysis)}
      </div>
    `;
  }

  renderFeedbackContent(feedbackData) {
    return `
      <div class="feedback-content">
        <div class="feedback-section">
          <h4>Summary</h4>
          <p>${feedbackData.summary || "No summary available"}</p>
        </div>
        
        ${
          feedbackData.strengths
            ? `
          <div class="feedback-section">
            <h4><i class="bi bi-check-circle text-success"></i> Strengths</h4>
            <ul>
              ${feedbackData.strengths
                .map((strength) => `<li>${strength}</li>`)
                .join("")}
            </ul>
          </div>
        `
            : ""
        }
        
        ${
          feedbackData.areasForImprovement
            ? `
          <div class="feedback-section">
            <h4><i class="bi bi-exclamation-circle text-warning"></i> Areas for Improvement</h4>
            <ul>
              ${feedbackData.areasForImprovement
                .map((area) => `<li>${area}</li>`)
                .join("")}
            </ul>
          </div>
        `
            : ""
        }
      </div>
    `;
  }

  renderRecommendations(recommendations) {
    if (!recommendations || recommendations.length === 0) {
      return "";
    }

    return `
      <div class="feedback-recommendations">
        <h4><i class="bi bi-lightbulb"></i> Study Recommendations</h4>
        <div class="recommendations-list">
          ${recommendations
            .map(
              (rec, index) => `
            <div class="recommendation-card">
              <div class="recommendation-number">${index + 1}</div>
              <div class="recommendation-content">
                <h5>${rec.title}</h5>
                <p>${rec.description}</p>
                ${
                  rec.action
                    ? `
                  <button class="btn btn-sm btn-primary" onclick="window.app.${
                    rec.action
                  }()">
                    ${rec.actionText || "Take Action"}
                  </button>
                `
                    : ""
                }
              </div>
            </div>
          `
            )
            .join("")}
        </div>
      </div>
    `;
  }

  renderAnalysisResults(analysis) {
    if (!analysis) {
      return "";
    }

    return `
      <div class="feedback-analysis">
        <h4><i class="bi bi-graph-up"></i> Analysis Results</h4>
        <div class="analysis-metrics">
          ${
            analysis.comprehension
              ? `
            <div class="metric-card">
              <div class="metric-label">Comprehension</div>
              <div class="metric-value">${analysis.comprehension}%</div>
              <div class="metric-bar">
                <div class="metric-fill" style="width: ${analysis.comprehension}%"></div>
              </div>
            </div>
          `
              : ""
          }
          
          ${
            analysis.retention
              ? `
            <div class="metric-card">
              <div class="metric-label">Retention</div>
              <div class="metric-value">${analysis.retention}%</div>
              <div class="metric-bar">
                <div class="metric-fill" style="width: ${analysis.retention}%"></div>
              </div>
            </div>
          `
              : ""
          }
          
          ${
            analysis.difficulty
              ? `
            <div class="metric-card">
              <div class="metric-label">Difficulty Level</div>
              <div class="metric-value">${analysis.difficulty}</div>
            </div>
          `
              : ""
          }
        </div>
      </div>
    `;
  }

  updateFeedback(newFeedback) {
    // Update feedback display
    logger.info("Updating feedback display");

    // Refresh feedback content
    this.render(newFeedback);

    // Animate changes
    this.container.classList.add("feedback-updating");
    setTimeout(() => {
      this.container.classList.remove("feedback-updating");
    }, 300);

    // Handle real-time updates
    this.feedbackData = newFeedback;
  }

  loadFeedbackData() {
    // Load any saved feedback from storage
    const saved = localStorage.getItem("lastFeedback");
    if (saved) {
      try {
        this.feedbackData = JSON.parse(saved);
        this.render(this.feedbackData);
      } catch (error) {
        logger.error("Error loading saved feedback:", error);
      }
    }
  }

  expandFeedback(feedbackId) {
    const element = this.container.querySelector(
      `[data-feedback-id="${feedbackId}"]`
    );
    if (element) {
      element.classList.add("expanded");
    }
  }

  collapseFeedback(feedbackId) {
    const element = this.container.querySelector(
      `[data-feedback-id="${feedbackId}"]`
    );
    if (element) {
      element.classList.remove("expanded");
    }
  }

  async copyFeedback(feedbackId) {
    try {
      const text = this.feedbackData
        ? JSON.stringify(this.feedbackData, null, 2)
        : "";
      await navigator.clipboard.writeText(text);
      notificationManager.success("Feedback copied to clipboard");
    } catch (error) {
      logger.error("Error copying feedback:", error);
      notificationManager.error("Failed to copy feedback");
    }
  }

  exportFeedback(feedbackId) {
    if (!this.feedbackData) return;

    const dataStr = JSON.stringify(this.feedbackData, null, 2);
    const dataBlob = new Blob([dataStr], { type: "application/json" });
    const url = URL.createObjectURL(dataBlob);
    const link = document.createElement("a");
    link.href = url;
    link.download = `feedback-${Date.now()}.json`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);

    notificationManager.success("Feedback exported successfully");
  }
}

export default Feedback;
