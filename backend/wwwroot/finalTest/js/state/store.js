// Global State Management for Student Study AI

import { logger } from "../utils/logger.js";
import { storageService } from "../utils/storageService.js";

class Store {
  constructor() {
    this.state = {
      // User state
      user: null,
      isGuest: true,
      isAuthenticated: false,

      // Application state
      currentView: "wizard",
      isLoading: false,
      error: null,

      // Data state
      files: [],
      groups: [],
      analytics: null,
      studyGuides: [],
      quizzes: [],
      conversations: [],

      // UI state
      notifications: [],
      modals: {
        auth: false,
        guestPrompt: false,
        fileUpload: false,
      },

      // Wizard state
      wizardStep: 1,
      wizardData: {
        uploadedFiles: [],
        selectedSubject: "",
        selectedLevel: "",
        generatedQuiz: null,
        generatedGuide: null,
      },

      // Study session state
      currentQuiz: null,
      quizAnswers: {},
      quizScore: null,
      chatHistory: [],

      // Preferences
      preferences: {
        theme: "light",
        notifications: true,
        autoSave: true,
        quizLength: 5,
        knowledgeLevel: "highSchool",
      },
    };

    this.listeners = [];
    this.middleware = [];
  }

  // Get current state
  getState() {
    return { ...this.state };
  }

  // Update state
  setState(updates) {
    const prevState = { ...this.state };

    // Apply middleware
    const processedUpdates = this.middleware.reduce((acc, middleware) => {
      return middleware(acc, prevState);
    }, updates);

    // Update state
    this.state = { ...this.state, ...processedUpdates };

    // Notify listeners
    this.notify(prevState, this.state);
  }

  // Subscribe to state changes
  subscribe(listener) {
    this.listeners.push(listener);

    // Return unsubscribe function
    return () => {
      const index = this.listeners.indexOf(listener);
      if (index > -1) {
        this.listeners.splice(index, 1);
      }
    };
  }

  // Notify all listeners
  notify(prevState, currentState) {
    this.listeners.forEach((listener) => {
      try {
        listener(currentState, prevState);
      } catch (error) {
        logger.error("Error in state listener:", error);
      }
    });
  }

  // Add middleware
  addMiddleware(middleware) {
    this.middleware.push(middleware);
  }

