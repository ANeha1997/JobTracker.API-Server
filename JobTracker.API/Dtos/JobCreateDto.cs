namespace JobTracker.API.Dtos
{
    public class JobCreateDto
    {
        public string Title { get; set; } = null!;
        public string? Location { get; set; }
        public string? EmploymentType { get; set; }
        public string? SeniorityLevel { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public DateTime? PostedDate { get; set; }
        public string? SourceId { get; set; }

        // *require* a company id when creating a job from Angular.
        public int CompanyId { get; set; }
    }
}
