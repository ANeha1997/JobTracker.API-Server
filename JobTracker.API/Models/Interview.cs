using System.ComponentModel.DataAnnotations;

namespace JobTracker.API.Models
{
    public enum InterviewType
    {
        Phone = 0,
        Online = 1,
        Onsite = 2,
        HR = 3,
        Technical = 4,
        Other = 5
    }

    public class Interview
    {
        public int Id { get; set; }

        [Required]
        public int JobApplicationId { get; set; }
        public JobApplication JobApplication { get; set; } = null!;

        public DateTime ScheduledAt { get; set; }

        public InterviewType Type { get; set; } = InterviewType.Other;

        [MaxLength(200)]
        public string? LocationOrLink { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [MaxLength(200)]
        public string? Result { get; set; }   // e.g. "Passed", "Rejected", etc.
    }
}
