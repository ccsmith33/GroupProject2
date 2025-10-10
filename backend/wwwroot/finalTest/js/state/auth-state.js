// Authentication State Management

import { store } from "./store.js";
import { CONFIG } from "../config.js";
import { authAPI } from "../api/auth.js";
import { logger } from "../utils/logger.js";
import { storageService } from "../utils/storageService.js";

class AuthState {
  constructor() {
    this.token = null; // Will be loaded asynchronously
    this.isInitialized = false;
  }

  // Initialize authentication state
  async initialize() {
    if (this.isInitialized) return;

    try {
      // Load token from storage
      this.token = await storageService.getItem(CONFIG.STORAGE_KEYS.TOKEN);

      if (this.token) {
        // Verify token is still valid
        const user = await this.getCurrentUser();
        if (user) {
          store.actions.setUser(user);
          store.actions.setGuest(false);
        } else {
          // Token is invalid, clear it
          await this.clearAuth();
        }
      } else {
        // No token, check for guest data
        await this.loadGuestData();
      }
    } catch (error) {
      logger.error("Error initializing auth state:", error);
      await this.clearAuth();
    } finally {
      this.isInitialized = true;
    }
  }

  // Check if user is authenticated
  isAuthenticated() {
    return !!this.token && store.getState().isAuthenticated;
  }

  // Check if user is guest
  isGuest() {
    return store.getState().isGuest;
  }

  // Get current user
  async getCurrentUser() {
    if (!this.token) return null;

    try {
      const response = await fetch(
        `${CONFIG.API_BASE}${CONFIG.ENDPOINTS.AUTH.ME}`,
        {
          headers: {
            Authorization: `Bearer ${this.token}`,
            "Content-Type": "application/json",
          },
        }
      );

      if (response.ok) {
        const data = await response.json();
        return data.data || data;
      } else {
        throw new Error("Failed to get current user");
      }
    } catch (error) {
      logger.error("Error getting current user:", error);
      return null;
    }
  }

  // Login user
  async login(email, password) {
    try {
      store.actions.setLoading(true);
      store.actions.clearError();

      const response = await authAPI.login(email, password);

      if (response.success || response.token || response.accessToken) {
        // Support multiple response formats: accessToken, token, data.accessToken, data.token
        const token =
          response.accessToken ||
          response.token ||
          response.data?.accessToken ||
          response.data?.token;
        const user = response.user || response.data?.user || response.data;

        if (token && user) {
          await this.setToken(token);
          store.actions.setUser(user);
          store.actions.setGuest(false);

          // Save guest data if exists
          await this.saveGuestData();

          store.actions.addNotification({
            type: "success",
            title: "Welcome back!",
            message: `Hello ${user.name || user.email}!`,
          });

          return { success: true, user };
        }
      }

      throw new Error(response.message || "Login failed");
    } catch (error) {
      const errorMessage = error.message || "Login failed. Please try again.";
      store.actions.setError(errorMessage);
      store.actions.addNotification({
        type: "error",
        title: "Login Failed",
        message: errorMessage,
      });
      return { success: false, error: errorMessage };
    } finally {
      store.actions.setLoading(false);
    }
  }

  // Register user
  async register(email, password, name) {
    try {
      store.actions.setLoading(true);
      store.actions.clearError();

      const response = await authAPI.register(email, password, name);

      if (response.success || response.token || response.accessToken) {
        // Support multiple response formats: accessToken, token, data.accessToken, data.token
        const token =
          response.accessToken ||
          response.token ||
          response.data?.accessToken ||
          response.data?.token;
        const user = response.user || response.data?.user || response.data;

        if (token && user) {
          await this.setToken(token);
          store.actions.setUser(user);
          store.actions.setGuest(false);

          // Save guest data if exists
          await this.saveGuestData();

          store.actions.addNotification({
            type: "success",
            title: "Account Created!",
            message: `Welcome to Study AI, ${user.name}!`,
          });

          return { success: true, user };
        }
      }

      throw new Error(response.message || "Registration failed");
    } catch (error) {
      const errorMessage =
        error.message || "Registration failed. Please try again.";
      store.actions.setError(errorMessage);
      store.actions.addNotification({
        type: "error",
        title: "Registration Failed",
        message: errorMessage,
      });
      return { success: false, error: errorMessage };
    } finally {
      store.actions.setLoading(false);
    }
  }

  // Logout user
  async logout() {
    try {
      if (this.token) {
        await authAPI.logout();
      }
    } catch (error) {
      logger.error("Error during logout:", error);
    } finally {
      await this.clearAuth();
      store.actions.addNotification({
        type: "info",
        title: "Logged Out",
        message: "You have been logged out successfully.",
      });
    }
  }

  // Set authentication token
  async setToken(token) {
    this.token = token;
    await storageService.setItem(CONFIG.STORAGE_KEYS.TOKEN, token);
  }

  // Clear authentication data
  async clearAuth() {
    this.token = null;
    await storageService.removeItem(CONFIG.STORAGE_KEYS.TOKEN);
    await storageService.removeItem(CONFIG.STORAGE_KEYS.USER);
    await storageService.removeItem(CONFIG.STORAGE_KEYS.AUTHENTICATED);

    store.actions.setUser(null);
    store.actions.setGuest(true);
  }

