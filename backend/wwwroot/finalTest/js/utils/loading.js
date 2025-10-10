// Loading States and Skeleton Screens Utility

import { logger } from "./logger.js";

/**
 * Create a skeleton screen for a card component
 * @param {Object} options - Skeleton options
 * @returns {string} HTML string for skeleton card
 */
export function createSkeletonCard(options = {}) {
  const {
    title = true,
    content = true,
    actions = true,
    image = false,
    lines = 3,
  } = options;

  return `
    <div class="card skeleton-card">
      ${image ? '<div class="skeleton-image"></div>' : ""}
      <div class="card-body">
        ${title ? '<div class="skeleton-title"></div>' : ""}
        ${
          content
            ? Array.from(
                { length: lines },
                () => '<div class="skeleton-line"></div>'
              ).join("")
            : ""
        }
        ${actions ? '<div class="skeleton-actions"></div>' : ""}
      </div>
    </div>
  `;
}

/**
 * Create a skeleton screen for a table
 * @param {Object} options - Skeleton options
 * @returns {string} HTML string for skeleton table
 */
export function createSkeletonTable(options = {}) {
  const { rows = 5, columns = 4, headers = true } = options;

  return `
    <div class="table-responsive">
      <table class="table table-hover">
        ${
          headers
            ? `
          <thead>
            <tr>
              ${Array.from(
                { length: columns },
                () => '<th><div class="skeleton-line"></div></th>'
              ).join("")}
            </tr>
          </thead>
        `
            : ""
        }
        <tbody>
          ${Array.from(
            { length: rows },
            () => `
            <tr>
              ${Array.from(
                { length: columns },
                () => '<td><div class="skeleton-line"></div></td>'
              ).join("")}
            </tr>
          `
          ).join("")}
        </tbody>
      </table>
    </div>
  `;
}

/**
 * Create a skeleton screen for a list
 * @param {Object} options - Skeleton options
 * @returns {string} HTML string for skeleton list
 */
export function createSkeletonList(options = {}) {
  const { items = 5, avatar = true, title = true, subtitle = true } = options;

  return `
    <div class="list-group">
      ${Array.from(
        { length: items },
        () => `
        <div class="list-group-item skeleton-list-item">
          ${avatar ? '<div class="skeleton-avatar"></div>' : ""}
          <div class="skeleton-content">
            ${title ? '<div class="skeleton-title"></div>' : ""}
            ${subtitle ? '<div class="skeleton-line"></div>' : ""}
          </div>
        </div>
      `
      ).join("")}
    </div>
  `;
}

/**
 * Create a skeleton screen for a chart
 * @param {Object} options - Skeleton options
 * @returns {string} HTML string for skeleton chart
 */
export function createSkeletonChart(options = {}) {
  const { type = "line", height = "300px" } = options;

  return `
    <div class="skeleton-chart" style="height: ${height}">
      <div class="skeleton-chart-content">
        <div class="skeleton-chart-bars"></div>
        <div class="skeleton-chart-lines"></div>
        <div class="skeleton-chart-dots"></div>
      </div>
    </div>
  `;
}

/**
 * Create a skeleton screen for a form
 * @param {Object} options - Skeleton options
 * @returns {string} HTML string for skeleton form
 */
export function createSkeletonForm(options = {}) {
  const { fields = 4, buttons = true } = options;

  return `
    <form class="skeleton-form">
      ${Array.from(
        { length: fields },
        () => `
        <div class="mb-3">
          <div class="skeleton-label"></div>
          <div class="skeleton-input"></div>
        </div>
      `
      ).join("")}
      ${
        buttons
          ? `
        <div class="skeleton-buttons">
          <div class="skeleton-button"></div>
          <div class="skeleton-button"></div>
        </div>
      `
          : ""
      }
    </form>
  `;
}

/**
 * Show loading state for a specific element
 * @param {HTMLElement|string} element - Element or selector
 * @param {Object} options - Loading options
 */
