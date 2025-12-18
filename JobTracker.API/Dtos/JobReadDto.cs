using System;

namespace JobTracker.API.Dtos
{
    public class JobReadDto
    {
        public int Id { get; set; }

        
        public string Title { get; set; } = "";

        public string? Location { get; set; }
        public string? EmploymentType { get; set; }
        public string? SeniorityLevel { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }

       
        public DateTime? PostedDate { get; set; }

        public string? CompanyName { get; set; }
        public string? SourceId { get; set; }

        
        //public CompanyDto? Company { get; set; }
    }
}
