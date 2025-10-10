// Profile Component
import { authAPI } from "../api/auth.js";
import { userAPI } from "../api/user.js";
import { store } from "../state/store.js";
import { authState } from "../state/auth-state.js";
import { notificationManager } from "../utils/notifications.js";
import { CONFIG } from "../config.js";
import { logger } from "../utils/logger.js";
import { sanitizeText, sanitizeEmail } from "../utils/sanitize.js";

class Profile {
  constructor() {
    this.isInitialized = false;
    this.user = null;
    this.stats = null;
    this.isEditing = false;
    this.isChangingPassword = false;
  }

  // Initialize profile component
  async init() {
    if (this.isInitialized) return;

    try {
      this.setupEventListeners();
      await this.loadProfileData();
      this.isInitialized = true;
    } catch (error) {
      logger.error("Profile initialization error:", error);
      notificationManager.error("Failed to initialize profile");
    }
  }

  // Setup event listeners
  setupEventListeners() {
    // Profile actions
    document.addEventListener("click", (e) => {
      if (e.target.matches("[data-profile-action]")) {
        this.handleProfileAction(e.target);
      }
    });

    // Form submissions
    document.addEventListener("submit", (e) => {
      if (e.target.matches("#profileEditForm")) {
        e.preventDefault();
        this.handleProfileEdit(e.target);
      }
      if (e.target.matches("#passwordChangeForm")) {
        e.preventDefault();
        this.handlePasswordChange(e.target);
      }
    });
  }

  // Load profile data
  async loadProfileData() {
    try {
      const state = store.getState();

      if (state.isAuthenticated && state.user) {
        this.user = state.user;
        await this.loadUserStats();
      } else {
        // Guest user - show limited profile
        this.user = {
          id: null,
          name: "Guest User",
          email: "guest@example.com",
          isGuest: true,
        };
        this.stats = {
          totalFiles: 0,
          totalStudySessions: 0,
          totalStudyTimeMinutes: 0,
          studyGuidesCreated: 0,
          quizzesTaken: 0,
        };
      }
    } catch (error) {
      logger.error("Error loading profile data:", error);
      notificationManager.error("Failed to load profile data");
    }
  }

  // Load user statistics
  async loadUserStats() {
    try {
      const statsResult = await userAPI.getStats();
      if (statsResult.success) {
        this.stats = statsResult.data;
      }
    } catch (error) {
      logger.error("Error loading user stats:", error);
      // Don't show error for stats, just use default values
      this.stats = {
        totalFiles: 0,
        totalStudySessions: 0,
        totalStudyTimeMinutes: 0,
        studyGuidesCreated: 0,
        quizzesTaken: 0,
      };
    }
  }

  // Render profile view
  renderProfileView() {
    const profileView = document.getElementById("profileView");
    if (!profileView) return;

    const state = store.getState();
    const isGuest = state.isGuest;

    profileView.innerHTML = `
      <div class="row">
        <div class="col-12">
          <div class="page-header">
            <h1 class="page-title">Profile</h1>
            <p class="page-subtitle">Manage your account and preferences</p>
          </div>
        </div>
      </div>

      ${isGuest ? this.renderGuestNotice() : ""}

      <div class="row">
        <div class="col-lg-8">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">
                <i class="bi bi-person me-2"></i>Account Information
              </h5>
            </div>
            <div class="card-body">
              ${this.renderProfileInfo()}
            </div>
          </div>

          ${
            !isGuest
              ? `
          <div class="card mt-4">
            <div class="card-header">
              <h5 class="card-title mb-0">
                <i class="bi bi-key me-2"></i>Security
              </h5>
            </div>
            <div class="card-body">
              ${this.renderPasswordChange()}
            </div>
          </div>
          `
              : ""
          }
        </div>

        <div class="col-lg-4">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">
                <i class="bi bi-graph-up me-2"></i>Statistics
              </h5>
            </div>
            <div class="card-body">
              ${this.renderUserStats()}
            </div>
          </div>

          ${
            !isGuest
              ? `
          <div class="card mt-4">
            <div class="card-header">
              <h5 class="card-title mb-0">
                <i class="bi bi-gear me-2"></i>Quick Actions
              </h5>
            </div>
            <div class="card-body">
              ${this.renderQuickActions()}
            </div>
          </div>
          `
              : ""
          }
        </div>
      </div>
    `;
  }

