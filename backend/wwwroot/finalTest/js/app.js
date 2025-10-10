/**
 * Main Application Controller
 *
 * This is the central controller that manages the entire application lifecycle,
 * including initialization, navigation, state management, and component coordination.
 *
 * @author Student Study AI Team
 * @version 1.0.0
 * @since 2024-01-01
 */

import { store } from "./state/store.js";
import { authState } from "./state/auth-state.js";
import { wizard } from "./components/wizard.js";
import { fileManager } from "./components/fileManager.js";
import { studyTools } from "./components/studyTools.js";
import { dashboard } from "./components/dashboard.js";
import { progressAnalytics } from "./components/progressAnalytics.js";
import { profile } from "./components/profile.js";
import { notificationManager } from "./utils/notifications.js";
import { CONFIG } from "./config.js";
import { initializeAccessibility } from "./utils/accessibility.js";
import { initializeLoadingStyles, withLoading } from "./utils/loading.js";
import { logger } from "./utils/logger.js";
import { storageService } from "./utils/storageService.js";

/**
 * Main Application Class
 *
 * Manages the application state, navigation, and component lifecycle.
 * Provides a centralized interface for all application functionality.
 */
class App {
  /**
   * Creates a new App instance
   *
   * @constructor
   */
  constructor() {
    /** @type {boolean} Whether the application has been initialized */
    this.isInitialized = false;

    /** @type {string} The currently active view */
    this.currentView = "wizard";

    /** @type {Function[]} Array of cleanup functions to call on destroy */
    this.cleanupFunctions = [];
  }

  /**
   * Initialize the application
   *
   * Sets up all necessary components, event listeners, and determines the initial view.
   * This method should be called once when the application starts.
   *
   * @async
   * @returns {Promise<void>} Resolves when initialization is complete
   * @throws {Error} If initialization fails
   */
  async init() {
    if (this.isInitialized) return;

    try {
      // Check required dependencies
      if (typeof bootstrap === "undefined") {
        throw new Error("Bootstrap not loaded");
      }
      if (typeof Chart === "undefined") {
        logger.warn("Chart.js not loaded - analytics features will be limited");
      }

      // Show loading screen
      this.showLoadingScreen();

      // Initialize authentication
      await authState.initialize();

      // Setup event listeners
      this.setupEventListeners();

      // Setup navigation
      this.setupNavigation();

      // Determine initial view
      await this.determineInitialView();

      // Subscribe to state changes
      this.setupStateSubscription();

      // Setup offline/online detection
      this.setupConnectivityDetection();

      // Initialize accessibility features
      initializeAccessibility();

      // Initialize loading styles
      initializeLoadingStyles();

      this.isInitialized = true;
    } catch (error) {
      logger.error("App initialization error:", error);
      notificationManager.error(
        "Failed to initialize application. Please refresh the page."
      );
    } finally {
      this.hideLoadingScreen();
    }
  }

  // Show loading screen
  showLoadingScreen() {
    const loadingScreen = document.getElementById("loadingScreen");
    const app = document.getElementById("app");

    if (loadingScreen) {
      loadingScreen.style.display = "flex";
    }
    if (app) {
      app.style.display = "none";
    }
  }

  // Hide loading screen
  hideLoadingScreen() {
    const loadingScreen = document.getElementById("loadingScreen");
    const app = document.getElementById("app");

    if (loadingScreen) {
      loadingScreen.style.display = "none";
    }
    if (app) {
      app.style.display = "block";
    }
  }

  // Determine initial view
  async determineInitialView() {
    const state = store.getState();

    // Check if wizard was completed
    const wizardCompleted = await storageService.getItem(
      "study_ai_wizard_completed"
    );

    if (wizardCompleted && state.isAuthenticated) {
      // User is authenticated and wizard completed, show dashboard
      this.showView("dashboard");
    } else if (wizardCompleted && state.isGuest) {
      // Guest user with completed wizard, show dashboard
      this.showView("dashboard");
    } else {
      // Show wizard for new users
      this.showWizard();
    }
  }

  // Show wizard
  showWizard() {
    wizard.init();
    this.currentView = "wizard";
    this.updateNavigation();
  }

  // Show specific view
  showView(viewName) {
    // Hide all views
    const views = [
      "wizardContainer",
      "dashboardView",
      "filesView",
      "studyView",
      "progressView",
      "profileView",
    ];
    views.forEach((viewId) => {
      const view = document.getElementById(viewId);
      if (view) {
        view.style.display = "none";
      }
    });

    // Show selected view
    const viewElement =
      document.getElementById(`${viewName}View`) ||
      document.getElementById(`${viewName}Container`);
    if (viewElement) {
      viewElement.style.display = "block";
    }

    this.currentView = viewName;
    this.updateNavigation();

    // Load view-specific content
    this.loadViewContent(viewName);
  }

