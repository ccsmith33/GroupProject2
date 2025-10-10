// Study Tools Component
import { analysisAPI } from "../api/analysis.js";
import { filesAPI } from "../api/files.js";
import { store } from "../state/store.js";
import { authState } from "../state/auth-state.js";
import { notificationManager } from "../utils/notifications.js";
import { CONFIG } from "../config.js";
import { logger } from "../utils/logger.js";
import { storageService } from "../utils/storageService.js";
import {
  sanitizeText,
  sanitizeQuizContent,
  sanitizeStudyGuideContent,
} from "../utils/sanitize.js";

class StudyTools {
  constructor() {
    this.isInitialized = false;
    this.currentView = "generator"; // generator, quiz, chat
    this.files = [];
    this.studyGuides = [];
    this.quizzes = [];
    this.conversations = [];
    this.currentConversation = null;
    this.currentQuiz = null;
    this.quizAnswers = {};
    this.quizResults = null;
    this.isGenerating = false;
    this.isQuizActive = false;
  }

  // Initialize study tools
  async init() {
    if (this.isInitialized) return;

    try {
      this.setupEventListeners();
      await this.loadData();
      await this.loadConversations();
      this.isInitialized = true;
    } catch (error) {
      logger.error("Study tools initialization error:", error);
      notificationManager.error("Failed to initialize study tools");
    }
  }

  // Setup event listeners
  setupEventListeners() {
    // Store cleanup functions
    this.cleanupFunctions = [];

    // View toggle buttons
    this.handleViewToggle = (e) => {
      if (e.target.matches("[data-study-view]")) {
        this.switchView(e.target.dataset.studyView);
      }
    };
    document.addEventListener("click", this.handleViewToggle);
    this.cleanupFunctions.push(() => {
      document.removeEventListener("click", this.handleViewToggle);
    });

    // Study guide generation
    this.handleStudyActionClick = (e) => {
      if (e.target.matches("[data-study-action]")) {
        this.handleStudyAction(e.target);
      }
    };
    document.addEventListener("click", this.handleStudyActionClick);
    this.cleanupFunctions.push(() => {
      document.removeEventListener("click", this.handleStudyActionClick);
    });

    // Quiz interactions
    this.handleQuizActionClick = (e) => {
      if (e.target.matches("[data-quiz-action]")) {
        this.handleQuizAction(e.target);
      }
    };
    document.addEventListener("click", this.handleQuizActionClick);
    this.cleanupFunctions.push(() => {
      document.removeEventListener("click", this.handleQuizActionClick);
    });

    // Chat interactions
    this.handleChatActionClick = (e) => {
      if (e.target.matches("[data-chat-action]")) {
        this.handleChatAction(e.target);
      }
    };
    document.addEventListener("click", this.handleChatActionClick);
    this.cleanupFunctions.push(() => {
      document.removeEventListener("click", this.handleChatActionClick);
    });

    // Conversation interactions
    this.handleConversationClick = (e) => {
      if (
        e.target.matches("[data-conversation-id]") ||
        e.target.closest("[data-conversation-id]")
      ) {
        const conversationItem = e.target.closest("[data-conversation-id]");
        if (conversationItem) {
          const conversationId = parseInt(
            conversationItem.dataset.conversationId
          );
          this.loadConversation(conversationId);
        }
      }
    };
    document.addEventListener("click", this.handleConversationClick);
    this.cleanupFunctions.push(() => {
      document.removeEventListener("click", this.handleConversationClick);
    });

    // Quiz option selection
    this.handleQuizOptionClick = (e) => {
      if (e.target.matches("[data-quiz-option]")) {
        this.selectQuizOption(e.target);
      }
    };
    document.addEventListener("click", this.handleQuizOptionClick);
    this.cleanupFunctions.push(() => {
      document.removeEventListener("click", this.handleQuizOptionClick);
    });

    // Chat input
    const chatInput = document.getElementById("chatInput");
    if (chatInput) {
      this.handleChatKeypress = (e) => {
        if (e.key === "Enter" && !e.shiftKey) {
          e.preventDefault();
          this.sendChatMessage();
        }
      };
      chatInput.addEventListener("keypress", this.handleChatKeypress);
      this.cleanupFunctions.push(() => {
        chatInput.removeEventListener("keypress", this.handleChatKeypress);
      });
    }
  }

  // Cleanup event listeners
  cleanup() {
    if (this.cleanupFunctions) {
      this.cleanupFunctions.forEach((cleanup) => cleanup());
      this.cleanupFunctions = [];
    }
  }

