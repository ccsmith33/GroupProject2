using StudentStudyAI.Models;
using System.Text.RegularExpressions;

namespace StudentStudyAI.Services
{
    public class ContextService
    {
        private readonly IDatabaseService _databaseService;

        public ContextService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<ContextData> GetContextForUserAsync(int userId, string userPrompt, int maxFiles = 5, int maxStudyGuides = 3, int maxConversations = 5)
        {
            // Extract keywords from user prompt
            var keywords = ExtractKeywords(userPrompt);
            var subject = ExtractSubject(userPrompt);
            var topic = ExtractTopic(userPrompt);

            // Get relevant files
            var relevantFiles = await GetRelevantFilesAsync(userId, keywords, subject, topic, maxFiles);

            // Get recent study guides
            var recentStudyGuides = await _databaseService.GetStudyGuidesByUserIdAsync(userId);
            var contextStudyGuides = recentStudyGuides.Take(maxStudyGuides).ToList();

            // Get recent conversations
            var recentConversations = await _databaseService.GetConversationsByUserIdAsync(userId, maxConversations);

            return new ContextData
            {
                Files = relevantFiles,
                StudyGuides = contextStudyGuides,
                Conversations = recentConversations,
                Subject = subject,
                Topic = topic,
                Keywords = keywords
            };
        }

        public async Task<ContextData> GetContextForFileUploadAsync(int userId, FileUpload uploadedFile, int maxFiles = 3, int maxStudyGuides = 2)
        {
            // Intelligently detect subject from file content if not provided
            var subject = uploadedFile.Subject;
            var topic = uploadedFile.Topic;
            
            if (string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(uploadedFile.ExtractedContent))
            {
                subject = DetectSubjectFromContent(uploadedFile.ExtractedContent);
            }
            
            if (string.IsNullOrEmpty(topic) && !string.IsNullOrEmpty(uploadedFile.ExtractedContent))
            {
                topic = DetectTopicFromContent(uploadedFile.ExtractedContent);
            }

            // Get related files by subject/topic
            var relatedFiles = await _databaseService.GetRelevantFilesForContextAsync(userId, subject, topic, maxFiles);

            // Get recent study guides
            var recentStudyGuides = await _databaseService.GetStudyGuidesByUserIdAsync(userId);
            var contextStudyGuides = recentStudyGuides.Take(maxStudyGuides).ToList();

            return new ContextData
            {
                Files = relatedFiles,
                StudyGuides = contextStudyGuides,
                Conversations = new List<Conversation>(),
                Subject = subject,
                Topic = topic,
                Keywords = new List<string>()
            };
        }

        private async Task<List<FileUpload>> GetRelevantFilesAsync(int userId, List<string> keywords, string? subject, string? topic, int maxFiles)
        {
            // First try to get files by subject and topic
            var files = await _databaseService.GetRelevantFilesForContextAsync(userId, subject, topic, maxFiles);

            // If we don't have enough files, get recent files
            if (files.Count < maxFiles)
            {
                var allFiles = await _databaseService.GetFilesByUserIdAsync(userId);
                var recentFiles = allFiles
                    .Where(f => f.IsProcessed && !files.Any(ef => ef.Id == f.Id))
                    .Take(maxFiles - files.Count)
                    .ToList();
                
                files.AddRange(recentFiles);
            }

            // If we still don't have enough, get any processed files
            if (files.Count < maxFiles)
            {
                var allFiles = await _databaseService.GetFilesByUserIdAsync(userId);
                var processedFiles = allFiles
                    .Where(f => f.IsProcessed && !files.Any(ef => ef.Id == f.Id))
                    .Take(maxFiles - files.Count)
                    .ToList();
                
                files.AddRange(processedFiles);
            }

            return files;
        }

        private List<string> ExtractKeywords(string prompt)
        {
            // Simple keyword extraction - remove common words and extract meaningful terms
            var commonWords = new HashSet<string>
            {
                "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by", "is", "are", "was", "were", "be", "been", "being", "have", "has", "had", "do", "does", "did", "will", "would", "could", "should", "may", "might", "can", "this", "that", "these", "those", "i", "you", "he", "she", "it", "we", "they", "me", "him", "her", "us", "them", "my", "your", "his", "her", "its", "our", "their", "help", "please", "create", "generate", "make", "give", "show", "tell", "explain", "what", "how", "why", "when", "where", "who"
            };

            var words = Regex.Split(prompt.ToLower(), @"\W+")
                .Where(w => w.Length > 2 && !commonWords.Contains(w))
                .Distinct()
                .Take(10)
                .ToList();

            return words;
        }