  /**
   * Load view-specific content
   *
   * Loads the appropriate content for the specified view by calling
   * the corresponding component initialization method.
   *
   * @async
   * @param {string} viewName - The name of the view to load content for
   * @returns {Promise<void>} Resolves when content is loaded
   * @throws {Error} If content cannot be loaded
   */
  async loadViewContent(viewName) {
    const viewElement =
      document.getElementById(`${viewName}View`) ||
      document.getElementById(`${viewName}Container`);

    if (!viewElement) return;

    try {
      // Show loading state
      viewElement.innerHTML = `
        <div class="skeleton-container">
          ${this.createViewSkeleton(viewName)}
        </div>
      `;

      // Load content based on view
      switch (viewName) {
        case "dashboard":
          await this.loadDashboard();
          break;
        case "files":
          await this.loadFiles();
          break;
        case "study":
          await this.loadStudyTools();
          break;
        case "progress":
          await this.loadProgress();
          break;
        case "profile":
          await this.loadProfile();
          break;
      }
    } catch (error) {
      logger.error(`Error loading ${viewName} view:`, error);
      notificationManager.error(
        `Failed to load ${viewName} view. Please try again.`
      );
    }
  }

  /**
   * Create skeleton content for a specific view
   *
   * @param {string} viewName - The name of the view
   * @returns {string} HTML string for skeleton content
   */
  createViewSkeleton(viewName) {
    switch (viewName) {
      case "dashboard":
        return `
          <div class="row mb-4">
            <div class="col-md-3 col-sm-6 mb-3">
              <div class="card skeleton-card">
                <div class="card-body">
                  <div class="skeleton-title"></div>
                  <div class="skeleton-line"></div>
                </div>
              </div>
            </div>
            <div class="col-md-3 col-sm-6 mb-3">
              <div class="card skeleton-card">
                <div class="card-body">
                  <div class="skeleton-title"></div>
                  <div class="skeleton-line"></div>
                </div>
              </div>
            </div>
            <div class="col-md-3 col-sm-6 mb-3">
              <div class="card skeleton-card">
                <div class="card-body">
                  <div class="skeleton-title"></div>
                  <div class="skeleton-line"></div>
                </div>
              </div>
            </div>
            <div class="col-md-3 col-sm-6 mb-3">
              <div class="card skeleton-card">
                <div class="card-body">
                  <div class="skeleton-title"></div>
                  <div class="skeleton-line"></div>
                </div>
              </div>
            </div>
          </div>
        `;
      case "files":
        return `
          <div class="row">
            <div class="col-12">
              <div class="card skeleton-card">
                <div class="card-body">
                  <div class="skeleton-title"></div>
                  <div class="skeleton-line"></div>
                  <div class="skeleton-line"></div>
                  <div class="skeleton-actions"></div>
                </div>
              </div>
            </div>
          </div>
        `;
      case "study":
        return `
          <div class="row">
            <div class="col-12">
              <div class="card skeleton-card">
                <div class="card-body">
                  <div class="skeleton-title"></div>
                  <div class="skeleton-line"></div>
                  <div class="skeleton-line"></div>
                  <div class="skeleton-line"></div>
                </div>
              </div>
            </div>
          </div>
        `;
      case "progress":
        return `
          <div class="row">
            <div class="col-12">
              <div class="card skeleton-card">
                <div class="card-body">
                  <div class="skeleton-title"></div>
                  <div class="skeleton-chart" style="height: 300px;"></div>
                </div>
              </div>
            </div>
          </div>
        `;
      case "profile":
        return `
          <div class="row">
            <div class="col-md-8">
              <div class="card skeleton-card">
                <div class="card-body">
                  <div class="skeleton-title"></div>
                  <div class="skeleton-line"></div>
                  <div class="skeleton-line"></div>
                  <div class="skeleton-line"></div>
                </div>
              </div>
            </div>
            <div class="col-md-4">
              <div class="card skeleton-card">
                <div class="card-body">
                  <div class="skeleton-title"></div>
                  <div class="skeleton-line"></div>
                  <div class="skeleton-line"></div>
                </div>
              </div>
            </div>
          </div>
        `;
      default:
        return `
          <div class="row">
            <div class="col-12">
              <div class="card skeleton-card">
                <div class="card-body">
                  <div class="skeleton-title"></div>
                  <div class="skeleton-line"></div>
                  <div class="skeleton-line"></div>
                </div>
              </div>
            </div>
          </div>
        `;
    }
  }

