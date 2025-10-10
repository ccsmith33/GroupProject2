// User API
import { CONFIG } from "../config.js";
import { apiCallWithRetry, handleError } from "../utils/retry.js";
import { storageService } from "../utils/storageService.js";

class UserAPI {
  constructor() {
    this.baseURL = CONFIG.API_BASE;
  }

  // Get authentication headers
  async getAuthHeaders() {
    const token = await storageService.getItem(CONFIG.STORAGE_KEYS.TOKEN);
    return {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    };
  }

  // Get user profile
  async getProfile() {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.USER.PROFILE}`,
        {
          method: "GET",
          headers: await this.getAuthHeaders(),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to get profile"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Update user profile
  async updateProfile(profileData) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.USER.PROFILE}`,
        {
          method: "PUT",
          headers: await this.getAuthHeaders(),
          body: JSON.stringify(profileData),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to update profile"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Get user statistics
  async getStats() {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.USER.STATS}`,
        {
          method: "GET",
          headers: await this.getAuthHeaders(),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to get stats"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Get study sessions
  async getStudySessions(limit = 20, offset = 0) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.USER.STUDY_SESSIONS}?limit=${limit}&offset=${offset}`,
        {
          method: "GET",
          headers: await this.getAuthHeaders(),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to get study sessions"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Create study session
  async createStudySession(sessionData) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.USER.STUDY_SESSIONS}`,
        {
          method: "POST",
          headers: await this.getAuthHeaders(),
          body: JSON.stringify(sessionData),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to create study session"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Update study session
  async updateStudySession(sessionId, sessionData) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.USER.STUDY_SESSIONS}/${sessionId}`,
        {
          method: "PUT",
          headers: await this.getAuthHeaders(),
          body: JSON.stringify(sessionData),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to update study session"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Delete study session
  async deleteStudySession(sessionId) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.USER.STUDY_SESSIONS}/${sessionId}`,
        {
          method: "DELETE",
          headers: await this.getAuthHeaders(),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to delete study session"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }
}

// Create global user API instance
const userAPI = new UserAPI();

export { userAPI };
