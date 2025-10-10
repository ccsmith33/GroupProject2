// File upload component functionality
class FileUpload {
  constructor(container) {
    this.container = container;
    this.uploadedFiles = [];
    this.maxFileSize = 10 * 1024 * 1024; // 10MB
    this.allowedTypes = [
      ".pdf",
      ".docx", 
      ".pptx",
      ".txt",
      ".jpg",
      ".jpeg",
      ".png",
    ];
    this.eventListeners = {};
    this.init();
  }

  init() {
    this.render();
    this.setupEventListeners();
    this.setupDragAndDrop();
  }

  render() {
    this.container.innerHTML = `
      <div class="file-upload-container">
        <!-- Upload Hero Section -->
        <div class="upload-hero">
          <div class="upload-hero-content">
            <h1 class="upload-title">Upload Your Study Materials</h1>
            <p class="upload-subtitle">Transform your documents into personalized study guides with AI</p>
            <p class="upload-tagline">Drag, drop, and discover your learning potential</p>
          </div>
        </div>

        <!-- Main Upload Section -->
        <div class="upload-main-section">
          <div class="upload-main-grid">
            <!-- Upload Area -->
            <div class="upload-area-section">
              <div class="upload-card">
                <h3 class="section-title">📤 Upload Files</h3>
                <div id="fileUploadArea" class="upload-area">
                  <div class="upload-content">
                    <div class="upload-icon">☁️</div>
                    <h4 class="upload-text">Drag & Drop Files Here</h4>
                    <p class="upload-description">or click to browse your device</p>
                    <p class="upload-formats">Supports: PDF, Word, PowerPoint, Images, Text</p>
                  </div>
                  <input type="file" id="fileInput" multiple accept=".pdf,.docx,.pptx,.txt,.jpg,.jpeg,.png" style="display: none;">
                </div>
                
                <!-- Upload Progress -->
                <div id="uploadProgress" class="upload-progress" style="display: none;">
                  <div class="progress-bar-container">
                    <div class="progress-bar" role="progressbar" style="width: 0%"></div>
                  </div>
                  <p class="progress-text">Uploading your files...</p>
                </div>

                <!-- File List -->
                <div id="fileList" class="file-list"></div>
              </div>
            </div>

            <!-- Analysis Settings -->
            <div class="analysis-settings-section">
              <div class="analysis-card">
                <h3 class="section-title">⚙️ Analysis Settings</h3>
                <div class="settings-form">
                  <div class="form-group">
                    <label for="subjectSelect" class="form-label">📚 Subject</label>
                    <select class="form-select" id="subjectSelect">
                      <option value="mathematics">Mathematics</option>
                      <option value="physics">Physics</option>
                      <option value="chemistry">Chemistry</option>
                      <option value="biology">Biology</option>
                      <option value="english">English</option>
                      <option value="history">History</option>
                      <option value="other">Other</option>
                    </select>
                  </div>
                  
                  <div class="form-group">
                    <label for="levelSelect" class="form-label">🎓 Student Level</label>
                    <select class="form-select" id="levelSelect">
                      <option value="beginner">Beginner</option>
                      <option value="intermediate" selected>Intermediate</option>
                      <option value="advanced">Advanced</option>
                    </select>
                  </div>
                  
                  <button id="analyzeBtn" class="analyze-button" disabled>
                    <span class="button-icon">🤖</span>
                    <span class="button-text">Analyze Materials</span>
                    <div class="button-shine"></div>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Features Section -->
        <div class="upload-features-section">
          <h2 class="section-title">What You'll Get</h2>
          <p class="features-subtitle">Transform your study materials into powerful learning tools</p>
          <div class="features-grid">
            <div class="feature-item">
              <div class="feature-content">
                <div class="feature-icon">📖</div>
                <h4 class="feature-title">Smart Study Guides</h4>
                <p class="feature-description">AI-generated summaries and key points tailored to your learning style</p>
              </div>
            </div>
            <div class="feature-item">
              <div class="feature-content">
                <div class="feature-icon">🎯</div>
                <h4 class="feature-title">Personalized Quizzes</h4>
                <p class="feature-description">Custom questions and assessments based on your uploaded materials</p>
              </div>
            </div>
            <div class="feature-item">
              <div class="feature-content">
                <div class="feature-icon">📊</div>
                <h4 class="feature-title">Progress Tracking</h4>
                <p class="feature-description">Monitor your learning journey with detailed analytics and insights</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  setupEventListeners() {
    const fileInput = document.getElementById("fileInput");
    const uploadArea = document.getElementById("fileUploadArea");
    const analyzeBtn = document.getElementById("analyzeBtn");

    if (fileInput) {
      fileInput.addEventListener("change", (e) => this.handleFileSelect(e));
    }

    if (uploadArea) {
      uploadArea.addEventListener("click", () => fileInput?.click());
    }

    if (analyzeBtn) {
      analyzeBtn.addEventListener("click", () => this.analyzeFiles());
    }
  }

  setupDragAndDrop() {
    const uploadArea = document.getElementById("fileUploadArea");

    if (!uploadArea) return;

    uploadArea.addEventListener("dragover", (e) => {
      e.preventDefault();
      uploadArea.classList.add("dragover");
    });

    uploadArea.addEventListener("dragleave", (e) => {
      e.preventDefault();
      uploadArea.classList.remove("dragover");
    });

    uploadArea.addEventListener("drop", (e) => {
      e.preventDefault();
      uploadArea.classList.remove("dragover");
      this.handleFileSelect(e);
    });
  }

  handleFileSelect(event) {
    const files = event.target?.files || event.dataTransfer?.files;
    if (!files) return;

    Array.from(files).forEach((file) => {
      if (this.validateFile(file)) {
        this.addFile(file);
      }
    });
  }

  validateFile(file) {
    // Check file size
    if (file.size > this.maxFileSize) {
      this.showError(`File ${file.name} is too large. Maximum size is 10MB.`);
      return false;
    }

    // Check file type
    const fileExtension = "." + file.name.split(".").pop().toLowerCase();
    if (!this.allowedTypes.includes(fileExtension)) {
      this.showError(
        `File ${
          file.name
        } is not supported. Allowed types: ${this.allowedTypes.join(", ")}`
      );
      return false;
    }

    return true;
  }

  addFile(file) {
    const fileId = this.generateFileId();
    const fileObj = {
      id: fileId,
      file: file,
      name: file.name,
      size: file.size,
      type: file.type,
      status: "ready",
    };

    this.uploadedFiles.push(fileObj);
    this.updateFileList();
    this.updateAnalyzeButton();
    
    // Emit event for other components
    this.emit("filesUploaded", this.uploadedFiles);
  }

  removeFile(fileId) {
    this.uploadedFiles = this.uploadedFiles.filter((f) => f.id !== fileId);
    this.updateFileList();
    this.updateAnalyzeButton();
  }

  updateFileList() {
    const fileList = document.getElementById("fileList");
    if (!fileList) return;

    fileList.innerHTML = "";

    this.uploadedFiles.forEach((fileObj) => {
      const fileItem = this.createFileItem(fileObj);
      fileList.appendChild(fileItem);
    });
  }

  createFileItem(fileObj) {
    const div = document.createElement("div");
    div.className = "file-item d-flex justify-content-between align-items-center mb-2 p-2 border rounded";
    div.innerHTML = `
      <div class="file-info d-flex align-items-center">
        <i class="fas fa-file file-icon me-2"></i>
        <div class="file-details">
          <h6 class="mb-0">${fileObj.name}</h6>
          <small class="text-muted">${this.formatFileSize(fileObj.size)}</small>
        </div>
      </div>
      <div class="file-actions">
        <button class="btn btn-sm btn-outline-danger remove-file-btn" data-file-id="${fileObj.id}">
          <i class="fas fa-trash"></i>
        </button>
      </div>
    `;
    
    // Add event listener for remove button
    const removeBtn = div.querySelector(".remove-file-btn");
    removeBtn.addEventListener("click", () => {
      this.removeFile(fileObj.id);
    });
    
    return div;
  }

  updateAnalyzeButton() {
    const analyzeBtn = document.getElementById("analyzeBtn");
    if (!analyzeBtn) return;

    analyzeBtn.disabled = this.uploadedFiles.length === 0;
  }

  async analyzeFiles() {
    if (this.uploadedFiles.length === 0) return;

    const subject = document.getElementById("subjectSelect")?.value || "mathematics";
    const studentLevel = document.getElementById("levelSelect")?.value || "intermediate";

    // Emit analysis request event
    this.emit("analysisRequested", {
      files: this.uploadedFiles,
      subject,
      studentLevel
    });
  }

  async readFileContent(file) {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = (e) => resolve(e.target.result);
      reader.onerror = (e) => reject(e);

      if (file.type.startsWith("text/")) {
        reader.readAsText(file);
      } else {
        // For non-text files, just return filename and basic info
        resolve(
          `[File: ${file.name}, Type: ${file.type}, Size: ${this.formatFileSize(
            file.size
          )}]`
        );
      }
    });
  }

  formatFileSize(bytes) {
    if (bytes === 0) return "0 Bytes";
    const k = 1024;
    const sizes = ["Bytes", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
  }

  generateFileId() {
    return "file_" + Math.random().toString(36).substr(2, 9);
  }

  showError(message) {
    // Simple error display - could be enhanced with toast notifications
    alert(message);
  }

  // Event system for component communication
  on(event, callback) {
    if (!this.eventListeners[event]) {
      this.eventListeners[event] = [];
    }
    this.eventListeners[event].push(callback);
  }

  emit(event, data) {
    if (this.eventListeners[event]) {
      this.eventListeners[event].forEach(callback => callback(data));
    }
  }

  enableAnalysis() {
    const analyzeBtn = document.getElementById("analyzeBtn");
    if (analyzeBtn) {
      analyzeBtn.disabled = false;
    }
  }
}
