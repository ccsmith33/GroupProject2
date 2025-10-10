// Centralized Logging Service
// Provides consistent logging with debug mode support

class Logger {
  constructor() {
    this.debugMode = this.getDebugMode();
    this.logLevel = this.getLogLevel();
  }

  /**
   * Get debug mode from localStorage or URL parameter
   * @returns {boolean} Whether debug mode is enabled
   */
  getDebugMode() {
    // Check URL parameter first
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get("debug") === "true") {
      return true;
    }

    // Check localStorage
    const stored = localStorage.getItem("debug");
    if (stored !== null) {
      return stored === "true";
    }

    // Default to false
    return false;
  }

  /**
   * Get log level from configuration
   * @returns {string} Log level (error, warn, info, debug)
   */
  getLogLevel() {
    const level = localStorage.getItem("logLevel") || "info";
    return ["error", "warn", "info", "debug"].includes(level) ? level : "info";
  }

  /**
   * Check if a log level should be output
   * @param {string} level - Log level to check
   * @returns {boolean} Whether to output this level
   */
  shouldLog(level) {
    const levels = { error: 0, warn: 1, info: 2, debug: 3 };
    return levels[level] <= levels[this.logLevel];
  }

  /**
   * Log debug information (only in debug mode)
   * @param {string} message - Log message
   * @param {...any} args - Additional arguments
   */
  debug(message, ...args) {
    if (this.shouldLog("debug") && this.debugMode) {
      console.log(`[DEBUG] ${message}`, ...args);
    }
  }

  /**
   * Log general information
   * @param {string} message - Log message
   * @param {...any} args - Additional arguments
   */
  info(message, ...args) {
    if (this.shouldLog("info")) {
      console.log(`[INFO] ${message}`, ...args);
    }
  }

  /**
   * Log warning messages
   * @param {string} message - Log message
   * @param {...any} args - Additional arguments
   */
  warn(message, ...args) {
    if (this.shouldLog("warn")) {
      console.warn(`[WARN] ${message}`, ...args);
    }
  }

  /**
   * Log error messages (always shown)
   * @param {string} message - Log message
   * @param {...any} args - Additional arguments
   */
  error(message, ...args) {
    if (this.shouldLog("error")) {
      console.error(`[ERROR] ${message}`, ...args);
    }
  }

  /**
   * Log API calls (debug mode only)
   * @param {string} method - HTTP method
   * @param {string} url - API endpoint
   * @param {any} data - Request/response data
   */
  api(method, url, data = null) {
    if (this.debugMode) {
      console.log(`[API] ${method.toUpperCase()} ${url}`, data);
    }
  }

  /**
   * Log performance timing
   * @param {string} label - Timing label
   * @param {number} startTime - Start time from performance.now()
   */
  time(label, startTime) {
    if (this.debugMode) {
      const duration = performance.now() - startTime;
      console.log(`[PERF] ${label}: ${duration.toFixed(2)}ms`);
    }
  }

  /**
   * Log user actions (debug mode only)
   * @param {string} action - User action
   * @param {any} data - Action data
   */
  user(action, data = null) {
    if (this.debugMode) {
      console.log(`[USER] ${action}`, data);
    }
  }

  /**
   * Log component lifecycle events
   * @param {string} component - Component name
   * @param {string} event - Lifecycle event
   * @param {any} data - Event data
   */
  component(component, event, data = null) {
    if (this.debugMode) {
      console.log(`[COMP] ${component}.${event}`, data);
    }
  }

  /**
   * Enable debug mode
   */
  enableDebug() {
    this.debugMode = true;
    localStorage.setItem("debug", "true");
    this.info("Debug mode enabled");
  }

  /**
   * Disable debug mode
   */
  disableDebug() {
    this.debugMode = false;
    localStorage.setItem("debug", "false");
    this.info("Debug mode disabled");
  }

  /**
   * Set log level
   * @param {string} level - New log level
   */
  setLogLevel(level) {
    if (["error", "warn", "info", "debug"].includes(level)) {
      this.logLevel = level;
      localStorage.setItem("logLevel", level);
      this.info(`Log level set to ${level}`);
    }
  }

  /**
   * Create a performance timer
   * @param {string} label - Timer label
   * @returns {Function} Function to call when done
   */
  startTimer(label) {
    const startTime = performance.now();
    return () => this.time(label, startTime);
  }

  /**
   * Log grouped messages (debug mode only)
   * @param {string} group - Group name
   * @param {Function} fn - Function to execute in group
   */
  group(group, fn) {
    if (this.debugMode) {
      console.group(`[GROUP] ${group}`);
      try {
        fn();
      } finally {
        console.groupEnd();
      }
    } else {
      fn();
    }
  }
}

// Create global logger instance
const logger = new Logger();

// Export for module use
export { logger, Logger };

// Make available globally for easy access
window.logger = logger;
