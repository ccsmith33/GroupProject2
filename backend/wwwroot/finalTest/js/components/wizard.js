// Wizard Component - 5-step onboarding flow

import { store } from "../state/store.js";
import { authState } from "../state/auth-state.js";
import { filesAPI } from "../api/files.js";
import { analysisAPI } from "../api/analysis.js";
import { CONFIG } from "../config.js";
import { logger } from "../utils/logger.js";

class Wizard {
  constructor() {
    this.currentStep = 1;
    this.totalSteps = 5;
    this.wizardData = {
      uploadedFiles: [],
      selectedSubject: "",
      selectedLevel: "highSchool",
      generatedQuiz: null,
      generatedGuide: null,
    };
    this.isInitialized = false;
  }

  // Initialize wizard
  init() {
    if (this.isInitialized) return;

    this.render();
    this.bindEvents();
    this.isInitialized = true;

    // Show wizard container
    const wizardContainer = document.getElementById("wizardContainer");
    if (wizardContainer) {
      wizardContainer.style.display = "block";
    }

    // Hide other views
    this.hideOtherViews();
  }

  // Render wizard
  render() {
    const wizardContainer = document.getElementById("wizardContainer");
    if (!wizardContainer) return;

    wizardContainer.innerHTML = `
            <div class="wizard-card">
                <div class="wizard-header">
                    <h1 class="wizard-title">Welcome to Study AI</h1>
                    <p class="wizard-subtitle">Let's get you started with your personalized learning experience</p>
                    
                    <div class="wizard-progress">
                        <div class="progress-container">
                            <span class="progress-text">Step ${
                              this.currentStep
                            } of ${this.totalSteps}</span>
                            <div class="progress-bar-container">
                                <div class="progress-bar-fill" style="width: ${
                                  (this.currentStep / this.totalSteps) * 100
                                }%"></div>
                            </div>
                            <span class="progress-text">${Math.round(
                              (this.currentStep / this.totalSteps) * 100
                            )}%</span>
                        </div>
                        <div class="step-indicators">
                            ${Array.from(
                              { length: this.totalSteps },
                              (_, i) => `
                                <div class="step-indicator ${
                                  i + 1 <= this.currentStep ? "active" : ""
                                } ${
                                i + 1 < this.currentStep ? "completed" : ""
                              }"></div>
                            `
                            ).join("")}
                        </div>
                    </div>
                </div>
                
                <div class="wizard-body">
                    ${this.renderStep()}
                </div>
                
                <div class="wizard-footer">
                    <div class="wizard-buttons">
                        ${
                          this.currentStep > 1
                            ? '<button class="wizard-btn wizard-btn-secondary" id="wizardPrevBtn">Previous</button>'
                            : ""
                        }
                        <button class="wizard-btn wizard-btn-primary" id="wizardNextBtn">
                            ${
                              this.currentStep === this.totalSteps
                                ? "Complete Setup"
                                : "Next"
                            }
                        </button>
                    </div>
                    ${
                      this.currentStep < this.totalSteps
                        ? '<a href="#" class="wizard-skip" id="wizardSkipBtn">Skip for now</a>'
                        : ""
                    }
                </div>
            </div>
        `;
  }

  // Render current step
  renderStep() {
    switch (this.currentStep) {
      case 1:
        return this.renderWelcomeStep();
      case 2:
        return this.renderUploadStep();
      case 3:
        return this.renderReviewStep();
      case 4:
        return this.renderStudyToolsStep();
      case 5:
        return this.renderCompleteStep();
      default:
        return this.renderWelcomeStep();
    }
  }

  // Step 1: Welcome
  renderWelcomeStep() {
    return `
            <div class="wizard-step active">
                <h3>Choose Your Learning Path</h3>
                <p>Get started with Study AI. You can try as a guest or create an account to save your progress.</p>
                
                ${
                  authState.isGuest()
                    ? `
                    <div class="wizard-guest-notice">
                        <i class="bi bi-info-circle"></i>
                        <strong>Guest Mode:</strong> You can explore all features, but your progress won't be saved.
                    </div>
                `
                    : ""
                }
                
                <div class="wizard-options">
                    <div class="wizard-option ${
                      authState.isGuest() ? "selected" : ""
                    }" data-option="guest">
                        <i class="bi bi-person"></i>
                        <h5>Continue as Guest</h5>
                        <p>Try all features without creating an account</p>
                    </div>
                    <div class="wizard-option ${
                      !authState.isGuest() ? "selected" : ""
                    }" data-option="auth">
                        <i class="bi bi-person-check"></i>
                        <h5>Sign In / Register</h5>
                        <p>Save your progress and access advanced features</p>
                    </div>
                </div>
                
                <div class="wizard-form" id="authForm" style="display: none;">
                    <div class="form-group">
                        <label class="form-label">Email</label>
                        <input type="email" class="form-control" id="authEmail" placeholder="Enter your email">
                    </div>
                    <div class="form-group">
                        <label class="form-label">Password</label>
                        <input type="password" class="form-control" id="authPassword" placeholder="Enter your password">
                    </div>
                    <div class="form-group" id="authNameGroup" style="display: none;">
                        <label class="form-label">Full Name</label>
                        <input type="text" class="form-control" id="authName" placeholder="Enter your full name">
                    </div>
                    <div class="form-group">
                        <button class="btn btn-primary w-100" id="authSubmitBtn">Sign In</button>
                        <button class="btn btn-outline-primary w-100 mt-2" id="authToggleBtn">Need an account? Register</button>
                    </div>
                </div>
            </div>
        `;
  }

  // Step 2: Upload Files
  renderUploadStep() {
    return `
            <div class="wizard-step active">
                <h3>Add Your Study Materials</h3>
                <p>Upload your study materials to get started. We'll automatically organize them by subject.</p>
                
                <div class="wizard-file-upload" id="fileUploadArea">
                    <i class="bi bi-cloud-upload"></i>
                    <h5>Drag & Drop Files Here</h5>
                    <p>or click to browse files</p>
                    <input type="file" id="fileInput" multiple accept=".pdf,.doc,.docx,.ppt,.pptx,.txt,.jpg,.jpeg,.png" style="display: none;">
                </div>
                
                <div class="wizard-file-list" id="fileList" style="display: none;">
                    <h5>Uploaded Files</h5>
                    <div id="fileItems"></div>
                </div>
                
                <div class="wizard-form mt-4">
                    <div class="row">
                        <div class="col-md-6">
                            <div class="form-group">
                                <label class="form-label">Subject (Optional)</label>
                                <input type="text" class="form-control" id="subjectInput" placeholder="e.g., Mathematics, Biology">
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="form-group">
                                <label class="form-label">Student Level</label>
                                <select class="form-control" id="levelSelect">
                                    ${CONFIG.KNOWLEDGE_LEVELS.map(
                                      (level) => `
                                        <option value="${level.value}" ${
                                        level.value === "highSchool"
                                          ? "selected"
                                          : ""
                                      }>
                                            ${level.label}
                                        </option>
                                    `
                                    ).join("")}
                                </select>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
  }

  // Step 3: Review Files
  renderReviewStep() {
    const files = this.wizardData.uploadedFiles;

    return `
            <div class="wizard-step active">
                <h3>Your Materials Are Ready!</h3>
                <p>We've organized your files by subject. You can review and modify the groupings.</p>
                
                <div class="wizard-preview">
                    <h4>Uploaded Files (${files.length})</h4>
                    <div class="wizard-preview-content">
                        ${
                          files.length === 0
                            ? `
                            <div class="empty-state">
                                <i class="bi bi-file-earmark"></i>
                                <h4>No files uploaded</h4>
                                <p>Go back to upload some study materials.</p>
                            </div>
                        `
                            : `
                            <div class="file-grid">
                                ${files
                                  .map(
                                    (file) => `
                                    <div class="file-item">
                                        <div class="file-icon" style="background-color: ${CONFIG.getSubjectColor(
                                          0
                                        )};">
                                            <i class="bi ${CONFIG.getFileIcon(
                                              file.fileType
                                            )}"></i>
                                        </div>
                                        <div class="file-details">
                                            <div class="file-name">${
                                              file.fileName
                                            }</div>
                                            <div class="file-meta">
                                                <span>${CONFIG.formatFileSize(
                                                  file.fileSize
                                                )}</span>
                                                <span>${
                                                  file.autoDetectedSubject ||
                                                  "Auto-detecting..."
                                                }</span>
                                            </div>
                                        </div>
                                    </div>
                                `
                                  )
                                  .join("")}
                            </div>
                        `
                        }
                    </div>
                </div>
            </div>
        `;
  }

  // Step 4: Study Tools
  renderStudyToolsStep() {
    return `
            <div class="wizard-step active">
                <h3>Let's Create Study Aids</h3>
                <p>Generate your first study guide or quiz to see how Study AI can help you learn.</p>
                
                <div class="wizard-options">
                    <div class="wizard-option" data-tool="quiz">
                        <i class="bi bi-question-circle"></i>
                        <h5>Generate Quiz</h5>
                        <p>Test your knowledge with AI-generated questions</p>
                    </div>
                    <div class="wizard-option" data-tool="guide">
                        <i class="bi bi-book"></i>
                        <h5>Create Study Guide</h5>
                        <p>Get a comprehensive study guide from your materials</p>
                    </div>
                </div>
                
                <div class="wizard-form" id="studyToolForm" style="display: none;">
                    <div class="form-group">
                        <label class="form-label">What would you like to study?</label>
                        <input type="text" class="form-control" id="studyPrompt" placeholder="e.g., Explain calculus derivatives, Create a quiz about photosynthesis">
                    </div>
                    <div class="form-group">
                        <button class="btn btn-primary w-100" id="generateToolBtn">
                            <span class="loading" id="toolLoading" style="display: none;"></span>
                            <span id="toolBtnText">Generate</span>
                        </button>
                    </div>
                </div>
                
                <div class="wizard-preview" id="toolPreview" style="display: none;">
                    <h4 id="toolPreviewTitle">Generated Content</h4>
                    <div class="wizard-preview-content" id="toolPreviewContent"></div>
                </div>
            </div>
        `;
  }

  // Step 5: Complete
  renderCompleteStep() {
    const isGuest = authState.isGuest();
    const filesCount = this.wizardData.uploadedFiles.length;

    return `
            <div class="wizard-step active">
                <div class="wizard-success">
                    <i class="bi bi-check-circle-fill"></i>
                    <h3>You're All Set!</h3>
                    <p>Welcome to Study AI! You're ready to start your personalized learning journey.</p>
                    
                    <div class="row mt-4">
                        <div class="col-md-6">
                            <div class="card">
                                <div class="card-body text-center">
                                    <i class="bi bi-folder text-primary" style="font-size: 2rem;"></i>
                                    <h5 class="mt-2">${filesCount} Files Uploaded</h5>
                                    <p class="text-muted">Your study materials are ready</p>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="card">
                                <div class="card-body text-center">
                                    <i class="bi bi-robot text-primary" style="font-size: 2rem;"></i>
                                    <h5 class="mt-2">AI-Powered Learning</h5>
                                    <p class="text-muted">Smart study tools at your fingertips</p>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    ${
                      isGuest
                        ? `
                        <div class="alert alert-info mt-4">
                            <i class="bi bi-info-circle me-2"></i>
                            <strong>Guest Mode:</strong> Your progress won't be saved. 
                            <a href="#" id="createAccountLink">Create an account</a> to save your work and access advanced features.
                        </div>
                    `
                        : ""
                    }
                </div>
            </div>
        `;
  }

  // Bind events
  bindEvents() {
    // Navigation buttons
    document
      .getElementById("wizardNextBtn")
      ?.addEventListener("click", () => this.nextStep());
    document
      .getElementById("wizardPrevBtn")
      ?.addEventListener("click", () => this.prevStep());
    document.getElementById("wizardSkipBtn")?.addEventListener("click", (e) => {
      e.preventDefault();
      this.completeWizard();
    });

    // Wizard action buttons
    document.addEventListener("click", (e) => {
      if (e.target.matches("[data-wizard-action]")) {
        this.handleWizardAction(e.target);
      }
    });

    // Step 1: Auth options
    document
      .querySelectorAll(".wizard-option[data-option]")
      .forEach((option) => {
        option.addEventListener("click", () =>
          this.selectAuthOption(option.dataset.option)
        );
      });

    // Auth form
    document
      .getElementById("authSubmitBtn")
      ?.addEventListener("click", () => this.handleAuth());
    document.getElementById("authToggleBtn")?.addEventListener("click", (e) => {
      e.preventDefault();
      this.toggleAuthMode();
    });

    // Step 2: File upload
    document.getElementById("fileUploadArea")?.addEventListener("click", () => {
      document.getElementById("fileInput").click();
    });

    document.getElementById("fileInput")?.addEventListener("change", (e) => {
      this.handleFileUpload(e.target.files);
    });

    // Drag and drop
    const uploadArea = document.getElementById("fileUploadArea");
    if (uploadArea) {
      uploadArea.addEventListener("dragover", (e) => {
        e.preventDefault();
        uploadArea.classList.add("dragover");
      });

      uploadArea.addEventListener("dragleave", () => {
        uploadArea.classList.remove("dragover");
      });

      uploadArea.addEventListener("drop", (e) => {
        e.preventDefault();
        uploadArea.classList.remove("dragover");
        this.handleFileUpload(e.dataTransfer.files);
      });
    }

    // Step 4: Study tools
    document.querySelectorAll(".wizard-option[data-tool]").forEach((option) => {
      option.addEventListener("click", () =>
        this.selectStudyTool(option.dataset.tool)
      );
    });

    document
      .getElementById("generateToolBtn")
      ?.addEventListener("click", () => this.generateStudyTool());

    // Step 5: Create account link
    document
      .getElementById("createAccountLink")
      ?.addEventListener("click", (e) => {
        e.preventDefault();
        this.showCreateAccountModal();
      });
  }

  // Navigation methods
  nextStep() {
    if (this.currentStep < this.totalSteps) {
      this.currentStep++;
      this.updateWizardData();
      this.render();
      this.bindEvents();
    } else {
      this.completeWizard();
    }
  }

  prevStep() {
    if (this.currentStep > 1) {
      this.currentStep--;
      this.render();
      this.bindEvents();
    }
  }

  // Handle wizard actions
  handleWizardAction(button) {
    const action = button.dataset.wizardAction;

    switch (action) {
      case "take-quiz":
        this.takeQuiz();
        break;
      case "show-auth-modal":
        window.app?.showAuthModal();
        break;
    }
  }

  // Step 1: Auth handling
  selectAuthOption(option) {
    document
      .querySelectorAll(".wizard-option[data-option]")
      .forEach((opt) => opt.classList.remove("selected"));
    document
      .querySelector(`.wizard-option[data-option="${option}"]`)
      .classList.add("selected");

    const authForm = document.getElementById("authForm");
    if (option === "auth") {
      authForm.style.display = "block";
    } else {
      authForm.style.display = "none";
    }
  }

  toggleAuthMode() {
    const submitBtn = document.getElementById("authSubmitBtn");
    const toggleBtn = document.getElementById("authToggleBtn");
    const nameGroup = document.getElementById("authNameGroup");

    if (submitBtn.textContent === "Sign In") {
      submitBtn.textContent = "Create Account";
      toggleBtn.textContent = "Already have an account? Sign In";
      nameGroup.style.display = "block";
    } else {
      submitBtn.textContent = "Sign In";
      toggleBtn.textContent = "Need an account? Register";
      nameGroup.style.display = "none";
    }
  }

  async handleAuth() {
    const email = document.getElementById("authEmail").value;
    const password = document.getElementById("authPassword").value;
    const name = document.getElementById("authName").value;
    const isRegister =
      document.getElementById("authSubmitBtn").textContent === "Create Account";

    if (!email || !password) {
      store.actions.addNotification({
        type: "error",
        title: "Validation Error",
        message: "Please fill in all required fields.",
      });
      return;
    }

    if (isRegister && !name) {
      store.actions.addNotification({
        type: "error",
        title: "Validation Error",
        message: "Please enter your full name.",
      });
      return;
    }

    try {
      let result;
      if (isRegister) {
        result = await authState.register(email, password, name);
      } else {
        result = await authState.login(email, password);
      }

      if (result.success) {
        this.nextStep();
      }
    } catch (error) {
      logger.error("Auth error:", error);
    }
  }

  // Step 2: File upload handling
  async handleFileUpload(files) {
    if (!files || files.length === 0) return;

    const fileArray = Array.from(files);
    const subject = document.getElementById("subjectInput").value;
    const level = document.getElementById("levelSelect").value;

    // Check guest limits
    if (authState.isGuest() && !authState.checkGuestLimits("uploadFile")) {
      store.actions.addNotification({
        type: "warning",
        title: "Upload Limit Reached",
        message: authState.getGuestLimitMessage("uploadFile"),
      });
      return;
    }

    for (const file of fileArray) {
      try {
        const result = await filesAPI.uploadFile(file, "guest", subject, level);

        if (result.success || result.data) {
          const uploadedFile = result.data || result;
          this.wizardData.uploadedFiles.push(uploadedFile);

          store.actions.addNotification({
            type: "success",
            title: "File Uploaded",
            message: `${file.name} uploaded successfully!`,
          });
        }
      } catch (error) {
        store.actions.addNotification({
          type: "error",
          title: "Upload Failed",
          message: `Failed to upload ${file.name}: ${error.message}`,
        });
      }
    }

    this.updateFileList();
  }

  updateFileList() {
    const fileList = document.getElementById("fileList");
    const fileItems = document.getElementById("fileItems");

    if (this.wizardData.uploadedFiles.length > 0) {
      fileList.style.display = "block";
      fileItems.innerHTML = this.wizardData.uploadedFiles
        .map(
          (file) => `
                <div class="wizard-file-item">
                    <div class="wizard-file-icon" style="background-color: ${CONFIG.getSubjectColor(
                      0
                    )};">
                        <i class="bi ${CONFIG.getFileIcon(file.fileType)}"></i>
                    </div>
                    <div class="wizard-file-details">
                        <div class="wizard-file-name">${file.fileName}</div>
                        <div class="wizard-file-meta">
                            <span>${CONFIG.formatFileSize(file.fileSize)}</span>
                            <span class="wizard-file-status processing">Processing...</span>
                        </div>
                    </div>
                </div>
            `
        )
        .join("");
    }
  }

  // Step 4: Study tools
  selectStudyTool(tool) {
    document
      .querySelectorAll(".wizard-option[data-tool]")
      .forEach((opt) => opt.classList.remove("selected"));
    document
      .querySelector(`.wizard-option[data-tool="${tool}"]`)
      .classList.add("selected");

    const form = document.getElementById("studyToolForm");
    form.style.display = "block";

    const prompt = document.getElementById("studyPrompt");
    prompt.placeholder =
      tool === "quiz"
        ? "e.g., Create a quiz about calculus derivatives"
        : "e.g., Explain photosynthesis in detail";
  }

  async generateStudyTool() {
    const tool = document.querySelector(".wizard-option[data-tool].selected")
      ?.dataset.tool;
    const prompt = document.getElementById("studyPrompt").value;

    if (!tool || !prompt.trim()) {
      store.actions.addNotification({
        type: "error",
        title: "Validation Error",
        message: "Please select a tool and enter a prompt.",
      });
      return;
    }

    const loadingBtn = document.getElementById("toolLoading");
    const btnText = document.getElementById("toolBtnText");
    const generateBtn = document.getElementById("generateToolBtn");

    loadingBtn.style.display = "inline-block";
    btnText.textContent = "Generating...";
    generateBtn.disabled = true;

    try {
      let result;
      if (tool === "quiz") {
        result = await analysisAPI.generateQuiz(
          prompt,
          "guest",
          this.wizardData.selectedLevel,
          5,
          this.wizardData.uploadedFiles
        );
        this.wizardData.generatedQuiz = result.data || result;
      } else {
        result = await analysisAPI.generateStudyGuide(
          prompt,
          "guest",
          this.wizardData.uploadedFiles,
          []
        );
        this.wizardData.generatedGuide = result.data || result;
      }

      this.showToolPreview(tool, result.data || result);
    } catch (error) {
      store.actions.addNotification({
        type: "error",
        title: "Generation Failed",
        message: `Failed to generate ${tool}: ${error.message}`,
      });
    } finally {
      loadingBtn.style.display = "none";
      btnText.textContent = "Generate";
      generateBtn.disabled = false;
    }
  }

  showToolPreview(tool, data) {
    const preview = document.getElementById("toolPreview");
    const title = document.getElementById("toolPreviewTitle");
    const content = document.getElementById("toolPreviewContent");

    preview.style.display = "block";
    title.textContent =
      tool === "quiz" ? "Generated Quiz" : "Generated Study Guide";

    if (tool === "quiz") {
      const questions = analysisAPI.parseQuizQuestions(data);
      content.innerHTML = `
                <div class="quiz-preview">
                    <h5>Quiz: ${data.title || "Generated Quiz"}</h5>
                    <p><strong>Questions:</strong> ${questions.length}</p>
                    <p><strong>Subject:</strong> ${
                      data.subject || "General"
                    }</p>
                    <p><strong>Level:</strong> ${
                      data.level || "High School"
                    }</p>
                    <div class="mt-3">
                        <button class="btn btn-primary" data-wizard-action="take-quiz">Take Quiz</button>
                    </div>
                </div>
            `;
    } else {
      content.innerHTML = `
                <div class="study-guide-preview">
                    <h5>Study Guide: ${
                      data.title || "Generated Study Guide"
                    }</h5>
                    <div class="study-guide-content" style="max-height: 300px; overflow-y: auto; border: 1px solid #e9ecef; padding: 1rem; border-radius: 0.5rem; background-color: #f8f9fa;">
                        ${
                          data.content
                            ? data.content.replace(/\n/g, "<br>")
                            : "Content not available"
                        }
                    </div>
                </div>
            `;
    }
  }

  // Complete wizard
  completeWizard() {
    store.actions.completeWizard();
    this.hideWizard();
  }

  hideWizard() {
    const wizardContainer = document.getElementById("wizardContainer");
    if (wizardContainer) {
      wizardContainer.style.display = "none";
    }
  }

  hideOtherViews() {
    const views = [
      "dashboardView",
      "filesView",
      "studyView",
      "progressView",
      "profileView",
    ];
    views.forEach((viewId) => {
      const view = document.getElementById(viewId);
      if (view) {
        view.style.display = "none";
      }
    });
  }

  updateWizardData() {
    // Update wizard data based on current step
    if (this.currentStep === 2) {
      this.wizardData.selectedSubject =
        document.getElementById("subjectInput")?.value || "";
      this.wizardData.selectedLevel =
        document.getElementById("levelSelect")?.value || "highSchool";
    }
  }

  showCreateAccountModal() {
    store.actions.openModal("guestPrompt");
  }
}

// Create global wizard instance
const wizard = new Wizard();

// Export for global access
window.wizard = wizard;

export { wizard };
