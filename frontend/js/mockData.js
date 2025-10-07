// Mock data for frontend development
const MockData = {
  // Sample analysis results for different scenarios
  goodAnalysis: {
    analysisId: "mock-good-001",
    timestamp: "2024-01-01T00:00:00Z",
    overallScore: 92,
    feedback: {
      summary:
        "Excellent work! You demonstrate strong understanding of calculus concepts and show clear problem-solving skills.",
      strengths: [
        "Clear step-by-step solutions",
        "Correct application of derivative rules",
        "Good use of mathematical notation",
        "Shows understanding of chain rule",
      ],
      weakAreas: [
        "Minor calculation errors in complex problems",
        "Could show more work for partial credit",
      ],
      recommendations: [
        "Double-check arithmetic in multi-step problems",
        "Practice more integration techniques",
        "Review limits and continuity concepts",
      ],
    },
    detailedAnalysis: {
      conceptUnderstanding: 95,
      problemSolving: 90,
      completeness: 88,
      accuracy: 92,
    },
    studyPlan: {
      priorityTopics: [
        "Integration by parts",
        "Trigonometric integrals",
        "Improper integrals",
      ],
      suggestedResources: [
        "Khan Academy Calculus II",
        "Paul's Online Math Notes",
        "Practice problems from textbook Chapter 7",
      ],
      estimatedTime: "3-4 hours",
      nextSteps: [
        "Complete integration practice set",
        "Review trigonometric identities",
        "Prepare for next exam",
      ],
    },
    confidence: 0.92,
  },

  averageAnalysis: {
    analysisId: "mock-average-001",
    timestamp: "2024-01-01T00:00:00Z",
    overallScore: 75,
    feedback: {
      summary:
        "Good progress overall. You understand the basic concepts but need more practice with complex problems.",
      strengths: [
        "Understands basic derivative rules",
        "Shows work clearly",
        "Attempts all problems",
      ],
      weakAreas: [
        "Struggles with chain rule applications",
        "Inconsistent with algebraic manipulation",
        "Needs more practice with word problems",
      ],
      recommendations: [
        "Focus on chain rule practice",
        "Review algebra fundamentals",
        "Work through more example problems",
      ],
    },
    detailedAnalysis: {
      conceptUnderstanding: 70,
      problemSolving: 75,
      completeness: 80,
      accuracy: 75,
    },
    studyPlan: {
      priorityTopics: [
        "Chain rule applications",
        "Algebraic manipulation",
        "Word problem setup",
      ],
      suggestedResources: [
        "Chain rule practice worksheets",
        "Algebra review materials",
        "Step-by-step problem solving guides",
      ],
      estimatedTime: "5-6 hours",
      nextSteps: [
        "Complete chain rule practice set",
        "Review algebra basics",
        "Schedule study group session",
      ],
    },
    confidence: 0.78,
  },

  poorAnalysis: {
    analysisId: "mock-poor-001",
    timestamp: "2024-01-01T00:00:00Z",
    overallScore: 45,
    feedback: {
      summary:
        "Significant improvement needed. Focus on understanding fundamental concepts before attempting complex problems.",
      strengths: [
        "Shows effort in attempting problems",
        "Basic arithmetic is correct",
      ],
      weakAreas: [
        "Fundamental understanding of derivatives",
        "Algebraic manipulation skills",
        "Problem-solving approach",
        "Mathematical notation",
      ],
      recommendations: [
        "Review basic derivative rules",
        "Practice algebra fundamentals",
        "Work with a tutor or study group",
        "Start with simpler problems",
      ],
    },
    detailedAnalysis: {
      conceptUnderstanding: 40,
      problemSolving: 35,
      completeness: 50,
      accuracy: 45,
    },
    studyPlan: {
      priorityTopics: [
        "Basic derivative rules",
        "Algebra fundamentals",
        "Function notation",
        "Graph interpretation",
      ],
      suggestedResources: [
        "Khan Academy Pre-calculus review",
        "Basic algebra practice",
        "One-on-one tutoring",
        "Study group sessions",
      ],
      estimatedTime: "10-12 hours",
      nextSteps: [
        "Schedule tutoring appointment",
        "Complete pre-calculus review",
        "Practice basic derivative rules daily",
        "Attend all office hours",
      ],
    },
    confidence: 0.65,
  },

  // Helper function to get random analysis
  getRandomAnalysis: function () {
    const analyses = [
      this.goodAnalysis,
      this.averageAnalysis,
      this.poorAnalysis,
    ];
    return analyses[Math.floor(Math.random() * analyses.length)];
  },

  // Helper function to get analysis by score range
  getAnalysisByScore: function (score) {
    if (score >= 85) return this.goodAnalysis;
    if (score >= 60) return this.averageAnalysis;
    return this.poorAnalysis;
  },
};

// Export for use in other files
if (typeof module !== "undefined" && module.exports) {
  module.exports = MockData;
}
