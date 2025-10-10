// Knowledge API - Analytics, progress tracking, and learning preferences

import { CONFIG } from "../config.js";
import { authState } from "../state/auth-state.js";
import { apiCallWithRetry, handleError } from "../utils/retry.js";
import { logger } from "../utils/logger.js";

class KnowledgeAPI {
  constructor() {
    this.baseURL = CONFIG.API_BASE;
  }

  // Get analytics for user
  async getAnalytics(userId) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.KNOWLEDGE.ANALYTICS}/${userId}/analytics`,
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
        // Return empty analytics for guests or when no data exists
        if (response.status === 404 || authState.isGuest()) {
          return {
            success: true,
            data: {
              totalFiles: 0,
              totalQuizzes: 0,
              averageScore: 0,
              studyTime: 0,
              subjectProfiles: {},
              recentActivity: [],
              knowledgeLevel: "highSchool",
              progress: [],
            },
          };
        }
        const error = new Error(
          data.message || data.detail || "Failed to get analytics"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    }).catch((error) => {
      logger.error("Get analytics error:", error);
      // Return empty analytics on error
      return {
        success: true,
        data: {
          totalFiles: 0,
          totalQuizzes: 0,
          averageScore: 0,
          studyTime: 0,
          subjectProfiles: {},
          recentActivity: [],
          knowledgeLevel: "highSchool",
          progress: [],
        },
      };
    });
  }

  // Get learning preferences
  async getPreferences(userId) {
    try {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.KNOWLEDGE.PREFERENCES}/${userId}/preferences`,
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
        // Return default preferences if not found
        if (response.status === 404) {
          return {
            success: true,
            data: {
              preferredQuizLength: 5,
              preferredKnowledgeLevel: "highSchool",
              studyStyle: "mixed",
              timeAvailable: 30,
              notifications: true,
              autoSave: true,
            },
          };
        }
        throw new Error(
          data.message || data.detail || "Failed to get preferences"
        );
      }

      return data;
    } catch (error) {
      logger.error("Get preferences error:", error);
      // Return default preferences on error
      return {
        success: true,
        data: {
          preferredQuizLength: 5,
          preferredKnowledgeLevel: "highSchool",
          studyStyle: "mixed",
          timeAvailable: 30,
          notifications: true,
          autoSave: true,
        },
      };
    }
  }

  // Update learning preferences
  async updatePreferences(userId, preferences) {
    try {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.KNOWLEDGE.PREFERENCES}/${userId}/preferences`,
        {
          method: "PUT",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
          body: JSON.stringify(preferences),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        throw new Error(
          data.message || data.detail || "Failed to update preferences"
        );
      }

      return data;
    } catch (error) {
      logger.error("Update preferences error:", error);
      throw error;
    }
  }

  // Get quiz performance
  async getQuizPerformance(userId) {
    try {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.KNOWLEDGE.PERFORMANCE}?userId=${userId}`,
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
        throw new Error(
          data.message || data.detail || "Failed to get quiz performance"
        );
      }

      return data;
    } catch (error) {
      logger.error("Get quiz performance error:", error);
      throw error;
    }
  }

  // Get knowledge progression
  async getKnowledgeProgression(userId) {
    try {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.KNOWLEDGE.PROGRESSION}/${userId}/progression`,
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
        throw new Error(
          data.message || data.detail || "Failed to get knowledge progression"
        );
      }

      return data;
    } catch (error) {
      logger.error("Get knowledge progression error:", error);
      throw error;
    }
  }

  // Analyze content difficulty
  async analyzeContentDifficulty(fileId) {
    try {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.KNOWLEDGE.CONTENT_ANALYSIS}/${fileId}`,
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
        throw new Error(
          data.message || data.detail || "Failed to analyze content difficulty"
        );
      }

      return data;
    } catch (error) {
      logger.error("Analyze content difficulty error:", error);
      throw error;
    }
  }

  // Update user knowledge level
  async updateKnowledgeLevel(userId, subject, level) {
    try {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.KNOWLEDGE.ANALYTICS}/${userId}/level`,
        {
          method: "POST",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            subject,
            level,
          }),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        throw new Error(
          data.message || data.detail || "Failed to update knowledge level"
        );
      }

      return data;
    } catch (error) {
      logger.error("Update knowledge level error:", error);
      throw error;
    }
  }

  // Get study sessions
  async getStudySessions(userId) {
    try {
      const response = await fetch(
        `${this.baseURL}/user/study-sessions?userId=${userId}`,
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
        throw new Error(
          data.message || data.detail || "Failed to get study sessions"
        );
      }

      return data;
    } catch (error) {
      logger.error("Get study sessions error:", error);
      throw error;
    }
  }

  // Create study session
  async createStudySession(userId, sessionData) {
    try {
      const response = await fetch(`${this.baseURL}/user/study-sessions`, {
        method: "POST",
        headers: {
          ...authState.getAuthHeader(),
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          userId,
          ...sessionData,
        }),
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(
          data.message || data.detail || "Failed to create study session"
        );
      }

      return data;
    } catch (error) {
      logger.error("Create study session error:", error);
      throw error;
    }
  }

  // Get user stats
  async getUserStats(userId) {
    try {
      const response = await fetch(
        `${this.baseURL}/user/stats?userId=${userId}`,
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
        throw new Error(
          data.message || data.detail || "Failed to get user stats"
        );
      }

      return data;
    } catch (error) {
      logger.error("Get user stats error:", error);
      throw error;
    }
  }

  // Process analytics data for charts
  processAnalyticsForCharts(analytics) {
    if (!analytics || !analytics.data) {
      return {
        subjectProgress: [],
        quizPerformance: [],
        studyTime: [],
        knowledgeLevels: [],
      };
    }

    const data = analytics.data;

    // Process subject progress for radar chart
    const subjectProgress = Object.entries(data.subjectProfiles || {}).map(
      ([subject, profile]) => ({
        subject,
        level: profile.knowledgeLevel || 0,
        quizzes: profile.quizCount || 0,
        averageScore: profile.averageScore || 0,
      })
    );

    // Process quiz performance for line chart
    const quizPerformance = (data.recentActivity || [])
      .filter((activity) => activity.type === "quiz")
      .map((activity) => ({
        date: new Date(activity.date).toLocaleDateString(),
        score: activity.score || 0,
        subject: activity.subject || "Unknown",
      }));

    // Process study time for bar chart
    const studyTime = Object.entries(data.subjectProfiles || {}).map(
      ([subject, profile]) => ({
        subject,
        time: profile.studyTime || 0,
      })
    );

    // Process knowledge levels for gauge chart
    const knowledgeLevels = Object.entries(data.subjectProfiles || {}).map(
      ([subject, profile]) => ({
        subject,
        level: profile.knowledgeLevel || 0,
        maxLevel: 6, // Based on CONFIG.KNOWLEDGE_LEVELS
      })
    );

    return {
      subjectProgress,
      quizPerformance,
      studyTime,
      knowledgeLevels,
    };
  }

  // Calculate overall progress percentage
  calculateOverallProgress(analytics) {
    if (!analytics || !analytics.data) {
      return 0;
    }

    const data = analytics.data;
    const subjects = Object.keys(data.subjectProfiles || {});

    if (subjects.length === 0) {
      return 0;
    }

    const totalLevel = subjects.reduce((sum, subject) => {
      const profile = data.subjectProfiles[subject];
      return sum + (profile.knowledgeLevel || 0);
    }, 0);

    const maxPossibleLevel = subjects.length * 6; // 6 is max knowledge level
    return Math.round((totalLevel / maxPossibleLevel) * 100);
  }

  // Get achievement badges
  getAchievementBadges(analytics) {
    if (!analytics || !analytics.data) {
      return [];
    }

    const data = analytics.data;
    const badges = [];

    // First file uploaded
    if (data.totalFiles > 0) {
      badges.push({
        id: "first-file",
        name: "First Steps",
        description: "Uploaded your first study material",
        icon: "bi-file-earmark-plus",
        color: "success",
        earned: true,
      });
    }

    // First quiz taken
    if (data.totalQuizzes > 0) {
      badges.push({
        id: "first-quiz",
        name: "Quiz Master",
        description: "Completed your first quiz",
        icon: "bi-question-circle",
        color: "primary",
        earned: true,
      });
    }

    // High score achievement
    if (data.averageScore >= 90) {
      badges.push({
        id: "high-scorer",
        name: "High Achiever",
        description: "Maintained 90%+ average score",
        icon: "bi-trophy",
        color: "warning",
        earned: true,
      });
    }

    // Study streak
    const studyDays = new Set(
      (data.recentActivity || []).map((activity) =>
        new Date(activity.date).toDateString()
      )
    ).size;

    if (studyDays >= 7) {
      badges.push({
        id: "study-streak",
        name: "Dedicated Learner",
        description: "Studied for 7 consecutive days",
        icon: "bi-fire",
        color: "danger",
        earned: true,
      });
    }

    // Subject mastery
    const masteredSubjects = Object.values(data.subjectProfiles || {}).filter(
      (profile) => profile.knowledgeLevel >= 5
    ).length;

    if (masteredSubjects >= 3) {
      badges.push({
        id: "subject-master",
        name: "Subject Master",
        description: "Mastered 3+ subjects",
        icon: "bi-award",
        color: "info",
        earned: true,
      });
    }

    return badges;
  }
}

// Create global knowledge API instance
const knowledgeAPI = new KnowledgeAPI();

export { knowledgeAPI };
