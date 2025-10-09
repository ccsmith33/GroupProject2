using StudentStudyAI.Models;
using System.Text.RegularExpressions;

namespace StudentStudyAI.Services
{
    public class ContentAnalysisService
    {
        private readonly ILogger<ContentAnalysisService> _logger;

        public ContentAnalysisService(ILogger<ContentAnalysisService> logger)
        {
            _logger = logger;
        }

        public ContentAnalysisResult AnalyzeContent(List<FileUpload> files)
        {
            if (!files.Any())
            {
                return new ContentAnalysisResult
                {
                    UniqueConcepts = 0,
                    ComplexityScore = 0,
                    ContentVolume = 0,
                    EstimatedQuestions = 3,
                    KnowledgeLevel = KnowledgeLevel.HighSchool,
                    TimeEstimate = 5
                };
            }

            var allContent = string.Join(" ", files.Select(f => f.ExtractedContent ?? ""));
            var uniqueConcepts = ExtractUniqueConcepts(files);
            var complexityScore = CalculateComplexity(files);
            var contentVolume = CalculateVolume(files);
            var knowledgeLevel = DetectKnowledgeLevel(files);

            var estimatedQuestions = CalculateQuestionPotential(uniqueConcepts, complexityScore, contentVolume);
            var timeEstimate = EstimateQuizTime(estimatedQuestions, complexityScore);

            return new ContentAnalysisResult
            {
                UniqueConcepts = uniqueConcepts,
                ComplexityScore = complexityScore,
                ContentVolume = contentVolume,
                EstimatedQuestions = estimatedQuestions,
                KnowledgeLevel = knowledgeLevel,
                TimeEstimate = timeEstimate
            };
        }

        private int ExtractUniqueConcepts(List<FileUpload> files)
        {
            var allContent = string.Join(" ", files.Select(f => f.ExtractedContent ?? ""));
            var lowerContent = allContent.ToLower();

            // Extract potential concepts using various patterns
            var conceptPatterns = new[]
            {
                @"\b\w+ion\b", // -tion words (concepts, definitions, etc.)
                @"\b\w+ism\b", // -ism words (theories, philosophies)
                @"\b\w+ity\b", // -ity words (properties, qualities)
                @"\b\w+ment\b", // -ment words (processes, states)
                @"\b\w+ness\b", // -ness words (qualities, states)
                @"\b\w+ing\b", // -ing words (processes, actions)
                @"\b\w+ed\b", // -ed words (past processes)
                @"\b\w+ly\b" // -ly words (adverbs, qualities)
            };

            var concepts = new HashSet<string>();
            foreach (var pattern in conceptPatterns)
            {
                var matches = Regex.Matches(lowerContent, pattern);
                foreach (Match match in matches)
                {
                    var concept = match.Value.Trim();
                    if (concept.Length > 4 && concept.Length < 20)
                    {
                        concepts.Add(concept);
                    }
                }
            }

            // Also count unique words that appear multiple times (likely important concepts)
            var words = Regex.Matches(lowerContent, @"\b\w{4,}\b")
                .Cast<Match>()
                .Select(m => m.Value)
                .GroupBy(w => w)
                .Where(g => g.Count() > 2)
                .Select(g => g.Key);

            concepts.UnionWith(words);

            return Math.Min(concepts.Count, 50); // Cap at 50 for practical purposes
        }

        private double CalculateComplexity(List<FileUpload> files)
        {
            if (!files.Any()) return 0;

            var allContent = string.Join(" ", files.Select(f => f.ExtractedContent ?? ""));
            var lowerContent = allContent.ToLower();

            // Complexity indicators
            var complexWords = new[] { "analysis", "synthesis", "evaluation", "hypothesis", "theorem", "algorithm", "methodology", "framework", "paradigm", "conceptual" };
            var complexWordCount = complexWords.Sum(word => Regex.Matches(lowerContent, $@"\b{word}\b").Count);

            // Technical terms
            var technicalTerms = new[] { "function", "variable", "equation", "derivative", "integral", "molecule", "organism", "ecosystem", "algorithm", "database" };
            var technicalTermCount = technicalTerms.Sum(term => Regex.Matches(lowerContent, $@"\b{term}\b").Count);

            // Sentence complexity (average words per sentence)
            var sentences = allContent.Split('.', '!', '?').Where(s => s.Trim().Length > 10);
            var avgWordsPerSentence = sentences.Any() ? sentences.Average(s => s.Split(' ').Length) : 10;

            // Calculate complexity score (0-10)
            var complexityScore = Math.Min(10, 
                (complexWordCount * 0.5) + 
                (technicalTermCount * 0.3) + 
                (avgWordsPerSentence / 5) + 
                (files.Count * 0.2)
            );

            return Math.Round(complexityScore, 1);
        }

