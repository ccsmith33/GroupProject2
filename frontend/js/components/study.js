// Study page component - Core functionality for file management, study guides, and quizzes
import { analysisAPI } from "../api/analysis.js";
import { filesAPI } from "../api/files.js";
import { notificationManager } from "../utils/notifications.js";
import {
  MarkdownParser,
  createDownloadLink,
  formatFileSize,
  getFileIcon,
} from "../utils/markdown.js";
import { authState } from "../state/auth-state.js";
import { store } from "../state/store.js";
import { logger } from "../utils/logger.js";

class Study {
  constructor(container) {
    this.container = container;
    this.currentView = "dashboard"; // dashboard, files, guides, quizzes
    this.files = [];
    this.studyGuides = [];
    this.quizzes = [];
    this.subjectGroups = [];
    this.groupedFiles = {};
    this.activeQuiz = null;
    this.activeGuide = null;
    this.quizAnswers = {};
    this.currentQuestionIndex = 0;
    this.isGenerating = false;
    this.init();
  }

  init() {
    this.render();
    this.setupEventListeners();
    this.loadData();
  }

  render() {
    this.container.innerHTML = `
      <div class="study-container">
        <div class="study-layout">
          <!-- Sidebar Navigation -->
          <div class="study-sidebar">
            <div class="study-sidebar-header">
              <h3 class="study-sidebar-title">Study Aids</h3>
            </div>
            <nav class="study-sidebar-nav">
              <button class="study-sidebar-tab ${
                this.currentView === "dashboard" ? "active" : ""
              }" data-view="dashboard">
                <i class="bi bi-grid-3x3-gap"></i>
                <span>Dashboard</span>
              </button>
              <button class="study-sidebar-tab ${
                this.currentView === "files" ? "active" : ""
              }" data-view="files">
                <i class="bi bi-folder2-open"></i>
                <span>My Files</span>
              </button>
              <button class="study-sidebar-tab ${
                this.currentView === "guides" ? "active" : ""
              }" data-view="guides">
                <i class="bi bi-book"></i>
                <span>Study Guides</span>
              </button>
              <button class="study-sidebar-tab ${
                this.currentView === "quizzes" ? "active" : ""
              }" data-view="quizzes">
                <i class="bi bi-question-circle"></i>
                <span>Quizzes</span>
              </button>
            </nav>
          </div>

          <!-- Main Content Area -->
          <div class="study-content">
            <div class="study-content-header">
              <h2 class="study-content-title" id="study-content-title">My Files</h2>
              <div class="study-content-actions" id="study-content-actions">
                <!-- Actions will be populated based on current view -->
              </div>
            </div>
            <div class="study-content-body" id="study-content-body">
              <!-- Content will be populated based on current view -->
            </div>
          </div>
        </div>
      </div>
    `;
  }

  setupEventListeners() {
    // Sidebar tab switching
    this.container.addEventListener("click", (e) => {
      if (e.target.closest(".study-sidebar-tab")) {
        const tab = e.target.closest(".study-sidebar-tab");
        const view = tab.dataset.view;
        this.switchView(view);
      }
    });

    // Content area event delegation
    this.container.addEventListener("click", (e) => {
      // File actions
      if (e.target.matches(".file-download-btn")) {
        const fileId = parseInt(e.target.dataset.fileId);
        this.handleFileDownload(fileId);
      }
      if (e.target.matches(".file-delete-btn")) {
        const fileId = parseInt(e.target.dataset.fileId);
        this.handleFileDelete(fileId);
      }

      // Study guide actions
      if (e.target.matches(".guide-view-btn")) {
        const guideId = parseInt(e.target.dataset.guideId);
        this.viewStudyGuide(guideId);
      }
      if (e.target.matches(".guide-back-btn")) {
        this.backToGuides();
      }
      if (e.target.matches(".generate-guide-btn")) {
        this.showGenerateGuideModal();
      }
      if (e.target.matches(".refresh-files-btn")) {
        this.loadData();
      }
      if (e.target.matches(".refresh-dashboard-btn")) {
        this.loadData();
      }

      // Chat functionality
      if (e.target.matches(".send-chat-btn")) {
        this.handleChatSend();
      }

      // Subject group collapse/expand
      if (e.target.closest(".subject-group-header")) {
        this.toggleSubjectGroup(e.target.closest(".subject-group-header"));
      }

      // Subject management
      if (e.target.matches(".create-subject-btn")) {
        this.showCreateSubjectModal();
      }
      if (e.target.matches(".edit-subject-btn")) {
        const groupId = parseInt(e.target.dataset.groupId);
        this.showEditSubjectModal(groupId);
      }
      if (e.target.matches(".delete-subject-btn")) {
        const groupId = parseInt(e.target.dataset.groupId);
        this.handleDeleteSubject(groupId);
      }

      // File reassignment
      if (e.target.matches(".reassign-file-btn")) {
        const fileId = parseInt(e.target.dataset.fileId);
        this.showReassignFileModal(fileId);
      }

      // Quick actions
      if (e.target.closest(".quick-action-card")) {
        const action = e.target.closest(".quick-action-card").dataset.action;
        this.handleQuickAction(action);
      }

      // Subject-based generation
      if (e.target.matches(".generate-guide-from-subject-btn")) {
        const groupId = parseInt(e.target.dataset.groupId);
        this.generateGuideFromSubject(groupId);
      }
      if (e.target.matches(".generate-quiz-from-subject-btn")) {
        const groupId = parseInt(e.target.dataset.groupId);
        this.generateQuizFromSubject(groupId);
      }

      // Auto-detected subject actions
      if (e.target.matches(".create-group-from-subject-btn")) {
        const subjectName = e.target.dataset.subject;
        this.createGroupFromSubject(subjectName);
      }
      if (e.target.matches(".generate-guide-from-auto-subject-btn")) {
        const subjectName = e.target.dataset.subject;
        this.generateGuideFromAutoSubject(subjectName);
      }
      if (e.target.matches(".generate-quiz-from-auto-subject-btn")) {
        const subjectName = e.target.dataset.subject;
        this.generateQuizFromAutoSubject(subjectName);
      }

      // Grouped subject actions
      if (e.target.matches(".generate-guide-from-grouped-subject-btn")) {
        const subjectName = e.target.dataset.subject;
        this.generateGuideFromGroupedSubject(subjectName);
      }
      if (e.target.matches(".generate-quiz-from-grouped-subject-btn")) {
        const subjectName = e.target.dataset.subject;
        this.generateQuizFromGroupedSubject(subjectName);
      }

      // Quiz actions
      if (e.target.matches(".quiz-start-btn")) {
        const quizId = parseInt(e.target.dataset.quizId);
        this.startQuiz(quizId);
      }
      if (e.target.matches(".generate-quiz-btn")) {
        this.showGenerateQuizModal();
      }
      if (e.target.matches(".quiz-option-btn")) {
        const optionIndex = parseInt(e.target.dataset.optionIndex);
        this.handleQuizAnswer(optionIndex);
      }
      if (e.target.matches(".quiz-next-btn")) {
        this.nextQuestion();
      }
      if (e.target.matches(".quiz-prev-btn")) {
        this.prevQuestion();
      }
      if (e.target.matches(".quiz-submit-btn")) {
        this.submitQuiz();
      }
      if (e.target.matches(".quiz-back-btn")) {
        this.backToQuizzes();
      }
    });
  }