  // Load dashboard content
  /**
   * Load dashboard content
   *
   * Initializes and renders the dashboard component with user statistics
   * and quick actions.
   *
   * @async
   * @returns {Promise<void>} Resolves when dashboard is loaded
   * @throws {Error} If dashboard cannot be loaded
   */
  async loadDashboard() {
    const dashboardView = document.getElementById("dashboardView");
    if (!dashboardView) return;

    // Initialize dashboard and render content
    await dashboard.init();
    dashboard.renderDashboard();
  }

  /**
   * Load files content
   *
   * Initializes and renders the file manager component for uploading
   * and managing study files.
   *
   * @async
   * @returns {Promise<void>} Resolves when files view is loaded
   * @throws {Error} If files view cannot be loaded
   */
  async loadFiles() {
    const filesView = document.getElementById("filesView");
    if (!filesView) return;

    // Initialize file manager and render content
    await fileManager.init();
    fileManager.renderUploadView();
  }

  /**
   * Load study tools content
   *
   * Initializes and renders the study tools component for generating
   * study guides, quizzes, and AI chat.
   *
   * @async
   * @returns {Promise<void>} Resolves when study tools view is loaded
   * @throws {Error} If study tools view cannot be loaded
   */
  async loadStudyTools() {
    const studyView = document.getElementById("studyView");
    if (!studyView) return;

    // Initialize study tools and render content
    await studyTools.init();
    studyTools.renderGeneratorView();
  }

  /**
   * Load progress content
   *
   * Initializes and renders the progress analytics component for
   * tracking learning progress and achievements.
   *
   * @async
   * @returns {Promise<void>} Resolves when progress view is loaded
   * @throws {Error} If progress view cannot be loaded
   */
  async loadProgress() {
    const progressView = document.getElementById("progressView");
    if (!progressView) return;

    // Initialize progress analytics and render content
    await progressAnalytics.init();
    progressAnalytics.renderProgressView();
  }

  /**
   * Load profile content
   *
   * Initializes and renders the profile component for managing
   * user account and preferences.
   *
   * @async
   * @returns {Promise<void>} Resolves when profile view is loaded
   * @throws {Error} If profile view cannot be loaded
   */
  async loadProfile() {
    const profileView = document.getElementById("profileView");
    if (!profileView) return;

    // Initialize profile and render content
    await profile.init();
    profile.renderProfileView();
  }

  // Setup event listeners
  setupEventListeners() {
    // Store cleanup functions
    this.cleanupFunctions = [];

    // Navigation clicks
    this.handleNavClick = (e) => {
      const navLink = e.target.closest("[data-view]");
      if (navLink) {
        e.preventDefault();
        const view = navLink.dataset.view;
        this.showView(view);
      }
    };
    document.addEventListener("click", this.handleNavClick);
    this.cleanupFunctions.push(() => {
      document.removeEventListener("click", this.handleNavClick);
    });

    // Auth toggle
    this.handleAuthToggle = (e) => {
      e.preventDefault();
      this.toggleAuth();
    };
    const authToggle = document.getElementById("authToggle");
    if (authToggle) {
      authToggle.addEventListener("click", this.handleAuthToggle);
      this.cleanupFunctions.push(() => {
        authToggle.removeEventListener("click", this.handleAuthToggle);
      });
    }

    // Modal events
    this.handleModalClick = (e) => {
      if (
        e.target.classList.contains("modal-overlay") ||
        e.target.classList.contains("modal-close")
      ) {
        this.closeModal();
      }
    };
    document.addEventListener("click", this.handleModalClick);
    this.cleanupFunctions.push(() => {
      document.removeEventListener("click", this.handleModalClick);
    });

    // Keyboard shortcuts
    this.handleKeydown = (e) => {
      this.handleKeyboardShortcuts(e);
    };
    document.addEventListener("keydown", this.handleKeydown);
    this.cleanupFunctions.push(() => {
      document.removeEventListener("keydown", this.handleKeydown);
    });
  }

  // Cleanup event listeners
  cleanup() {
    if (this.cleanupFunctions) {
      this.cleanupFunctions.forEach((cleanup) => cleanup());
      this.cleanupFunctions = [];
    }
  }