        private int CalculateVolume(List<FileUpload> files)
        {
            return files.Sum(f => (f.ExtractedContent ?? "").Length);
        }

        private KnowledgeLevel DetectKnowledgeLevel(List<FileUpload> files)
        {
            if (!files.Any()) return KnowledgeLevel.HighSchool;

            var allContent = string.Join(" ", files.Select(f => f.ExtractedContent ?? ""));
            var lowerContent = allContent.ToLower();

            // Academic level indicators
            var elementaryIndicators = new[] { "basic", "simple", "easy", "beginner", "introduction", "fundamental" };
            var middleSchoolIndicators = new[] { "intermediate", "standard", "regular", "common", "typical" };
            var highSchoolIndicators = new[] { "advanced", "complex", "detailed", "comprehensive", "thorough" };
            var collegeIndicators = new[] { "theoretical", "research", "analysis", "methodology", "framework", "paradigm" };
            var graduateIndicators = new[] { "doctoral", "dissertation", "thesis", "peer-reviewed", "empirical", "quantitative" };
            var expertIndicators = new[] { "cutting-edge", "novel", "innovative", "breakthrough", "pioneering", "groundbreaking" };

            var scores = new Dictionary<KnowledgeLevel, int>
            {
                [KnowledgeLevel.Elementary] = CountOccurrences(lowerContent, elementaryIndicators),
                [KnowledgeLevel.MiddleSchool] = CountOccurrences(lowerContent, middleSchoolIndicators),
                [KnowledgeLevel.HighSchool] = CountOccurrences(lowerContent, highSchoolIndicators),
                [KnowledgeLevel.College] = CountOccurrences(lowerContent, collegeIndicators),
                [KnowledgeLevel.Graduate] = CountOccurrences(lowerContent, graduateIndicators),
                [KnowledgeLevel.Expert] = CountOccurrences(lowerContent, expertIndicators)
            };

            // Also consider content length and complexity
            var contentLength = allContent.Length;
            var complexityScore = CalculateComplexity(files);

            // Adjust scores based on content characteristics
            if (contentLength > 10000) scores[KnowledgeLevel.College] += 2;
            if (contentLength > 50000) scores[KnowledgeLevel.Graduate] += 2;
            if (complexityScore > 7) scores[KnowledgeLevel.College] += 1;
            if (complexityScore > 8) scores[KnowledgeLevel.Graduate] += 1;

            return scores.OrderByDescending(x => x.Value).First().Key;
        }

        private int CalculateQuestionPotential(int uniqueConcepts, double complexityScore, int contentVolume)
        {
            // Base calculation on content analysis
            var baseQuestions = Math.Max(3, Math.Min(50, uniqueConcepts * 0.8));
            
            // Apply complexity multiplier
            var complexityMultiplier = Math.Max(0.5, Math.Min(2.0, complexityScore / 5.0));
            
            // Apply content volume multiplier
            var volumeMultiplier = Math.Min(1.5, contentVolume / 10000.0);
            
            var finalCount = (int)(baseQuestions * complexityMultiplier * volumeMultiplier);
            
            return Math.Max(3, Math.Min(50, finalCount));
        }

        private int EstimateQuizTime(int questionCount, double complexityScore)
        {
            // Base time: 1 minute per question
            var baseTime = questionCount;
            
            // Adjust for complexity
            var complexityMultiplier = Math.Max(0.5, Math.Min(2.0, complexityScore / 5.0));
            
            return (int)(baseTime * complexityMultiplier);
        }

        private int CountOccurrences(string content, string[] indicators)
        {
            return indicators.Sum(indicator => Regex.Matches(content, $@"\b{Regex.Escape(indicator)}\b", RegexOptions.IgnoreCase).Count);
        }
    }

    public class ContentAnalysisResult
    {
        public int UniqueConcepts { get; set; }
        public double ComplexityScore { get; set; }
        public int ContentVolume { get; set; }
        public int EstimatedQuestions { get; set; }
        public KnowledgeLevel KnowledgeLevel { get; set; }
        public int TimeEstimate { get; set; } // in minutes
    }

    public enum KnowledgeLevel
    {
        Elementary = 1,    // K-5
        MiddleSchool = 2,  // 6-8
        HighSchool = 3,    // 9-12
        College = 4,       // Undergraduate
        Graduate = 5,      // Graduate/Professional
        Expert = 6         // Advanced/Research
    }
}