  async loadData() {
    try {
      const userId = store.getState().user?.id || 1;
      console.log("Loading data for userId:", userId);
      console.log("Store state:", store.getState());

      // Load files
      console.log("Loading files...");
      const filesResult = await filesAPI.getFiles(userId);
      console.log("Files result:", filesResult);
      // Files API returns {files: Array, count: number} directly
      if (filesResult && filesResult.files) {
        this.files = filesResult.files || [];
        console.log("Files loaded:", this.files);
      }

      // Load study guides
      try {
        console.log("Loading study guides...");
        const guidesResult = await analysisAPI.getStudyGuides(userId);
        console.log("Study guides result:", guidesResult);
        if (guidesResult.success) {
          this.studyGuides = guidesResult.data || [];
          console.log("Study guides loaded:", this.studyGuides);
        }
      } catch (error) {
        console.warn("No study guides found or error loading guides:", error);
        logger.warn("No study guides found or error loading guides:", error);
        this.studyGuides = [];
      }

      // Load quizzes
      try {
        console.log("Loading quizzes...");
        const quizzesResult = await analysisAPI.getQuizzes(userId);
        console.log("Quizzes result:", quizzesResult);
        if (quizzesResult.success) {
          this.quizzes = quizzesResult.data || [];
          console.log("Quizzes loaded:", this.quizzes);
        }
      } catch (error) {
        console.warn("No quizzes found or error loading quizzes:", error);
        logger.warn("No quizzes found or error loading quizzes:", error);
        this.quizzes = [];
      }

      // Load grouped files
      try {
        console.log("Loading grouped files...");
        const groupedResult = await filesAPI.getGroupedFiles(userId);
        console.log("Grouped files result:", groupedResult);
        if (groupedResult.success) {
          this.groupedFiles = groupedResult.data || {};
          console.log("Grouped files loaded:", this.groupedFiles);
        }
      } catch (error) {
        console.warn("Error loading grouped files:", error);
        logger.warn("Error loading grouped files:", error);
        this.groupedFiles = {};
      }

      // Load subject groups
      try {
        console.log("Loading subject groups...");
        const groupsResult = await filesAPI.getSubjectGroups(userId);
        console.log("Subject groups result:", groupsResult);
        console.log("Subject groups result type:", typeof groupsResult);
        console.log("Subject groups result.success:", groupsResult.success);
        console.log("Subject groups result.data:", groupsResult.data);

        if (groupsResult.success) {
          this.subjectGroups = groupsResult.data || [];
          console.log("Subject groups loaded:", this.subjectGroups);
          console.log(
            "Subject groups loaded length:",
            this.subjectGroups.length
          );
        } else {
          console.warn("Subject groups API returned success: false");
          this.subjectGroups = [];
        }
      } catch (error) {
        console.warn("Error loading subject groups:", error);
        logger.warn("Error loading subject groups:", error);
        this.subjectGroups = [];
      }

      console.log(
        "Final data - Files:",
        this.files,
        "Guides:",
        this.studyGuides,
        "Quizzes:",
        this.quizzes,
        "Subject Groups:",
        this.subjectGroups
      );
      console.log("Files length:", this.files.length);
      this.renderCurrentView();
    } catch (error) {
      console.error("Error loading study data:", error);
      logger.error("Error loading study data:", error);
      notificationManager.error("Failed to load study data");
    }
  }

  switchView(view) {
    this.currentView = view;

    // Update active tab
    this.container.querySelectorAll(".study-sidebar-tab").forEach((tab) => {
      tab.classList.remove("active");
    });
    this.container
      .querySelector(`[data-view="${view}"]`)
      .classList.add("active");

    this.renderCurrentView();
  }

  renderCurrentView() {
    const title = this.container.querySelector("#study-content-title");
    const actions = this.container.querySelector("#study-content-actions");
    const body = this.container.querySelector("#study-content-body");

    switch (this.currentView) {
      case "dashboard":
        title.textContent = "Dashboard";
        actions.innerHTML = this.renderDashboardActions();
        body.innerHTML = this.renderDashboardView();
        break;
      case "files":
        title.textContent = "My Files";
        actions.innerHTML = this.renderFilesActions();
        body.innerHTML = this.renderFilesView();
        break;
      case "guides":
        title.textContent = "Study Guides";
        actions.innerHTML = this.renderGuidesActions();
        body.innerHTML = this.renderGuidesView();
        break;
      case "quizzes":
        title.textContent = "Quizzes";
        actions.innerHTML = this.renderQuizzesActions();
        body.innerHTML = this.renderQuizzesView();
        break;
    }
  }

  // Dashboard View
  renderDashboardActions() {
    return `
      <button class="btn btn-primary refresh-dashboard-btn">
        <i class="bi bi-arrow-clockwise"></i>
        Refresh
      </button>
    `;
  }

  renderDashboardView() {
    return `
      <div class="dashboard-layout">
        <!-- Chat Section (Always at top) -->
        <div class="dashboard-chat-section">
          <div class="chat-header">
            <h3><i class="bi bi-chat-dots"></i> Study Assistant</h3>
            <p>Ask questions about your study materials</p>
          </div>
          <div id="dashboard-chat-container" class="chat-container">
            ${this.renderChatInterface()}
          </div>
        </div>
        
        <!-- Subject Groups Section -->
        <div class="dashboard-subjects-section">
          <div class="section-header">
            <h3><i class="bi bi-folder2-open"></i> My Subjects</h3>
            <button class="btn btn-sm btn-outline-primary create-subject-btn">
              <i class="bi bi-plus-circle"></i> New Subject
            </button>
          </div>
          ${this.renderSubjectGroups()}
        </div>
        
        <!-- Quick Actions Section -->
        <div class="dashboard-actions-section">
          <h3><i class="bi bi-lightning"></i> Quick Actions</h3>
          ${this.renderQuickActions()}
        </div>
      </div>
    `;
  }

  renderChatInterface() {
    return `
      <div class="chat-messages" id="chat-messages"></div>
      <div class="chat-input-area">
        <textarea id="chat-input" class="form-control" rows="2" placeholder="Ask a question about your study materials..."></textarea>
        <button class="btn btn-primary send-chat-btn">
          <i class="bi bi-send"></i> Send
        </button>
      </div>
    `;
  }