  // Load initial data
  async loadData() {
    try {
      const state = store.getState();
      const userId = state.user?.id || 1;

      // Load files for selection
      const filesResult = await filesAPI.getFiles(userId);
      if (filesResult.success) {
        this.files = filesResult.data || [];
      }

      // Load study guides
      const guidesResult = await analysisAPI.getStudyGuides(userId);
      if (guidesResult.success) {
        this.studyGuides = guidesResult.data || [];
      }

      // Load quizzes
      const quizzesResult = await analysisAPI.getQuizzes(userId);
      if (quizzesResult.success) {
        this.quizzes = quizzesResult.data || [];
      }

      // Load conversations
      const conversationsResult = await analysisAPI.getConversations(userId);
      if (conversationsResult.success) {
        this.conversations = conversationsResult.data || [];
      }
    } catch (error) {
      logger.error("Error loading study tools data:", error);
    }
  }

  // Switch between study tool views
  switchView(viewName) {
    this.currentView = viewName;
    this.updateViewDisplay();
    this.loadViewContent();
  }

  // Update view display
  updateViewDisplay() {
    // Update active tab
    const tabs = document.querySelectorAll("[data-study-view]");
    if (tabs && tabs.length > 0) {
      tabs.forEach((tab) => {
        if (tab) {
          tab.classList.remove("active");
          if (tab.dataset.studyView === this.currentView) {
            tab.classList.add("active");
          }
        }
      });
    }

    // Show/hide view containers
    const views = ["generatorView", "quizView", "chatView"];
    views.forEach((viewId) => {
      const view = document.getElementById(viewId);
      if (view) {
        view.style.display =
          viewId === `${this.currentView}View` ? "block" : "none";
      }
    });
  }

  // Load view content
  async loadViewContent() {
    switch (this.currentView) {
      case "generator":
        this.renderGeneratorView();
        break;
      case "quiz":
        this.renderQuizView();
        break;
      case "chat":
        this.renderChatView();
        break;
    }
  }

  // Render generator view
  renderGeneratorView() {
    const generatorView = document.getElementById("generatorView");
    if (!generatorView) return;

    const state = store.getState();
    const isGuest = state.isGuest;

    generatorView.innerHTML = `
      <div class="row">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">
                <i class="bi bi-magic me-2"></i>AI Study Tools
              </h5>
            </div>
            <div class="card-body">
              ${isGuest ? this.renderGuestNotice() : ""}
              
              <div class="row">
                <div class="col-md-6">
                  <div class="study-tool-card">
                    <div class="study-tool-header">
                      <i class="bi bi-book study-tool-icon"></i>
                      <h6>Study Guide Generator</h6>
                    </div>
                    <div class="study-tool-body">
                      <p class="text-muted">Generate comprehensive study guides from your uploaded materials.</p>
                      <div class="study-tool-actions">
                        <button class="btn btn-primary" data-study-action="generate-guide">
                          <i class="bi bi-plus-circle me-2"></i>Generate Guide
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
                
                <div class="col-md-6">
                  <div class="study-tool-card">
                    <div class="study-tool-header">
                      <i class="bi bi-question-circle study-tool-icon"></i>
                      <h6>Quiz Generator</h6>
                    </div>
                    <div class="study-tool-body">
                      <p class="text-muted">Create interactive quizzes to test your knowledge.</p>
                      <div class="study-tool-actions">
                        <button class="btn btn-success" data-study-action="generate-quiz">
                          <i class="bi bi-plus-circle me-2"></i>Generate Quiz
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              ${this.renderRecentContent()}
            </div>
          </div>
        </div>
      </div>
    `;
  }

  // Render guest notice
  renderGuestNotice() {
    return `
      <div class="alert alert-info d-flex align-items-center mb-4">
        <i class="bi bi-info-circle me-2"></i>
        <div>
          <strong>Guest Mode:</strong> You're using the limited AI model. 
          <a href="#" class="alert-link" data-study-action="show-auth-modal">Create an account</a> for access to the full AI model.
        </div>
      </div>
    `;
  }

  // Render recent content
  renderRecentContent() {
    const recentGuides = this.studyGuides.slice(0, 3);
    const recentQuizzes = this.quizzes.slice(0, 3);

    return `
      <div class="row mt-4">
        <div class="col-md-6">
          <h6 class="text-primary">
            <i class="bi bi-clock me-2"></i>Recent Study Guides
          </h6>
          ${
            recentGuides.length === 0
              ? this.renderEmptyState("study guides")
              : this.renderStudyGuidesList(recentGuides)
          }
        </div>
        
        <div class="col-md-6">
          <h6 class="text-success">
            <i class="bi bi-clock me-2"></i>Recent Quizzes
          </h6>
          ${
            recentQuizzes.length === 0
              ? this.renderEmptyState("quizzes")
              : this.renderQuizzesList(recentQuizzes)
          }
        </div>
      </div>
    `;
  }

