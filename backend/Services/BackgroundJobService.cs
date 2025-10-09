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
            _timer = new Timer(ProcessJobs, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
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

        private async void ProcessJobs(object? state)
        {
            if (_jobQueue.IsEmpty)
                return;

            await _semaphore.WaitAsync();
            try
            {
                if (_jobQueue.TryDequeue(out var job))
                {
                    _ = Task.Run(async () => await ExecuteJobAsync(job));
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
                _logger.LogInformation("Executing job: {JobType} - {JobId}", job.JobType, job.JobId);
                
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

                _logger.LogInformation("Job completed successfully: {JobType} - {JobId}", job.JobType, job.JobId);
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
            var fileProcessingService = scope.ServiceProvider.GetRequiredService<FileProcessingService>();
            var databaseService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
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
                            // Create a temporary file object with extracted content for detection
                            var tempFile = new FileUpload
                            {
                                Id = fileId,
                                ExtractedContent = processedFile.ExtractedText,
                                FileName = file.FileName
                            };
                            
                            // Detect subject and topic from content
                            var detectedSubject = contextService.DetectSubjectFromContent(processedFile.ExtractedText);
                            var detectedTopic = contextService.DetectTopicFromContent(processedFile.ExtractedText);
                            
                            // Update the file with detected subject and topic
                            await databaseService.UpdateFileAutoDetectionAsync(fileId, detectedSubject, detectedTopic);
                            
                            _logger.LogInformation("Auto-detected subject '{Subject}' and topic '{Topic}' for file {FileId}", 
                                detectedSubject, detectedTopic, fileId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error during auto-detection for file {FileId}", fileId);
                        }
                    }
                }
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