  renderSubjectGroups() {
    console.log("Rendering subject groups:", this.subjectGroups);
    console.log("Subject groups length:", this.subjectGroups.length);
    console.log("Subject groups type:", typeof this.subjectGroups);
    console.log("Subject groups is array:", Array.isArray(this.subjectGroups));
    console.log("Grouped files:", this.groupedFiles);
    console.log("Grouped files filesBySubject:", this.groupedFiles.filesBySubject);
    console.log("Grouped files customGroups:", this.groupedFiles.customGroups);
    console.log("Grouped files ungroupedFiles:", this.groupedFiles.ungroupedFiles);
    
    // Get auto-detected subjects from files
    const autoDetectedSubjects = this.getAutoDetectedSubjects();
    console.log("Auto-detected subjects:", autoDetectedSubjects);
    
    // Also check if groupedFiles has subjects we can use
    const groupedSubjects = Object.keys(this.groupedFiles.filesBySubject || {});
    console.log("Subjects from grouped files:", groupedSubjects);

    const hasCustomGroups = this.subjectGroups.length > 0;
    const hasAutoDetectedSubjects = autoDetectedSubjects.length > 0;
    const hasGroupedSubjects = groupedSubjects.length > 0;

    if (!hasCustomGroups && !hasAutoDetectedSubjects && !hasGroupedSubjects) {
      console.log("No subject groups found, showing empty state");
      return `
        <div class="empty-state">
          <i class="bi bi-folder-x empty-state-icon"></i>
          <h4>No subjects detected yet</h4>
          <p>Upload files to automatically organize them by subject</p>
          <div style="margin-top: 1rem; padding: 1rem; background: #f8f9fa; border-radius: 0.5rem; font-size: 0.9rem;">
            <strong>Debug Info:</strong><br>
            Custom Subject Groups: ${JSON.stringify(this.subjectGroups)}<br>
            Files Count: ${this.files.length}<br>
            Auto-detected Subjects: ${JSON.stringify(autoDetectedSubjects)}<br>
            Files: ${JSON.stringify(this.files.slice(0, 2))}...
          </div>
        </div>
      `;
    }

    console.log(
      "Rendering subject groups tree with",
      this.subjectGroups.length,
      "custom groups,",
      autoDetectedSubjects.length,
      "auto-detected subjects, and",
      groupedSubjects.length,
      "grouped subjects"
    );
    return `
      <div class="subject-groups-tree">
        ${this.subjectGroups
          .map((group) => this.renderSubjectGroup(group))
          .join("")}
        ${autoDetectedSubjects
          .map((subject) => this.renderAutoDetectedSubject(subject))
          .join("")}
        ${groupedSubjects
          .map((subject) => this.renderGroupedSubject(subject))
          .join("")}
      </div>
    `;
  }

  getAutoDetectedSubjects() {
    const subjects = new Set();
    console.log("Checking files for subjects:", this.files.slice(0, 3));
    
    this.files.forEach((file) => {
      console.log("File subject fields:", {
        AutoDetectedSubject: file.AutoDetectedSubject,
        UserDefinedSubject: file.UserDefinedSubject,
        Subject: file.Subject,
        subject: file.subject,
        UserDefinedSubject: file.UserDefinedSubject,
        userDefinedSubject: file.userDefinedSubject,
        AutoDetectedSubject: file.AutoDetectedSubject,
        autoDetectedSubject: file.autoDetectedSubject
      });
      
      // Try multiple possible field names
      const subject = file.AutoDetectedSubject || 
                     file.autoDetectedSubject || 
                     file.UserDefinedSubject || 
                     file.userDefinedSubject ||
                     file.Subject || 
                     file.subject;
                     
      if (subject && subject.trim()) {
        subjects.add(subject.trim());
        console.log("Found subject:", subject);
      }
    });
    
    console.log("Final auto-detected subjects:", Array.from(subjects));
    return Array.from(subjects).sort();
  }

  renderAutoDetectedSubject(subjectName) {
    const filesInSubject = this.files.filter((f) => {
      const subject = f.AutoDetectedSubject || 
                     f.autoDetectedSubject || 
                     f.UserDefinedSubject || 
                     f.userDefinedSubject ||
                     f.Subject || 
                     f.subject;
      return subject === subjectName;
    });
    const fileCount = filesInSubject.length;

    return `
      <div class="subject-group-item auto-detected" data-subject="${subjectName}">
        <div class="subject-group-header" data-toggle="collapse">
          <i class="bi bi-chevron-right collapse-icon"></i>
          <i class="bi bi-folder2-open"></i>
          <span class="subject-name">${subjectName}</span>
          <span class="file-count">(${fileCount} files)</span>
          <div class="subject-actions">
            <button class="btn btn-sm btn-link create-group-from-subject-btn" data-subject="${subjectName}">
              <i class="bi bi-plus-circle"></i>
            </button>
          </div>
        </div>
        <div class="subject-group-files collapsed">
          ${filesInSubject
            .map((file) => this.renderFileItemCompact(file))
            .join("")}
          <div class="subject-group-actions-bar">
            <button class="btn btn-sm btn-outline-primary generate-guide-from-auto-subject-btn" data-subject="${subjectName}">
              <i class="bi bi-book"></i> Generate Study Guide
            </button>
            <button class="btn btn-sm btn-outline-primary generate-quiz-from-auto-subject-btn" data-subject="${subjectName}">
              <i class="bi bi-question-circle"></i> Generate Quiz
            </button>
          </div>
        </div>
      </div>
    `;
  }

  renderGroupedSubject(subjectName) {
    const filesInSubject = this.groupedFiles.filesBySubject[subjectName] || [];
    const fileCount = filesInSubject.length;

    return `
      <div class="subject-group-item grouped-subject" data-subject="${subjectName}">
        <div class="subject-group-header" data-toggle="collapse">
          <i class="bi bi-chevron-right collapse-icon"></i>
          <i class="bi bi-folder2-open"></i>
          <span class="subject-name">${subjectName}</span>
          <span class="file-count">(${fileCount} files)</span>
          <div class="subject-actions">
            <button class="btn btn-sm btn-link create-group-from-subject-btn" data-subject="${subjectName}">
              <i class="bi bi-plus-circle"></i>
            </button>
          </div>
        </div>
        <div class="subject-group-files collapsed">
          ${filesInSubject
            .map((file) => this.renderFileItemCompact(file))
            .join("")}
          <div class="subject-group-actions-bar">
            <button class="btn btn-sm btn-outline-primary generate-guide-from-grouped-subject-btn" data-subject="${subjectName}">
              <i class="bi bi-book"></i> Generate Study Guide
            </button>
            <button class="btn btn-sm btn-outline-primary generate-quiz-from-grouped-subject-btn" data-subject="${subjectName}">
              <i class="bi bi-question-circle"></i> Generate Quiz
            </button>
          </div>
        </div>
      </div>
    `;
  }