  // Setup navigation
  setupNavigation() {
    // Update user display
    this.updateUserDisplay();

    // Update navigation state
    this.updateNavigation();
  }

  // Update user display
  updateUserDisplay() {
    const userDisplay = document.getElementById("userDisplay");
    const authToggleText = document.getElementById("authToggleText");

    if (userDisplay && authToggleText) {
      const state = store.getState();
      if (state.isAuthenticated && state.user) {
        userDisplay.textContent = state.user.name || state.user.email;
        authToggleText.textContent = "Logout";
      } else {
        userDisplay.textContent = "Guest";
        authToggleText.textContent = "Login";
      }
    }
  }

  // Update navigation
  updateNavigation() {
    // Update active nav link
    const navLinks = document.querySelectorAll(".nav-link[data-view]");
    if (navLinks && navLinks.length > 0) {
      navLinks.forEach((link) => {
        if (link) {
          link.classList.remove("active");
          if (link.dataset.view === this.currentView) {
            link.classList.add("active");
          }
        }
      });
    }

    // Update page title
    const titles = {
      wizard: "Welcome - Study AI",
      dashboard: "Dashboard - Study AI",
      files: "My Files - Study AI",
      study: "Study Tools - Study AI",
      progress: "Progress - Study AI",
      profile: "Profile - Study AI",
    };

    document.title = titles[this.currentView] || "Study AI";
  }

  // Setup state subscription
  setupStateSubscription() {
    store.subscribe((state, prevState) => {
      // Handle authentication changes
      if (state.isAuthenticated !== prevState.isAuthenticated) {
        this.updateUserDisplay();
      }

      // Handle view changes
      if (state.currentView !== prevState.currentView) {
        this.showView(state.currentView);
      }

      // Handle notifications
      if (state.notifications.length > prevState.notifications.length) {
        // New notification added, let notification manager handle it
      }

      // Handle errors
      if (state.error && state.error !== prevState.error) {
        notificationManager.error(state.error);
        store.actions.clearError();
      }
    });
  }

  // Setup connectivity detection
  setupConnectivityDetection() {
    const handleOnline = () => {
      notificationManager.showOnlineNotification();
    };

    const handleOffline = () => {
      notificationManager.showOfflineNotification();
    };

    window.addEventListener("online", handleOnline);
    window.addEventListener("offline", handleOffline);

    // Store cleanup functions
    this.cleanupFunctions.push(() => {
      window.removeEventListener("online", handleOnline);
      window.removeEventListener("offline", handleOffline);
    });
  }

  // Toggle authentication
  toggleAuth() {
    const state = store.getState();

    if (state.isAuthenticated) {
      // Logout
      authState.logout();
    } else {
      // Show login modal
      this.showAuthModal();
    }
  }

  // Show auth modal
  showAuthModal() {
    store.actions.openModal("auth");
  }

  // Close modal
  closeModal() {
    store.actions.closeModal("auth");
    store.actions.closeModal("guestPrompt");
  }

  // Handle keyboard shortcuts
  handleKeyboardShortcuts(e) {
    // Ctrl/Cmd + K for search
    if ((e.ctrlKey || e.metaKey) && e.key === "k") {
      e.preventDefault();
      this.focusSearch();
    }

    // Escape to close modals
    if (e.key === "Escape") {
      this.closeModal();
    }

    // Number keys for navigation
    if (e.key >= "1" && e.key <= "5") {
      const views = ["dashboard", "files", "study", "progress", "profile"];
      const viewIndex = parseInt(e.key) - 1;
      if (views[viewIndex]) {
        this.showView(views[viewIndex]);
      }
    }
  }

  // Focus search (placeholder)
  focusSearch() {
    // This will be implemented when search functionality is added
    logger.debug("Focus search");
  }

  // Handle route changes
  handleRouteChange() {
    const hash = window.location.hash.substring(1);
    if (hash && hash !== this.currentView) {
      this.showView(hash);
    }
  }

  // Update URL hash
  updateURL(viewName) {
    if (viewName !== "wizard") {
      window.location.hash = viewName;
    } else {
      window.location.hash = "";
    }
  }

  // Cleanup
  cleanup() {
    this.cleanupFunctions.forEach((fn) => fn());
    this.cleanupFunctions = [];
  }
}

// Create global app instance
const app = new App();

// Initialize app when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  app.init();
});

// Handle route changes
window.addEventListener("hashchange", () => {
  app.handleRouteChange();
});

// Handle page unload
window.addEventListener("beforeunload", () => {
  app.cleanup();
});

// Export for global access
window.app = app;

export { app };