export function showLoading(element, options = {}) {
  const el =
    typeof element === "string" ? document.querySelector(element) : element;
  if (!el) return;

  const { type = "spinner", message = "Loading...", overlay = true } = options;

  // Store original content
  el.dataset.originalContent = el.innerHTML;

  // Create loading content
  let loadingContent = "";

  if (type === "spinner") {
    loadingContent = `
      <div class="loading-state ${overlay ? "loading-overlay" : ""}">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
        <div class="loading-message">${message}</div>
      </div>
    `;
  } else if (type === "skeleton") {
    loadingContent = `
      <div class="skeleton-container">
        ${createSkeletonCard({ lines: 3 })}
      </div>
    `;
  }

  el.innerHTML = loadingContent;
  el.classList.add("loading");
}

/**
 * Hide loading state for a specific element
 * @param {HTMLElement|string} element - Element or selector
 */
export function hideLoading(element) {
  const el =
    typeof element === "string" ? document.querySelector(element) : element;
  if (!el) return;

  // Restore original content
  if (el.dataset.originalContent) {
    el.innerHTML = el.dataset.originalContent;
    delete el.dataset.originalContent;
  }

  el.classList.remove("loading");
}

/**
 * Create a loading button
 * @param {HTMLElement} button - Button element
 * @param {string} text - Loading text
 */
export function setButtonLoading(button, text = "Loading...") {
  if (!button) return;

  // Store original content
  button.dataset.originalContent = button.innerHTML;
  button.dataset.originalDisabled = button.disabled;

  // Set loading state
  button.disabled = true;
  button.innerHTML = `
    <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
    ${text}
  `;
}

/**
 * Reset a loading button
 * @param {HTMLElement} button - Button element
 */
export function resetButtonLoading(button) {
  if (!button) return;

  // Restore original content
  if (button.dataset.originalContent) {
    button.innerHTML = button.dataset.originalContent;
    delete button.dataset.originalContent;
  }

  if (button.dataset.originalDisabled !== undefined) {
    button.disabled = button.dataset.originalDisabled === "true";
    delete button.dataset.originalDisabled;
  }
}

/**
 * Create a progress bar for file uploads
 * @param {Object} options - Progress bar options
 * @returns {string} HTML string for progress bar
 */
export function createProgressBar(options = {}) {
  const {
    value = 0,
    max = 100,
    label = "Uploading...",
    showPercentage = true,
  } = options;

  return `
    <div class="progress-container">
      <div class="progress-label">${label}</div>
      <div class="progress" role="progressbar" aria-valuenow="${value}" aria-valuemin="0" aria-valuemax="${max}">
        <div class="progress-bar" style="width: ${value}%"></div>
      </div>
      ${
        showPercentage ? `<div class="progress-percentage">${value}%</div>` : ""
      }
    </div>
  `;
}

/**
 * Update progress bar
 * @param {HTMLElement|string} element - Progress bar element
 * @param {number} value - New progress value
 * @param {string} label - New label
 */
export function updateProgressBar(element, value, label = null) {
  const el =
    typeof element === "string" ? document.querySelector(element) : element;
  if (!el) return;

  const progressBar = el.querySelector(".progress-bar");
  const progressLabel = el.querySelector(".progress-label");
  const progressPercentage = el.querySelector(".progress-percentage");

  if (progressBar) {
    progressBar.style.width = `${value}%`;
    progressBar.setAttribute("aria-valuenow", value);
  }

  if (progressLabel && label) {
    progressLabel.textContent = label;
  }

  if (progressPercentage) {
    progressPercentage.textContent = `${value}%`;
  }
}

/**
 * Create a loading overlay for the entire page
 * @param {string} message - Loading message
 */
export function showPageLoading(message = "Loading...") {
  const overlay = document.createElement("div");
  overlay.id = "pageLoadingOverlay";
  overlay.className = "page-loading-overlay";
  overlay.innerHTML = `
    <div class="page-loading-content">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
      <div class="page-loading-message">${message}</div>
    </div>
  `;

  document.body.appendChild(overlay);
}

/**
 * Hide the page loading overlay
 */
export function hidePageLoading() {
  const overlay = document.getElementById("pageLoadingOverlay");
  if (overlay) {
    overlay.remove();
  }
}

