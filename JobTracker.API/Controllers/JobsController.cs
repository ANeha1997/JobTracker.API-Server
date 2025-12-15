using AutoMapper;
using AutoMapper.QueryableExtensions;
using JobTracker.API.Dtos;
using JobTracker.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public JobsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // -------------------------------------------------
        // GET: api/jobs  (search + filters + paging)
        // Example: /api/jobs?search=engineer&location=New%20York&page=1&pageSize=20
        // -------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<PagedResult<JobReadDto>>> GetJobs(
            [FromQuery] string? search,
            [FromQuery] string? location,
            [FromQuery] int? companyId,
            [FromQuery] decimal? minSalary,
            [FromQuery] decimal? maxSalary,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 1000) pageSize = 50;

            var query = _context.Jobs
                .Include(j => j.Company)
                .AsQueryable();

            // text search on title
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(j => j.Title != null && j.Title.Contains(s));
            }

            // filter by location (job or company location)
            if (!string.IsNullOrWhiteSpace(location))
            {
                var loc = location.Trim();
                query = query.Where(j =>
                    (j.Location != null && j.Location.Contains(loc)) ||
                    (j.Company != null && j.Company.Location != null &&
                     j.Company.Location.Contains(loc)));
            }

            // filter by company
            if (companyId.HasValue)
            {
                query = query.Where(j => j.CompanyId == companyId.Value);
            }

            // min salary
            if (minSalary.HasValue)
            {
                var min = minSalary.Value;
                query = query.Where(j =>
                    (j.SalaryMin.HasValue && j.SalaryMin.Value >= min) ||
                    (j.SalaryMax.HasValue && j.SalaryMax.Value >= min));
            }

            // max salary
            if (maxSalary.HasValue)
            {
                var max = maxSalary.Value;
                query = query.Where(j =>
                    j.SalaryMax.HasValue && j.SalaryMax.Value <= max);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(j => j.PostedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<JobReadDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var result = new PagedResult<JobReadDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };

            return Ok(result);
        }

        // -------------------------------------------------
        // GET: api/jobs/{id}
        // -------------------------------------------------
        [HttpGet("{id:int}")]
        public async Task<ActionResult<JobReadDto>> GetJob(int id)
        {
            var job = await _context.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null)
                return NotFound();

            var dto = _mapper.Map<JobReadDto>(job);
            return Ok(dto);
        }

        // -------------------------------------------------
        // POST: api/jobs   (create manual job entry)
        // -------------------------------------------------
        [HttpPost]
        public async Task<ActionResult<JobReadDto>> CreateJob(JobCreateDto dto)
        {
            var job = _mapper.Map<Job>(dto);
            job.PostedDate = DateTime.UtcNow;

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            var readDto = _mapper.Map<JobReadDto>(job);
            return CreatedAtAction(nameof(GetJob), new { id = job.Id }, readDto);
        }

        // -------------------------------------------------
        // PUT: api/jobs/{id}   (update)
        // -------------------------------------------------
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateJob(int id, JobCreateDto dto)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
                return NotFound();

            _mapper.Map(dto, job);   // copy fields from dto into entity
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // -------------------------------------------------
        // DELETE: api/jobs/{id}
        // -------------------------------------------------
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
                return NotFound();

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}