// Analysis API - Study guides, quizzes, and chat

import { CONFIG } from "../config.js";
import { authState } from "../state/auth-state.js";
import { apiCallWithRetry, handleError } from "../utils/retry.js";
import { logger } from "../utils/logger.js";

class AnalysisAPI {
  constructor() {
    this.baseURL = CONFIG.API_BASE;
  }

  // Generate study guide
  async generateStudyGuide(
    prompt,
    userId,
    contextFiles = [],
    contextGuides = []
  ) {
    // Check guest limits
    if (
      authState.isGuest() &&
      !authState.checkGuestLimits("generateStudyGuide")
    ) {
      return {
        success: false,
        error: authState.getGuestLimitMessage("generateStudyGuide"),
      };
    }

    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.ANALYSIS.STUDY_GUIDE}`,
        {
          method: "POST",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            userPrompt: prompt,
            userId: userId || "guest",
            contextFiles,
            contextGuides,
            model: authState.getModel(),
          }),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Study guide generation failed"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Generate quiz
  async generateQuiz(
    prompt,
    userId,
    level = "highSchool",
    length = 5,
    contextFiles = []
  ) {
    // Check guest limits
    if (authState.isGuest() && !authState.checkGuestLimits("generateQuiz")) {
      return {
        success: false,
        error: authState.getGuestLimitMessage("generateQuiz"),
      };
    }

    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.ANALYSIS.QUIZ}`,
        {
          method: "POST",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            userPrompt: prompt,
            userId: userId || "guest",
            knowledgeLevel: level,
            quizLength: length,
            contextFiles,
            model: authState.getModel(),
          }),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Quiz generation failed"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Submit quiz attempt
  async submitQuizAttempt(quizId, answers, userId) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.ANALYSIS.SUBMIT_QUIZ}`,
        {
          method: "POST",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            quizId,
            answers,
            userId: userId || "guest",
          }),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "Failed to submit quiz attempt"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Chat with AI
  async chat(
    message,
    userId,
    conversationId = null,
    contextFiles = [],
    contextGuides = []
  ) {
    // Check guest limits
    if (authState.isGuest() && !authState.checkGuestLimits("chat")) {
      return { success: false, error: authState.getGuestLimitMessage("chat") };
    }

    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.ANALYSIS.CHAT}`,
        {
          method: "POST",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            message,
            userId: userId || "guest",
            conversationId,
            contextFiles,
            contextGuides,
            model: authState.getModel(),
          }),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(data.message || data.detail || "Chat failed");
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Analyze file content
  async analyzeFile(fileId, subject = null, topic = null) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.ANALYSIS.FILE_ANALYSIS}/${fileId}`,
        {
          method: "POST",
          headers: {
            ...authState.getAuthHeader(),
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            subject,
            topic,
            model: authState.getModel(),
          }),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        const error = new Error(
          data.message || data.detail || "File analysis failed"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Get study guides for user
  async getStudyGuides(userId) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.ANALYSIS.STUDY_GUIDE}/${userId}`,
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
          data.message || data.detail || "Failed to get study guides"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Get quizzes for user
  async getQuizzes(userId) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.ANALYSIS.QUIZ}/${userId}`,
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
          data.message || data.detail || "Failed to get quizzes"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Get conversations for user
  async getConversations(userId) {
    return apiCallWithRetry(async () => {
      const response = await fetch(
        `${this.baseURL}${CONFIG.ENDPOINTS.ANALYSIS.CHAT}/${userId}`,
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
          data.message || data.detail || "Failed to get conversations"
        );
        error.status = response.status;
        throw error;
      }

      return data;
    });
  }

  // Parse quiz questions from response
  parseQuizQuestions(quizData) {
    try {
      if (typeof quizData.questions === "string") {
        return JSON.parse(quizData.questions);
      }
      return quizData.questions || [];
    } catch (error) {
      logger.error("Error parsing quiz questions:", error);
      return [];
    }
  }

  // Parse study guide content
  parseStudyGuideContent(guideData) {
    try {
      if (typeof guideData.content === "string") {
        return guideData.content;
      }
      return guideData.content || guideData.data?.content || "";
    } catch (error) {
      logger.error("Error parsing study guide content:", error);
      return "";
    }
  }

  // Parse chat response
  parseChatResponse(chatData) {
    try {
      if (typeof chatData.message === "string") {
        return chatData.message;
      }
      return chatData.message || chatData.data?.message || "";
    } catch (error) {
      logger.error("Error parsing chat response:", error);
      return "";
    }
  }

  // Validate quiz answers
  validateQuizAnswers(questions, answers) {
    const validation = {
      isValid: true,
      errors: [],
      score: 0,
      totalQuestions: questions.length,
    };

    questions.forEach((question, index) => {
      const answer = answers[question.id || index];
      if (!answer) {
        validation.errors.push(`Question ${index + 1} not answered`);
        validation.isValid = false;
      } else if (answer === question.correctAnswer) {
        validation.score++;
      }
    });

    validation.percentage = Math.round(
      (validation.score / validation.totalQuestions) * 100
    );

    return validation;
  }

  // Get quiz feedback based on score
  getQuizFeedback(score, totalQuestions) {
    const percentage = Math.round((score / totalQuestions) * 100);

    if (percentage >= 90) {
      return {
        level: "excellent",
        message:
          "Outstanding work! You have a strong understanding of this material.",
        color: "success",
      };
    } else if (percentage >= 80) {
      return {
        level: "good",
        message: "Great job! You have a good grasp of the concepts.",
        color: "success",
      };
    } else if (percentage >= 70) {
      return {
        level: "satisfactory",
        message: "Good effort! Consider reviewing some areas for improvement.",
        color: "warning",
      };
    } else if (percentage >= 60) {
      return {
        level: "needs-improvement",
        message: "Keep studying! Focus on the areas you missed.",
        color: "warning",
      };
    } else {
      return {
        level: "needs-work",
        message: "Don't give up! Review the material and try again.",
        color: "danger",
      };
    }
  }
}

// Create global analysis API instance
const analysisAPI = new AnalysisAPI();

export { analysisAPI };