/**
 * Create a loading state for async operations
 * @param {Function} asyncFunction - Async function to execute
 * @param {HTMLElement|string} element - Element to show loading on
 * @param {Object} options - Loading options
 * @returns {Promise} Promise that resolves with function result
 */
export async function withLoading(asyncFunction, element, options = {}) {
  const {
    type = "spinner",
    message = "Loading...",
    errorMessage = "An error occurred",
  } = options;

  try {
    showLoading(element, { type, message });
    const result = await asyncFunction();
    return result;
  } catch (error) {
    logger.error("Error in withLoading:", error);
    throw error;
  } finally {
    hideLoading(element);
  }
}

/**
 * Initialize loading states CSS
 */
export function initializeLoadingStyles() {
  const style = document.createElement("style");
  style.textContent = `
    .skeleton-card {
      animation: skeleton-pulse 1.5s ease-in-out infinite;
    }
    
    .skeleton-line {
      height: 1rem;
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite;
      border-radius: 4px;
      margin-bottom: 0.5rem;
    }
    
    .skeleton-title {
      height: 1.5rem;
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite;
      border-radius: 4px;
      margin-bottom: 1rem;
      width: 60%;
    }
    
    .skeleton-avatar {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite;
      margin-right: 1rem;
    }
    
    .skeleton-image {
      height: 200px;
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite;
      border-radius: 4px 4px 0 0;
    }
    
    .skeleton-actions {
      height: 2rem;
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite;
      border-radius: 4px;
      width: 100px;
    }
    
    .skeleton-chart {
      background: #f8f9fa;
      border-radius: 8px;
      position: relative;
      overflow: hidden;
    }
    
    .skeleton-chart-content {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      display: flex;
      align-items: end;
      justify-content: space-around;
      padding: 1rem;
    }
    
    .skeleton-chart-bars {
      width: 20px;
      height: 80%;
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite;
      border-radius: 4px 4px 0 0;
    }
    
    .skeleton-chart-lines {
      width: 20px;
      height: 60%;
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite;
      border-radius: 4px 4px 0 0;
    }
    
    .skeleton-chart-dots {
      width: 20px;
      height: 40%;
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite;
      border-radius: 4px 4px 0 0;
    }
    
    .skeleton-label {
      height: 1rem;
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite;
      border-radius: 4px;
      margin-bottom: 0.5rem;
      width: 30%;
    }
    
    .skeleton-input {
      height: 2.5rem;
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite;
      border-radius: 4px;
    }
    
    .skeleton-button {
      height: 2.5rem;
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite;
      border-radius: 4px;
      width: 100px;
      margin-right: 0.5rem;
    }
    
    .skeleton-buttons {
      display: flex;
      margin-top: 1rem;
    }
    
    .skeleton-list-item {
      display: flex;
      align-items: center;
      padding: 1rem;
      border-bottom: 1px solid #e9ecef;
    }
    
    .skeleton-content {
      flex: 1;
    }
    
    .loading-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 2rem;
      text-align: center;
    }
    
    .loading-overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(255, 255, 255, 0.9);
      z-index: 10;
    }
    
    .loading-message {
      margin-top: 1rem;
      color: #6c757d;
    }
    
    .page-loading-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(255, 255, 255, 0.95);
      z-index: 9999;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    
    .page-loading-content {
      text-align: center;
    }
    
    .page-loading-message {
      margin-top: 1rem;
      font-size: 1.1rem;
      color: #6c757d;
    }
    
    .progress-container {
      margin: 1rem 0;
    }
    
    .progress-label {
      margin-bottom: 0.5rem;
      font-weight: 500;
    }
    
    .progress-percentage {
      margin-top: 0.5rem;
      text-align: right;
      font-size: 0.9rem;
      color: #6c757d;
    }
    
    @keyframes skeleton-loading {
      0% {
        background-position: -200% 0;
      }
      100% {
        background-position: 200% 0;
      }
    }
    
    @keyframes skeleton-pulse {
      0%, 100% {
        opacity: 1;
      }
      50% {
        opacity: 0.5;
      }
    }
  `;

  document.head.appendChild(style);
}
