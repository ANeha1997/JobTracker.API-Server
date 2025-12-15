using System.ComponentModel.DataAnnotations;

namespace JobTracker.API.Models
{
    public enum ApplicationStatus
    {
        Applied = 0,
        PhoneScreen = 1,
        Interview = 2,
        Offer = 3,
        Rejected = 4,
        Withdrawn = 5
    }

    public class JobApplication
    {
        public int Id { get; set; }

        // the logged-in user who owns this application
        [Required]
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        // optional link to imported job and company
        public int? JobId { get; set; }
        public Job? Job { get; set; }

        public int? CompanyId { get; set; }
        public Company? Company { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(200)]
        public string? Location { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? SourceUrl { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
    }
}
