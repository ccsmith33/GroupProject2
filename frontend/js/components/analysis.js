// Analysis component for displaying analysis results
class Analysis {
  constructor(container) {
    this.container = container;
    this.isAnalyzing = false;
    this.eventListeners = {};
    this.init();
  }

  init() {
    this.render();
  }

  render() {
    this.container.innerHTML = `
      <div id="analysisResults" class="row mt-4" style="display: none;">
        <div class="col-12">
          <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
              <h5>Analysis Results</h5>
              <span id="analysisScore" class="badge fs-6"></span>
            </div>
            <div class="card-body">
              <!-- Loading State -->
              <div id="analysisLoading" class="text-center">
                <div class="spinner-border" role="status">
                  <span class="visually-hidden">Analyzing...</span>
                </div>
                <p class="mt-2">AI is analyzing your materials...</p>
              </div>

              <!-- Results Content -->
              <div id="analysisContent" style="display: none;">
                <!-- Summary -->
                <div class="mb-4">
                  <h6>Summary</h6>
                  <p id="analysisSummary" class="text-muted"></p>
                </div>

                <!-- Detailed Analysis -->
                <div class="row mb-4">
                  <div class="col-md-3">
                    <div class="text-center">
                      <div class="h4 text-primary" id="conceptScore">0</div>
                      <small class="text-muted">Concept Understanding</small>
                    </div>
                  </div>
                  <div class="col-md-3">
                    <div class="text-center">
                      <div class="h4 text-info" id="problemScore">0</div>
                      <small class="text-muted">Problem Solving</small>
                    </div>
                  </div>
                  <div class="col-md-3">
                    <div class="text-center">
                      <div class="h4 text-warning" id="completenessScore">0</div>
                      <small class="text-muted">Completeness</small>
                    </div>
                  </div>
                  <div class="col-md-3">
                    <div class="text-center">
                      <div class="h4 text-success" id="accuracyScore">0</div>
                      <small class="text-muted">Accuracy</small>
                    </div>
                  </div>
                </div>

                <!-- Strengths and Weaknesses -->
                <div class="row mb-4">
                  <div class="col-md-6">
                    <h6 class="text-success">Strengths</h6>
                    <ul id="strengthsList" class="list-unstyled"></ul>
                  </div>
                  <div class="col-md-6">
                    <h6 class="text-danger">Areas for Improvement</h6>
                    <ul id="weakAreasList" class="list-unstyled"></ul>
                  </div>
                </div>

                <!-- Study Plan -->
                <div class="mb-4">
                  <h6>Study Plan</h6>
                  <div class="row">
                    <div class="col-md-6">
                      <h6 class="text-primary">Priority Topics</h6>
                      <ul id="priorityTopicsList" class="list-unstyled"></ul>
                    </div>
                    <div class="col-md-6">
                      <h6 class="text-info">Next Steps</h6>
                      <ul id="nextStepsList" class="list-unstyled"></ul>
                    </div>
                  </div>
                </div>

                <!-- Recommendations -->
                <div class="mb-4">
                  <h6>Recommendations</h6>
                  <ul id="recommendationsList" class="list-unstyled"></ul>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  async startAnalysis(data) {
    this.isAnalyzing = true;
    this.showLoadingState();

    try {
      // Combine all file contents for analysis
      let combinedContent = "";
      for (const fileObj of data.files) {
        const content = await this.readFileContent(fileObj.file);
        combinedContent += `\n\n--- ${fileObj.name} ---\n${content}`;
      }

      // Use mock data for now instead of API
      const result = this.getMockAnalysisResult(data.subject, data.studentLevel);

      this.displayResults(result);
      this.emit("analysisComplete", result);

    } catch (error) {
      console.error("Analysis failed:", error);
      this.showError("Analysis failed. Please try again.");
    } finally {
      this.isAnalyzing = false;
    }
  }

  getMockAnalysisResult(subject, level) {
    // Return mock analysis data
    return {
      overallScore: Math.floor(Math.random() * 40) + 60, // 60-100
      feedback: {
        summary: `Your ${subject} work shows ${level} level understanding with room for improvement.`,
        strengths: [
          "Good problem-solving approach",
          "Clear explanations",
          "Logical reasoning"
        ],
        weakAreas: [
          "Need more practice with complex problems",
          "Work on showing all steps clearly"
        ],
        recommendations: [
          "Practice more similar problems",
          "Review fundamental concepts",
          "Ask for help when stuck"
        ]
      },
      detailedAnalysis: {
        conceptUnderstanding: Math.floor(Math.random() * 30) + 70,
        problemSolving: Math.floor(Math.random() * 30) + 70,
        completeness: Math.floor(Math.random() * 30) + 70,
        accuracy: Math.floor(Math.random() * 30) + 70
      },
      studyPlan: {
        priorityTopics: [
          "Advanced problem solving",
          "Concept application",
          "Practice exercises"
        ],
        nextSteps: [
          "Complete practice problems",
          "Review weak areas",
          "Seek additional help"
        ]
      }
    };
  }

  showLoadingState() {
    const resultsSection = document.getElementById("analysisResults");
    const loadingDiv = document.getElementById("analysisLoading");
    const contentDiv = document.getElementById("analysisContent");

    if (resultsSection) {
      resultsSection.style.display = "block";
      resultsSection.scrollIntoView({ behavior: "smooth" });
    }

    if (loadingDiv) {
      loadingDiv.style.display = "block";
    }

    if (contentDiv) {
      contentDiv.style.display = "none";
    }
  }

  displayResults(result) {
    const loadingDiv = document.getElementById("analysisLoading");
    const contentDiv = document.getElementById("analysisContent");

    if (loadingDiv) {
      loadingDiv.style.display = "none";
    }

    if (contentDiv) {
      contentDiv.style.display = "block";
    }

    // Update score
    const scoreElement = document.getElementById("analysisScore");
    if (scoreElement) {
      scoreElement.textContent = `${result.overallScore}%`;
      scoreElement.className = `badge ${this.getScoreClass(result.overallScore)} fs-6`;
    }

    // Update summary
    const summaryElement = document.getElementById("analysisSummary");
    if (summaryElement) {
      summaryElement.textContent = result.feedback.summary;
    }

    // Update detailed scores
    this.updateScoreElement("conceptScore", result.detailedAnalysis.conceptUnderstanding);
    this.updateScoreElement("problemScore", result.detailedAnalysis.problemSolving);
    this.updateScoreElement("completenessScore", result.detailedAnalysis.completeness);
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
          `[File: ${file.name}, Type: ${file.type}, Size: ${this.formatFileSize(file.size)}]`
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

  showError(message) {
    const loadingDiv = document.getElementById("analysisLoading");
    if (loadingDiv) {
      loadingDiv.innerHTML = `
        <div class="alert alert-danger" role="alert">
          <i class="fas fa-exclamation-triangle"></i>
          ${message}
        </div>
      `;
    }
  }

  enableAnalysis() {
    // This method can be called when files are uploaded
    // to enable the analysis button
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
}