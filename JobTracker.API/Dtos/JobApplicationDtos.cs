using JobTracker.API.Models;

namespace JobTracker.API.Dtos
{
    public class JobApplicationCreateDto
    {
        // If JobId is provided, backend will auto-fill Title/Location/Company.
        public int? JobId { get; set; }

        // Optional for manual entry (when JobId is null)
        public int? CompanyId { get; set; }
        public string? Title { get; set; }
        public string? Location { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
        public DateTime? AppliedDate { get; set; }
        public string? SourceUrl { get; set; }
        public string? Notes { get; set; }
    }

    public class JobApplicationReadDto
    {
        public int Id { get; set; }

        public int? JobId { get; set; }
        public int? CompanyId { get; set; }

        public string Title { get; set; } = null!;
        public string? Location { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime AppliedDate { get; set; }
        public string? SourceUrl { get; set; }
        public string? Notes { get; set; }

        // convenience for UI
        public string? CompanyName { get; set; }
    }
}
