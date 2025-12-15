// JobTracker.API/Dtos/InterviewDtos.cs
namespace JobTracker.API.Dtos
{
    public class InterviewCreateDto
    {
        // must be > 0
        public int JobApplicationId { get; set; }

        // 
        public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;

        public string Type { get; set; } = string.Empty;
        public string? LocationOrLink { get; set; }
        public string? Notes { get; set; }
        public string? Result { get; set; }
    }

    public class InterviewReadDto
    {
        public int Id { get; set; }
        public int JobApplicationId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string? Type { get; set; }
        public string? LocationOrLink { get; set; }
        public string? Notes { get; set; }
        public string? Result { get; set; }
    }
}