  // Render empty state
  renderEmptyState(type) {
    return `
      <div class="empty-state-small text-center py-3">
        <i class="bi bi-inbox text-muted"></i>
        <p class="text-muted mb-0">No ${type} yet</p>
      </div>
    `;
  }

  // Render study guides list
  renderStudyGuidesList(guides) {
    return `
      <div class="content-list">
        ${guides.map((guide) => this.renderStudyGuideItem(guide)).join("")}
      </div>
    `;
  }

  // Render study guide item
  renderStudyGuideItem(guide) {
    return `
      <div class="content-item">
        <div class="content-header">
          <h6 class="content-title">${
            guide.title || "Untitled Study Guide"
          }</h6>
          <span class="content-date">${CONFIG.formatDate(
            guide.createdAt
          )}</span>
        </div>
        <div class="content-body">
          <p class="content-preview">${CONFIG.truncateText(
            guide.content || "",
            100
          )}</p>
          <div class="content-actions">
            <button class="btn btn-sm btn-outline-primary" data-study-action="view-guide" data-guide-id="${
              guide.id
            }">
              <i class="bi bi-eye me-1"></i>View
            </button>
            <button class="btn btn-sm btn-outline-success" data-study-action="generate-quiz-from-guide" data-guide-id="${
              guide.id
            }">
              <i class="bi bi-question-circle me-1"></i>Quiz
            </button>
          </div>
        </div>
      </div>
    `;
  }

  // Render quizzes list
  renderQuizzesList(quizzes) {
    return `
      <div class="content-list">
        ${quizzes.map((quiz) => this.renderQuizItem(quiz)).join("")}
      </div>
    `;
  }

  // Render quiz item
  renderQuizItem(quiz) {
    const questionCount = quiz.questions
      ? JSON.parse(quiz.questions).length
      : 0;

    return `
      <div class="content-item">
        <div class="content-header">
          <h6 class="content-title">${quiz.title || "Untitled Quiz"}</h6>
          <span class="content-date">${CONFIG.formatDate(quiz.createdAt)}</span>
        </div>
        <div class="content-body">
          <p class="content-preview">
            ${questionCount} questions • ${quiz.subject || "General"} • ${
      quiz.level || "Beginner"
    }
          </p>
          <div class="content-actions">
            <button class="btn btn-sm btn-outline-primary" data-quiz-action="start-quiz" data-quiz-id="${
              quiz.id
            }">
              <i class="bi bi-play me-1"></i>Start
            </button>
            <button class="btn btn-sm btn-outline-info" data-quiz-action="view-quiz" data-quiz-id="${
              quiz.id
            }">
              <i class="bi bi-eye me-1"></i>Preview
            </button>
          </div>
        </div>
      </div>
    `;
  }

  // Render quiz view
  renderQuizView() {
    const quizView = document.getElementById("quizView");
    if (!quizView) return;

    if (this.isQuizActive && this.currentQuiz) {
      this.renderActiveQuiz();
    } else {
      this.renderQuizList();
    }
  }

  // Render quiz list
  renderQuizList() {
    const quizView = document.getElementById("quizView");
    if (!quizView) return;

    quizView.innerHTML = `
      <div class="row">
        <div class="col-12">
          <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
              <h5 class="card-title mb-0">
                <i class="bi bi-question-circle me-2"></i>Available Quizzes
              </h5>
              <button class="btn btn-primary" data-study-action="generate-quiz">
                <i class="bi bi-plus me-2"></i>Generate New Quiz
              </button>
            </div>
            <div class="card-body">
              ${
                this.quizzes.length === 0
                  ? this.renderEmptyQuizState()
                  : this.renderQuizzesGrid()
              }
            </div>
          </div>
        </div>
      </div>
    `;
  }

  // Render empty quiz state
  renderEmptyQuizState() {
    return `
      <div class="empty-state text-center py-5">
        <i class="bi bi-question-circle display-1 text-muted"></i>
        <h4 class="mt-3">No Quizzes Yet</h4>
        <p class="text-muted">Generate your first quiz to test your knowledge!</p>
        <button class="btn btn-primary" data-study-action="generate-quiz">
          <i class="bi bi-plus me-2"></i>Generate Quiz
        </button>
      </div>
    `;
  }

  // Render quizzes grid
  renderQuizzesGrid() {
    return `
      <div class="row">
        ${this.quizzes.map((quiz) => this.renderQuizCard(quiz)).join("")}
      </div>
    `;
  }