        private string? ExtractSubject(string prompt)
        {
            var subjectKeywords = new Dictionary<string, string>
            {
                // Mathematics & Related
                ["math"] = "Mathematics",
                ["mathematics"] = "Mathematics",
                ["algebra"] = "Mathematics",
                ["geometry"] = "Mathematics",
                ["calculus"] = "Mathematics",
                ["statistics"] = "Mathematics",
                ["trigonometry"] = "Mathematics",
                ["trig"] = "Mathematics",
                ["precalculus"] = "Mathematics",
                ["pre-calculus"] = "Mathematics",
                ["linear algebra"] = "Mathematics",
                ["differential equations"] = "Mathematics",
                ["discrete math"] = "Mathematics",
                ["number theory"] = "Mathematics",
                ["probability"] = "Mathematics",
                ["data analysis"] = "Mathematics",
                ["quantitative"] = "Mathematics",
                ["numerical"] = "Mathematics",
                
                // Science & Related
                ["science"] = "Science",
                ["biology"] = "Biology",
                ["biological"] = "Biology",
                ["life science"] = "Biology",
                ["anatomy"] = "Biology",
                ["physiology"] = "Biology",
                ["genetics"] = "Biology",
                ["ecology"] = "Biology",
                ["botany"] = "Biology",
                ["zoology"] = "Biology",
                ["microbiology"] = "Biology",
                ["chemistry"] = "Chemistry",
                ["chemical"] = "Chemistry",
                ["organic chemistry"] = "Chemistry",
                ["inorganic chemistry"] = "Chemistry",
                ["biochemistry"] = "Chemistry",
                ["physical chemistry"] = "Chemistry",
                ["physics"] = "Physics",
                ["physical"] = "Physics",
                ["mechanics"] = "Physics",
                ["thermodynamics"] = "Physics",
                ["electromagnetism"] = "Physics",
                ["quantum"] = "Physics",
                ["astronomy"] = "Physics",
                ["astrophysics"] = "Physics",
                ["earth science"] = "Earth Science",
                ["geology"] = "Earth Science",
                ["geological"] = "Earth Science",
                ["meteorology"] = "Earth Science",
                ["environmental science"] = "Environmental Science",
                ["environmental"] = "Environmental Science",
                
                // Language & Literature
                ["english"] = "English",
                ["literature"] = "English",
                ["writing"] = "English",
                ["language"] = "English",
                ["grammar"] = "English",
                ["composition"] = "English",
                ["rhetoric"] = "English",
                ["poetry"] = "English",
                ["drama"] = "English",
                ["theater"] = "English",
                ["theatre"] = "English",
                ["creative writing"] = "English",
                ["technical writing"] = "English",
                ["journalism"] = "English",
                
                // Social Sciences
                ["history"] = "History",
                ["historical"] = "History",
                ["world history"] = "History",
                ["american history"] = "History",
                ["european history"] = "History",
                ["ancient history"] = "History",
                ["modern history"] = "History",
                ["political science"] = "Political Science",
                ["politics"] = "Political Science",
                ["government"] = "Political Science",
                ["economics"] = "Economics",
                ["economic"] = "Economics",
                ["microeconomics"] = "Economics",
                ["macroeconomics"] = "Economics",
                ["psychology"] = "Psychology",
                ["psychological"] = "Psychology",
                ["sociology"] = "Sociology",
                ["sociological"] = "Sociology",
                ["anthropology"] = "Anthropology",
                ["anthropological"] = "Anthropology",
                ["geography"] = "Geography",
                ["geographical"] = "Geography",
                ["philosophy"] = "Philosophy",
                ["philosophical"] = "Philosophy",
                
                // Arts & Creative
                ["art"] = "Art",
                ["artistic"] = "Art",
                ["visual arts"] = "Art",
                ["drawing"] = "Art",
                ["painting"] = "Art",
                ["sculpture"] = "Art",
                ["design"] = "Art",
                ["graphic design"] = "Art",
                ["music"] = "Music",
                ["musical"] = "Music",
                ["music theory"] = "Music",
                ["composition"] = "Music",
                ["performance"] = "Music",
                ["dance"] = "Dance",
                ["dancing"] = "Dance",
                ["choreography"] = "Dance",
                
                // Technology & Computer Science
                ["computer science"] = "Computer Science",
                ["programming"] = "Computer Science",
                ["coding"] = "Computer Science",
                ["software"] = "Computer Science",
                ["algorithms"] = "Computer Science",
                ["data structures"] = "Computer Science",
                ["artificial intelligence"] = "Computer Science",
                ["machine learning"] = "Computer Science",
                ["cybersecurity"] = "Computer Science",
                ["networking"] = "Computer Science",
                ["database"] = "Computer Science",
                ["web development"] = "Computer Science",
                ["information technology"] = "Computer Science",
                ["it"] = "Computer Science",
                
                // Business & Finance
                ["business"] = "Business",
                ["management"] = "Business",
                ["marketing"] = "Business",
                ["finance"] = "Finance",
                ["financial"] = "Finance",
                ["accounting"] = "Accounting",
                ["accounting"] = "Accounting",
                ["entrepreneurship"] = "Business",
                ["leadership"] = "Business",
                ["strategy"] = "Business",
                
                // Health & Medicine
                ["medicine"] = "Medicine",
                ["medical"] = "Medicine",
                ["health"] = "Health",
                ["healthcare"] = "Health",
                ["nursing"] = "Nursing",
                ["pharmacy"] = "Pharmacy",
                ["pharmaceutical"] = "Pharmacy",
                ["public health"] = "Health",
                
                // Engineering
                ["engineering"] = "Engineering",
                ["civil engineering"] = "Engineering",
                ["mechanical engineering"] = "Engineering",
                ["electrical engineering"] = "Engineering",
                ["chemical engineering"] = "Engineering",
                ["computer engineering"] = "Engineering",
                ["biomedical engineering"] = "Engineering",
                
                // Other Academic
                ["research"] = "Research",
                ["methodology"] = "Research",
                ["data science"] = "Data Science",
                ["analytics"] = "Data Science",
                ["social studies"] = "Social Studies"
            };

            var lowerPrompt = prompt.ToLower();
            foreach (var kvp in subjectKeywords)
            {
                if (lowerPrompt.Contains(kvp.Key))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        private string? ExtractTopic(string prompt)
        {
            // Look for specific topic indicators
            var topicPatterns = new[]
            {
                @"about\s+([^.!?]+)",
                @"on\s+([^.!?]+)",
                @"regarding\s+([^.!?]+)",
                @"concerning\s+([^.!?]+)",
                @"topic\s+([^.!?]+)",
                @"subject\s+([^.!?]+)"
            };

            foreach (var pattern in topicPatterns)
            {
                var match = Regex.Match(prompt, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var topic = match.Groups[1].Value.Trim();
                    if (topic.Length > 3 && topic.Length < 100)
                    {
                        return topic;
                    }
                }
            }

            return null;
        }

        public string? DetectSubjectFromContent(string content)
        {
            var lowerContent = content.ToLower();
            var subjectScores = new Dictionary<string, int>();

            // Mathematics indicators
            var mathIndicators = new[] { "derivative", "integral", "equation", "function", "graph", "slope", "variable", "algebra", "geometry", "trigonometry", "calculus", "statistics", "probability", "matrix", "vector", "theorem", "proof", "solve", "calculate", "formula", "mathematical" };
            subjectScores["Mathematics"] = CountOccurrences(lowerContent, mathIndicators);

            // Biology indicators
            var bioIndicators = new[] { "cell", "dna", "protein", "organism", "evolution", "genetics", "ecosystem", "photosynthesis", "respiration", "mitosis", "meiosis", "chromosome", "gene", "species", "habitat", "biodiversity", "anatomy", "physiology" };
            subjectScores["Biology"] = CountOccurrences(lowerContent, bioIndicators);

            // Chemistry indicators
            var chemIndicators = new[] { "molecule", "atom", "element", "compound", "reaction", "bond", "ion", "acid", "base", "ph", "oxidation", "reduction", "catalyst", "synthesis", "organic", "inorganic", "periodic table", "valence" };
            subjectScores["Chemistry"] = CountOccurrences(lowerContent, chemIndicators);

            // Physics indicators
            var physicsIndicators = new[] { "force", "energy", "momentum", "velocity", "acceleration", "mass", "gravity", "electricity", "magnetism", "wave", "frequency", "amplitude", "quantum", "thermodynamics", "mechanics", "optics", "nuclear" };
            subjectScores["Physics"] = CountOccurrences(lowerContent, physicsIndicators);

            // English/Literature indicators
            var englishIndicators = new[] { "character", "theme", "plot", "setting", "symbolism", "metaphor", "simile", "poetry", "prose", "narrative", "dialogue", "author", "literature", "novel", "poem", "essay", "rhetoric", "grammar" };
            subjectScores["English"] = CountOccurrences(lowerContent, englishIndicators);

            // History indicators
            var historyIndicators = new[] { "war", "battle", "revolution", "empire", "kingdom", "century", "ancient", "medieval", "renaissance", "industrial", "colonial", "independence", "treaty", "government", "political", "social", "economic", "cultural" };
            subjectScores["History"] = CountOccurrences(lowerContent, historyIndicators);

            // Business/Management indicators (check this first to avoid CS conflicts)
            var businessIndicators = new[] { "management", "business", "marketing", "finance", "accounting", "economics", "strategy", "leadership", "organization", "operations", "human resources", "hr", "project management", "supply chain", "logistics", "entrepreneurship", "corporate", "company", "profit", "revenue", "cost", "budget", "investment", "stakeholder", "customer", "client", "market", "competition", "competitive", "swot", "analysis", "planning", "decision", "policy", "procedure", "process", "efficiency", "productivity", "quality", "performance", "kpi", "metrics", "roi", "return on investment" };
            subjectScores["Business/Management"] = CountOccurrences(lowerContent, businessIndicators);

            // Computer Science indicators (more specific to avoid conflicts)
            var csIndicators = new[] { "algorithm", "programming", "code", "software", "database design", "network protocol", "security", "artificial intelligence", "machine learning", "data structure", "programming function", "variable declaration", "loop", "array", "object-oriented", "class definition", "method implementation", "compiler", "debugging", "syntax", "api", "framework", "library", "repository", "version control", "git", "deployment", "server", "client-server" };
            subjectScores["Computer Science"] = CountOccurrences(lowerContent, csIndicators);

            // Return the subject with the highest score
            return subjectScores.OrderByDescending(x => x.Value).FirstOrDefault().Key;
        }

        public string? DetectTopicFromContent(string content)
        {
            // Look for common topic patterns in the content
            var topicPatterns = new[]
            {
                @"chapter\s+(\d+[:\s]+[^.!?]+)",
                @"lesson\s+(\d+[:\s]+[^.!?]+)",
                @"unit\s+(\d+[:\s]+[^.!?]+)",
                @"section\s+(\d+[:\s]+[^.!?]+)",
                @"topic\s+([^.!?]+)",
                @"subject\s+([^.!?]+)",
                @"about\s+([^.!?]+)",
                @"focus\s+on\s+([^.!?]+)"
            };

            foreach (var pattern in topicPatterns)
            {
                var match = Regex.Match(content, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var topic = match.Groups[1].Value.Trim();
                    if (topic.Length > 3 && topic.Length < 100)
                    {
                        return topic;
                    }
                }
            }

            // If no specific topic found, try to extract from the first few sentences
            var sentences = content.Split('.', '!', '?').Take(3);
            foreach (var sentence in sentences)
            {
                if (sentence.Length > 20 && sentence.Length < 200)
                {
                    return sentence.Trim();
                }
            }

            return null;
        }

        private int CountOccurrences(string content, string[] indicators)
        {
            return indicators.Sum(indicator => Regex.Matches(content, $@"\b{Regex.Escape(indicator)}\b", RegexOptions.IgnoreCase).Count);
        }
    }

    public class ContextData
    {
        public List<FileUpload> Files { get; set; } = new();
        public List<StudyGuide> StudyGuides { get; set; } = new();
        public List<Conversation> Conversations { get; set; } = new();
        public string? Subject { get; set; }
        public string? Topic { get; set; }
        public List<string> Keywords { get; set; } = new();
    }
}
