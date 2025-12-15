namespace JobTracker.API.Dtos
{
    public class InterviewNotificationDto
    {
        public int InterviewId { get; set; }
        public DateTime ScheduledAt { get; set; }

        public int JobApplicationId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
    }
}
