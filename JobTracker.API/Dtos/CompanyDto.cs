namespace JobTracker.API.Dtos
{
    public class CompanyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Industry { get; set; }
        public string? Location { get; set; }
    }
}