  renderSubjectGroup(group) {
    const filesInGroup = this.files.filter(
      (f) => f.SubjectGroupId === group.Id || f.subjectGroupId === group.id
    );
    const fileCount = filesInGroup.length;

    return `
      <div class="subject-group-item" data-group-id="${group.Id || group.id}">
        <div class="subject-group-header" data-toggle="collapse">
          <i class="bi bi-chevron-right collapse-icon"></i>
          <i class="bi bi-folder2"></i>
          <span class="subject-name">${
            group.GroupName || group.groupName
          }</span>
          <span class="file-count">(${fileCount} files)</span>
          <div class="subject-actions">
            <button class="btn btn-sm btn-link edit-subject-btn" data-group-id="${
              group.Id || group.id
            }">
              <i class="bi bi-pencil"></i>
            </button>
            <button class="btn btn-sm btn-link delete-subject-btn" data-group-id="${
              group.Id || group.id
            }">
              <i class="bi bi-trash"></i>
            </button>
          </div>
        </div>
        <div class="subject-group-files collapsed">
          ${filesInGroup
            .map((file) => this.renderFileItemCompact(file))
            .join("")}
          <div class="subject-group-actions-bar">
            <button class="btn btn-sm btn-outline-primary generate-guide-from-subject-btn" data-group-id="${
              group.Id || group.id
            }">
              <i class="bi bi-book"></i> Generate Study Guide
            </button>
            <button class="btn btn-sm btn-outline-primary generate-quiz-from-subject-btn" data-group-id="${
              group.Id || group.id
            }">
              <i class="bi bi-question-circle"></i> Generate Quiz
            </button>
          </div>
        </div>
      </div>
    `;
  }

  renderFileItemCompact(file) {
    const fileName = this.extractFileName(file);
    const fileId = file.Id || file.fileId || file.id;

    return `
      <div class="file-compact" data-file-id="${fileId}">
        <i class="bi ${getFileIcon(file.FileType || file.fileType)}"></i>
        <span class="file-compact-name">${fileName}</span>
        <button class="btn btn-sm btn-link reassign-file-btn" data-file-id="${fileId}">
          <i class="bi bi-arrow-left-right"></i>
        </button>
      </div>
    `;
  }

  renderQuickActions() {
    return `
      <div class="quick-actions-grid">
        <div class="quick-action-card quick-action-featured" data-action="analyze-all">
          <i class="bi bi-magic"></i>
          <h4>Analyze All Materials</h4>
          <p>Generate personalized study plans for all subjects</p>
        </div>
        <div class="quick-action-card" data-action="bulk-generate-guides">
          <i class="bi bi-book-fill"></i>
          <h4>Bulk Generate Guides</h4>
          <p>Create study guides for all subjects</p>
        </div>
        <div class="quick-action-card" data-action="bulk-generate-quizzes">
          <i class="bi bi-question-circle-fill"></i>
          <h4>Bulk Generate Quizzes</h4>
          <p>Create quizzes for all subjects</p>
        </div>
        <div class="quick-action-card" data-action="reorganize-files">
          <i class="bi bi-folder-symlink"></i>
          <h4>Reorganize Files</h4>
          <p>Re-detect and regroup all files</p>
        </div>
      </div>
    `;
  }

  // Files View
  renderFilesActions() {
    return `
      <button class="btn btn-primary refresh-files-btn">
        <i class="bi bi-arrow-clockwise"></i>
        Refresh
      </button>
    `;
  }

  renderFilesView() {
    if (this.files.length === 0) {
      return `
        <div class="empty-state">
          <i class="bi bi-folder2-open empty-state-icon"></i>
          <h3>No files uploaded yet</h3>
          <p>Upload some files to get started with your study materials.</p>
          <button class="btn btn-primary" onclick="window.dispatchEvent(new CustomEvent('navigateToPage', {detail: 'file-upload'}))">
            Upload Files
          </button>
        </div>
      `;
    }

    return `
      <div class="files-list">
        ${this.files.map((file) => this.renderFileItem(file)).join("")}
      </div>
    `;
  }

  renderFileItem(file) {
    console.log("Rendering file item:", file);
    console.log("Available properties:", Object.keys(file));
    console.log("File name properties check:");
    console.log("- FileName:", file.FileName);
    console.log("- fileName:", file.fileName);
    console.log("- OriginalFileName:", file.OriginalFileName);
    console.log("- originalFileName:", file.originalFileName);
    console.log("- name:", file.name);
    const iconClass = getFileIcon(file.FileType || file.fileType);
    const fileSize = formatFileSize(file.FileSize || file.fileSize);
    const uploadDate = new Date(
      file.UploadedAt || file.uploadedAt
    ).toLocaleDateString();
    // Try multiple possible property names for file name
    let fileName =
      file.FileName ||
      file.fileName ||
      file.OriginalFileName ||
      file.originalFileName ||
      file.name ||
      file.displayName ||
      file.title ||
      "Unknown File";

    // Clean up processed filename (remove prefixes like "1_1760127111_0fb1d32a_")
    if (fileName && fileName.includes("_") && fileName.split("_").length >= 3) {
      const parts = fileName.split("_");
      // Take the last part which should be the actual filename
      fileName = parts[parts.length - 1];
    }

    console.log("Final fileName result:", fileName);
    const fileId = file.Id || file.fileId || file.id;

    return `
      <div class="file-list-item">
        <div class="file-info">
          <div class="file-icon">
            <i class="bi ${iconClass}"></i>
          </div>
          <div class="file-details">
            <h4 class="file-name">${fileName}</h4>
            <p class="file-meta">${fileSize} • ${uploadDate}</p>
            ${
              file.Subject || file.subject
                ? `<p class="file-subject">Subject: ${
                    file.Subject || file.subject
                  }</p>`
                : ""
            }
            ${
              file.StudentLevel || file.studentLevel
                ? `<p class="file-level">Level: ${
                    file.StudentLevel || file.studentLevel
                  }</p>`
                : ""
            }
          </div>
        </div>
        <div class="file-actions">
          <button class="btn btn-outline-primary btn-sm file-download-btn" data-file-id="${fileId}">
            <i class="bi bi-download"></i>
            Download
          </button>
          <button class="btn btn-outline-danger btn-sm file-delete-btn" data-file-id="${fileId}">
            <i class="bi bi-trash"></i>
            Delete
          </button>
        </div>
      </div>
    `;
  }

