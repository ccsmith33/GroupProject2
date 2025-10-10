// XSS Protection Utilities

/**
 * Sanitizes HTML content to prevent XSS attacks
 * @param {string} html - The HTML string to sanitize
 * @returns {string} - Sanitized HTML string
 */
export function sanitizeHTML(html) {
  if (typeof html !== "string") {
    return "";
  }

  // Create a temporary div element
  const temp = document.createElement("div");
  temp.textContent = html;
  return temp.innerHTML;
}

/**
 * Sanitizes text content by escaping HTML entities
 * @param {string} text - The text to sanitize
 * @returns {string} - Sanitized text
 */
export function sanitizeText(text) {
  if (typeof text !== "string") {
    return "";
  }

  return text
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#x27;")
    .replace(/\//g, "&#x2F;");
}

/**
 * Sanitizes file names to prevent path traversal and XSS
 * @param {string} filename - The filename to sanitize
 * @returns {string} - Sanitized filename
 */
export function sanitizeFilename(filename) {
  if (typeof filename !== "string") {
    return "unknown";
  }

  // Remove path traversal attempts
  let sanitized = filename.replace(/\.\./g, "");

  // Remove null bytes
  sanitized = sanitized.replace(/\0/g, "");

  // Remove control characters
  sanitized = sanitized.replace(/[\x00-\x1F\x7F]/g, "");

  // Limit length
  if (sanitized.length > 255) {
    const ext = sanitized.split(".").pop();
    const name = sanitized.substring(0, 255 - ext.length - 1);
    sanitized = name + "." + ext;
  }

  return sanitized;
}

/**
 * Sanitizes user input for display in HTML attributes
 * @param {string} input - The input to sanitize
 * @returns {string} - Sanitized input
 */
export function sanitizeAttribute(input) {
  if (typeof input !== "string") {
    return "";
  }

  return input
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#x27;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/&/g, "&amp;");
}

/**
 * Sanitizes JSON data before parsing
 * @param {string} jsonString - The JSON string to sanitize
 * @returns {string} - Sanitized JSON string
 */
export function sanitizeJSON(jsonString) {
  if (typeof jsonString !== "string") {
    return "{}";
  }

  // Remove potential script tags and other dangerous content
  return jsonString
    .replace(/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi, "")
    .replace(/javascript:/gi, "")
    .replace(/on\w+\s*=/gi, "");
}

/**
 * Validates and sanitizes email addresses
 * @param {string} email - The email to validate and sanitize
 * @returns {string|null} - Sanitized email or null if invalid
 */
export function sanitizeEmail(email) {
  if (typeof email !== "string") {
    return null;
  }

  const sanitized = email.trim().toLowerCase();
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  if (emailRegex.test(sanitized)) {
    return sanitized;
  }

  return null;
}

/**
 * Sanitizes user input for search queries
 * @param {string} query - The search query to sanitize
 * @returns {string} - Sanitized query
 */
export function sanitizeSearchQuery(query) {
  if (typeof query !== "string") {
    return "";
  }

  return query
    .trim()
    .replace(/[<>]/g, "")
    .replace(/javascript:/gi, "")
    .replace(/on\w+\s*=/gi, "")
    .substring(0, 100); // Limit length
}

/**
 * Creates a safe HTML element with sanitized content
 * @param {string} tagName - The HTML tag name
 * @param {string} content - The content to put inside the element
 * @param {Object} attributes - Optional attributes for the element
 * @returns {HTMLElement} - The created element
 */
export function createSafeElement(tagName, content, attributes = {}) {
  const element = document.createElement(tagName);

  // Set sanitized content
  if (content) {
    element.textContent = sanitizeText(content);
  }

  // Set sanitized attributes
  Object.entries(attributes).forEach(([key, value]) => {
    if (typeof value === "string" && value.length > 0) {
      element.setAttribute(key, sanitizeAttribute(value));
    }
  });

  return element;
}
