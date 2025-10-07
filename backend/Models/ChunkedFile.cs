namespace StudentStudyAI.Models
{
    public class ChunkedFile
    {
        public int Id { get; set; }
        public int FileUploadId { get; set; }
        public int ChunkIndex { get; set; }
        public string Content { get; set; } = string.Empty;
        public int TokenCount { get; set; }
        public bool IsProcessed { get; set; } = false;
        public DateTime CreatedAt { get; set; }
    }
}