  async handleFileDownload(fileId) {
    try {
      notificationManager.info("Downloading file...");
      const blob = await filesAPI.downloadFile(fileId);
      const file = this.files.find((f) => (f.Id || f.fileId || f.id) == fileId);
      const fileName =
        file?.FileName ||
        file?.fileName ||
        file?.OriginalFileName ||
        file?.originalFileName ||
        "file";
      createDownloadLink(blob, fileName);
      notificationManager.success("File downloaded successfully!");
    } catch (error) {
      logger.error("Error downloading file:", error);
      notificationManager.error("Failed to download file");
    }
  }

  async handleFileDelete(fileId) {
    if (
      !confirm(
        "Are you sure you want to delete this file? This action cannot be undone."
      )
    ) {
      return;
    }

    try {
      const result = await filesAPI.deleteFile(fileId);
      if (result.success) {
        this.files = this.files.filter(
          (f) => (f.Id || f.fileId || f.id) != fileId
        );
        this.renderCurrentView();
        notificationManager.success("File moved to trash");
      }
    } catch (error) {
      logger.error("Error deleting file:", error);
      notificationManager.error("Failed to delete file");
    }
  }

  // Study Guides View
  renderGuidesActions() {
    if (this.activeGuide) {
      return `
        <button class="btn btn-outline-secondary btn-sm guide-back-btn">
          <i class="bi bi-arrow-left"></i>
          Back to Guides
        </button>
        <button class="btn btn-primary generate-guide-btn">
          <i class="bi bi-plus-circle"></i>
          Generate New Guide
        </button>
      `;
    }
    return `
      <button class="btn btn-primary generate-guide-btn">
        <i class="bi bi-plus-circle"></i>
        Generate New Guide
      </button>
    `;
  }

  renderGuidesView() {
    if (this.activeGuide) {
      return this.renderStudyGuideView();
    }

    if (this.studyGuides.length === 0) {
      return `
        <div class="empty-state">
          <i class="bi bi-book empty-state-icon"></i>
          <h3>No study guides yet</h3>
          <p>Generate your first study guide to get started.</p>
          <button class="btn btn-primary generate-guide-btn">
            Generate Study Guide
          </button>
        </div>
      `;
    }

    return `
      <div class="guides-list">
        ${this.studyGuides.map((guide) => this.renderGuideItem(guide)).join("")}
      </div>
    `;
  }

  renderGuideItem(guide) {
    const createdDate = new Date(guide.createdAt).toLocaleDateString();
    const subject = guide.subject || "General";
    const topic = guide.topic || "Study Guide";

    return `
      <div class="guide-list-item">
        <div class="guide-info">
          <h4 class="guide-title">${guide.title}</h4>
          <p class="guide-meta">${subject} • ${topic} • ${createdDate}</p>
        </div>
        <div class="guide-actions">
          <button class="btn btn-primary btn-sm guide-view-btn" data-guide-id="${guide.id}">
            <i class="bi bi-eye"></i>
            View
          </button>
        </div>
      </div>
    `;
  }

  viewStudyGuide(guideId) {
    const guide = this.studyGuides.find((g) => g.id === guideId);
    if (!guide) return;

    this.activeGuide = guide;
    this.renderCurrentView();
  }

  renderStudyGuideView() {
    if (!this.activeGuide) return "";

    const createdDate = new Date(
      this.activeGuide.createdAt
    ).toLocaleDateString();
    const subject = this.activeGuide.subject || "General";
    const topic = this.activeGuide.topic || "Study Guide";
    const content = MarkdownParser.formatForDisplay(this.activeGuide.content);

    return `
      <div class="study-guide-viewer">
        <div class="study-guide-header">
          <h3 class="study-guide-title">${this.activeGuide.title}</h3>
          <div class="study-guide-meta">
            <span class="study-guide-subject">${subject}</span>
            <span class="study-guide-topic">${topic}</span>
            <span class="study-guide-date">${createdDate}</span>
          </div>
        </div>
        <div class="study-guide-content">
          ${content}
        </div>
      </div>
    `;
  }

  backToGuides() {
    this.activeGuide = null;
    this.renderCurrentView();
  }

  showGenerateGuideModal() {
    const modal = document.createElement("div");
    modal.className = "modal fade";
    modal.innerHTML = `
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Generate Study Guide</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <div class="mb-3">
              <label for="guide-prompt" class="form-label">What would you like to study?</label>
              <textarea class="form-control" id="guide-prompt" rows="3" 
                placeholder="e.g., 'Create a study guide about photosynthesis for my biology class'"></textarea>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
            <button type="button" class="btn btn-primary" id="generate-guide-confirm">
              ${
                this.isGenerating
                  ? '<span class="spinner-border spinner-border-sm me-2"></span>Generating...'
                  : "Generate Guide"
              }
            </button>
          </div>
        </div>
      </div>
    `;

    document.body.appendChild(modal);
    const bsModal = new bootstrap.Modal(modal);
    bsModal.show();

    modal
      .querySelector("#generate-guide-confirm")
      .addEventListener("click", () => {
        const prompt = modal.querySelector("#guide-prompt").value.trim();
        if (prompt) {
          this.generateStudyGuide(prompt);
          bsModal.hide();
        }
      });

    modal.addEventListener("hidden.bs.modal", () => {
      document.body.removeChild(modal);
    });
  }

  async generateStudyGuide(prompt) {
    try {
      this.isGenerating = true;
      notificationManager.info("Generating study guide...");

      const userId = store.getState().user?.id || 1;
      const result = await analysisAPI.generateStudyGuide(prompt, userId);

      if (result.success) {
        this.studyGuides.unshift(result.data);
        this.renderCurrentView();
        notificationManager.success("Study guide generated successfully!");
      } else {
        notificationManager.error(
          result.error || "Failed to generate study guide"
        );
      }
    } catch (error) {
      logger.error("Error generating study guide:", error);
      notificationManager.error("Failed to generate study guide");
    } finally {
      this.isGenerating = false;
    }
  }

  // Quizzes View
  renderQuizzesActions() {
    return `
      <button class="btn btn-primary generate-quiz-btn">
        <i class="bi bi-plus-circle"></i>
        Generate New Quiz
      </button>
    `;
  }

  renderQuizzesView() {
    if (this.activeQuiz) {
      return this.renderQuizInterface();
    }

    if (this.quizzes.length === 0) {
      return `
        <div class="empty-state">
          <i class="bi bi-question-circle empty-state-icon"></i>
          <h3>No quizzes yet</h3>
          <p>Generate your first quiz to test your knowledge.</p>
          <button class="btn btn-primary generate-quiz-btn">
            Generate Quiz
          </button>
        </div>
      `;
    }

    return `
      <div class="quizzes-list">
        ${this.quizzes.map((quiz) => this.renderQuizItem(quiz)).join("")}
      </div>
    `;
  }

