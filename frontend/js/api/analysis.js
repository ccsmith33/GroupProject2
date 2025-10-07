// Import mock data
import { MockData } from "../mockData.js";

// API calls for analysis functionality
class AnalysisAPI {
  constructor() {
    this.baseUrl = "http://localhost:5000/api"; // Backend API URL
  }

  async analyzeContent(content, fileType, subject, studentLevel) {
    try {
      const response = await fetch(`${this.baseUrl}/analysis/analyze`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          content: content,
          fileType: fileType,
          subject: subject,
          studentLevel: studentLevel,
        }),
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error("Analysis API error:", error);
      // Fallback to mock data for development
      return this.getMockAnalysis(content);
    }
  }

  async getAnalysisStatus(analysisId) {
    try {
      const response = await fetch(
        `${this.baseUrl}/analysis/status/${analysisId}`
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error("Status API error:", error);
      return { analysisId, status: "completed", progress: 100 };
    }
  }

  // Mock data fallback for development
  getMockAnalysis(content) {
    const contentLength = content.length;

    if (contentLength > 1000) {
      return MockData.goodAnalysis;
    } else if (contentLength > 500) {
      return MockData.averageAnalysis;
    } else {
      return MockData.poorAnalysis;
    }
  }

  // Simulate analysis with delay for realistic UX
  async analyzeWithDelay(content, fileType, subject, studentLevel) {
    // Show loading state
    this.showLoadingState();

    // Simulate processing time
    await new Promise((resolve) => setTimeout(resolve, 2000));

    // Get analysis result
    const result = await this.analyzeContent(
      content,
      fileType,
      subject,
      studentLevel
    );

    // Hide loading state
    this.hideLoadingState();

    return result;
  }

  showLoadingState() {
    const loadingElement = document.getElementById("analysisLoading");
    const contentElement = document.getElementById("analysisContent");

    if (loadingElement) loadingElement.style.display = "block";
    if (contentElement) contentElement.style.display = "none";
  }

  hideLoadingState() {
    const loadingElement = document.getElementById("analysisLoading");
    const contentElement = document.getElementById("analysisContent");

    if (loadingElement) loadingElement.style.display = "none";
    if (contentElement) contentElement.style.display = "block";
  }
}

// Create global instance
const analysisAPI = new AnalysisAPI();

// Export for module usage
export default AnalysisAPI;
