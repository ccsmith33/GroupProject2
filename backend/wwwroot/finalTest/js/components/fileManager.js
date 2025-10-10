// File Manager Component
import { filesAPI } from "../api/files.js";
import { analysisAPI } from "../api/analysis.js";
import { store } from "../state/store.js";
import { authState } from "../state/auth-state.js";
import { notificationManager } from "../utils/notifications.js";
import { CONFIG } from "../config.js";
import { logger } from "../utils/logger.js";
import {
  sanitizeText,
  sanitizeFilename,
  sanitizeAttribute,
} from "../utils/sanitize.js";

class FileManager {
  constructor() {
    this.isInitialized = false;
    this.currentView = "upload"; // upload, list, groups
    this.files = [];
    this.groupedFiles = null;
    this.subjectGroups = [];
    this.uploadQueue = [];
    this.isUploading = false;
    this.processingFiles = new Set();
    this.pollingInterval = null;
    this.selectedFiles = new Set();
    this.bulkMode = false;
  }

  // Initialize file manager
  async init() {
    if (this.isInitialized) return;

    try {
      this.setupEventListeners();
      await this.loadFiles();
      await this.loadSubjectGroups();
      this.isInitialized = true;
    } catch (error) {
      logger.error("File manager initialization error:", error);
      notificationManager.error("Failed to initialize file manager");
    }
  }

  // Setup event listeners
  setupEventListeners() {
    // Store cleanup functions
    this.cleanupFunctions = [];

    // File upload area events
    const uploadArea = document.getElementById("fileUploadArea");
    if (uploadArea) {
      this.handleDragOver = this.handleDragOver.bind(this);
      this.handleDragLeave = this.handleDragLeave.bind(this);
      this.handleDrop = this.handleDrop.bind(this);
      this.handleUploadClick = () => {
        document.getElementById("fileInput")?.click();
      };

      uploadArea.addEventListener("dragover", this.handleDragOver);
      uploadArea.addEventListener("dragleave", this.handleDragLeave);
      uploadArea.addEventListener("drop", this.handleDrop);
      uploadArea.addEventListener("click", this.handleUploadClick);

      this.cleanupFunctions.push(() => {
        uploadArea.removeEventListener("dragover", this.handleDragOver);
        uploadArea.removeEventListener("dragleave", this.handleDragLeave);
        uploadArea.removeEventListener("drop", this.handleDrop);
        uploadArea.removeEventListener("click", this.handleUploadClick);
      });
    }

    // File input change
    const fileInput = document.getElementById("fileInput");
    if (fileInput) {
      this.handleFileSelect = this.handleFileSelect.bind(this);
      fileInput.addEventListener("change", this.handleFileSelect);
      this.cleanupFunctions.push(() => {
        fileInput.removeEventListener("change", this.handleFileSelect);
      });
    }

    // View toggle buttons
    this.handleViewToggle = (e) => {
      if (e.target.matches("[data-file-view]")) {
        this.switchView(e.target.dataset.fileView);
      }
    };
    document.addEventListener("click", this.handleViewToggle);
    this.cleanupFunctions.push(() => {
      document.removeEventListener("click", this.handleViewToggle);
    });

    // File action buttons
    this.handleFileActionClick = (e) => {
      if (e.target.matches("[data-file-action]")) {
        this.handleFileAction(e.target);
      }
    };
    document.addEventListener("click", this.handleFileActionClick);
    this.cleanupFunctions.push(() => {
      document.removeEventListener("click", this.handleFileActionClick);
    });

    // Subject group actions
    this.handleGroupActionClick = (e) => {
      if (e.target.matches("[data-group-action]")) {
        this.handleGroupAction(e.target);
      }
    };
    document.addEventListener("click", this.handleGroupActionClick);
    this.cleanupFunctions.push(() => {
      document.removeEventListener("click", this.handleGroupActionClick);
    });
  }

  // Cleanup event listeners
  cleanup() {
    if (this.cleanupFunctions) {
      this.cleanupFunctions.forEach((cleanup) => cleanup());
      this.cleanupFunctions = [];
    }
  }

  // Switch between file views
  switchView(viewName) {
    this.currentView = viewName;
    this.updateViewDisplay();
    this.loadViewContent();
  }

