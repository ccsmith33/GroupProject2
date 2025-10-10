// Form Validation Utilities

import { CONFIG } from "../config.js";

class ValidationManager {
  constructor() {
    this.rules = new Map();
    this.setupDefaultRules();
  }

  // Setup default validation rules
  setupDefaultRules() {
    // Email validation
    this.addRule("email", {
      required: true,
      pattern: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
      message: "Please enter a valid email address",
    });

    // Password validation
    this.addRule("password", {
      required: true,
      minLength: 8,
      pattern:
        /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]/,
      message:
        "Password must be at least 8 characters with uppercase, lowercase, number, and special character",
    });

    // Name validation
    this.addRule("name", {
      required: true,
      minLength: 2,
      maxLength: 50,
      pattern: /^[a-zA-Z\s]+$/,
      message:
        "Name must be 2-50 characters and contain only letters and spaces",
    });

    // File validation
    this.addRule("file", {
      required: true,
      maxSize: CONFIG.MAX_FILE_SIZE,
      allowedTypes: CONFIG.ALLOWED_FILE_TYPES,
      message: `File must be less than ${CONFIG.formatFileSize(
        CONFIG.MAX_FILE_SIZE
      )} and be a supported type`,
    });

    // Quiz prompt validation
    this.addRule("quizPrompt", {
      required: true,
      minLength: 10,
      maxLength: 500,
      message: "Quiz prompt must be 10-500 characters",
    });

