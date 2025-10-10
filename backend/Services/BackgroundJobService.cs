using System.Collections.Concurrent;
using StudentStudyAI.Models;

namespace StudentStudyAI.Services
{
    public class BackgroundJobService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundJobService> _logger;
        private readonly ConcurrentQueue<BackgroundJob> _jobQueue;
        private readonly SemaphoreSlim _semaphore;
        private readonly Timer? _timer;
        private readonly int _maxConcurrentJobs = 3;

        public BackgroundJobService(IServiceProvider serviceProvider, ILogger<BackgroundJobService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _jobQueue = new ConcurrentQueue<BackgroundJob>();
            _semaphore = new SemaphoreSlim(_maxConcurrentJobs, _maxConcurrentJobs);
            _timer = new Timer(ProcessJobs, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background Job Service started");
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background Job Service stopped");
            _timer?.Change(Timeout.Infinite, 0);
            await Task.CompletedTask;
        }

        public void EnqueueJob(BackgroundJob job)
        {
            _jobQueue.Enqueue(job);
            _logger.LogInformation("Job enqueued: {JobType} - {JobId}", job.JobType, job.JobId);
        }

        private void ProcessJobs(object? state)
        {
            if (_jobQueue.IsEmpty)
            {
                return;
            }

            _semaphore.Wait();
            try
            {
                if (_jobQueue.TryDequeue(out var job))
                {
                    _ = Task.Run(async () => 
                    {
                        try
                        {
                            await ExecuteJobAsync(job);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error executing job {JobId}: {ErrorMessage}", job.JobId, ex.Message);
                        }
                    });
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task ExecuteJobAsync(BackgroundJob job)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                
                switch (job.JobType)
                {
                    case JobType.FileProcessing:
                        await ProcessFileJobAsync(scope, job);
                        break;
                    case JobType.AIAnalysis:
                        await ProcessAIAnalysisJobAsync(scope, job);
                        break;
                    case JobType.EmailNotification:
                        await ProcessEmailJobAsync(scope, job);
                        break;
                    default:
                        _logger.LogWarning("Unknown job type: {JobType}", job.JobType);
                        break;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Job failed: {JobType} - {JobId}", job.JobType, job.JobId);
                
                // Retry logic
                if (job.RetryCount < 3)
                {
                    job.RetryCount++;
                    job.ScheduledFor = DateTime.UtcNow.AddMinutes(Math.Pow(2, job.RetryCount)); // Exponential backoff
                    _jobQueue.Enqueue(job);
                    _logger.LogInformation("Job scheduled for retry: {JobType} - {JobId} (Attempt {RetryCount})", 
                        job.JobType, job.JobId, job.RetryCount);
                }
            }
        }

        private async Task ProcessFileJobAsync(IServiceScope scope, BackgroundJob job)
        {
            try
            {
                var fileProcessingService = scope.ServiceProvider.GetRequiredService<IFileProcessingService>();
                var databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
                var contextService = scope.ServiceProvider.GetRequiredService<ContextService>();

                if (job.Data.TryGetValue("fileId", out var fileIdObj) && int.TryParse(fileIdObj.ToString(), out var fileId))
                {
                    var file = await databaseService.GetFileUploadAsync(fileId);
                    if (file != null)
                    {
                        var processedFile = await fileProcessingService.ProcessFileAsync(file);
                        await databaseService.UpdateFileProcessingStatusAsync(fileId, processedFile.Status, processedFile.ExtractedText);
                        
                        // Run auto-detection if file was processed successfully
                        if (processedFile.Status == "completed" && !string.IsNullOrEmpty(processedFile.ExtractedText))
                        {
                            try
                            {
                                // Use AI analysis for better subject/topic detection
                                var openAIService = scope.ServiceProvider.GetRequiredService<OpenAIService>();
                                var fileAnalysis = await openAIService.AnalyzeFileContentAsync(file, null, null);
                                
                                // Update database with AI-detected subject and topic
                                await databaseService.UpdateFileAutoDetectionAsync(fileId, fileAnalysis.Subject, fileAnalysis.Topic);
                                
                                _logger.LogInformation("AI analysis completed for file {FileId}: Subject='{Subject}', Topic='{Topic}'", 
                                    fileId, fileAnalysis.Subject, fileAnalysis.Topic);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error during AI analysis for file {FileId}: {ErrorMessage}", fileId, ex.Message);
                                
                                // Fallback to keyword detection if AI fails
                                try
                                {
                                    var detectedSubject = contextService.DetectSubjectFromContent(processedFile.ExtractedText);
                                    var detectedTopic = contextService.DetectTopicFromContent(processedFile.ExtractedText);
                                    await databaseService.UpdateFileAutoDetectionAsync(fileId, detectedSubject, detectedTopic);
                                    _logger.LogInformation("Fallback keyword detection completed for file {FileId}: Subject='{Subject}', Topic='{Topic}'", 
                                        fileId, detectedSubject, detectedTopic);
                                }
                                catch (Exception fallbackEx)
                                {
                                    _logger.LogError(fallbackEx, "Both AI analysis and keyword detection failed for file {FileId}", fileId);
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError("File not found in database: {FileId}", fileId);
                    }
                }
                else
                {
                    _logger.LogError("Invalid fileId in job data: {JobData}", string.Join(", ", job.Data.Select(kv => $"{kv.Key}={kv.Value}")));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error in file processing job {JobId}: {ErrorMessage}", job.JobId, ex.Message);
            }
        }

        private async Task ProcessAIAnalysisJobAsync(IServiceScope scope, BackgroundJob job)
        {
            var analysisService = scope.ServiceProvider.GetRequiredService<AnalysisService>();
            var databaseService = scope.ServiceProvider.GetRequiredService<DatabaseService>();

            if (job.Data.TryGetValue("fileId", out var fileIdObj) && int.TryParse(fileIdObj.ToString(), out var fileId) &&
                job.Data.TryGetValue("userId", out var userIdObj) && int.TryParse(userIdObj.ToString(), out var userId))
            {
                var analysis = await analysisService.AnalyzeFileAsync(fileId, userId);
                // Convert FileAnalysis to AnalysisResult for database storage
                var analysisResult = new AnalysisResult
                {
                    FileUploadId = fileId,
                    UserId = userId,
                    Subject = analysis.Subject,
                    Topic = analysis.Topic,
                    Feedback = analysis.Summary,
                    CreatedAt = DateTime.UtcNow
                };
                await databaseService.CreateAnalysisResultAsync(analysisResult);
            }
        }

        private async Task ProcessEmailJobAsync(IServiceScope scope, BackgroundJob job)
        {
            // Email service would be implemented here
            _logger.LogInformation("Email job processed: {JobId}", job.JobId);
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _semaphore?.Dispose();
        }
    }

    public class BackgroundJob
    {
        public string JobId { get; set; } = Guid.NewGuid().ToString();
        public JobType JobType { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ScheduledFor { get; set; } = DateTime.UtcNow;
        public int RetryCount { get; set; } = 0;
        public JobStatus Status { get; set; } = JobStatus.Pending;
    }

    public enum JobType
    {
        FileProcessing,
        AIAnalysis,
        EmailNotification
    }

    public enum JobStatus
    {
        Pending,
        Processing,
        Completed,
        Failed
    }
}
