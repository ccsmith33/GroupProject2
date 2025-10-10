// Authentication API

import { CONFIG } from "../config.js";
import { apiCallWithRetry, handleError } from "../utils/retry.js";
import { logger } from "../utils/logger.js";
import { storageService } from "../utils/storageService.js";

class AuthAPI {
  constructor() {
    this.baseURL = CONFIG.API_BASE;
  }

  // Register new user
  async register(email, password, name) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.AUTH.REGISTER}`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            email,
            password,
            name,
          }),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Registration failed"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Login user
  async login(email, password) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.AUTH.LOGIN}`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            email,
            password,
          }),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(data.message || data.detail || "Login failed");
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Logout user
  async logout() {
    try {
      const token = await storageService.getItem(CONFIG.STORAGE_KEYS.TOKEN);

      if (!token) {
        return { success: true };
      }

      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.AUTH.LOGOUT}`,
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
        }
      );

      const data = await response.json();
      return data;
    } catch (error) {
      logger.error("Logout error:", error);
      // Don't throw error for logout, just log it
      return { success: true };
    }
  }

  // Get current user
  async getCurrentUser() {
    const token = await storageService.getItem(CONFIG.STORAGE_KEYS.TOKEN);

    if (!token) {
      return { success: false, error: "No authentication token" };
    }

    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.AUTH.ME}`,
        {
          method: "GET",
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to get user"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Change password
  async changePassword(currentPassword, newPassword) {
    const token = await storageService.getItem(CONFIG.STORAGE_KEYS.TOKEN);

    if (!token) {
      return { success: false, error: "No authentication token" };
    }

    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.AUTH.CHANGE_PASSWORD}`,
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            currentPassword,
            newPassword,
          }),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Password change failed"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Validate token (no refresh endpoint available)
  async validateToken() {
    const token = await storageService.getItem(CONFIG.STORAGE_KEYS.TOKEN);

    if (!token) {
      return false;
    }

    const result = await apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.AUTH.ME}`,
        {
          method: "GET",
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
        }
      );

      if (!response.ok) {
        const error = new Error("Token validation failed");
        error.status = response.status;
        throw error;
      }

      return response;
    });

    return result.success;
  }
}

// Create global auth API instance
const authAPI = new AuthAPI();

export { authAPI };
