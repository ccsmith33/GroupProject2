// Import dependencies
import AnalysisAPI from "../api/analysis.js";

// File upload component functionality
class FileUpload {
  constructor() {
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
    this.init();
  }

  init() {
    this.setupEventListeners();
    this.setupDragAndDrop();
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
    div.className = "file-item";
    div.innerHTML = `
            <div class="file-info">
                <i class="fas fa-file file-icon"></i>
                <div class="file-details">
                    <h6>${fileObj.name}</h6>
                    <small>${this.formatFileSize(fileObj.size)}</small>
                </div>
            </div>
            <div class="file-actions">
                <button class="btn btn-sm btn-outline-danger" onclick="fileUpload.removeFile('${
                  fileObj.id
                }')">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        `;
    return div;
  }

  updateAnalyzeButton() {
    const analyzeBtn = document.getElementById("analyzeBtn");
    if (!analyzeBtn) return;

    analyzeBtn.disabled = this.uploadedFiles.length === 0;
  }

  async analyzeFiles() {
    if (this.uploadedFiles.length === 0) return;

    const subject =
      document.getElementById("subjectSelect")?.value || "mathematics";
    const studentLevel =
      document.getElementById("levelSelect")?.value || "intermediate";

    // Show analysis results section
    const resultsSection = document.getElementById("analysisResults");
    if (resultsSection) {
      resultsSection.style.display = "block";
      resultsSection.scrollIntoView({ behavior: "smooth" });
    }

    // Combine all file contents for analysis
    let combinedContent = "";
    for (const fileObj of this.uploadedFiles) {
      const content = await this.readFileContent(fileObj.file);
      combinedContent += `\n\n--- ${fileObj.name} ---\n${content}`;
    }

    // Call analysis API
    try {
      const result = await analysisAPI.analyzeWithDelay(
        combinedContent,
        "mixed",
        subject,
        studentLevel
      );

      this.displayAnalysisResults(result);
    } catch (error) {
      console.error("Analysis failed:", error);
      this.showError("Analysis failed. Please try again.");
    }
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

  displayAnalysisResults(result) {
    // Update score
    const scoreElement = document.getElementById("analysisScore");
    if (scoreElement) {
      scoreElement.textContent = `${result.overallScore}%`;
      scoreElement.className = `badge ${this.getScoreClass(
        result.overallScore
      )} fs-6`;
    }

    // Update summary
    const summaryElement = document.getElementById("analysisSummary");
    if (summaryElement) {
      summaryElement.textContent = result.feedback.summary;
    }

    // Update detailed scores
    this.updateScoreElement(
      "conceptScore",
      result.detailedAnalysis.conceptUnderstanding
    );
    this.updateScoreElement(
      "problemScore",
      result.detailedAnalysis.problemSolving
    );
    this.updateScoreElement(
      "completenessScore",
      result.detailedAnalysis.completeness
    );
    this.updateScoreElement("accuracyScore", result.detailedAnalysis.accuracy);

    // Update lists
    this.updateList("strengthsList", result.feedback.strengths);
    this.updateList("weakAreasList", result.feedback.weakAreas);
    this.updateList("priorityTopicsList", result.studyPlan.priorityTopics);
    this.updateList("nextStepsList", result.studyPlan.nextSteps);
    this.updateList("recommendationsList", result.feedback.recommendations);
  }

  updateScoreElement(elementId, score) {
    const element = document.getElementById(elementId);
    if (element) {
      element.textContent = score;
    }
  }

  updateList(elementId, items) {
    const element = document.getElementById(elementId);
    if (!element) return;

    element.innerHTML = "";
    items.forEach((item) => {
      const li = document.createElement("li");
      li.textContent = item;
      li.className = "mb-1";
      element.appendChild(li);
    });
  }

  getScoreClass(score) {
    if (score >= 85) return "bg-success";
    if (score >= 70) return "bg-info";
    if (score >= 50) return "bg-warning";
    return "bg-danger";
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
}

// Create global instance
const fileUpload = new FileUpload();

// Export for module usage
export default FileUpload;