  // Action creators
  actions = {
    // User actions
    setUser: (user) => {
      store.setState({
        user,
        isAuthenticated: !!user,
        isGuest: !user,
      });
    },

    setGuest: (isGuest) => {
      store.setState({ isGuest });
    },

    // View actions
    setCurrentView: (view) => {
      store.setState({ currentView: view });
    },

    // Loading actions
    setLoading: (isLoading) => {
      store.setState({ isLoading });
    },

    setError: (error) => {
      store.setState({ error });
    },

    clearError: () => {
      store.setState({ error: null });
    },

    // File actions
    setFiles: (files) => {
      store.setState({ files });
    },

    addFile: (file) => {
      const currentFiles = store.getState().files;
      store.setState({ files: [...currentFiles, file] });
    },

    updateFile: (fileId, updates) => {
      const currentFiles = store.getState().files;
      const updatedFiles = currentFiles.map((file) =>
        file.id === fileId ? { ...file, ...updates } : file
      );
      store.setState({ files: updatedFiles });
    },

    removeFile: (fileId) => {
      const currentFiles = store.getState().files;
      const filteredFiles = currentFiles.filter((file) => file.id !== fileId);
      store.setState({ files: filteredFiles });
    },

    // Group actions
    setGroups: (groups) => {
      store.setState({ groups });
    },

    addGroup: (group) => {
      const currentGroups = store.getState().groups;
      store.setState({ groups: [...currentGroups, group] });
    },

    updateGroup: (groupId, updates) => {
      const currentGroups = store.getState().groups;
      const updatedGroups = currentGroups.map((group) =>
        group.id === groupId ? { ...group, ...updates } : group
      );
      store.setState({ groups: updatedGroups });
    },

    removeGroup: (groupId) => {
      const currentGroups = store.getState().groups;
      const filteredGroups = currentGroups.filter(
        (group) => group.id !== groupId
      );
      store.setState({ groups: filteredGroups });
    },

    // Analytics actions
    setAnalytics: (analytics) => {
      store.setState({ analytics });
    },

    // Study content actions
    setStudyGuides: (guides) => {
      store.setState({ studyGuides: guides });
    },

    addStudyGuide: (guide) => {
      const currentGuides = store.getState().studyGuides;
      store.setState({ studyGuides: [...currentGuides, guide] });
    },

    setQuizzes: (quizzes) => {
      store.setState({ quizzes });
    },

    addQuiz: (quiz) => {
      const currentQuizzes = store.getState().quizzes;
      store.setState({ quizzes: [...currentQuizzes, quiz] });
    },

    // Quiz session actions
    setCurrentQuiz: (quiz) => {
      store.setState({
        currentQuiz: quiz,
        quizAnswers: {},
        quizScore: null,
      });
    },

    updateQuizAnswer: (questionId, answer) => {
      const currentAnswers = store.getState().quizAnswers;
      store.setState({
        quizAnswers: { ...currentAnswers, [questionId]: answer },
      });
    },

    setQuizScore: (score) => {
      store.setState({ quizScore: score });
    },

    clearQuiz: () => {
      store.setState({
        currentQuiz: null,
        quizAnswers: {},
        quizScore: null,
      });
    },

    // Chat actions
    setChatHistory: (history) => {
      store.setState({ chatHistory: history });
    },

    addChatMessage: (message) => {
      const currentHistory = store.getState().chatHistory;
      store.setState({ chatHistory: [...currentHistory, message] });
    },

    clearChat: () => {
      store.setState({ chatHistory: [] });
    },

    // Notification actions
    addNotification: (notification) => {
      const currentNotifications = store.getState().notifications;
      const id = Date.now() + Math.random();
      const newNotification = { id, ...notification };
      store.setState({
        notifications: [...currentNotifications, newNotification],
      });

      // Auto-remove after duration
      if (notification.duration !== false) {
        setTimeout(() => {
          store.actions.removeNotification(id);
        }, notification.duration || 5000);
      }
    },

    removeNotification: (id) => {
      const currentNotifications = store.getState().notifications;
      const filteredNotifications = currentNotifications.filter(
        (n) => n.id !== id
      );
      store.setState({ notifications: filteredNotifications });
    },

    clearNotifications: () => {
      store.setState({ notifications: [] });
    },

    // Modal actions
    openModal: (modalName) => {
      const currentModals = store.getState().modals;
      store.setState({
        modals: { ...currentModals, [modalName]: true },
      });
    },

    closeModal: (modalName) => {
      const currentModals = store.getState().modals;
      store.setState({
        modals: { ...currentModals, [modalName]: false },
      });
    },

    // Wizard actions
    setWizardStep: (step) => {
      store.setState({ wizardStep: step });
    },

    nextWizardStep: () => {
      const currentStep = store.getState().wizardStep;
      store.setState({ wizardStep: Math.min(currentStep + 1, 5) });
    },

    prevWizardStep: () => {
      const currentStep = store.getState().wizardStep;
      store.setState({ wizardStep: Math.max(currentStep - 1, 1) });
    },

    updateWizardData: (data) => {
      const currentWizardData = store.getState().wizardData;
      store.setState({
        wizardData: { ...currentWizardData, ...data },
      });
    },

    completeWizard: () => {
      store.setState({
        currentView: "dashboard",
        wizardStep: 1,
        wizardData: {
          uploadedFiles: [],
          selectedSubject: "",
          selectedLevel: "",
          generatedQuiz: null,
          generatedGuide: null,
        },
      });

      // Mark wizard as completed
      storageService.setItem("study_ai_wizard_completed", "true");
    },

    // Preference actions
    updatePreferences: (preferences) => {
      const currentPreferences = store.getState().preferences;
      store.setState({
        preferences: { ...currentPreferences, ...preferences },
      });

      // Save to storage
      storageService.setItem(
        "study_ai_preferences",
        JSON.stringify({
          ...currentPreferences,
          ...preferences,
        })
      );
    },

    // Utility actions
    reset: () => {
      store.setState({
        user: null,
        isGuest: true,
        isAuthenticated: false,
        currentView: "wizard",
        isLoading: false,
        error: null,
        files: [],
        groups: [],
        analytics: null,
        studyGuides: [],
        quizzes: [],
        conversations: [],
        notifications: [],
        modals: {
          auth: false,
          guestPrompt: false,
          fileUpload: false,
        },
        wizardStep: 1,
        wizardData: {
          uploadedFiles: [],
          selectedSubject: "",
          selectedLevel: "",
          generatedQuiz: null,
          generatedGuide: null,
        },
        currentQuiz: null,
        quizAnswers: {},
        quizScore: null,
        chatHistory: [],
        preferences: {
          theme: "light",
          notifications: true,
          autoSave: true,
          quizLength: 5,
          knowledgeLevel: "highSchool",
        },
      });
    },
  };
}

// Create global store instance
const store = new Store();

// Add persistence middleware
store.addMiddleware((updates, prevState) => {
  // Save user data to localStorage when user changes
  if (updates.user !== undefined) {
    if (updates.user) {
      storageService.setItem("study_ai_user", JSON.stringify(updates.user));
    } else {
      storageService.removeItem("study_ai_user");
    }
  }

  // Save authentication state
  if (updates.isAuthenticated !== undefined) {
    if (updates.isAuthenticated) {
      storageService.setItem("study_ai_authenticated", "true");
    } else {
      storageService.removeItem("study_ai_authenticated");
    }
  }

  return updates;
});

// Initialize from storage
const initializeFromStorage = async () => {
  try {
    // Load user data
    const savedUser = await storageService.getItem("study_ai_user");
    if (savedUser) {
      const user = JSON.parse(savedUser);
      store.setState({
        user,
        isAuthenticated: true,
        isGuest: false,
      });
    }

    // Load preferences
    const savedPreferences = await storageService.getItem(
      "study_ai_preferences"
    );
    if (savedPreferences) {
      const preferences = JSON.parse(savedPreferences);
      store.setState({ preferences });
    }

    // Check if wizard was completed
    const wizardCompleted = await storageService.getItem(
      "study_ai_wizard_completed"
    );
    if (wizardCompleted) {
      store.setState({ currentView: "dashboard" });
    }
  } catch (error) {
    logger.error("Error loading from storage:", error);
  }
};

// Initialize on load
initializeFromStorage().catch((error) => {
  logger.error("Failed to initialize from storage:", error);
});

export { store };
