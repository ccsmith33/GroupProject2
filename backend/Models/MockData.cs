using StudentStudyAI.Models;
using System.Text.Json;

namespace StudentStudyAI.Models
{
    public static class MockData
    {
        public static AnalysisResult GetGoodAnalysis()
        {
            var feedback = new Feedback
            {
                Summary = "Excellent work! You demonstrate strong understanding of calculus concepts and show clear problem-solving skills.",
                Strengths = new List<string>
                {
                    "Clear step-by-step solutions",
                    "Correct application of derivative rules",
                    "Good use of mathematical notation",
                    "Shows understanding of chain rule"
                },
                WeakAreas = new List<string>
                {
                    "Minor calculation errors in complex problems",
                    "Could show more work for partial credit"
                },
                Recommendations = new List<string>
                {
                    "Double-check arithmetic in multi-step problems",
                    "Practice more integration techniques",
                    "Review limits and continuity concepts"
                }
            };

            var studyPlan = new StudyPlan
            {
                PriorityTopics = new List<string>
                {
                    "Integration by parts",
                    "Trigonometric integrals",
                    "Improper integrals"
                },
                SuggestedResources = new List<string>
                {
                    "Khan Academy Calculus II",
                    "Paul's Online Math Notes",
                    "Practice problems from textbook Chapter 7"
                },
                EstimatedTime = "2-3 hours per week",
                NextSteps = new List<string>
                {
                    "Complete integration practice set",
                    "Review trigonometric identities",
                    "Work on time management for exams"
                }
            };

            return new AnalysisResult
            {
                OverallScore = 92,
                FeedbackData = feedback,
                StudyPlanData = studyPlan,
                Confidence = 0.92
            };
        }

        public static AnalysisResult GetAverageAnalysis()
        {
            var feedback = new Feedback
            {
                Summary = "Good effort! You show understanding of basic concepts but need more practice with complex problems.",
                Strengths = new List<string>
                {
                    "Understands basic derivative rules",
                    "Shows work clearly",
                    "Attempts all problems"
                },
                WeakAreas = new List<string>
                {
                    "Struggles with chain rule applications",
                    "Needs more practice with integration",
                    "Algebraic manipulation errors"
                },
                Recommendations = new List<string>
                {
                    "Review chain rule thoroughly",
                    "Practice more integration problems",
                    "Work on algebraic simplification"
                }
            };

            var studyPlan = new StudyPlan
            {
                PriorityTopics = new List<string>
                {
                    "Chain rule applications",
                    "Basic integration techniques",
                    "Algebraic simplification"
                },
                SuggestedResources = new List<string>
                {
                    "Textbook Chapter 3 review",
                    "Online practice problems",
                    "Study group sessions"
                },
                EstimatedTime = "4-5 hours per week",
                NextSteps = new List<string>
                {
                    "Complete chain rule worksheet",
                    "Practice integration daily",
                    "Join study group"
                }
            };

            return new AnalysisResult
            {
                OverallScore = 75,
                FeedbackData = feedback,
                StudyPlanData = studyPlan,
                Confidence = 0.75
            };
        }

        public static AnalysisResult GetPoorAnalysis()
        {
            var feedback = new Feedback
            {
                Summary = "Needs significant improvement. Focus on understanding fundamental concepts before attempting complex problems.",
                Strengths = new List<string>
                {
                    "Shows effort in attempting problems",
                    "Basic arithmetic is correct"
                },
                WeakAreas = new List<string>
                {
                    "Fundamental concept gaps",
                    "Incorrect application of rules",
                    "Missing key steps in solutions"
                },
                Recommendations = new List<string>
                {
                    "Review basic derivative rules",
                    "Practice fundamental concepts",
                    "Seek additional help from instructor"
                }
            };

            var studyPlan = new StudyPlan
            {
                PriorityTopics = new List<string>
                {
                    "Basic derivative rules",
                    "Function composition",
                    "Limit concepts"
                },
                SuggestedResources = new List<string>
                {
                    "Khan Academy Pre-calculus review",
                    "Textbook Chapter 1-2",
                    "Office hours with instructor"
                },
                EstimatedTime = "6-8 hours per week",
                NextSteps = new List<string>
                {
                    "Schedule meeting with instructor",
                    "Complete pre-calculus review",
                    "Practice basic derivatives daily"
                }
            };

            return new AnalysisResult
            {
                OverallScore = 45,
                FeedbackData = feedback,
                StudyPlanData = studyPlan,
                Confidence = 0.45
            };
        }

        // Static properties for easy access
        public static AnalysisResult goodAnalysis => GetGoodAnalysis();
        public static AnalysisResult averageAnalysis => GetAverageAnalysis();
        public static AnalysisResult poorAnalysis => GetPoorAnalysis();
    }
}