using System.Text.Json.Serialization;

namespace JobTracker.API.Models
{
    public class Company
    {
        public int Id { get; set; }

        // LinkedIn company_id from companies.csv
        public long ExternalCompanyId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public string? Location { get; set; }

        public ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
