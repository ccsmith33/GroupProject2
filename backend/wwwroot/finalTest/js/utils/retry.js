// Retry Logic and Error Handling Utilities

/**
 * Retry configuration options
 */
const RETRY_CONFIG = {
  maxRetries: 3,
  baseDelay: 1000, // 1 second
  maxDelay: 10000, // 10 seconds
  backoffMultiplier: 2,
  timeout: 30000, // 30 seconds
};

/**
 * Calculate delay for exponential backoff
 * @param {number} attempt - Current attempt number (0-based)
 * @returns {number} - Delay in milliseconds
 */
function calculateDelay(attempt) {
  const delay =
    RETRY_CONFIG.baseDelay * Math.pow(RETRY_CONFIG.backoffMultiplier, attempt);
  return Math.min(delay, RETRY_CONFIG.maxDelay);
}

/**
 * Check if an error is retryable
 * @param {Error} error - The error to check
 * @returns {boolean} - Whether the error should trigger a retry
 */
function isRetryableError(error) {
  // Network errors
  if (error.name === "TypeError" && error.message.includes("fetch")) {
    return true;
  }

  // HTTP status codes that should be retried
  if (error.status) {
    return [408, 429, 500, 502, 503, 504].includes(error.status);
  }

  // Timeout errors
  if (error.name === "TimeoutError") {
    return true;
  }

  return false;
}

/**
 * Create a timeout promise
 * @param {number} timeoutMs - Timeout in milliseconds
 * @returns {Promise} - Promise that rejects after timeout
 */
function createTimeout(timeoutMs) {
  return new Promise((_, reject) => {
    setTimeout(() => {
      reject(new Error(`Request timeout after ${timeoutMs}ms`));
    }, timeoutMs);
  });
}

/**
 * Retry a function with exponential backoff
 * @param {Function} fn - Function to retry
 * @param {Object} options - Retry options
 * @returns {Promise} - Promise that resolves with function result
 */
export async function retry(fn, options = {}) {
  const config = { ...RETRY_CONFIG, ...options };
  let lastError;

  for (let attempt = 0; attempt <= config.maxRetries; attempt++) {
    try {
      // Create timeout promise
      const timeoutPromise = createTimeout(config.timeout);

      // Race between function and timeout
      const result = await Promise.race([fn(), timeoutPromise]);
      return result;
    } catch (error) {
      lastError = error;

      // Don't retry if it's the last attempt or error is not retryable
      if (attempt === config.maxRetries || !isRetryableError(error)) {
        break;
      }

      // Calculate delay and wait
      const delay = calculateDelay(attempt);
      await new Promise((resolve) => setTimeout(resolve, delay));
    }
  }

  throw lastError;
}

/**
 * Enhanced fetch with retry logic and timeout
 * @param {string} url - URL to fetch
 * @param {Object} options - Fetch options
 * @param {Object} retryOptions - Retry configuration
 * @returns {Promise<Response>} - Fetch response
 */
export async function fetchWithRetry(url, options = {}, retryOptions = {}) {
  return retry(async () => {
    const response = await fetch(url, options);

    // Throw error for non-2xx status codes
    if (!response.ok) {
      const error = new Error(
        `HTTP ${response.status}: ${response.statusText}`
      );
      error.status = response.status;
      error.response = response;
      throw error;
    }

    return response;
  }, retryOptions);
}

/**
 * Enhanced API call with retry and error handling
 * @param {Function} apiCall - API function to call
 * @param {Object} retryOptions - Retry configuration
 * @returns {Promise<Object>} - API response
 */
