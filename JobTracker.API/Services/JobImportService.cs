using CsvHelper;
using CsvHelper.Configuration;
using JobTracker.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace JobTracker.API.Services
{
    public class JobImportService
    {
        private readonly ApplicationDbContext _context;

        public JobImportService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static CsvConfiguration GetCsvConfig() =>
            new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                BadDataFound = null,       // ignore weird rows
                MissingFieldFound = null   // ignore missing fields
            };

        // 1) Import companies.csv
        public async Task<int> ImportCompaniesAsync(string companiesPath)
        {
            using var stream = File.OpenRead(companiesPath);
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, GetCsvConfig());

            // read header
            await csv.ReadAsync();
            csv.ReadHeader();

            int imported = 0;

            while (await csv.ReadAsync())
            {
                // company_id,name,description,company_size,state,country,city,zip_code,address,url
                var idRaw = csv.GetField<string>("company_id");

                if (string.IsNullOrWhiteSpace(idRaw))
                    continue;

                // handle "12345.0" -> "12345"
                var idPart = idRaw.Split('.')[0];

                if (!long.TryParse(idPart, out var externalId))
                    continue;

                var name = csv.GetField<string>("name");
                var city = csv.GetField<string>("city");
                var state = csv.GetField<string>("state");
                var country = csv.GetField<string>("country");

                var location = string.Join(", ",
                    new[] { city, state, country }.Where(s => !string.IsNullOrWhiteSpace(s)));

                // avoid duplicates
                var exists = await _context.Companies
                    .AnyAsync(c => c.ExternalCompanyId == externalId);

                if (exists)
                    continue;

                var company = new Company
                {
                    ExternalCompanyId = externalId,
                    Name = name ?? string.Empty,
                    Location = location
                };

                _context.Companies.Add(company);
                imported++;
            }

            await _context.SaveChangesAsync();
            return imported;
        }

        // 2) Import job_postings.csv
        public async Task<int> ImportJobsAsync(string jobsPath)
        {
            using var stream = File.OpenRead(jobsPath);
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, GetCsvConfig());

            await csv.ReadAsync();
            csv.ReadHeader();

            int imported = 0;

            while (await csv.ReadAsync())
            {
                // job_id,company_id,title,description,max_salary,med_salary,min_salary,
                // pay_period,formatted_work_type,location,...,formatted_experience_level,...

                var jobId = csv.GetField<string>("job_id");

                var companyIdRaw = csv.GetField<string>("company_id");
                if (string.IsNullOrWhiteSpace(companyIdRaw))
                    continue;

                var companyIdPart = companyIdRaw.Split('.')[0];
                if (!long.TryParse(companyIdPart, out var externalCompanyId))
                    continue;

                // 
                var title = csv.GetField<string>("title");

                // DB requires Title
                if (string.IsNullOrWhiteSpace(title))
                    continue;

                var location = csv.GetField<string>("location");
                var workType = csv.GetField<string>("formatted_work_type");
                var seniority = csv.GetField<string>("formatted_experience_level");

                decimal? minSalary = null;
                decimal? maxSalary = null;

                var minRaw = csv.GetField<string>("min_salary");
                var maxRaw = csv.GetField<string>("max_salary");

                if (decimal.TryParse(minRaw, out var min))
                    minSalary = min;

                if (decimal.TryParse(maxRaw, out var max))
                    maxSalary = max;

                // find company by ExternalCompanyId
                var company = await _context.Companies
                    .FirstOrDefaultAsync(c => c.ExternalCompanyId == externalCompanyId);

                // if not found, create a placeholder so we can still import the job
                if (company == null)
                {
                    company = new Company
                    {
                        ExternalCompanyId = externalCompanyId,
                        Name = $"Company {externalCompanyId}"
                    };
                    _context.Companies.Add(company);
                    await _context.SaveChangesAsync();
                }

                // before creating 'var job = new Job { ... };'
                var existingJob = await _context.Jobs
                    .AnyAsync(j => j.ExternalJobId == jobId);

                if (existingJob)
                    continue;

                var job = new Job
                {
                    ExternalJobId = jobId ?? string.Empty,
                    Title = title,                    
                    Location = location,
                    EmploymentType = workType,
                    SeniorityLevel = seniority,
                    SalaryMin = minSalary,
                    SalaryMax = maxSalary,
                    PostedDate = DateTime.UtcNow,
                    CompanyId = company.Id
                };

                _context.Jobs.Add(job);
                imported++;
            }

            await _context.SaveChangesAsync();
            return imported;
        }
    }
}
