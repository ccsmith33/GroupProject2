// Files API

import { CONFIG } from "../config.js";
import { authState } from "../state/auth-state.js";
import { apiCallWithRetry, handleError } from "../utils/retry.js";
import { logger } from "../utils/logger.js";

class FilesAPI {
  constructor() {
    this.baseURL = CONFIG.API_BASE;
  }

  // Upload file
  async uploadFile(file, userId, subject = "", level = "") {
    // Check guest limits
    if (authState.isGuest() && !authState.checkGuestLimits("uploadFile")) {
      return {
        success: false,
        error: authState.getGuestLimitMessage("uploadFile"),
      };
    }

    return apiCallWithRetry(async () => {
      const formData = new FormData();
      formData.append("file", file);
      formData.append("userId", userId || "guest");
      formData.append("subject", subject);
      formData.append("studentLevel", level);

      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.FILES.UPLOAD}`,
        {
          method: "POST",
          body: formData,
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "File upload failed"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Get file list
  async getFiles(userId) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.FILES.LIST}?userId=${userId}`,
        {
          method: "GET",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to get files"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Get grouped files
  async getGroupedFiles(userId) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.FILES.GROUPED}/${userId}`,
        {
          method: "GET",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to get grouped files"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Get single file
  async getFile(fileId) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.FILES.GET}/${fileId}`,
        {
          method: "GET",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to get file"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Get file status
  async getFileStatus(fileId) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.FILES.GET}/${fileId}/status`,
        {
          method: "GET",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to get file status"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Delete file (soft delete)
  async deleteFile(fileId) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.FILES.DELETE}/${fileId}`,
        {
          method: "DELETE",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to delete file"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Restore file
  async restoreFile(fileId) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.FILES.RESTORE}/${fileId}/restore`,
        {
          method: "POST",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to restore file"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Process file manually
  async processFile(fileId) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.FILES.PROCESS}/${fileId}`,
        {
          method: "POST",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to process file"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Update file subject and topic
  async updateFileSubject(fileId, subject, topic) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.FILES.GET}/${fileId}/subject`,
        {
          method: "PUT",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            subject,
            topic,
          }),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to update file subject"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Bulk update file subjects
  async bulkUpdateSubjects(updates) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.FILES.BULK_UPDATE}`,
        {
          method: "POST",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
          body: JSON.stringify({ updates }),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to bulk update subjects"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Subject Groups API
  async createSubjectGroup(userId, groupName, description, color) {
    try {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.FILES.GROUPS}`,
        {
          method: "POST",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            userId,
            groupName,
            description,
            color,
          }),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        throw new Error(
          data.message || data.detail || "Failed to create subject group"
        );
      }

      return data;
    } catch (error) {
      logger.error("Create subject group error:", error);
      throw error;
    }
  }

  async getSubjectGroups(userId) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.FILES.GROUPS}?userId=${userId}`,
        {
          method: "GET",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to get subject groups"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  async updateSubjectGroup(groupId, updates) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.FILES.GROUPS}/${groupId}`,
        {
          method: "PUT",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
          body: JSON.stringify(updates),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to update subject group"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  async deleteSubjectGroup(groupId) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.FILES.GROUPS}/${groupId}`,
        {
          method: "DELETE",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to delete subject group"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }
}

// Create global files API instance
const filesAPI = new FilesAPI();

export { filesAPI };