  // Get authorization header
  getAuthHeader() {
    return this.token ? { Authorization: `Bearer ${this.token}` } : {};
  }

  // Get model for current user (guest vs authenticated)
  getModel() {
    return this.isGuest() ? CONFIG.GUEST_MODEL : CONFIG.AUTH_MODEL;
  }

  // Check if action requires authentication
  requiresAuth(action) {
    const guestActions = [
      "uploadFile",
      "generateQuiz",
      "generateStudyGuide",
      "chat",
    ];
    return guestActions.includes(action);
  }

  // Prompt guest to sign up
  promptGuestSignup(action) {
    if (this.isGuest() && this.requiresAuth(action)) {
      store.actions.openModal("guestPrompt");
      return true;
    }
    return false;
  }

  // Save guest data before authentication
  async saveGuestData() {
    if (this.isGuest()) {
      const guestData = {
        files: store.getState().files,
        groups: store.getState().groups,
        studyGuides: store.getState().studyGuides,
        quizzes: store.getState().quizzes,
        chatHistory: store.getState().chatHistory,
        wizardData: store.getState().wizardData,
        timestamp: Date.now(),
      };

      await storageService.setItem(
        CONFIG.STORAGE_KEYS.GUEST_DATA,
        JSON.stringify(guestData)
      );
    }
  }

  // Load guest data
  async loadGuestData() {
    try {
      const savedGuestData = await storageService.getItem(
        CONFIG.STORAGE_KEYS.GUEST_DATA
      );
      if (savedGuestData) {
        const guestData = JSON.parse(savedGuestData);

        // Check if data is not too old (24 hours)
        const maxAge = CONFIG.GUEST_LIMITS.SESSION_DURATION;
        if (Date.now() - guestData.timestamp < maxAge) {
          // Restore guest data
          store.setState({
            files: guestData.files || [],
            groups: guestData.groups || [],
            studyGuides: guestData.studyGuides || [],
            quizzes: guestData.quizzes || [],
            chatHistory: guestData.chatHistory || [],
            wizardData: guestData.wizardData || {
              uploadedFiles: [],
              selectedSubject: "",
              selectedLevel: "",
              generatedQuiz: null,
              generatedGuide: null,
            },
          });
        } else {
          // Data is too old, clear it
          await storageService.removeItem(CONFIG.STORAGE_KEYS.GUEST_DATA);
        }
      }
    } catch (error) {
      logger.error("Error loading guest data:", error);
      await storageService.removeItem(CONFIG.STORAGE_KEYS.GUEST_DATA);
    }
  }

  // Clear guest data
  async clearGuestData() {
    await storageService.removeItem(CONFIG.STORAGE_KEYS.GUEST_DATA);
  }

  // Migrate guest data to authenticated user
  async migrateGuestData() {
    try {
      const guestData = JSON.parse(
        (await storageService.getItem(CONFIG.STORAGE_KEYS.GUEST_DATA)) || "{}"
      );

      if (guestData.files && guestData.files.length > 0) {
        // Files are already in the database, just update the UI
        store.actions.setFiles(guestData.files);
      }

      if (guestData.groups && guestData.groups.length > 0) {
        store.actions.setGroups(guestData.groups);
      }

      if (guestData.studyGuides && guestData.studyGuides.length > 0) {
        store.actions.setStudyGuides(guestData.studyGuides);
      }

      if (guestData.quizzes && guestData.quizzes.length > 0) {
        store.actions.setQuizzes(guestData.quizzes);
      }

      if (guestData.chatHistory && guestData.chatHistory.length > 0) {
        store.actions.setChatHistory(guestData.chatHistory);
      }

      // Clear guest data after migration
      this.clearGuestData();
    } catch (error) {
      logger.error("Error migrating guest data:", error);
    }
  }

  // Check guest limits
  checkGuestLimits(action) {
    if (!this.isGuest()) return true;

    const state = store.getState();

    switch (action) {
      case "uploadFile":
        return state.files.length < CONFIG.GUEST_LIMITS.MAX_FILES;
      case "generateQuiz":
        return state.quizzes.length < CONFIG.GUEST_LIMITS.MAX_QUIZZES;
      case "chat":
        return state.chatHistory.length < CONFIG.GUEST_LIMITS.MAX_CHAT_MESSAGES;
      default:
        return true;
    }
  }

  // Get guest limit message
  getGuestLimitMessage(action) {
    const state = store.getState();

    switch (action) {
      case "uploadFile":
        return `Guest users can upload up to ${CONFIG.GUEST_LIMITS.MAX_FILES} files. Sign up to upload unlimited files!`;
      case "generateQuiz":
        return `Guest users can generate up to ${CONFIG.GUEST_LIMITS.MAX_QUIZZES} quizzes. Sign up for unlimited quizzes!`;
      case "chat":
        return `Guest users can send up to ${CONFIG.GUEST_LIMITS.MAX_CHAT_MESSAGES} messages. Sign up for unlimited chat!`;
      default:
        return "Sign up to unlock all features!";
    }
  }
}

// Create global auth state instance
const authState = new AuthState();

export { authState };