    // Study guide prompt validation
    this.addRule("studyPrompt", {
      required: true,
      minLength: 10,
      maxLength: 1000,
      message: "Study guide prompt must be 10-1000 characters",
    });
  }

  // Add custom validation rule
  addRule(name, rule) {
    this.rules.set(name, rule);
  }

  // Validate field
  validateField(value, ruleName, customRule = null) {
    const rule = customRule || this.rules.get(ruleName);
    if (!rule) {
      return { isValid: true, message: "" };
    }

    const errors = [];

    // Required validation
    if (rule.required && (!value || value.toString().trim() === "")) {
      errors.push(rule.message || `${ruleName} is required`);
      return { isValid: false, message: errors[0] };
    }

    // Skip other validations if value is empty and not required
    if (!value || value.toString().trim() === "") {
      return { isValid: true, message: "" };
    }

    // Length validations
    if (rule.minLength && value.length < rule.minLength) {
      errors.push(`${ruleName} must be at least ${rule.minLength} characters`);
    }

    if (rule.maxLength && value.length > rule.maxLength) {
      errors.push(
        `${ruleName} must be no more than ${rule.maxLength} characters`
      );
    }

    // Pattern validation
    if (rule.pattern && !rule.pattern.test(value)) {
      errors.push(rule.message || `${ruleName} format is invalid`);
    }

    // File size validation
    if (rule.maxSize && value.size && value.size > rule.maxSize) {
      errors.push(
        `File must be less than ${CONFIG.formatFileSize(rule.maxSize)}`
      );
    }

    // File type validation
    if (
      rule.allowedTypes &&
      value.type &&
      !rule.allowedTypes.includes(value.type)
    ) {
      errors.push(
        `File type not supported. Allowed types: ${rule.allowedTypes.join(
          ", "
        )}`
      );
    }

    return {
      isValid: errors.length === 0,
      message: errors[0] || "",
      errors: errors,
    };
  }

  // Validate form
  validateForm(formData, rules) {
    const errors = {};
    let isValid = true;

    for (const [fieldName, value] of Object.entries(formData)) {
      const rule = rules[fieldName];
      if (rule) {
        const validation = this.validateField(
          value,
          rule.name || fieldName,
          rule
        );
        if (!validation.isValid) {
          errors[fieldName] = validation.message;
          isValid = false;
        }
      }
    }

    return { isValid, errors };
  }

  // Validate email
  validateEmail(email) {
    return this.validateField(email, "email");
  }

  // Validate password
  validatePassword(password) {
    const validation = this.validateField(password, "password");

    if (validation.isValid) {
      // Additional password strength analysis
      const strength = this.analyzePasswordStrength(password);
      return {
        isValid: true,
        message: "",
        strength: strength,
      };
    }

    return validation;
  }

  // Analyze password strength
  analyzePasswordStrength(password) {
    let score = 0;
    const feedback = [];

    // Length check
    if (password.length >= 8) {
      score += 1;
    } else {
      feedback.push("Use at least 8 characters");
    }

    // Lowercase check
    if (/[a-z]/.test(password)) {
      score += 1;
    } else {
      feedback.push("Add lowercase letters");
    }

    // Uppercase check
    if (/[A-Z]/.test(password)) {
      score += 1;
    } else {
      feedback.push("Add uppercase letters");
    }

    // Number check
    if (/\d/.test(password)) {
      score += 1;
    } else {
      feedback.push("Add numbers");
    }

    // Special character check
    if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) {
      score += 1;
    } else {
      feedback.push("Add special characters");
    }

    // Determine strength level
    let level = "weak";
    if (score >= 4) level = "strong";
    else if (score >= 3) level = "medium";

    return {
      score: score,
      level: level,
      feedback: feedback,
      percentage: (score / 5) * 100,
    };
  }

  // Validate file
  validateFile(file) {
    return this.validateField(file, "file");
  }

  // Validate quiz prompt
  validateQuizPrompt(prompt) {
    return this.validateField(prompt, "quizPrompt");
  }

  // Validate study guide prompt
  validateStudyPrompt(prompt) {
    return this.validateField(prompt, "studyPrompt");
  }

  // Sanitize input
  sanitizeInput(input) {
    if (typeof input !== "string") return input;

    return input
      .trim()
      .replace(/[<>]/g, "") // Remove potential HTML tags
      .replace(/javascript:/gi, "") // Remove javascript: protocol
      .replace(/on\w+\s*=/gi, ""); // Remove event handlers
  }

  // Validate and sanitize form data
  validateAndSanitizeForm(formData, rules) {
    const sanitizedData = {};
    const errors = {};
    let isValid = true;

    for (const [fieldName, value] of Object.entries(formData)) {
      // Sanitize string values
      const sanitizedValue =
        typeof value === "string" ? this.sanitizeInput(value) : value;
      sanitizedData[fieldName] = sanitizedValue;

      // Validate field
      const rule = rules[fieldName];
      if (rule) {
        const validation = this.validateField(
          sanitizedValue,
          rule.name || fieldName,
          rule
        );
        if (!validation.isValid) {
          errors[fieldName] = validation.message;
          isValid = false;
        }
      }
    }

    return {
      isValid,
      data: sanitizedData,
      errors,
    };
  }

  // Real-time validation for form fields
  setupRealTimeValidation(formElement) {
    const inputs = formElement.querySelectorAll("input, textarea, select");

    inputs.forEach((input) => {
      // Get validation rule from data attribute
      const ruleName = input.dataset.validate;
      if (!ruleName) return;

      // Validate on blur
      input.addEventListener("blur", () => {
        this.validateAndShowError(input, ruleName);
      });

      // Clear error on input
      input.addEventListener("input", () => {
        this.clearFieldError(input);
      });
    });
  }

  // Validate field and show error
  validateAndShowError(input, ruleName) {
    const validation = this.validateField(input.value, ruleName);

    if (!validation.isValid) {
      this.showFieldError(input, validation.message);
    } else {
      this.clearFieldError(input);
    }

    return validation.isValid;
  }

  // Show field error
  showFieldError(input, message) {
    this.clearFieldError(input);

    // Add error class
    input.classList.add("is-invalid");

    // Create error message element
    const errorElement = document.createElement("div");
    errorElement.className = "invalid-feedback";
    errorElement.textContent = message;

    // Insert after input
    input.parentNode.insertBefore(errorElement, input.nextSibling);
  }

  // Clear field error
  clearFieldError(input) {
    input.classList.remove("is-invalid");

    // Remove error message
    const errorElement = input.parentNode.querySelector(".invalid-feedback");
    if (errorElement) {
      errorElement.remove();
    }
  }

  // Validate entire form
  validateFormElement(formElement) {
    const formData = new FormData(formElement);
    const data = Object.fromEntries(formData.entries());

    // Get rules from data attributes
    const rules = {};
    const inputs = formElement.querySelectorAll("[data-validate]");

    inputs.forEach((input) => {
      const ruleName = input.dataset.validate;
      if (ruleName) {
        rules[input.name] = { name: ruleName };
      }
    });

    return this.validateAndSanitizeForm(data, rules);
  }

  // Check if form is valid
  isFormValid(formElement) {
    const validation = this.validateFormElement(formElement);
    return validation.isValid;
  }

  // Get form errors
  getFormErrors(formElement) {
    const validation = this.validateFormElement(formElement);
    return validation.errors;
  }

  // Show form errors
  showFormErrors(formElement, errors) {
    Object.entries(errors).forEach(([fieldName, message]) => {
      const input = formElement.querySelector(`[name="${fieldName}"]`);
      if (input) {
        this.showFieldError(input, message);
      }
    });
  }

  // Clear all form errors
  clearFormErrors(formElement) {
    const inputs = formElement.querySelectorAll(".is-invalid");
    inputs.forEach((input) => {
      this.clearFieldError(input);
    });
  }
}

// Create global validation manager
const validationManager = new ValidationManager();

// Export for global access
window.validationManager = validationManager;

export { validationManager };
