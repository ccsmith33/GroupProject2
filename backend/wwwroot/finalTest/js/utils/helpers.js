// Helper Utilities

import { CONFIG } from "../config.js";
import { logger } from "./logger.js";

class HelperManager {
  constructor() {
    this.debounceTimers = new Map();
  }

  // Format file size
  formatFileSize(bytes) {
    return CONFIG.formatFileSize(bytes);
  }

  // Format date
  formatDate(date) {
    return CONFIG.formatDate(date);
  }

  // Truncate text
  truncateText(text, maxLength = 100) {
    return CONFIG.truncateText(text, maxLength);
  }

  // Debounce function
  debounce(func, delay, key = null) {
    const timerKey = key || func.name || "default";

    return (...args) => {
      clearTimeout(this.debounceTimers.get(timerKey));
      this.debounceTimers.set(
        timerKey,
        setTimeout(() => func.apply(this, args), delay)
      );
    };
  }

  // Throttle function
  throttle(func, delay) {
    let lastCall = 0;
    return (...args) => {
      const now = Date.now();
      if (now - lastCall >= delay) {
        lastCall = now;
        return func.apply(this, args);
      }
    };
  }

  // Generate unique ID
  generateId(prefix = "id") {
    return `${prefix}_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  // Generate UUID v4
  generateUUID() {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(
      /[xy]/g,
      function (c) {
        const r = (Math.random() * 16) | 0;
        const v = c === "x" ? r : (r & 0x3) | 0x8;
        return v.toString(16);
      }
    );
  }

  // Get file icon
  getFileIcon(fileType) {
    return CONFIG.getFileIcon(fileType);
  }

  // Get subject color
  getSubjectColor(index) {
    return CONFIG.getSubjectColor(index);
  }

  // Check if value is empty
  isEmpty(value) {
    if (value === null || value === undefined) return true;
    if (typeof value === "string") return value.trim() === "";
    if (Array.isArray(value)) return value.length === 0;
    if (typeof value === "object") return Object.keys(value).length === 0;
    return false;
  }

  // Deep clone object
  deepClone(obj) {
    if (obj === null || typeof obj !== "object") return obj;
    if (obj instanceof Date) return new Date(obj.getTime());
    if (obj instanceof Array) return obj.map((item) => this.deepClone(item));
    if (typeof obj === "object") {
      const clonedObj = {};
      for (const key in obj) {
        if (obj.hasOwnProperty(key)) {
          clonedObj[key] = this.deepClone(obj[key]);
        }
      }
      return clonedObj;
    }
  }

  // Merge objects deeply
  deepMerge(target, source) {
    const result = { ...target };

    for (const key in source) {
      if (source.hasOwnProperty(key)) {
        if (
          source[key] &&
          typeof source[key] === "object" &&
          !Array.isArray(source[key])
        ) {
          result[key] = this.deepMerge(result[key] || {}, source[key]);
        } else {
          result[key] = source[key];
        }
      }
    }

    return result;
  }

  // Capitalize first letter
  capitalize(str) {
    if (!str || typeof str !== "string") return str;
    return str.charAt(0).toUpperCase() + str.slice(1).toLowerCase();
  }

  // Convert to title case
  toTitleCase(str) {
    if (!str || typeof str !== "string") return str;
    return str.replace(
      /\w\S*/g,
      (txt) => txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase()
    );
  }

  // Convert camelCase to kebab-case
  camelToKebab(str) {
    return str.replace(/([a-z0-9])([A-Z])/g, "$1-$2").toLowerCase();
  }

  // Convert kebab-case to camelCase
  kebabToCamel(str) {
    return str.replace(/-([a-z])/g, (match, letter) => letter.toUpperCase());
  }

  // Escape HTML
  escapeHtml(text) {
    const div = document.createElement("div");
    div.textContent = text;
    return div.innerHTML;
  }

  // Unescape HTML
  unescapeHtml(html) {
    const div = document.createElement("div");
    div.innerHTML = html;
    return div.textContent || div.innerText || "";
  }

  // Parse JSON safely
  parseJSON(jsonString, defaultValue = null) {
    try {
      return JSON.parse(jsonString);
    } catch (error) {
      logger.warn("JSON parse error:", error);
      return defaultValue;
    }
  }

  // Stringify JSON safely
  stringifyJSON(obj, defaultValue = "{}") {
    try {
      return JSON.stringify(obj);
    } catch (error) {
      logger.warn("JSON stringify error:", error);
      return defaultValue;
    }
  }

  // Get query parameter
  getQueryParam(name, url = window.location.href) {
    const urlObj = new URL(url);
    return urlObj.searchParams.get(name);
  }

  // Set query parameter
  setQueryParam(name, value, url = window.location.href) {
    const urlObj = new URL(url);
    urlObj.searchParams.set(name, value);
    return urlObj.toString();
  }

  // Remove query parameter
  removeQueryParam(name, url = window.location.href) {
    const urlObj = new URL(url);
    urlObj.searchParams.delete(name);
    return urlObj.toString();
  }

  // Check if device is mobile
  isMobile() {
    return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(
      navigator.userAgent
    );
  }

  // Check if device is tablet
  isTablet() {
    return /iPad|Android(?!.*Mobile)/i.test(navigator.userAgent);
  }

  // Check if device is desktop
  isDesktop() {
    return !this.isMobile() && !this.isTablet();
  }

  // Get device type
  getDeviceType() {
    if (this.isMobile()) return "mobile";
    if (this.isTablet()) return "tablet";
    return "desktop";
  }

  // Check if online
  isOnline() {
    return navigator.onLine;
  }

  // Wait for element to exist
  waitForElement(selector, timeout = 5000) {
    return new Promise((resolve, reject) => {
      const element = document.querySelector(selector);
      if (element) {
        resolve(element);
        return;
      }

      const observer = new MutationObserver((mutations, obs) => {
        const element = document.querySelector(selector);
        if (element) {
          obs.disconnect();
          resolve(element);
        }
      });

      observer.observe(document.body, {
        childList: true,
        subtree: true,
      });

      setTimeout(() => {
        observer.disconnect();
        reject(new Error(`Element ${selector} not found within ${timeout}ms`));
      }, timeout);
    });
  }

  // Wait for condition to be true
  waitForCondition(condition, timeout = 5000, interval = 100) {
    return new Promise((resolve, reject) => {
      const startTime = Date.now();

      const check = () => {
        if (condition()) {
          resolve();
        } else if (Date.now() - startTime >= timeout) {
          reject(new Error("Condition not met within timeout"));
        } else {
          setTimeout(check, interval);
        }
      };

      check();
    });
  }

  // Copy text to clipboard
  async copyToClipboard(text) {
    try {
      await navigator.clipboard.writeText(text);
      return true;
    } catch (error) {
      // Fallback for older browsers
      const textArea = document.createElement("textarea");
      textArea.value = text;
      textArea.style.position = "fixed";
      textArea.style.left = "-999999px";
      textArea.style.top = "-999999px";
      document.body.appendChild(textArea);
      textArea.focus();
      textArea.select();

      try {
        document.execCommand("copy");
        document.body.removeChild(textArea);
        return true;
      } catch (fallbackError) {
        document.body.removeChild(textArea);
        return false;
      }
    }
  }

  // Download file
  downloadFile(data, filename, type = "text/plain") {
    const blob = new Blob([data], { type });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  }

  // Format number with commas
  formatNumber(num) {
    return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
  }

  // Format percentage
  formatPercentage(value, decimals = 1) {
    return `${(value * 100).toFixed(decimals)}%`;
  }

  // Calculate percentage
  calculatePercentage(part, total) {
    if (total === 0) return 0;
    return (part / total) * 100;
  }

  // Round to decimal places
  roundToDecimals(num, decimals = 2) {
    return Math.round(num * Math.pow(10, decimals)) / Math.pow(10, decimals);
  }

  // Generate random number between min and max
  randomBetween(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
  }

  // Generate random string
  randomString(length = 8) {
    const chars =
      "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    let result = "";
    for (let i = 0; i < length; i++) {
      result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return result;
  }

  // Shuffle array
  shuffleArray(array) {
    const shuffled = [...array];
    for (let i = shuffled.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]];
    }
    return shuffled;
  }

  // Get random item from array
  getRandomItem(array) {
    return array[Math.floor(Math.random() * array.length)];
  }

  // Group array by key
  groupBy(array, key) {
    return array.reduce((groups, item) => {
      const group = item[key];
      groups[group] = groups[group] || [];
      groups[group].push(item);
      return groups;
    }, {});
  }

  // Sort array by key
  sortBy(array, key, direction = "asc") {
    return [...array].sort((a, b) => {
      const aVal = a[key];
      const bVal = b[key];

      if (aVal < bVal) return direction === "asc" ? -1 : 1;
      if (aVal > bVal) return direction === "asc" ? 1 : -1;
      return 0;
    });
  }

  // Remove duplicates from array
  removeDuplicates(array, key = null) {
    if (key) {
      const seen = new Set();
      return array.filter((item) => {
        const value = item[key];
        if (seen.has(value)) {
          return false;
        }
        seen.add(value);
        return true;
      });
    }
    return [...new Set(array)];
  }

  // Chunk array into smaller arrays
  chunkArray(array, size) {
    const chunks = [];
    for (let i = 0; i < array.length; i += size) {
      chunks.push(array.slice(i, i + size));
    }
    return chunks;
  }

  // Flatten nested array
  flattenArray(array) {
    return array.reduce((flat, item) => {
      return flat.concat(Array.isArray(item) ? this.flattenArray(item) : item);
    }, []);
  }

  // Get intersection of two arrays
  getIntersection(array1, array2) {
    return array1.filter((item) => array2.includes(item));
  }

  // Get difference of two arrays
  getDifference(array1, array2) {
    return array1.filter((item) => !array2.includes(item));
  }

  // Get union of two arrays
  getUnion(array1, array2) {
    return [...new Set([...array1, ...array2])];
  }

  // Check if arrays are equal
  arraysEqual(array1, array2) {
    if (array1.length !== array2.length) return false;
    return array1.every((item, index) => item === array2[index]);
  }

  // Get current timestamp
  getTimestamp() {
    return Date.now();
  }

  // Get current date string
  getDateString() {
    return new Date().toISOString().split("T")[0];
  }

  // Get current time string
  getTimeString() {
    return new Date().toTimeString().split(" ")[0];
  }

  // Sleep/delay function
  sleep(ms) {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }

  // Retry function with exponential backoff
  async retry(fn, maxAttempts = 3, delay = 1000) {
    for (let attempt = 1; attempt <= maxAttempts; attempt++) {
      try {
        return await fn();
      } catch (error) {
        if (attempt === maxAttempts) {
          throw error;
        }
        await this.sleep(delay * Math.pow(2, attempt - 1));
      }
    }
  }

  // Create element with attributes
  createElement(tag, attributes = {}, textContent = "") {
    const element = document.createElement(tag);

    Object.entries(attributes).forEach(([key, value]) => {
      if (key === "className") {
        element.className = value;
      } else if (key === "textContent") {
        element.textContent = value;
      } else if (key === "innerHTML") {
        element.innerHTML = value;
      } else {
        element.setAttribute(key, value);
      }
    });

    if (textContent) {
      element.textContent = textContent;
    }

    return element;
  }

  // Remove element safely
  removeElement(element) {
    if (element && element.parentNode) {
      element.parentNode.removeChild(element);
    }
  }

  // Add event listener with automatic cleanup
  addEventListenerWithCleanup(element, event, handler, options = {}) {
    element.addEventListener(event, handler, options);

    return () => {
      element.removeEventListener(event, handler, options);
    };
  }

  // Create cleanup function for multiple event listeners
  createCleanupFunction() {
    const cleanupFunctions = [];

    return {
      add: (cleanupFn) => {
        cleanupFunctions.push(cleanupFn);
      },
      cleanup: () => {
        cleanupFunctions.forEach((fn) => fn());
        cleanupFunctions.length = 0;
      },
    };
  }
}

// Create global helper manager
const helperManager = new HelperManager();

// Export for global access
window.helperManager = helperManager;

export { helperManager };
