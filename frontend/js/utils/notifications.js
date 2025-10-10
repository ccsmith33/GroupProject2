// Notification System

import { store } from "../state/store.js";

class NotificationManager {
  constructor() {
    this.container = null;
    this.init();
  }

  init() {
    // Create notification container if it doesn't exist
    this.container = document.getElementById("toastContainer");
    if (!this.container) {
      this.container = document.createElement("div");
      this.container.className =
        "toast-container position-fixed top-0 end-0 p-3";
      this.container.id = "toastContainer";
      this.container.style.zIndex = "9999";
      document.body.appendChild(this.container);
    }

    // Subscribe to store notifications
    store.subscribe((state) => {
      if (state.notifications.length > 0) {
        this.renderNotifications(state.notifications);
      }
    });
  }

  // Show notification
  show(message, type = "info", options = {}) {
    const notification = {
      id: Date.now() + Math.random(),
      message,
      type,
      title: options.title || this.getDefaultTitle(type),
      duration: options.duration || 5000,
      persistent: options.persistent || false,
      actions: options.actions || [],
    };

    store.actions.addNotification(notification);
    return notification.id;
  }

  // Show success notification
  success(message, options = {}) {
    return this.show(message, "success", options);
  }

  // Show error notification
  error(message, options = {}) {
    return this.show(message, "error", options);
  }

  // Show warning notification
  warning(message, options = {}) {
    return this.show(message, "warning", options);
  }

  // Show info notification
  info(message, options = {}) {
    return this.show(message, "info", options);
  }

  // Remove notification
  remove(id) {
    store.actions.removeNotification(id);
  }

  // Clear all notifications
  clear() {
    store.actions.clearNotifications();
  }

  // Render notifications
  renderNotifications(notifications) {
    // Clear existing notifications
    this.container.innerHTML = "";

    // Render each notification
    notifications.forEach((notification) => {
      const toast = this.createToast(notification);
      this.container.appendChild(toast);

      // Auto-remove after duration
      if (!notification.persistent && notification.duration > 0) {
        setTimeout(() => {
          this.remove(notification.id);
        }, notification.duration);
      }
    });
  }

  // Create toast element
  createToast(notification) {
    const toast = document.createElement("div");
    toast.className = `toast notification notification-${notification.type}`;
    toast.setAttribute("role", "alert");
    toast.setAttribute("aria-live", "assertive");
    toast.setAttribute("aria-atomic", "true");
    toast.style.minWidth = "300px";

    toast.innerHTML = `
            <div class="toast-header">
                <i class="bi ${this.getIcon(
                  notification.type
                )} notification-icon"></i>
                <strong class="notification-title me-auto">${
                  notification.title
                }</strong>
                <button type="button" class="notification-close" data-bs-dismiss="toast" aria-label="Close">
                    <i class="bi bi-x"></i>
                </button>
            </div>
            <div class="toast-body">
                <div class="notification-content">
                    <div class="notification-message">${
                      notification.message
                    }</div>
                    ${
                      notification.actions.length > 0
                        ? `
                        <div class="notification-actions mt-2">
                            ${notification.actions
                              .map(
                                (action) => `
                                <button class="btn btn-sm ${
                                  action.class || "btn-outline-primary"
                                }" 
                                        onclick="${action.onclick}">
                                    ${action.text}
                                </button>
                            `
                              )
                              .join("")}
                        </div>
                    `
                        : ""
                    }
                </div>
            </div>
        `;

    // Add close event listener
    const closeBtn = toast.querySelector(".notification-close");
    closeBtn.addEventListener("click", () => {
      this.remove(notification.id);
    });

    // Check if Bootstrap is available
    if (typeof bootstrap !== "undefined") {
      // Initialize Bootstrap toast
      const bsToast = new bootstrap.Toast(toast, {
        autohide: !notification.persistent,
        delay: notification.duration,
      });

      // Show toast
      bsToast.show();
    } else {
      // Fallback if Bootstrap not available
      toast.classList.add("show");
    }

    return toast;
  }

  // Get default title based on type
  getDefaultTitle(type) {
    const titles = {
      success: "Success",
      error: "Error",
      warning: "Warning",
      info: "Information",
    };
    return titles[type] || "Notification";
  }

  // Get icon based on type
  getIcon(type) {
    const icons = {
      success: "bi-check-circle-fill",
      error: "bi-exclamation-triangle-fill",
      warning: "bi-exclamation-circle-fill",
      info: "bi-info-circle-fill",
    };
    return icons[type] || "bi-info-circle-fill";
  }