  renderQuizItem(quiz) {
    const createdDate = new Date(quiz.createdAt).toLocaleDateString();
    const subject = quiz.subject || "General";
    const questionCount = quiz.questionsList?.length || 0;

    return `
      <div class="quiz-list-item">
        <div class="quiz-info">
          <h4 class="quiz-title">${quiz.title}</h4>
          <p class="quiz-meta">${subject} • ${questionCount} questions • ${createdDate}</p>
        </div>
        <div class="quiz-actions">
          <button class="btn btn-primary btn-sm quiz-start-btn" data-quiz-id="${quiz.id}">
            <i class="bi bi-play-circle"></i>
            Start Quiz
          </button>
        </div>
      </div>
    `;
  }

  startQuiz(quizId) {
    const quiz = this.quizzes.find((q) => q.id === quizId);
    if (!quiz) return;

    this.activeQuiz = quiz;
    this.quizAnswers = {};
    this.currentQuestionIndex = 0;
    this.renderCurrentView();
  }

  renderQuizInterface() {
    if (!this.activeQuiz) return "";

    const questions = this.activeQuiz.questionsList || [];
    const currentQuestion = questions[this.currentQuestionIndex];
    const totalQuestions = questions.length;
    const progress = ((this.currentQuestionIndex + 1) / totalQuestions) * 100;

    if (!currentQuestion) {
      return this.renderQuizResults();
    }

    return `
      <div class="quiz-interface">
        <div class="quiz-header">
          <div class="quiz-progress">
            <div class="progress">
              <div class="progress-bar" style="width: ${progress}%"></div>
            </div>
            <span class="quiz-progress-text">Question ${
              this.currentQuestionIndex + 1
            } of ${totalQuestions}</span>
          </div>
          <button class="btn btn-outline-secondary btn-sm quiz-back-btn">
            <i class="bi bi-arrow-left"></i>
            Back to Quizzes
          </button>
        </div>

        <div class="quiz-question">
          <h3 class="question-text">${currentQuestion.question}</h3>
          <div class="quiz-options">
            ${currentQuestion.options
              .map(
                (option, index) => `
              <button class="btn btn-outline-primary quiz-option-btn ${
                this.quizAnswers[this.currentQuestionIndex] === index
                  ? "active"
                  : ""
              }" 
                      data-option-index="${index}">
                <span class="option-letter">${String.fromCharCode(
                  65 + index
                )}</span>
                <span class="option-text">${option}</span>
              </button>
            `
              )
              .join("")}
          </div>
        </div>

        <div class="quiz-navigation">
          <button class="btn btn-outline-secondary quiz-prev-btn" ${
            this.currentQuestionIndex === 0 ? "disabled" : ""
          }>
            <i class="bi bi-arrow-left"></i>
            Previous
          </button>
          <button class="btn btn-primary quiz-next-btn" ${
            this.currentQuestionIndex === totalQuestions - 1
              ? 'style="display:none"'
              : ""
          }>
            Next
            <i class="bi bi-arrow-right"></i>
          </button>
          <button class="btn btn-success quiz-submit-btn" ${
            this.currentQuestionIndex === totalQuestions - 1
              ? ""
              : 'style="display:none"'
          }>
            Submit Quiz
            <i class="bi bi-check-circle"></i>
          </button>
        </div>
      </div>
    `;
  }

  handleQuizAnswer(optionIndex) {
    this.quizAnswers[this.currentQuestionIndex] = optionIndex;
    this.renderCurrentView();
  }

  nextQuestion() {
    if (
      this.currentQuestionIndex <
      (this.activeQuiz.questionsList?.length || 0) - 1
    ) {
      this.currentQuestionIndex++;
      this.renderCurrentView();
    }
  }

  prevQuestion() {
    if (this.currentQuestionIndex > 0) {
      this.currentQuestionIndex--;
      this.renderCurrentView();
    }
  }

  async submitQuiz() {
    try {
      const questions = this.activeQuiz.questionsList || [];
      let correctAnswers = 0;

      questions.forEach((question, index) => {
        const userAnswer = this.quizAnswers[index];
        if (userAnswer === question.correctAnswerIndex) {
          correctAnswers++;
        }
      });

      const score = Math.round((correctAnswers / questions.length) * 100);

      // Store quiz attempt
      const userId = store.getState().user?.id || 1;
      const answers = questions.map((question, index) => ({
        questionId: question.id || index,
        selectedOptionIndex: this.quizAnswers[index] || -1,
        isCorrect: this.quizAnswers[index] === question.correctAnswerIndex,
      }));

      await analysisAPI.submitQuizAttempt(this.activeQuiz.id, answers, userId);

      this.quizScore = score;
      this.quizCorrectAnswers = correctAnswers;
      this.quizTotalQuestions = questions.length;
      this.renderCurrentView();

      notificationManager.success(`Quiz completed! Score: ${score}%`);
    } catch (error) {
      logger.error("Error submitting quiz:", error);
      notificationManager.error("Failed to submit quiz");
    }
  }

  renderQuizResults() {
    const questions = this.activeQuiz.questionsList || [];
    const score = this.quizScore || 0;
    const correctAnswers = this.quizCorrectAnswers || 0;
    const totalQuestions = this.quizTotalQuestions || questions.length;

    return `
      <div class="quiz-results">
        <div class="quiz-results-header">
          <h3>Quiz Results</h3>
          <div class="quiz-score">
            <div class="score-circle">
              <span class="score-number">${score}</span>
              <span class="score-percent">%</span>
            </div>
            <p class="score-text">${correctAnswers} out of ${totalQuestions} correct</p>
          </div>
        </div>

        <div class="quiz-questions-review">
          <h4>Question Review</h4>
          ${questions
            .map((question, index) => {
              const userAnswer = this.quizAnswers[index];
              const isCorrect = userAnswer === question.correctAnswerIndex;
              const userOption =
                userAnswer !== undefined
                  ? question.options[userAnswer]
                  : "Not answered";
              const correctOption =
                question.options[question.correctAnswerIndex];

              return `
              <div class="question-review-item ${
                isCorrect ? "correct" : "incorrect"
              }">
                <div class="question-review-header">
                  <span class="question-number">Q${index + 1}</span>
                  <span class="question-status">
                    <i class="bi ${
                      isCorrect
                        ? "bi-check-circle-fill text-success"
                        : "bi-x-circle-fill text-danger"
                    }"></i>
                  </span>
                </div>
                <p class="question-text">${question.question}</p>
                <div class="answer-comparison">
                  <div class="user-answer">
                    <strong>Your answer:</strong> ${userOption}
                  </div>
                  ${
                    !isCorrect
                      ? `
                    <div class="correct-answer">
                      <strong>Correct answer:</strong> ${correctOption}
                    </div>
                  `
                      : ""
                  }
                </div>
                ${
                  question.explanation
                    ? `
                  <div class="question-explanation">
                    <strong>Explanation:</strong> ${question.explanation}
                  </div>
                `
                    : ""
                }
              </div>
            `;
            })
            .join("")}
        </div>

        <div class="quiz-results-actions">
          <button class="btn btn-outline-secondary quiz-back-btn">
            <i class="bi bi-arrow-left"></i>
            Back to Quizzes
          </button>
          <button class="btn btn-primary" onclick="location.reload()">
            <i class="bi bi-arrow-clockwise"></i>
            Retake Quiz
          </button>
        </div>
      </div>
    `;
  }

  backToQuizzes() {
    this.activeQuiz = null;
    this.quizAnswers = {};
    this.currentQuestionIndex = 0;
    this.renderCurrentView();
  }

  showGenerateQuizModal() {
    const modal = document.createElement("div");
    modal.className = "modal fade";
    modal.innerHTML = `
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Generate Quiz</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <div class="mb-3">
              <label for="quiz-prompt" class="form-label">What would you like to be quizzed on?</label>
              <textarea class="form-control" id="quiz-prompt" rows="3" 
                placeholder="e.g., 'Create a quiz about the American Revolution for my history class'"></textarea>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
            <button type="button" class="btn btn-primary" id="generate-quiz-confirm">
              ${
                this.isGenerating
                  ? '<span class="spinner-border spinner-border-sm me-2"></span>Generating...'
                  : "Generate Quiz"
              }
            </button>
          </div>
        </div>
      </div>
    `;

    document.body.appendChild(modal);
    const bsModal = new bootstrap.Modal(modal);
    bsModal.show();

    modal
      .querySelector("#generate-quiz-confirm")
      .addEventListener("click", () => {
        const prompt = modal.querySelector("#quiz-prompt").value.trim();
        if (prompt) {
          this.generateQuiz(prompt);
          bsModal.hide();
        }
      });

    modal.addEventListener("hidden.bs.modal", () => {
      document.body.removeChild(modal);
    });
  }

  async generateQuiz(prompt) {
    try {
      this.isGenerating = true;
      notificationManager.info("Generating quiz...");

      const userId = store.getState().user?.id || 1;
      const result = await analysisAPI.generateQuiz(prompt, userId);

      if (result.success) {
        this.quizzes.unshift(result.data);
        this.renderCurrentView();
        notificationManager.success("Quiz generated successfully!");
      } else {
        notificationManager.error(result.error || "Failed to generate quiz");
      }
    } catch (error) {
      logger.error("Error generating quiz:", error);
      notificationManager.error("Failed to generate quiz");
    } finally {
      this.isGenerating = false;
    }
  }

  // Dashboard Handler Methods
  toggleSubjectGroup(header) {
    const filesContainer = header.nextElementSibling;
    const icon = header.querySelector(".collapse-icon");

    filesContainer.classList.toggle("collapsed");
    icon.classList.toggle("bi-chevron-right");
    icon.classList.toggle("bi-chevron-down");
  }

  async handleChatSend() {
    const input = document.getElementById("chat-input");
    const message = input.value.trim();

    if (!message) return;

    try {
      // Add user message to chat
      this.addChatMessage("user", message);
      input.value = "";

      const userId = store.getState().user?.id || 1;
      const response = await analysisAPI.chat(message, userId, [], []);

      // Add AI response to chat
      this.addChatMessage("assistant", response.message || response.response);
    } catch (error) {
      logger.error("Chat error:", error);
      notificationManager.error("Failed to send message");
    }
  }

  addChatMessage(role, content) {
    const messagesContainer = document.getElementById("chat-messages");
    const messageDiv = document.createElement("div");
    messageDiv.className = `chat-message chat-message-${role}`;
    messageDiv.innerHTML = `
      <div class="message-content">${content}</div>
    `;
    messagesContainer.appendChild(messageDiv);
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
  }

  extractFileName(file) {
    let fileName =
      file.FileName || file.fileName || file.OriginalFileName || "Unknown File";

    if (fileName && fileName.includes("_") && fileName.split("_").length >= 3) {
      const parts = fileName.split("_");
      fileName = parts[parts.length - 1];
    }

    return fileName;
  }

  async handleDeleteSubject(groupId) {
    if (
      !confirm(
        "Delete this subject? Files will remain but lose their grouping."
      )
    ) {
      return;
    }

    try {
      const result = await filesAPI.deleteSubjectGroup(groupId);
      if (result.success) {
        notificationManager.success("Subject deleted");
        this.loadData();
      }
    } catch (error) {
      logger.error("Error deleting subject:", error);
      notificationManager.error("Failed to delete subject");
    }
  }

  async generateGuideFromSubject(groupId) {
    const group = this.subjectGroups.find((g) => (g.Id || g.id) === groupId);
    const filesInGroup = this.files.filter(
      (f) => (f.SubjectGroupId || f.subjectGroupId) === groupId
    );
    const fileIds = filesInGroup.map((f) => f.Id || f.fileId || f.id);

    try {
      notificationManager.info("Generating study guide...");
      const userId = store.getState().user?.id || 1;
      const result = await analysisAPI.generateStudyGuide(
        `Generate a comprehensive study guide for ${
          group.GroupName || group.groupName
        }`,
        userId,
        fileIds,
        []
      );

      if (result.success) {
        notificationManager.success("Study guide generated!");
        this.currentView = "guides";
        this.loadData();
      }
    } catch (error) {
      logger.error("Error generating guide:", error);
      notificationManager.error("Failed to generate study guide");
    }
  }

  async generateQuizFromSubject(groupId) {
    const group = this.subjectGroups.find((g) => (g.Id || g.id) === groupId);
    const filesInGroup = this.files.filter(
      (f) => (f.SubjectGroupId || f.subjectGroupId) === groupId
    );
    const fileIds = filesInGroup.map((f) => f.Id || f.fileId || f.id);

    try {
      notificationManager.info("Generating quiz...");
      const userId = store.getState().user?.id || 1;
      const result = await analysisAPI.generateQuiz(
        `Generate a quiz for ${group.GroupName || group.groupName}`,
        userId,
        fileIds,
        []
      );

      if (result.success) {
        notificationManager.success("Quiz generated!");
        this.currentView = "quizzes";
        this.loadData();
      }
    } catch (error) {
      logger.error("Error generating quiz:", error);
      notificationManager.error("Failed to generate quiz");
    }
  }

  handleQuickAction(action) {
    switch (action) {
      case "analyze-all":
        this.analyzeAllMaterials();
        break;
      case "bulk-generate-guides":
        this.bulkGenerateGuides();
        break;
      case "bulk-generate-quizzes":
        this.bulkGenerateQuizzes();
        break;
      case "reorganize-files":
        this.reorganizeFiles();
        break;
    }
  }

  async analyzeAllMaterials() {
    if (
      !confirm(
        `Analyze all materials and generate personalized study plans for all ${this.subjectGroups.length} subjects?`
      )
    )
      return;

    notificationManager.info(
      "Analyzing all materials and generating personalized study plans..."
    );

    try {
      const userId = store.getState().user?.id || 1;
      const allFileIds = this.files.map((f) => f.Id || f.fileId || f.id);

      // Generate comprehensive analysis for all subjects
      const result = await analysisAPI.generateStudyGuide(
        `Analyze all uploaded materials and create a comprehensive, personalized study plan covering all subjects. Provide detailed recommendations for each subject area, study schedules, and priority topics.`,
        userId,
        allFileIds,
        []
      );

      if (result.success) {
        notificationManager.success("Personalized study plan generated!");
        this.currentView = "guides";
        this.loadData();
      }
    } catch (error) {
      logger.error("Error analyzing all materials:", error);
      notificationManager.error("Failed to analyze all materials");
    }
  }

  async bulkGenerateGuides() {
    if (
      !confirm(
        `Generate study guides for all ${this.subjectGroups.length} subjects?`
      )
    )
      return;

    notificationManager.info("Generating study guides for all subjects...");

    for (const group of this.subjectGroups) {
      try {
        await this.generateGuideFromSubject(group.Id || group.id);
        await new Promise((resolve) => setTimeout(resolve, 1000)); // Rate limiting
      } catch (error) {
        logger.error(`Error generating guide for ${group.GroupName}:`, error);
      }
    }

    notificationManager.success("Bulk generation complete!");
    this.loadData();
  }

  async bulkGenerateQuizzes() {
    if (
      !confirm(
        `Generate quizzes for all ${this.subjectGroups.length} subjects?`
      )
    )
      return;

    notificationManager.info("Generating quizzes for all subjects...");

    for (const group of this.subjectGroups) {
      try {
        await this.generateQuizFromSubject(group.Id || group.id);
        await new Promise((resolve) => setTimeout(resolve, 1000)); // Rate limiting
      } catch (error) {
        logger.error(`Error generating quiz for ${group.GroupName}:`, error);
      }
    }

    notificationManager.success("Bulk generation complete!");
    this.loadData();
  }

  async reorganizeFiles() {
    if (
      !confirm(
        "Re-detect and regroup all files? This may change current subject groupings."
      )
    )
      return;

    try {
      notificationManager.info("Reorganizing files...");
      const userId = store.getState().user?.id || 1;

      // Call backend to reorganize files
      const result = await filesAPI.reorganizeFiles(userId);

      if (result.success) {
        notificationManager.success("Files reorganized successfully!");
        this.loadData();
      }
    } catch (error) {
      logger.error("Error reorganizing files:", error);
      notificationManager.error("Failed to reorganize files");
    }
  }

  // Auto-detected subject handlers
  async createGroupFromSubject(subjectName) {
    try {
      const userId = store.getState().user?.id || 1;
      const result = await filesAPI.createSubjectGroup(
        userId,
        subjectName,
        `Auto-created from detected subject: ${subjectName}`,
        "#3498db"
      );

      if (result.success) {
        notificationManager.success(`Created subject group: ${subjectName}`);
        this.loadData();
      }
    } catch (error) {
      logger.error("Error creating group from subject:", error);
      notificationManager.error("Failed to create subject group");
    }
  }

  async generateGuideFromAutoSubject(subjectName) {
    const filesInSubject = this.files.filter(
      (f) => (f.AutoDetectedSubject || f.UserDefinedSubject) === subjectName
    );
    const fileIds = filesInSubject.map((f) => f.Id || f.fileId || f.id);

    try {
      notificationManager.info("Generating study guide...");
      const userId = store.getState().user?.id || 1;
      const result = await analysisAPI.generateStudyGuide(
        `Generate a comprehensive study guide for ${subjectName}`,
        userId,
        fileIds,
        []
      );

      if (result.success) {
        notificationManager.success("Study guide generated!");
        this.currentView = "guides";
        this.loadData();
      }
    } catch (error) {
      logger.error("Error generating guide:", error);
      notificationManager.error("Failed to generate study guide");
    }
  }

  async generateQuizFromAutoSubject(subjectName) {
    const filesInSubject = this.files.filter(
      (f) => (f.AutoDetectedSubject || f.UserDefinedSubject) === subjectName
    );
    const fileIds = filesInSubject.map((f) => f.Id || f.fileId || f.id);

    try {
      notificationManager.info("Generating quiz...");
      const userId = store.getState().user?.id || 1;
      const result = await analysisAPI.generateQuiz(
        `Generate a quiz for ${subjectName}`,
        userId,
        fileIds,
        []
      );

      if (result.success) {
        notificationManager.success("Quiz generated!");
        this.currentView = "quizzes";
        this.loadData();
      }
    } catch (error) {
      logger.error("Error generating quiz:", error);
      notificationManager.error("Failed to generate quiz");
    }
  }

  async generateGuideFromGroupedSubject(subjectName) {
    const filesInSubject = this.groupedFiles.filesBySubject[subjectName] || [];
    const fileIds = filesInSubject.map((f) => f.Id || f.fileId || f.id);

    try {
      notificationManager.info("Generating study guide...");
      const userId = store.getState().user?.id || 1;
      const result = await analysisAPI.generateStudyGuide(
        `Generate a comprehensive study guide for ${subjectName}`,
        userId,
        fileIds,
        []
      );

      if (result.success) {
        notificationManager.success("Study guide generated!");
        this.currentView = "guides";
        this.loadData();
      }
    } catch (error) {
      logger.error("Error generating guide:", error);
      notificationManager.error("Failed to generate study guide");
    }
  }

  async generateQuizFromGroupedSubject(subjectName) {
    const filesInSubject = this.groupedFiles.filesBySubject[subjectName] || [];
    const fileIds = filesInSubject.map((f) => f.Id || f.fileId || f.id);

    try {
      notificationManager.info("Generating quiz...");
      const userId = store.getState().user?.id || 1;
      const result = await analysisAPI.generateQuiz(
        `Generate a quiz for ${subjectName}`,
        userId,
        fileIds,
        []
      );

      if (result.success) {
        notificationManager.success("Quiz generated!");
        this.currentView = "quizzes";
        this.loadData();
      }
    } catch (error) {
      logger.error("Error generating quiz:", error);
      notificationManager.error("Failed to generate quiz");
    }
  }

  // Placeholder methods for modals (to be implemented)
  showCreateSubjectModal() {
    notificationManager.info("Create subject functionality coming soon!");
  }

  showEditSubjectModal(groupId) {
    notificationManager.info("Edit subject functionality coming soon!");
  }

  showReassignFileModal(fileId) {
    notificationManager.info("Reassign file functionality coming soon!");
  }
}

export default Study;