  // Render guest notice
  renderGuestNotice() {
    return `
      <div class="alert alert-info d-flex align-items-center mb-4">
        <i class="bi bi-info-circle me-2"></i>
        <div class="flex-grow-1">
          <strong>Guest Mode:</strong> You're viewing limited profile information. 
          <a href="#" class="alert-link" data-profile-action="show-auth-modal">Create an account</a> for full profile management.
        </div>
      </div>
    `;
  }

  // Render profile information
  renderProfileInfo() {
    if (!this.user) return "";

    const safeName = sanitizeText(this.user.name || "Unknown");
    const safeEmail = sanitizeText(this.user.email || "No email");

    if (this.isEditing && !this.user.isGuest) {
      return `
        <form id="profileEditForm">
          <div class="row">
            <div class="col-md-6">
              <div class="mb-3">
                <label for="profileName" class="form-label">Full Name</label>
                <input type="text" class="form-control" id="profileName" name="name" 
                       value="${sanitizeText(this.user.name || "")}" required>
              </div>
            </div>
            <div class="col-md-6">
              <div class="mb-3">
                <label for="profileEmail" class="form-label">Email Address</label>
                <input type="email" class="form-control" id="profileEmail" name="email" 
                       value="${sanitizeText(this.user.email || "")}" required>
              </div>
            </div>
          </div>
          <div class="d-flex gap-2">
            <button type="submit" class="btn btn-primary">
              <i class="bi bi-check me-1"></i>Save Changes
            </button>
            <button type="button" class="btn btn-secondary" data-profile-action="cancel-edit">
              <i class="bi bi-x me-1"></i>Cancel
            </button>
          </div>
        </form>
      `;
    }

    return `
      <div class="profile-info">
        <div class="row">
          <div class="col-sm-3">
            <div class="profile-avatar">
              <i class="bi bi-person-circle display-1 text-muted"></i>
            </div>
          </div>
          <div class="col-sm-9">
            <div class="profile-details">
              <h4 class="profile-name">${safeName}</h4>
              <p class="profile-email text-muted">
                <i class="bi bi-envelope me-1"></i>${safeEmail}
              </p>
              <p class="profile-role">
                <span class="badge ${
                  this.user.isGuest ? "bg-secondary" : "bg-primary"
                }">
                  ${this.user.isGuest ? "Guest User" : "Registered User"}
                </span>
              </p>
              ${
                !this.user.isGuest
                  ? `
              <div class="profile-actions mt-3">
                <button class="btn btn-outline-primary" data-profile-action="edit-profile">
                  <i class="bi bi-pencil me-1"></i>Edit Profile
                </button>
              </div>
              `
                  : ""
              }
            </div>
          </div>
        </div>
      </div>
    `;
  }

  // Render password change form
  renderPasswordChange() {
    if (this.isChangingPassword) {
      return `
        <form id="passwordChangeForm">
          <div class="mb-3">
            <label for="currentPassword" class="form-label">Current Password</label>
            <input type="password" class="form-control" id="currentPassword" name="currentPassword" required>
          </div>
          <div class="mb-3">
            <label for="newPassword" class="form-label">New Password</label>
            <input type="password" class="form-control" id="newPassword" name="newPassword" required>
            <div class="form-text">Password must be at least 8 characters long</div>
          </div>
          <div class="mb-3">
            <label for="confirmPassword" class="form-label">Confirm New Password</label>
            <input type="password" class="form-control" id="confirmPassword" name="confirmPassword" required>
          </div>
          <div class="d-flex gap-2">
            <button type="submit" class="btn btn-primary">
              <i class="bi bi-check me-1"></i>Change Password
            </button>
            <button type="button" class="btn btn-secondary" data-profile-action="cancel-password-change">
              <i class="bi bi-x me-1"></i>Cancel
            </button>
          </div>
        </form>
      `;
    }

    return `
      <div class="password-section">
        <p class="text-muted mb-3">Keep your account secure by changing your password regularly.</p>
        <button class="btn btn-outline-primary" data-profile-action="change-password">
          <i class="bi bi-key me-1"></i>Change Password
        </button>
      </div>
    `;
  }