export async function apiCallWithRetry(apiCall, retryOptions = {}) {
  try {
    const response = await retry(apiCall, retryOptions);
    return response; // Return data directly
  } catch (error) {
    console.error("API call failed:", error);

    // Format error message based on error type
    let errorMessage = "An unexpected error occurred";

    if (error.status) {
      switch (error.status) {
        case 400:
          errorMessage = "Invalid request. Please check your input.";
          break;
        case 401:
          errorMessage = "Authentication required. Please log in again.";
          break;
        case 403:
          errorMessage =
            "Access denied. You don't have permission for this action.";
          break;
        case 404:
          errorMessage = "Resource not found.";
          break;
        case 408:
          errorMessage = "Request timeout. Please try again.";
          break;
        case 429:
          errorMessage =
            "Too many requests. Please wait a moment and try again.";
          break;
        case 500:
          errorMessage = "Server error. Please try again later.";
          break;
        case 502:
        case 503:
        case 504:
          errorMessage =
            "Service temporarily unavailable. Please try again later.";
          break;
        default:
          errorMessage = `Request failed with status ${error.status}`;
      }
    } else if (error.name === "TypeError" && error.message.includes("fetch")) {
      errorMessage = "Network error. Please check your connection.";
    } else if (error.message.includes("timeout")) {
      errorMessage = "Request timed out. Please try again.";
    } else if (error.message) {
      errorMessage = error.message;
    }

    // Create enhanced error with user-friendly message
    const enhancedError = new Error(errorMessage);
    enhancedError.status = error.status;
    enhancedError.originalError = error;
    throw enhancedError; // Always throw, let caller handle
  }
}

/**
 * Check network connectivity
 * @returns {Promise<boolean>} - Whether network is available
 */
export async function checkConnectivity() {
  try {
    const response = await fetch("/api/health", {
      method: "HEAD",
      cache: "no-cache",
    });
    return response.ok;
  } catch {
    return false;
  }
}

/**
 * Wait for network connectivity
 * @param {number} maxWaitTime - Maximum time to wait in milliseconds
 * @returns {Promise<boolean>} - Whether connectivity was restored
 */
export async function waitForConnectivity(maxWaitTime = 30000) {
  const startTime = Date.now();

  while (Date.now() - startTime < maxWaitTime) {
    if (await checkConnectivity()) {
      return true;
    }

    // Wait 1 second before checking again
    await new Promise((resolve) => setTimeout(resolve, 1000));
  }

  return false;
}

/**
 * Enhanced error handler with user-friendly messages
 * @param {Error} error - The error to handle
 * @param {string} context - Context where error occurred
 * @returns {Object} - Formatted error information
 */
export function handleError(error, context = "Unknown") {
  console.error(`Error in ${context}:`, error);

  let userMessage = "An unexpected error occurred";
  let shouldRetry = false;

  if (error.status) {
    switch (error.status) {
      case 400:
        userMessage = "Invalid request. Please check your input and try again.";
        break;
      case 401:
        userMessage = "Your session has expired. Please log in again.";
        break;
      case 403:
        userMessage = "You don't have permission to perform this action.";
        break;
      case 404:
        userMessage = "The requested resource was not found.";
        break;
      case 408:
        userMessage = "Request timed out. Please try again.";
        shouldRetry = true;
        break;
      case 429:
        userMessage = "Too many requests. Please wait a moment and try again.";
        shouldRetry = true;
        break;
      case 500:
        userMessage = "Server error. Please try again later.";
        shouldRetry = true;
        break;
      case 502:
      case 503:
      case 504:
        userMessage =
          "Service temporarily unavailable. Please try again later.";
        shouldRetry = true;
        break;
      default:
        userMessage = `Request failed with status ${error.status}`;
    }
  } else if (error.name === "TypeError" && error.message.includes("fetch")) {
    userMessage = "Network error. Please check your internet connection.";
    shouldRetry = true;
  } else if (error.message.includes("timeout")) {
    userMessage = "Request timed out. Please try again.";
    shouldRetry = true;
  } else if (error.message) {
    userMessage = error.message;
  }

  return {
    message: userMessage,
    shouldRetry,
    status: error.status,
    originalError: error,
  };
}

/**
 * Create a debounced function
 * @param {Function} func - Function to debounce
 * @param {number} wait - Wait time in milliseconds
 * @returns {Function} - Debounced function
 */
export function debounce(func, wait) {
  let timeout;
  return function executedFunction(...args) {
    const later = () => {
      clearTimeout(timeout);
      func(...args);
    };
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
  };
}

/**
 * Create a throttled function
 * @param {Function} func - Function to throttle
 * @param {number} limit - Time limit in milliseconds
 * @returns {Function} - Throttled function
 */
export function throttle(func, limit) {
  let inThrottle;
  return function executedFunction(...args) {
    if (!inThrottle) {
      func.apply(this, args);
      inThrottle = true;
      setTimeout(() => (inThrottle = false), limit);
    }
  };
}
