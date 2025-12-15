using JobTracker.API.Auth;
using JobTracker.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly JobImportService _service;
        private readonly IWebHostEnvironment _env;

        public ImportController(JobImportService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        // POST api/import/run
        [HttpPost("run")]
        public async Task<IActionResult> RunImport()
        {
            // Data folder 
            var root = Path.Combine(_env.ContentRootPath, "Data");
            var companiesPath = Path.Combine(root, "companies.csv");
            var jobsPath = Path.Combine(root, "job_postings.csv");

            if (!System.IO.File.Exists(companiesPath) || !System.IO.File.Exists(jobsPath))
                return NotFound("CSV files not found in Data folder.");

            var companiesImported = await _service.ImportCompaniesAsync(companiesPath);
            var jobsImported = await _service.ImportJobsAsync(jobsPath);

            return Ok(new
            {
                companiesImported,
                jobsImported
            });
        }
    }
}