  // Render user statistics
  renderUserStats() {
    if (!this.stats) return "";

    const totalHours = Math.round(this.stats.totalStudyTimeMinutes / 60);
    const avgSessionTime =
      this.stats.totalStudySessions > 0
        ? Math.round(
            this.stats.totalStudyTimeMinutes / this.stats.totalStudySessions
          )
        : 0;

    return `
      <div class="stats-grid">
        <div class="stat-item">
          <div class="stat-value">${this.stats.totalFiles || 0}</div>
          <div class="stat-label">Files Uploaded</div>
        </div>
        <div class="stat-item">
          <div class="stat-value">${this.stats.totalStudySessions || 0}</div>
          <div class="stat-label">Study Sessions</div>
        </div>
        <div class="stat-item">
          <div class="stat-value">${totalHours}h</div>
          <div class="stat-label">Study Time</div>
        </div>
        <div class="stat-item">
          <div class="stat-value">${this.stats.studyGuidesCreated || 0}</div>
          <div class="stat-label">Study Guides</div>
        </div>
        <div class="stat-item">
          <div class="stat-value">${this.stats.quizzesTaken || 0}</div>
          <div class="stat-label">Quizzes Taken</div>
        </div>
        <div class="stat-item">
          <div class="stat-value">${avgSessionTime}m</div>
          <div class="stat-label">Avg Session</div>
        </div>
      </div>
    `;
  }

  // Render quick actions
  renderQuickActions() {
    return `
      <div class="quick-actions">
        <button class="btn btn-outline-primary w-100 mb-2" data-profile-action="upload-files">
          <i class="bi bi-cloud-upload me-2"></i>Upload Files
        </button>
        <button class="btn btn-outline-success w-100 mb-2" data-profile-action="create-guide">
          <i class="bi bi-book me-2"></i>Create Study Guide
        </button>
        <button class="btn btn-outline-info w-100 mb-2" data-profile-action="take-quiz">
          <i class="bi bi-question-circle me-2"></i>Take Quiz
        </button>
        <button class="btn btn-outline-warning w-100" data-profile-action="view-progress">
          <i class="bi bi-graph-up me-2"></i>View Progress
        </button>
      </div>
    `;
  }

  // Handle profile actions
  async handleProfileAction(button) {
    const action = button.dataset.profileAction;

    switch (action) {
      case "edit-profile":
        this.isEditing = true;
        this.renderProfileView();
        break;
      case "cancel-edit":
        this.isEditing = false;
        this.renderProfileView();
        break;
      case "change-password":
        this.isChangingPassword = true;
        this.renderProfileView();
        break;
      case "cancel-password-change":
        this.isChangingPassword = false;
        this.renderProfileView();
        break;
      case "show-auth-modal":
        window.app?.showAuthModal();
        break;
      case "upload-files":
        window.app?.showView("files");
        break;
      case "create-guide":
        window.app?.showView("study");
        break;
      case "take-quiz":
        window.app?.showView("study");
        break;
      case "view-progress":
        window.app?.showView("progress");
        break;
    }
  }

  // Handle profile edit form submission
  async handleProfileEdit(form) {
    try {
      const formData = new FormData(form);
      const name = formData.get("name");
      const email = formData.get("email");

      // Validate email
      const sanitizedEmail = sanitizeEmail(email);
      if (!sanitizedEmail) {
        notificationManager.error("Please enter a valid email address");
        return;
      }

      // Update profile
      const result = await userAPI.updateProfile({
        name: sanitizeText(name),
        email: sanitizedEmail,
      });

      if (result.success) {
        // Update local user data
        this.user = {
          ...this.user,
          name: sanitizeText(name),
          email: sanitizedEmail,
        };
        store.actions.setUser(this.user);

        this.isEditing = false;
        this.renderProfileView();
        notificationManager.success("Profile updated successfully");
      } else {
        notificationManager.error(result.message || "Failed to update profile");
      }
    } catch (error) {
      logger.error("Profile update error:", error);
      notificationManager.error("Failed to update profile");
    }
  }

  // Handle password change form submission
  async handlePasswordChange(form) {
    try {
      const formData = new FormData(form);
      const currentPassword = formData.get("currentPassword");
      const newPassword = formData.get("newPassword");
      const confirmPassword = formData.get("confirmPassword");

      // Validate passwords
      if (newPassword !== confirmPassword) {
        notificationManager.error("New passwords do not match");
        return;
      }

      if (newPassword.length < 8) {
        notificationManager.error(
          "Password must be at least 8 characters long"
        );
        return;
      }

      // Change password
      const result = await authAPI.changePassword(currentPassword, newPassword);

      if (result.success) {
        this.isChangingPassword = false;
        this.renderProfileView();
        notificationManager.success("Password changed successfully");
        form.reset();
      } else {
        notificationManager.error(
          result.message || "Failed to change password"
        );
      }
    } catch (error) {
      logger.error("Password change error:", error);
      notificationManager.error("Failed to change password");
    }
  }
}

// Create global profile instance
const profile = new Profile();

export { profile };