  // Render quiz card
  renderQuizCard(quiz) {
    const questionCount = quiz.questions
      ? JSON.parse(quiz.questions).length
      : 0;
    const subjectColor = CONFIG.getSubjectColor(quiz.subject || "General");

    return `
      <div class="col-md-6 col-lg-4 mb-3">
        <div class="quiz-card">
          <div class="quiz-card-header" style="background-color: ${subjectColor}20; border-left: 4px solid ${subjectColor};">
            <h6 class="quiz-card-title">${quiz.title || "Untitled Quiz"}</h6>
            <span class="quiz-card-badge" style="background-color: ${subjectColor};">
              ${quiz.subject || "General"}
            </span>
          </div>
          <div class="quiz-card-body">
            <div class="quiz-card-meta">
              <span class="quiz-meta-item">
                <i class="bi bi-question-circle me-1"></i>${questionCount} questions
              </span>
              <span class="quiz-meta-item">
                <i class="bi bi-graph-up me-1"></i>${quiz.level || "Beginner"}
              </span>
            </div>
            <p class="quiz-card-description">
              ${CONFIG.truncateText(
                quiz.description || "Test your knowledge with this quiz.",
                80
              )}
            </p>
            <div class="quiz-card-actions">
              <button class="btn btn-primary btn-sm" data-quiz-action="start-quiz" data-quiz-id="${
                quiz.id
              }">
                <i class="bi bi-play me-1"></i>Start Quiz
              </button>
              <button class="btn btn-outline-secondary btn-sm" data-quiz-action="view-quiz" data-quiz-id="${
                quiz.id
              }">
                <i class="bi bi-eye me-1"></i>Preview
              </button>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  // Render active quiz
  renderActiveQuiz() {
    const quizView = document.getElementById("quizView");
    if (!quizView || !this.currentQuiz) return;

    const questions = JSON.parse(this.currentQuiz.questions || "[]");
    const currentQuestionIndex = this.quizAnswers.currentQuestionIndex || 0;
    const currentQuestion = questions[currentQuestionIndex];

    if (!currentQuestion) {
      this.finishQuiz();
      return;
    }

    quizView.innerHTML = `
      <div class="row">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              <div class="quiz-progress">
                <div class="quiz-progress-info">
                  <h5 class="quiz-title">${
                    this.currentQuiz.title || "Quiz"
                  }</h5>
                  <span class="quiz-progress-text">Question ${
                    currentQuestionIndex + 1
                  } of ${questions.length}</span>
                </div>
                <div class="quiz-progress-bar">
                  <div class="progress">
                    <div class="progress-bar" style="width: ${
                      ((currentQuestionIndex + 1) / questions.length) * 100
                    }%"></div>
                  </div>
                </div>
              </div>
            </div>
            <div class="card-body">
              <div class="quiz-question">
                <div class="quiz-question-header">
                  <div class="quiz-question-number">${
                    currentQuestionIndex + 1
                  }</div>
                  <div class="quiz-question-text">${
                    currentQuestion.question
                  }</div>
                </div>
                <div class="quiz-options">
                  ${currentQuestion.options
                    .map((option, index) =>
                      this.renderQuizOption(option, index, currentQuestionIndex)
                    )
                    .join("")}
                </div>
              </div>
              
              <div class="quiz-navigation">
                <button class="btn btn-outline-secondary" data-quiz-action="previous-question" ${
                  currentQuestionIndex === 0 ? "disabled" : ""
                }>
                  <i class="bi bi-arrow-left me-2"></i>Previous
                </button>
                <button class="btn btn-primary" data-quiz-action="next-question" ${
                  !this.quizAnswers[`q${currentQuestionIndex}`]
                    ? "disabled"
                    : ""
                }>
                  ${
                    currentQuestionIndex === questions.length - 1
                      ? "Finish Quiz"
                      : "Next"
                  }
                  <i class="bi bi-arrow-right ms-2"></i>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  // Render quiz option
  renderQuizOption(option, index, questionIndex) {
    const isSelected = this.quizAnswers[`q${questionIndex}`] === index;

    return `
      <div class="quiz-option ${
        isSelected ? "selected" : ""
      }" data-quiz-option="${index}" data-question="${questionIndex}">
        <div class="quiz-option-radio"></div>
        <div class="quiz-option-text">${option}</div>
      </div>
    `;
  }