  // Update view display
  updateViewDisplay() {
    // Update active tab
    const tabs = document.querySelectorAll("[data-file-view]");
    if (tabs && tabs.length > 0) {
      tabs.forEach((tab) => {
        if (tab) {
          tab.classList.remove("active");
          if (tab.dataset.fileView === this.currentView) {
            tab.classList.add("active");
          }
        }
      });
    }

    // Show/hide view containers
    const views = ["uploadView", "listView", "groupsView"];
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
      case "upload":
        this.renderUploadView();
        break;
      case "list":
        await this.renderFileList();
        break;
      case "groups":
        await this.renderGroupedFiles();
        break;
    }
  }

  // Render upload view
  renderUploadView() {
    const uploadView = document.getElementById("uploadView");
    if (!uploadView) return;

    const state = store.getState();
    const isGuest = state.isGuest;

    uploadView.innerHTML = `
      <div class="row">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">
                <i class="bi bi-cloud-upload me-2"></i>Upload Files
              </h5>
            </div>
            <div class="card-body">
              ${isGuest ? this.renderGuestUploadNotice() : ""}
              
              <div id="fileUploadArea" class="upload-area ${
                isGuest ? "guest-mode" : ""
              }">
                <div class="upload-content">
                  <i class="bi bi-cloud-upload upload-icon"></i>
                  <h5>Drag & Drop Files Here</h5>
                  <p class="text-muted">or click to browse</p>
                  <button class="btn btn-primary" type="button">
                    <i class="bi bi-folder2-open me-2"></i>Choose Files
                  </button>
                </div>
                <input type="file" id="fileInput" multiple accept="${CONFIG.ALLOWED_FILE_TYPES.join(
                  ","
                )}" style="display: none;">
              </div>

              <div class="upload-info mt-3">
                <small class="text-muted">
                  <i class="bi bi-info-circle me-1"></i>
                  Supported formats: ${CONFIG.ALLOWED_FILE_TYPES.join(", ")} | 
                  Max size: ${CONFIG.MAX_FILE_SIZE_MB}MB per file
                </small>
              </div>

              ${this.renderUploadQueue()}
            </div>
          </div>
        </div>
      </div>
    `;
  }

  // Render guest upload notice
  renderGuestUploadNotice() {
    return `
      <div class="alert alert-warning d-flex align-items-center mb-3">
        <i class="bi bi-exclamation-triangle me-2"></i>
        <div>
          <strong>Guest Mode:</strong> Files are stored locally and will be lost when you close the browser. 
          <a href="#" class="alert-link" data-file-action="show-auth-modal">Create an account</a> to save your files permanently.
        </div>
      </div>
    `;
  }

  // Render upload queue
  renderUploadQueue() {
    if (this.uploadQueue.length === 0) return "";

    return `
      <div class="upload-queue mt-4">
        <h6><i class="bi bi-list-ul me-2"></i>Upload Queue</h6>
        <div class="queue-items">
          ${this.uploadQueue
            .map((file, index) => this.renderQueueItem(file, index))
            .join("")}
        </div>
        <div class="queue-actions mt-3">
          <button class="btn btn-success" data-file-action="start-upload" ${
            this.isUploading ? "disabled" : ""
          }>
            <i class="bi bi-upload me-2"></i>Upload All
          </button>
          <button class="btn btn-outline-secondary" data-file-action="clear-queue">
            <i class="bi bi-trash me-2"></i>Clear Queue
          </button>
        </div>
      </div>
    `;
  }

  // Render queue item
  renderQueueItem(file, index) {
    return `
      <div class="queue-item" data-file-index="${index}">
        <div class="file-info">
          <i class="bi ${CONFIG.getFileIcon(file.type)} me-2"></i>
          <span class="file-name">${sanitizeText(file.name)}</span>
          <span class="file-size text-muted">(${CONFIG.formatFileSize(
            file.size
          )})</span>
        </div>
        <div class="file-actions">
          <button class="btn btn-sm btn-outline-danger" data-file-action="remove-from-queue" data-queue-index="${index}">
            <i class="bi bi-x"></i>
          </button>
        </div>
      </div>
    `;
  }

  // Handle drag over
  handleDragOver(e) {
    e.preventDefault();
    e.currentTarget.classList.add("drag-over");
  }

  // Handle drag leave
  handleDragLeave(e) {
    e.preventDefault();
    e.currentTarget.classList.remove("drag-over");
  }

  // Handle drop
  handleDrop(e) {
    e.preventDefault();
    e.currentTarget.classList.remove("drag-over");

    const files = Array.from(e.dataTransfer.files);
    this.addFilesToQueue(files);
  }

  // Handle file select
  handleFileSelect(e) {
    const files = Array.from(e.target.files);
    this.addFilesToQueue(files);
  }

  // Add files to upload queue
  addFilesToQueue(files) {
    const state = store.getState();

    // Check guest limits
    if (state.isGuest) {
      const currentFileCount = this.files.length + this.uploadQueue.length;
      if (currentFileCount + files.length > CONFIG.GUEST_FILE_LIMIT) {
        notificationManager.warning(
          `Guest users can only upload ${CONFIG.GUEST_FILE_LIMIT} files. Please create an account for unlimited uploads.`
        );
        return;
      }
    }

    // Validate files
    const validFiles = files.filter((file) => this.validateFile(file));

    if (validFiles.length !== files.length) {
      notificationManager.warning(
        "Some files were skipped due to invalid format or size."
      );
    }

    // Add to queue
    this.uploadQueue.push(...validFiles);
    this.renderUploadView();

    notificationManager.success(
      `${validFiles.length} file(s) added to upload queue`
    );
  }

  // Validate file
  validateFile(file) {
    // Check file type
    const fileExtension = "." + file.name.split(".").pop().toLowerCase();
    if (!CONFIG.ALLOWED_FILE_TYPES.includes(fileExtension)) {
      notificationManager.error(`File type ${fileExtension} is not supported`);
      return false;
    }

    // Check file size
    if (file.size > CONFIG.MAX_FILE_SIZE_MB * 1024 * 1024) {
      notificationManager.error(
        `File ${file.name} is too large. Maximum size is ${CONFIG.MAX_FILE_SIZE_MB}MB`
      );
      return false;
    }

    return true;
  }

  // Remove file from queue
  removeFromQueue(index) {
    this.uploadQueue.splice(index, 1);
    this.renderUploadView();
  }

  // Clear upload queue
  clearQueue() {
    this.uploadQueue = [];
    this.renderUploadView();
  }

  // Start upload process
  async startUpload() {
    if (this.isUploading || this.uploadQueue.length === 0) return;

    this.isUploading = true;
    this.renderUploadView();

    try {
      const state = store.getState();
      const results = [];

      for (const file of this.uploadQueue) {
        try {
          const result = await filesAPI.uploadFile(file, {
            userId: state.user?.id || 1,
            subject: "General",
            studentLevel: "college",
          });

          if (result.success) {
            results.push({ file: file.name, success: true });
          } else {
            results.push({
              file: file.name,
              success: false,
              error: result.message,
            });
          }
        } catch (error) {
          results.push({
            file: file.name,
            success: false,
            error: error.message,
          });
        }
      }

      // Show results
      const successCount = results.filter((r) => r.success).length;
      const failCount = results.filter((r) => !r.success).length;

      if (successCount > 0) {
        notificationManager.success(
          `${successCount} file(s) uploaded successfully`
        );
      }
      if (failCount > 0) {
        notificationManager.error(`${failCount} file(s) failed to upload`);
      }

      // Clear queue and reload files
      this.uploadQueue = [];
      await this.loadFiles();
      this.renderUploadView();
    } catch (error) {
      logger.error("Upload error:", error);
      notificationManager.error("Upload failed. Please try again.");
    } finally {
      this.isUploading = false;
    }
  }

  // Load files from API
  async loadFiles() {
    try {
      const state = store.getState();
      const result = await filesAPI.getFiles(state.user?.id || 1);

      if (result.success) {
        this.files = result.data || [];
        store.actions.setFiles(this.files);
      } else {
        throw new Error(result.message || "Failed to load files");
      }
    } catch (error) {
      logger.error("Error loading files:", error);
      notificationManager.error("Failed to load files");
      this.files = [];
    }
  }

  // Load subject groups
  async loadSubjectGroups() {
    try {
      const state = store.getState();
      const result = await filesAPI.getSubjectGroups(state.user?.id || 1);

      if (result.success) {
        this.subjectGroups = result.data || [];
      }
    } catch (error) {
      logger.error("Error loading subject groups:", error);
      this.subjectGroups = [];
    }
  }

  // Render file list
  async renderFileList() {
    const listView = document.getElementById("listView");
    if (!listView) return;

    await this.loadFiles();

    listView.innerHTML = `
      <div class="row">
        <div class="col-12">
          <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
              <h5 class="card-title mb-0">
                <i class="bi bi-folder me-2"></i>All Files (${
                  this.files.length
                })
              </h5>
              <div class="btn-group" role="group">
                <button class="btn btn-outline-primary btn-sm" data-file-action="refresh-files">
                  <i class="bi bi-arrow-clockwise me-1"></i>Refresh
                </button>
                <button class="btn btn-outline-success btn-sm" data-file-action="switch-to-upload">
                  <i class="bi bi-plus me-1"></i>Upload More
                </button>
                <button class="btn btn-outline-secondary btn-sm" data-file-action="toggle-bulk-mode">
                  <i class="bi bi-check2-square me-1"></i>${
                    this.bulkMode ? "Exit Bulk" : "Bulk Mode"
                  }
                </button>
              </div>
            </div>
            <div class="card-body p-0">
              ${this.renderBulkActions()}
            <div class="card-body">
              ${
                this.files.length === 0
                  ? this.renderEmptyState()
                  : this.renderFilesTable()
              }
            </div>
          </div>
        </div>
      </div>
    `;
  }

  // Render empty state
  renderEmptyState() {
    return `
      <div class="empty-state text-center py-5">
        <i class="bi bi-folder-x display-1 text-muted"></i>
        <h4 class="mt-3">No Files Yet</h4>
        <p class="text-muted">Upload your first study material to get started!</p>
        <button class="btn btn-primary" data-file-action="switch-to-upload">
          <i class="bi bi-cloud-upload me-2"></i>Upload Files
        </button>
      </div>
    `;
  }

  // Render files table
  renderFilesTable() {
    return `
      <div class="table-responsive">
        <table class="table table-hover">
          <thead>
            <tr>
              ${
                this.bulkMode
                  ? '<th><input type="checkbox" class="form-check-input" data-file-action="select-all"></th>'
                  : ""
              }
              <th>File</th>
              <th>Subject</th>
              <th>Status</th>
              <th>Uploaded</th>
              <th>Size</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            ${this.files.map((file) => this.renderFileRow(file)).join("")}
          </tbody>
        </table>
      </div>
    `;
  }

  // Render file row
  renderFileRow(file) {
    const statusBadge = this.getStatusBadge(file);
    const subjectDisplay =
      file.autoDetectedSubject || file.subject || "Unknown";
    const isSelected = this.selectedFiles.has(file.id);

    return `
      <tr class="${isSelected ? "table-active" : ""}">
        ${
          this.bulkMode
            ? `
        <td>
          <input type="checkbox" class="form-check-input" 
                 data-file-action="toggle-file-selection" 
                 data-file-id="${file.id}"
                 ${isSelected ? "checked" : ""}>
        </td>
        `
            : ""
        }
        <td>
          <div class="d-flex align-items-center">
            <i class="bi ${CONFIG.getFileIcon(
              file.fileType
            )} me-2 text-primary"></i>
            <div>
              <div class="fw-medium">${sanitizeText(file.fileName)}</div>
              <small class="text-muted">${file.fileType}</small>
            </div>
          </div>
        </td>
        <td>
          <span class="badge bg-secondary">${sanitizeText(
            subjectDisplay
          )}</span>
          ${
            file.autoDetectedSubject
              ? `<br><small class="text-muted">AI detected</small>`
              : ""
          }
        </td>
        <td>${statusBadge}</td>
        <td>
          <small class="text-muted">${CONFIG.formatDate(
            file.uploadedAt
          )}</small>
        </td>
        <td>
          <small class="text-muted">${CONFIG.formatFileSize(
            file.fileSize
          )}</small>
        </td>
        <td>
          <div class="btn-group btn-group-sm" role="group">
            <button class="btn btn-outline-primary" data-file-action="view" data-file-id="${
              file.id
            }">
              <i class="bi bi-eye"></i>
            </button>
            <button class="btn btn-outline-success" data-file-action="process" data-file-id="${
              file.id
            }">
              <i class="bi bi-gear"></i>
            </button>
            <button class="btn btn-outline-danger" data-file-action="delete" data-file-id="${
              file.id
            }">
              <i class="bi bi-trash"></i>
            </button>
          </div>
        </td>
      </tr>
    `;
  }

  // Get status badge
  getStatusBadge(file) {
    if (file.isProcessed) {
      return '<span class="badge bg-success">Processed</span>';
    } else if (file.processingStatus === "processing") {
      return '<span class="badge bg-warning">Processing...</span>';
    } else if (file.processingStatus === "failed") {
      return '<span class="badge bg-danger">Failed</span>';
    } else {
      return '<span class="badge bg-secondary">Pending</span>';
    }
  }

  // Render grouped files
  async renderGroupedFiles() {
    const groupsView = document.getElementById("groupsView");
    if (!groupsView) return;

    try {
      const state = store.getState();
      const result = await filesAPI.getGroupedFiles(state.user?.id || 1);

      if (result.success) {
        this.groupedFiles = result.data;
        this.renderGroupedContent();
      } else {
        throw new Error(result.message || "Failed to load grouped files");
      }
    } catch (error) {
      logger.error("Error loading grouped files:", error);
      notificationManager.error("Failed to load grouped files");
      this.renderGroupedContent();
    }
  }

  // Render grouped content
  renderGroupedContent() {
    const groupsView = document.getElementById("groupsView");
    if (!groupsView) return;

    const filesBySubject = this.groupedFiles?.filesBySubject || {};
    const customGroups = this.groupedFiles?.customGroups || [];
    const ungroupedFiles = this.groupedFiles?.ungroupedFiles || [];

    groupsView.innerHTML = `
      <div class="row">
        <div class="col-12">
          <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
              <h5 class="card-title mb-0">
                <i class="bi bi-collection me-2"></i>File Groups
              </h5>
              <div class="btn-group" role="group">
                <button class="btn btn-outline-primary btn-sm" data-file-action="refresh-grouped-files">
                  <i class="bi bi-arrow-clockwise me-1"></i>Refresh
                </button>
                <button class="btn btn-outline-success btn-sm" data-file-action="create-subject-group">
                  <i class="bi bi-plus me-1"></i>New Group
                </button>
              </div>
            </div>
            <div class="card-body">
              ${this.renderAutoGroupedFiles(filesBySubject)}
              ${this.renderCustomGroups(customGroups)}
              ${this.renderUngroupedFiles(ungroupedFiles)}
            </div>
          </div>
        </div>
      </div>
    `;
  }

  // Render auto-grouped files
  renderAutoGroupedFiles(filesBySubject) {
    const subjects = Object.keys(filesBySubject);
    if (subjects.length === 0) return "";

    return `
      <div class="mb-4">
        <h6 class="text-primary">
          <i class="bi bi-robot me-2"></i>Auto-Grouped by Subject
        </h6>
        <div class="row">
          ${subjects
            .map((subject) =>
              this.renderSubjectGroup(subject, filesBySubject[subject])
            )
            .join("")}
        </div>
      </div>
    `;
  }

  // Render subject group
  renderSubjectGroup(subject, files) {
    return `
      <div class="col-md-6 col-lg-4 mb-3">
        <div class="card border-primary">
          <div class="card-header bg-primary text-white">
            <h6 class="mb-0">
              <i class="bi bi-tag me-2"></i>${subject}
              <span class="badge bg-light text-dark ms-2">${files.length}</span>
            </h6>
          </div>
          <div class="card-body">
            <div class="file-list">
              ${files.map((file) => this.renderGroupedFileItem(file)).join("")}
            </div>
          </div>
        </div>
      </div>
    `;
  }

  // Render custom groups
  renderCustomGroups(customGroups) {
    if (customGroups.length === 0) return "";

    return `
      <div class="mb-4">
        <h6 class="text-success">
          <i class="bi bi-collection me-2"></i>Custom Groups
        </h6>
        <div class="row">
          ${customGroups.map((group) => this.renderCustomGroup(group)).join("")}
        </div>
      </div>
    `;
  }

  // Render custom group
  renderCustomGroup(group) {
    return `
      <div class="col-md-6 col-lg-4 mb-3">
        <div class="card border-success">
          <div class="card-header bg-success text-white d-flex justify-content-between align-items-center">
            <h6 class="mb-0">
              <i class="bi bi-folder me-2"></i>${group.groupName}
            </h6>
            <div class="btn-group btn-group-sm">
              <button class="btn btn-outline-light" data-group-action="edit" data-group-id="${
                group.id
              }">
                <i class="bi bi-pencil"></i>
              </button>
              <button class="btn btn-outline-light" data-group-action="delete" data-group-id="${
                group.id
              }">
                <i class="bi bi-trash"></i>
              </button>
            </div>
          </div>
          <div class="card-body">
            <p class="text-muted small">${
              group.description || "No description"
            }</p>
            <div class="file-list">
              ${(group.files || [])
                .map((file) => this.renderGroupedFileItem(file))
                .join("")}
            </div>
          </div>
        </div>
      </div>
    `;
  }

  // Render ungrouped files
  renderUngroupedFiles(ungroupedFiles) {
    if (ungroupedFiles.length === 0) return "";

    return `
      <div class="mb-4">
        <h6 class="text-warning">
          <i class="bi bi-question-circle me-2"></i>Ungrouped Files
        </h6>
        <div class="alert alert-warning">
          <i class="bi bi-info-circle me-2"></i>
          These files haven't been automatically grouped yet. They will be processed and grouped soon.
        </div>
        <div class="row">
          ${ungroupedFiles
            .map((file) => this.renderUngroupedFileItem(file))
            .join("")}
        </div>
      </div>
    `;
  }

  // Render grouped file item
  renderGroupedFileItem(file) {
    return `
      <div class="file-item d-flex align-items-center justify-content-between py-1">
        <div class="d-flex align-items-center">
          <i class="bi ${CONFIG.getFileIcon(
            file.fileType
          )} me-2 text-muted"></i>
          <span class="file-name">${sanitizeText(file.fileName)}</span>
        </div>
        <div class="btn-group btn-group-sm">
          <button class="btn btn-outline-primary" data-file-action="view" data-file-id="${
            file.id
          }">
            <i class="bi bi-eye"></i>
          </button>
          <button class="btn btn-outline-success" data-file-action="process" data-file-id="${
            file.id
          }">
            <i class="bi bi-gear"></i>
          </button>
        </div>
      </div>
    `;
  }

  // Render ungrouped file item
  renderUngroupedFileItem(file) {
    return `
      <div class="col-md-6 col-lg-4 mb-2">
        <div class="card border-warning">
          <div class="card-body p-2">
            <div class="d-flex align-items-center">
              <i class="bi ${CONFIG.getFileIcon(
                file.fileType
              )} me-2 text-warning"></i>
              <div class="flex-grow-1">
                <div class="fw-medium small">${file.fileName}</div>
                <small class="text-muted">${CONFIG.formatFileSize(
                  file.fileSize
                )}</small>
              </div>
              <button class="btn btn-outline-warning btn-sm" data-file-action="process" data-file-id="${
                file.id
              }">
                <i class="bi bi-gear"></i>
              </button>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  // Handle file actions
  async handleFileAction(button) {
    const action = button.dataset.fileAction;
    const fileId = parseInt(button.dataset.fileId);
    const file = this.files.find((f) => f.id === fileId);

    switch (action) {
      case "view":
        if (!file) {
          notificationManager.error("File not found");
          return;
        }
        this.viewFile(file);
        break;
      case "process":
        if (!file) {
          notificationManager.error("File not found");
          return;
        }
        await this.processFile(file);
        break;
      case "delete":
        if (!file) {
          notificationManager.error("File not found");
          return;
        }
        await this.deleteFile(file);
        break;
      case "switch-to-upload":
        this.switchView("upload");
        break;
      case "start-upload":
        await this.startUpload();
        break;
      case "clear-queue":
        this.clearQueue();
        break;
      case "remove-from-queue":
        const queueIndex = parseInt(button.dataset.queueIndex);
        this.removeFromQueue(queueIndex);
        break;
      case "refresh-files":
        await this.loadFiles();
        break;
      case "refresh-grouped-files":
        await this.loadGroupedFiles();
        break;
      case "create-subject-group":
        this.createSubjectGroup();
        break;
      case "show-auth-modal":
        window.app?.showAuthModal();
        break;
      case "toggle-bulk-mode":
        this.toggleBulkMode();
        break;
      case "select-all":
        if (this.selectedFiles.size === this.files.length) {
          this.deselectAllFiles();
        } else {
          this.selectAllFiles();
        }
        break;
      case "exit-bulk-mode":
        this.bulkMode = false;
        this.selectedFiles.clear();
        this.updateFileDisplay();
        break;
      case "toggle-file-selection":
        const fileId = parseInt(button.dataset.fileId);
        this.toggleFileSelection(fileId);
        break;
      case "bulk-process":
      case "bulk-delete":
      case "bulk-update-subject":
        this.handleBulkAction(action);
        break;
    }
  }

  // Handle group actions
  async handleGroupAction(button) {
    const action = button.dataset.groupAction;
    const groupId = parseInt(button.dataset.groupId);

    switch (action) {
      case "edit":
        this.editSubjectGroup(groupId);
        break;
      case "delete":
        await this.deleteSubjectGroup(groupId);
        break;
    }
  }

  // View file details
  viewFile(file) {
    // This would open a modal or navigate to file details
    notificationManager.info(`Viewing file: ${file.fileName}`);
  }

  // Process file
  async processFile(file) {
    try {
      notificationManager.info(`Processing file: ${file.fileName}`);

      const result = await filesAPI.processFile(file.id);

      if (result.success) {
        // Add to processing queue for status polling
        this.addToProcessingQueue(file.id);

        // Update file status immediately
        file.processingStatus = "processing";
        this.updateFileDisplay();

        notificationManager.info(`File processing started: ${file.fileName}`);
      } else {
        throw new Error(result.message || "Processing failed");
      }
    } catch (error) {
      logger.error("Error processing file:", error);
      notificationManager.error(`Failed to process file: ${error.message}`);
    }
  }

  // Delete file
  async deleteFile(file) {
    if (!confirm(`Are you sure you want to delete "${file.fileName}"?`)) {
      return;
    }

    try {
      const result = await filesAPI.deleteFile(file.id);

      if (result.success) {
        notificationManager.success(`File deleted: ${file.fileName}`);
        await this.loadFiles();
        await this.renderGroupedFiles();
      } else {
        throw new Error(result.message || "Delete failed");
      }
    } catch (error) {
      logger.error("Error deleting file:", error);
      notificationManager.error(`Failed to delete file: ${error.message}`);
    }
  }

  // Create subject group
  createSubjectGroup() {
    // This would open a modal for creating a new subject group
    notificationManager.info("Create subject group functionality coming soon");
  }

  // Edit subject group
  editSubjectGroup(groupId) {
    // This would open a modal for editing the subject group
    notificationManager.info("Edit subject group functionality coming soon");
  }

  // Delete subject group
  async deleteSubjectGroup(groupId) {
    if (!confirm("Are you sure you want to delete this subject group?")) {
      return;
    }

    try {
      const result = await filesAPI.deleteSubjectGroup(groupId);

      if (result.success) {
        notificationManager.success("Subject group deleted");
        await this.loadSubjectGroups();
        await this.renderGroupedFiles();
      } else {
        throw new Error(result.message || "Delete failed");
      }
    } catch (error) {
      logger.error("Error deleting subject group:", error);
      notificationManager.error(
        `Failed to delete subject group: ${error.message}`
      );
    }
  }

  // Refresh files
  async refreshFiles() {
    await this.loadFiles();
    await this.renderFileList();
    notificationManager.success("Files refreshed");
  }

  // Refresh grouped files
  async refreshGroupedFiles() {
    await this.renderGroupedFiles();
    notificationManager.success("Grouped files refreshed");
  }

  // Start polling for processing files
  startProcessingPolling() {
    if (this.pollingInterval) return;

    this.pollingInterval = setInterval(async () => {
      await this.checkProcessingStatus();
    }, 5000); // Poll every 5 seconds
  }

  // Stop polling for processing files
  stopProcessingPolling() {
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
      this.pollingInterval = null;
    }
  }

  // Check processing status of files
  async checkProcessingStatus() {
    if (this.processingFiles.size === 0) {
      this.stopProcessingPolling();
      return;
    }

    try {
      const fileIds = Array.from(this.processingFiles);
      const result = await filesAPI.getFilesStatus(fileIds);

      if (result.success) {
        const statuses = result.data;
        let hasUpdates = false;

        for (const [fileId, status] of Object.entries(statuses)) {
          const file = this.files.find((f) => f.id === parseInt(fileId));
          if (file) {
            const oldStatus = file.processingStatus;
            file.processingStatus = status.status;
            file.isProcessed = status.isProcessed;

            if (oldStatus !== status.status) {
              hasUpdates = true;

              if (status.status === "completed" || status.status === "failed") {
                this.processingFiles.delete(parseInt(fileId));

                if (status.status === "completed") {
                  notificationManager.success(
                    `File processed: ${file.fileName}`
                  );
                } else {
                  notificationManager.error(
                    `File processing failed: ${file.fileName}`
                  );
                }
              }
            }
          }
        }

        if (hasUpdates) {
          this.updateFileDisplay();
        }
      }
    } catch (error) {
      logger.error("Error checking processing status:", error);
    }
  }

  // Update file display after status changes
  updateFileDisplay() {
    if (this.currentView === "list") {
      this.renderFileList();
    } else if (this.currentView === "groups") {
      this.renderGroupedFiles();
    }
  }

  // Add file to processing queue
  addToProcessingQueue(fileId) {
    this.processingFiles.add(fileId);
    this.startProcessingPolling();
  }

  // Remove file from processing queue
  removeFromProcessingQueue(fileId) {
    this.processingFiles.delete(fileId);
    if (this.processingFiles.size === 0) {
      this.stopProcessingPolling();
    }
  }

  // Toggle bulk mode
  toggleBulkMode() {
    this.bulkMode = !this.bulkMode;
    this.selectedFiles.clear();
    this.updateFileDisplay();
  }

  // Toggle file selection
  toggleFileSelection(fileId) {
    if (this.selectedFiles.has(fileId)) {
      this.selectedFiles.delete(fileId);
    } else {
      this.selectedFiles.add(fileId);
    }
    this.updateFileDisplay();
  }

  // Select all files
  selectAllFiles() {
    this.files.forEach((file) => {
      this.selectedFiles.add(file.id);
    });
    this.updateFileDisplay();
  }

  // Deselect all files
  deselectAllFiles() {
    this.selectedFiles.clear();
    this.updateFileDisplay();
  }

  // Get selected files
  getSelectedFiles() {
    return this.files.filter((file) => this.selectedFiles.has(file.id));
  }

  // Render bulk actions toolbar
  renderBulkActions() {
    if (!this.bulkMode || this.selectedFiles.size === 0) {
      return "";
    }

    const selectedCount = this.selectedFiles.size;
    const allSelected = this.selectedFiles.size === this.files.length;

    return `
      <div class="bulk-actions-toolbar">
        <div class="d-flex align-items-center justify-content-between">
          <div class="d-flex align-items-center">
            <span class="me-3">
              <strong>${selectedCount}</strong> file${
      selectedCount !== 1 ? "s" : ""
    } selected
            </span>
            <div class="btn-group btn-group-sm me-3">
              <button class="btn btn-outline-primary" data-file-action="bulk-process">
                <i class="bi bi-gear me-1"></i>Process
              </button>
              <button class="btn btn-outline-danger" data-file-action="bulk-delete">
                <i class="bi bi-trash me-1"></i>Delete
              </button>
              <button class="btn btn-outline-secondary" data-file-action="bulk-update-subject">
                <i class="bi bi-tag me-1"></i>Update Subject
              </button>
            </div>
          </div>
          <div class="d-flex align-items-center">
            <button class="btn btn-sm btn-outline-secondary me-2" data-file-action="select-all">
              ${allSelected ? "Deselect All" : "Select All"}
            </button>
            <button class="btn btn-sm btn-outline-secondary" data-file-action="exit-bulk-mode">
              <i class="bi bi-x"></i>
            </button>
          </div>
        </div>
      </div>
    `;
  }

  // Handle bulk actions
  async handleBulkAction(action) {
    const selectedFiles = this.getSelectedFiles();

    if (selectedFiles.length === 0) {
      notificationManager.warning("No files selected");
      return;
    }

    switch (action) {
      case "bulk-process":
        await this.bulkProcessFiles(selectedFiles);
        break;
      case "bulk-delete":
        await this.bulkDeleteFiles(selectedFiles);
        break;
      case "bulk-update-subject":
        await this.bulkUpdateSubject(selectedFiles);
        break;
    }
  }

  // Bulk process files
  async bulkProcessFiles(files) {
    try {
      notificationManager.info(`Processing ${files.length} files...`);

      for (const file of files) {
        await this.processFile(file);
      }

      notificationManager.success(`Started processing ${files.length} files`);
      this.deselectAllFiles();
    } catch (error) {
      logger.error("Bulk process error:", error);
      notificationManager.error("Failed to process some files");
    }
  }

  // Bulk delete files
  async bulkDeleteFiles(files) {
    if (!confirm(`Are you sure you want to delete ${files.length} files?`)) {
      return;
    }

    try {
      notificationManager.info(`Deleting ${files.length} files...`);

      const deletePromises = files.map((file) => this.deleteFile(file));
      await Promise.all(deletePromises);

      notificationManager.success(`Deleted ${files.length} files`);
      this.deselectAllFiles();
    } catch (error) {
      logger.error("Bulk delete error:", error);
      notificationManager.error("Failed to delete some files");
    }
  }

  // Bulk update subject
  async bulkUpdateSubject(files) {
    const newSubject = prompt("Enter new subject for selected files:");
    if (!newSubject) return;

    try {
      notificationManager.info(`Updating subject for ${files.length} files...`);

      const updatePromises = files.map((file) =>
        filesAPI.updateFile(file.id, { subject: sanitizeText(newSubject) })
      );

      await Promise.all(updatePromises);

      // Update local file data
      files.forEach((file) => {
        file.subject = sanitizeText(newSubject);
      });

      this.updateFileDisplay();
      notificationManager.success(`Updated subject for ${files.length} files`);
      this.deselectAllFiles();
    } catch (error) {
      logger.error("Bulk update subject error:", error);
      notificationManager.error("Failed to update subject for some files");
    }
  }
}

// Create global file manager instance
const fileManager = new FileManager();

// Export for global access
window.fileManager = fileManager;

export { fileManager };