  // Show guest prompt notification
  showGuestPrompt(action) {
    const notification = {
      id: "guest-prompt",
      type: "warning",
      title: "Save Your Progress!",
      message: `You're in guest mode. ${action} to save your progress and access advanced features.`,
      persistent: true,
      actions: [
        {
          text: "Create Account",
          class: "btn-primary",
          onclick: "notificationManager.showCreateAccountModal()",
        },
        {
          text: "Continue as Guest",
          class: "btn-outline-secondary",
          onclick: 'notificationManager.remove("guest-prompt")',
        },
      ],
    };

    // Remove existing guest prompt
    this.remove("guest-prompt");

    // Add new guest prompt
    store.actions.addNotification(notification);
  }

  // Show create account modal
  showCreateAccountModal() {
    store.actions.openModal("guestPrompt");
    this.remove("guest-prompt");
  }

  // Show file upload progress
  showUploadProgress(fileName, progress) {
    const notificationId = `upload-${fileName}`;

    if (progress === 100) {
      // Upload complete
      setTimeout(() => {
        this.remove(notificationId);
      }, 2000);
    }

    const notification = {
      id: notificationId,
      type: "info",
      title: "Uploading File",
      message: `${fileName} - ${progress}%`,
      persistent: progress < 100,
      duration: progress < 100 ? false : 2000,
    };

    // Update or create notification
    const existing = store
      .getState()
      .notifications.find((n) => n.id === notificationId);
    if (existing) {
      store.actions.removeNotification(notificationId);
    }
    store.actions.addNotification(notification);
  }

  // Show processing status
  showProcessingStatus(fileName, status) {
    const notificationId = `processing-${fileName}`;

    const statusMessages = {
      uploading: "Uploading file...",
      processing: "Processing file...",
      analyzing: "Analyzing content...",
      completed: "Processing complete!",
      error: "Processing failed",
    };

    const statusTypes = {
      uploading: "info",
      processing: "info",
      analyzing: "info",
      completed: "success",
      error: "error",
    };

    const notification = {
      id: notificationId,
      type: statusTypes[status] || "info",
      title: "File Processing",
      message: `${fileName} - ${statusMessages[status] || status}`,
      persistent: status !== "completed" && status !== "error",
      duration: status === "completed" ? 3000 : false,
    };

    // Update or create notification
    const existing = store
      .getState()
      .notifications.find((n) => n.id === notificationId);
    if (existing) {
      store.actions.removeNotification(notificationId);
    }
    store.actions.addNotification(notification);
  }

  // Show quiz results
  showQuizResults(score, totalQuestions) {
    const percentage = Math.round((score / totalQuestions) * 100);
    const type =
      percentage >= 70 ? "success" : percentage >= 50 ? "warning" : "error";

    const message =
      percentage >= 90
        ? `Excellent! You scored ${score}/${totalQuestions} (${percentage}%)`
        : percentage >= 70
        ? `Good job! You scored ${score}/${totalQuestions} (${percentage}%)`
        : `Keep studying! You scored ${score}/${totalQuestions} (${percentage}%)`;

    this.show(message, type, {
      title: "Quiz Results",
      duration: 5000,
    });
  }

  // Show study guide generated
  showStudyGuideGenerated(title) {
    this.success(`Study guide "${title}" has been generated successfully!`, {
      title: "Study Guide Created",
      duration: 4000,
    });
  }

  // Show error with retry option
  showErrorWithRetry(message, retryCallback) {
    this.error(message, {
      title: "Error",
      persistent: true,
      actions: [
        {
          text: "Retry",
          class: "btn-primary",
          onclick: retryCallback,
        },
        {
          text: "Dismiss",
          class: "btn-outline-secondary",
          onclick: `notificationManager.remove("${Date.now()}")`,
        },
      ],
    });
  }

  // Show network error
  showNetworkError() {
    this.error(
      "Network connection lost. Please check your internet connection and try again.",
      {
        title: "Connection Error",
        persistent: true,
        actions: [
          {
            text: "Retry",
            class: "btn-primary",
            onclick: "location.reload()",
          },
        ],
      }
    );
  }

  // Show offline notification
  showOfflineNotification() {
    this.warning(
      "You are currently offline. Some features may not be available.",
      {
        title: "Offline Mode",
        persistent: true,
      }
    );
  }

  // Show online notification
  showOnlineNotification() {
    this.success("You are back online! All features are now available.", {
      title: "Back Online",
      duration: 3000,
    });
  }
}

// Create global notification manager
const notificationManager = new NotificationManager();

// Export for global access
window.notificationManager = notificationManager;

export { notificationManager };
