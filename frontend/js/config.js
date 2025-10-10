// Configuration constants for the Student Study AI application

export const CONFIG = {
  // API Configuration
  API_BASE: "http://localhost:5000/api",

  // AI Model Configuration
  GUEST_MODEL: "gpt-5-nano",
  AUTH_MODEL: "gpt-5",

  // User Configuration
  GUEST_USER_ID: 1, // Guest user ID for database storage

  // File Upload Configuration
  MAX_FILE_SIZE: 10 * 1024 * 1024, // 10MB
  ALLOWED_FILE_TYPES: [
    "application/pdf",
    "application/msword",
    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
    "application/vnd.ms-powerpoint",
    "application/vnd.openxmlformats-officedocument.presentationml.presentation",
    "text/plain",
    "image/jpeg",
    "image/png",
  ],

  // UI Configuration
  TOAST_DURATION: 5000, // 5 seconds
  DEBOUNCE_DELAY: 300, // 300ms
  AUTO_REFRESH_INTERVAL: 30000, // 30 seconds

  // Quiz Configuration
  DEFAULT_QUIZ_LENGTH: 5,
  MAX_QUIZ_LENGTH: 50,
  QUIZ_TIME_LIMIT: 30, // minutes

  // Knowledge Levels
  KNOWLEDGE_LEVELS: [
    { value: "elementary", label: "Elementary (K-5)", color: "#28a745" },
    { value: "middleSchool", label: "Middle School (6-8)", color: "#17a2b8" },
    { value: "highSchool", label: "High School (9-12)", color: "#007bff" },
    { value: "college", label: "College", color: "#6f42c1" },
    { value: "graduate", label: "Graduate", color: "#e83e8c" },
    { value: "expert", label: "Expert", color: "#fd7e14" },
  ],

  // Subject Colors for Grouping
  SUBJECT_COLORS: [
    "#007bff",
    "#28a745",
    "#dc3545",
    "#ffc107",
    "#17a2b8",
    "#6f42c1",
    "#e83e8c",
    "#fd7e14",
    "#20c997",
    "#6c757d",
  ],

  // File Type Icons
  FILE_ICONS: {
    pdf: "bi-file-earmark-pdf",
    doc: "bi-file-earmark-word",
    docx: "bi-file-earmark-word",
    ppt: "bi-file-earmark-ppt",
    pptx: "bi-file-earmark-ppt",
    txt: "bi-file-earmark-text",
    jpg: "bi-file-earmark-image",
    jpeg: "bi-file-earmark-image",
    png: "bi-file-earmark-image",
  },

  // Local Storage Keys
  STORAGE_KEYS: {
    TOKEN: "study_ai_token",
    USER: "study_ai_user",
    AUTHENTICATED: "study_ai_authenticated",
    WIZARD_COMPLETED: "study_ai_wizard_completed",
    PREFERENCES: "study_ai_preferences",
    GUEST_DATA: "study_ai_guest_data",
  },

  // API Endpoints
  ENDPOINTS: {
    AUTH: {
      REGISTER: "/auth/register",
      LOGIN: "/auth/login",
      LOGOUT: "/auth/logout",
      ME: "/auth/me",
      CHANGE_PASSWORD: "/auth/change-password",
    },
    FILES: {
      UPLOAD: "/files/upload",
      LIST: "/files/list",
      GET: "/files",
      DELETE: "/files",
      RESTORE: "/files",
      DOWNLOAD: "/files",
      GROUPED: "/files",
      GROUPS: "/files/groups",
      PROCESS: "/files/process",
      REORGANIZE: "/files/reorganize",
    },
    ANALYSIS: {
      STUDY_GUIDE: "/analysis/generate-study-guide",
      STUDY_GUIDES: "/analysis/study-guides",
      QUIZ: "/analysis/generate-quiz",
      QUIZZES: "/analysis/quizzes",
      CHAT: "/analysis/chat",
      SUBMIT_QUIZ: "/analysis/submit-quiz-attempt",
      FILE_ANALYSIS: "/analysis/analyze-file",
    },
    KNOWLEDGE: {
      ANALYTICS: "/knowledge",
      PREFERENCES: "/knowledge",
      PERFORMANCE: "/knowledge/performance",
      PROGRESSION: "/knowledge",
      CONTENT_ANALYSIS: "/knowledge/analyze-content",
    },
    USER: {
      PROFILE: "/user/profile",
      STUDY_SESSIONS: "/user/study-sessions",
      STATS: "/user/stats",
    },
  },

  // Error Messages
  ERROR_MESSAGES: {
    NETWORK_ERROR: "Network error. Please check your connection.",
    UNAUTHORIZED: "Please log in to continue.",
    FORBIDDEN: "You do not have permission to perform this action.",
    NOT_FOUND: "The requested resource was not found.",
    VALIDATION_ERROR: "Please check your input and try again.",
    FILE_TOO_LARGE: "File is too large. Maximum size is 10MB.",
    INVALID_FILE_TYPE: "File type not supported.",
    GENERIC_ERROR: "An unexpected error occurred. Please try again.",
  },

  // Success Messages
  SUCCESS_MESSAGES: {
    FILE_UPLOADED: "File uploaded successfully!",
    FILE_DELETED: "File moved to trash.",
    FILE_RESTORED: "File restored successfully.",
    QUIZ_GENERATED: "Quiz generated successfully!",
    STUDY_GUIDE_GENERATED: "Study guide created!",
    PROFILE_UPDATED: "Profile updated successfully.",
    PREFERENCES_SAVED: "Preferences saved successfully.",
    ACCOUNT_CREATED: "Account created successfully!",
    LOGIN_SUCCESS: "Welcome back!",
  },

  // Guest Mode Configuration
  GUEST_LIMITS: {
    MAX_FILES: 5,
    MAX_QUIZZES: 3,
    MAX_CHAT_MESSAGES: 10,
    SESSION_DURATION: 24 * 60 * 60 * 1000, // 24 hours
  },
};

// Helper function to get file icon class
export function getFileIcon(fileType) {
  const extension = fileType.toLowerCase().replace(".", "");
  return CONFIG.FILE_ICONS[extension] || "bi-file-earmark";
}

// Helper function to get subject color
export function getSubjectColor(index) {
  return CONFIG.SUBJECT_COLORS[index % CONFIG.SUBJECT_COLORS.length];
}

// Helper function to format file size
export function formatFileSize(bytes) {
  if (bytes === 0) return "0 Bytes";
  const k = 1024;
  const sizes = ["Bytes", "KB", "MB", "GB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
}

// Helper function to format date
export function formatDate(date) {
  return new Date(date).toLocaleDateString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

// Helper function to truncate text
export function truncateText(text, maxLength = 100) {
  if (text.length <= maxLength) return text;
  return text.substring(0, maxLength) + "...";
}

// Re-export debounce from retry.js for backward compatibility
export { debounce } from "./utils/retry.js";

// Helper function to validate email
export function isValidEmail(email) {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}

// Helper function to validate password strength
export function validatePassword(password) {
  const minLength = 8;
  const hasUpperCase = /[A-Z]/.test(password);
  const hasLowerCase = /[a-z]/.test(password);
  const hasNumbers = /\d/.test(password);
  const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(password);

  return {
    isValid:
      password.length >= minLength &&
      hasUpperCase &&
      hasLowerCase &&
      hasNumbers,
    strength: {
      length: password.length >= minLength,
      upperCase: hasUpperCase,
      lowerCase: hasLowerCase,
      numbers: hasNumbers,
      specialChar: hasSpecialChar,
    },
  };
}