  // Render chat view
  renderChatView() {
    const chatView = document.getElementById("chatView");
    if (!chatView) return;

    const state = store.getState();
    const isGuest = state.isGuest;

    chatView.innerHTML = `
      <div class="row">
        <div class="col-lg-3">
          <div class="card">
            <div class="card-header">
              <h6 class="card-title mb-0">
                <i class="bi bi-chat-square-text me-2"></i>Conversations
              </h6>
            </div>
            <div class="card-body p-2">
              ${this.renderConversationList()}
            </div>
          </div>
        </div>
        <div class="col-lg-9">
          <div class="card chat-container">
            <div class="card-header d-flex justify-content-between align-items-center">
              <h5 class="card-title mb-0">
                <i class="bi bi-chat-dots me-2"></i>AI Study Assistant
              </h5>
              <div class="chat-model-indicator">
                <span class="badge ${isGuest ? "bg-warning" : "bg-success"}">
                  ${isGuest ? "GPT-4o Mini" : "GPT-4o"}
                </span>
              </div>
            </div>
            <div class="card-body p-0">
              <div class="chat-messages" id="chatMessages">
                ${this.renderChatMessages()}
              </div>
              <div class="chat-input">
                <textarea 
                  class="chat-input-field" 
                  id="chatInput" 
                  placeholder="Ask me anything about your study materials..."
                  rows="2"
                ></textarea>
                <button class="chat-send-btn" data-chat-action="send-message">
                  <i class="bi bi-send"></i>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  // Render chat messages
  renderChatMessages() {
    if (this.conversations.length === 0) {
      return `
        <div class="chat-welcome">
          <div class="chat-message assistant">
            <div class="chat-message-avatar">
              <i class="bi bi-robot"></i>
            </div>
            <div class="chat-message-content">
              <div class="chat-message-text">
                Hello! I'm your AI study assistant. I can help you understand your study materials, answer questions, and provide explanations. What would you like to know?
              </div>
            </div>
          </div>
        </div>
      `;
    }

    return this.conversations
      .map((conversation) => this.renderChatMessage(conversation))
      .join("");
  }

  // Render chat message
  renderChatMessage(conversation) {
    return `
      <div class="chat-message user">
        <div class="chat-message-avatar">
          <i class="bi bi-person"></i>
        </div>
        <div class="chat-message-content">
          <div class="chat-message-text">${conversation.userMessage}</div>
        </div>
      </div>
      <div class="chat-message assistant">
        <div class="chat-message-avatar">
          <i class="bi bi-robot"></i>
        </div>
        <div class="chat-message-content">
          <div class="chat-message-text">${conversation.aiResponse}</div>
        </div>
      </div>
    `;
  }

  // Handle study actions
  async handleStudyAction(button) {
    const action = button.dataset.studyAction;

    switch (action) {
      case "generate-guide":
        this.showGenerateGuideModal();
        break;
      case "generate-quiz":
        this.showGenerateQuizModal();
        break;
      case "view-guide":
        const guideId = parseInt(button.dataset.guideId);
        this.viewStudyGuide(guideId);
        break;
      case "generate-quiz-from-guide":
        const guideIdForQuiz = parseInt(button.dataset.guideId);
        this.generateQuizFromGuide(guideIdForQuiz);
        break;
      case "show-auth-modal":
        window.app?.showAuthModal();
        break;
      case "new-conversation":
        this.createNewConversation();
        break;
    }
  }

  // Handle quiz actions
  async handleQuizAction(button) {
    const action = button.dataset.quizAction;
    const quizId = parseInt(button.dataset.quizId);

    switch (action) {
      case "start-quiz":
        await this.startQuiz(quizId);
        break;
      case "view-quiz":
        this.viewQuiz(quizId);
        break;
      case "next-question":
        this.nextQuestion();
        break;
      case "previous-question":
        this.previousQuestion();
        break;
    }
  }

  // Handle chat actions
  async handleChatAction(button) {
    const action = button.dataset.chatAction;

    switch (action) {
      case "send-message":
        await this.sendChatMessage();
        break;
    }
  }

  // Select quiz option
  selectQuizOption(optionElement) {
    const questionIndex = parseInt(optionElement.dataset.question);
    const optionIndex = parseInt(optionElement.dataset.quizOption);

    // Update selection
    this.quizAnswers[`q${questionIndex}`] = optionIndex;

    // Update UI
    const questionContainer = optionElement.closest(".quiz-options");
    questionContainer.querySelectorAll(".quiz-option").forEach((option) => {
      option.classList.remove("selected");
    });
    optionElement.classList.add("selected");

    // Enable next button
    const nextButton = document.querySelector(
      '[data-quiz-action="next-question"]'
    );
    if (nextButton) {
      nextButton.disabled = false;
    }
  }

  // Show generate guide modal
  showGenerateGuideModal() {
    const state = store.getState();
    if (state.isGuest && this.files.length === 0) {
      notificationManager.warning(
        "Please upload some files first to generate study guides."
      );
      return;
    }

    // This would open a modal for study guide generation
    // For now, we'll generate directly
    this.generateStudyGuide();
  }

  // Show generate quiz modal
  showGenerateQuizModal() {
    const state = store.getState();
    if (state.isGuest && this.files.length === 0) {
      notificationManager.warning(
        "Please upload some files first to generate quizzes."
      );
      return;
    }

    // This would open a modal for quiz generation
    // For now, we'll generate directly
    this.generateQuiz();
  }

  // Generate study guide
  async generateStudyGuide() {
    if (this.isGenerating) return;

    this.isGenerating = true;
    notificationManager.info("Generating study guide...");

    try {
      const state = store.getState();
      const result = await analysisAPI.generateStudyGuide({
        userId: state.user?.id || 1,
        prompt: "Create a comprehensive study guide from my uploaded materials",
        fileIds: this.files.map((f) => f.id),
      });

      if (result.success) {
        notificationManager.success("Study guide generated successfully!");
        await this.loadData();
        this.renderGeneratorView();
      } else {
        throw new Error(result.message || "Failed to generate study guide");
      }
    } catch (error) {
      logger.error("Error generating study guide:", error);
      notificationManager.error(
        `Failed to generate study guide: ${error.message}`
      );
    } finally {
      this.isGenerating = false;
    }
  }

  // Generate quiz
  async generateQuiz() {
    if (this.isGenerating) return;

    this.isGenerating = true;
    notificationManager.info("Generating quiz...");

    try {
      const state = store.getState();
      const result = await analysisAPI.generateQuiz({
        userId: state.user?.id || 1,
        prompt: "Create a quiz from my uploaded materials",
        fileIds: this.files.map((f) => f.id),
      });

      if (result.success) {
        notificationManager.success("Quiz generated successfully!");
        await this.loadData();
        this.renderGeneratorView();
      } else {
        throw new Error(result.message || "Failed to generate quiz");
      }
    } catch (error) {
      logger.error("Error generating quiz:", error);
      notificationManager.error(`Failed to generate quiz: ${error.message}`);
    } finally {
      this.isGenerating = false;
    }
  }

  // Generate quiz from guide
  async generateQuizFromGuide(guideId) {
    if (this.isGenerating) return;

    this.isGenerating = true;
    notificationManager.info("Generating quiz from study guide...");

    try {
      const state = store.getState();
      const result = await analysisAPI.generateQuiz({
        userId: state.user?.id || 1,
        prompt: "Create a quiz based on this study guide",
        studyGuideIds: [guideId],
      });

      if (result.success) {
        notificationManager.success("Quiz generated successfully!");
        await this.loadData();
        this.renderGeneratorView();
      } else {
        throw new Error(result.message || "Failed to generate quiz");
      }
    } catch (error) {
      logger.error("Error generating quiz from guide:", error);
      notificationManager.error(`Failed to generate quiz: ${error.message}`);
    } finally {
      this.isGenerating = false;
    }
  }

  // View study guide
  viewStudyGuide(guideId) {
    const guide = this.studyGuides.find((g) => g.id === guideId);
    if (!guide) return;

    // This would open a modal or navigate to study guide view
    notificationManager.info(`Viewing study guide: ${guide.title}`);
  }

  // View quiz
  viewQuiz(quizId) {
    const quiz = this.quizzes.find((q) => q.id === quizId);
    if (!quiz) return;

    // This would open a modal or navigate to quiz view
    notificationManager.info(`Viewing quiz: ${quiz.title}`);
  }

  // Start quiz
  async startQuiz(quizId) {
    try {
      const quiz = this.quizzes.find((q) => q.id === quizId);
      if (!quiz) {
        notificationManager.error("Quiz not found");
        return;
      }

      this.currentQuiz = quiz;
      this.quizAnswers = { currentQuestionIndex: 0 };
      this.isQuizActive = true;
      this.quizResults = null;

      this.renderQuizView();
      notificationManager.success("Quiz started!");
    } catch (error) {
      logger.error("Error starting quiz:", error);
      notificationManager.error("Failed to start quiz");
    }
  }

  // Next question
  nextQuestion() {
    const questions = JSON.parse(this.currentQuiz.questions || "[]");
    const currentIndex = this.quizAnswers.currentQuestionIndex || 0;

    if (currentIndex < questions.length - 1) {
      this.quizAnswers.currentQuestionIndex = currentIndex + 1;
      this.renderActiveQuiz();
    } else {
      this.finishQuiz();
    }
  }

  // Previous question
  previousQuestion() {
    const currentIndex = this.quizAnswers.currentQuestionIndex || 0;
    if (currentIndex > 0) {
      this.quizAnswers.currentQuestionIndex = currentIndex - 1;
      this.renderActiveQuiz();
    }
  }

  // Finish quiz
  async finishQuiz() {
    try {
      const questions = JSON.parse(this.currentQuiz.questions || "[]");
      let correctAnswers = 0;

      // Calculate score
      questions.forEach((question, index) => {
        const userAnswer = this.quizAnswers[`q${index}`];
        if (userAnswer === question.correctAnswer) {
          correctAnswers++;
        }
      });

      const score = Math.round((correctAnswers / questions.length) * 100);
      this.quizResults = {
        score,
        correctAnswers,
        totalQuestions: questions.length,
        answers: this.quizAnswers,
      };

      // Submit quiz attempt
      const state = store.getState();
      await analysisAPI.submitQuizAttempt({
        quizId: this.currentQuiz.id,
        userId: state.user?.id || 1,
        answers: this.quizAnswers,
        score: score,
      });

      this.renderQuizResults();
      notificationManager.success(`Quiz completed! Score: ${score}%`);
    } catch (error) {
      logger.error("Error finishing quiz:", error);
      notificationManager.error("Failed to submit quiz results");
    }
  }

  // Render quiz results
  renderQuizResults() {
    const quizView = document.getElementById("quizView");
    if (!quizView || !this.quizResults) return;

    const { score, correctAnswers, totalQuestions } = this.quizResults;
    const isPassing = score >= 70;

    quizView.innerHTML = `
      <div class="row">
        <div class="col-12">
          <div class="card">
            <div class="card-header text-center">
              <h5 class="card-title mb-0">
                <i class="bi bi-trophy me-2"></i>Quiz Results
              </h5>
            </div>
            <div class="card-body text-center">
              <div class="quiz-results">
                <div class="quiz-score ${
                  isPassing ? "score-passing" : "score-failing"
                }">
                  <div class="score-circle">
                    <span class="score-number">${score}%</span>
                  </div>
                  <h4 class="score-label">${
                    isPassing ? "Great Job!" : "Keep Studying!"
                  }</h4>
                </div>
                
