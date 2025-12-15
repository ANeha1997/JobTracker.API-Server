namespace JobTracker.API.Models
{
    public class Job
    {
        public int Id { get; set; }

        // LinkedIn job_id
        public string ExternalJobId { get; set; } = null!;

       
        public string Title { get; set; } = string.Empty;

        public string? Location { get; set; }
        public string? EmploymentType { get; set; }
        public string? SeniorityLevel { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public DateTime PostedDate { get; set; } = DateTime.UtcNow;

        public string? SourceId { get; set; }

        public int CompanyId { get; set; }
        public Company? Company { get; set; }
    }
}