                <div class="quiz-stats">
                  <div class="stat-item">
                    <span class="stat-value">${correctAnswers}</span>
                    <span class="stat-label">Correct</span>
                  </div>
                  <div class="stat-item">
                    <span class="stat-value">${
                      totalQuestions - correctAnswers
                    }</span>
                    <span class="stat-label">Incorrect</span>
                  </div>
                  <div class="stat-item">
                    <span class="stat-value">${totalQuestions}</span>
                    <span class="stat-label">Total</span>
                  </div>
                </div>
                
                <div class="quiz-actions">
                  <button class="btn btn-primary" data-quiz-action="retake-quiz" data-quiz-id="${
                    this.currentQuiz.id
                  }">
                    <i class="bi bi-arrow-clockwise me-2"></i>Retake Quiz
                  </button>
                  <button class="btn btn-outline-secondary" data-quiz-action="view-answers" data-quiz-id="${
                    this.currentQuiz.id
                  }">
                    <i class="bi bi-eye me-2"></i>View Answers
                  </button>
                  <button class="btn btn-outline-primary" data-quiz-action="back-to-quizzes">
                    <i class="bi bi-arrow-left me-2"></i>Back to Quizzes
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    `;

    this.isQuizActive = false;
  }

  // Send chat message
  async sendChatMessage() {
    const chatInput = document.getElementById("chatInput");
    const message = chatInput?.value.trim();

    if (!message) return;

    // Add user message to chat
    this.addChatMessage("user", message);
    chatInput.value = "";

    try {
      const state = store.getState();
      const result = await analysisAPI.chat({
        userId: state.user?.id || 1,
        message: message,
        fileIds: this.files.map((f) => f.id),
      });

      if (result.success) {
        this.addChatMessage("assistant", result.data.response);
      } else {
        throw new Error(result.message || "Failed to get response");
      }
    } catch (error) {
      logger.error("Error sending chat message:", error);
      this.addChatMessage(
        "assistant",
        "Sorry, I encountered an error. Please try again."
      );
    }
  }

  // Render chat messages
  renderChatMessages() {
    if (
      !this.currentConversation ||
      this.currentConversation.messages.length === 0
    ) {
      return `
        <div class="chat-welcome">
          <div class="text-center py-5">
            <i class="bi bi-chat-dots display-1 text-muted"></i>
            <h5 class="mt-3">Start a conversation</h5>
            <p class="text-muted">Ask me anything about your study materials!</p>
          </div>
        </div>
      `;
    }

    return this.currentConversation.messages
      .map(
        (message) => `
      <div class="chat-message ${message.sender}">
        <div class="chat-message-avatar">
          <i class="bi ${
            message.sender === "user" ? "bi-person" : "bi-robot"
          }"></i>
        </div>
        <div class="chat-message-content">
          <div class="chat-message-text">${sanitizeText(message.content)}</div>
          <div class="chat-message-time">${CONFIG.formatDate(
            message.timestamp
          )}</div>
        </div>
      </div>
    `
      )
      .join("");
  }

  // Add chat message
  addChatMessage(sender, message) {
    // Add to conversation
    this.addMessageToConversation(message, sender);

    // Update UI
    const chatMessages = document.getElementById("chatMessages");
    if (!chatMessages) return;

    // Clear welcome message if present
    const welcome = chatMessages.querySelector(".chat-welcome");
    if (welcome) {
      welcome.remove();
    }

    const messageElement = document.createElement("div");
    messageElement.className = `chat-message ${sender}`;

    const avatar = sender === "user" ? "bi-person" : "bi-robot";

    messageElement.innerHTML = `
      <div class="chat-message-avatar">
        <i class="bi ${avatar}"></i>
      </div>
      <div class="chat-message-content">
        <div class="chat-message-text">${sanitizeText(message)}</div>
        <div class="chat-message-time">${CONFIG.formatDate(
          new Date().toISOString()
        )}</div>
      </div>
    `;

    chatMessages.appendChild(messageElement);
    chatMessages.scrollTop = chatMessages.scrollHeight;
  }

  // Load conversation history
  async loadConversations() {
    try {
      const state = store.getState();

      if (state.isGuest) {
        // Load from localStorage for guests
        const savedConversations = await storageService.getItem(
          "study_ai_conversations"
        );
        if (savedConversations) {
          this.conversations = JSON.parse(savedConversations);
        }
      } else {
        // Load from server for authenticated users
        // This would be implemented when the backend supports conversation storage
        this.conversations = [];
      }
    } catch (error) {
      logger.error("Error loading conversations:", error);
      this.conversations = [];
    }
  }

  // Save conversation history
  async saveConversations() {
    try {
      const state = store.getState();

      if (state.isGuest) {
        // Save to localStorage for guests
        await storageService.setItem(
          "study_ai_conversations",
          JSON.stringify(this.conversations)
        );
      } else {
        // Save to server for authenticated users
        // This would be implemented when the backend supports conversation storage
      }
    } catch (error) {
      logger.error("Error saving conversations:", error);
    }
  }

  // Create new conversation
  createNewConversation() {
    const conversation = {
      id: Date.now(),
      title: "New Conversation",
      messages: [],
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };

    this.conversations.unshift(conversation);
    this.currentConversation = conversation;
    this.saveConversations();
    this.renderChatView();
  }

  // Load conversation
  loadConversation(conversationId) {
    const conversation = this.conversations.find(
      (c) => c.id === conversationId
    );
    if (conversation) {
      this.currentConversation = conversation;
      this.renderChatView();
    }
  }

  // Save current conversation
  async saveCurrentConversation() {
    if (!this.currentConversation) return;

    this.currentConversation.updatedAt = new Date().toISOString();
    await this.saveConversations();
  }

  // Add message to current conversation
  addMessageToConversation(message, sender) {
    if (!this.currentConversation) {
      this.createNewConversation();
    }

    const messageObj = {
      id: Date.now(),
      content: message,
      sender: sender,
      timestamp: new Date().toISOString(),
    };

    this.currentConversation.messages.push(messageObj);
    this.currentConversation.updatedAt = new Date().toISOString();

    // Update conversation title if it's the first user message
    if (
      sender === "user" &&
      this.currentConversation.messages.filter((m) => m.sender === "user")
        .length === 1
    ) {
      this.currentConversation.title =
        message.length > 50 ? message.substring(0, 50) + "..." : message;
    }

    this.saveCurrentConversation();
  }

  // Render conversation list
  renderConversationList() {
    if (this.conversations.length === 0) {
      return `
        <div class="conversation-list-empty">
          <p class="text-muted text-center">No conversations yet</p>
          <button class="btn btn-primary btn-sm" data-study-action="new-conversation">
            Start New Conversation
          </button>
        </div>
      `;
    }

    return `
      <div class="conversation-list">
        <div class="conversation-list-header">
          <h6>Recent Conversations</h6>
          <button class="btn btn-sm btn-outline-primary" data-study-action="new-conversation">
            <i class="bi bi-plus"></i>
          </button>
        </div>
        <div class="conversation-items">
          ${this.conversations
            .map(
              (conv) => `
            <div class="conversation-item ${
              this.currentConversation?.id === conv.id ? "active" : ""
            }" 
                 data-conversation-id="${conv.id}">
              <div class="conversation-title">${sanitizeText(conv.title)}</div>
              <div class="conversation-meta">
                <small class="text-muted">${CONFIG.formatDate(
                  conv.updatedAt
                )}</small>
                <span class="message-count">${
                  conv.messages.length
                } messages</span>
              </div>
            </div>
          `
            )
            .join("")}
        </div>
      </div>
    `;
  }

  // Handle conversation actions
  handleConversationAction(button) {
    const action = button.dataset.studyAction;
    const conversationId = parseInt(button.dataset.conversationId);

    switch (action) {
      case "new-conversation":
        this.createNewConversation();
        break;
      case "load-conversation":
        this.loadConversation(conversationId);
        break;
    }
  }
}

// Create global study tools instance
const studyTools = new StudyTools();

// Export for global access
window.studyTools = studyTools;

export { studyTools };
