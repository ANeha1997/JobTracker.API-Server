using AutoMapper;
using AutoMapper.QueryableExtensions;
using JobTracker.API;
using JobTracker.API.Dtos;
using JobTracker.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobTracker.API.Controllers
{
    [Authorize]   // any logged-in user
    [Route("api/[controller]")]
    [ApiController]
    public class JobApplicationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public JobApplicationsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // -------------------------------------------------
        // GET: api/JobApplications   (only current user)
        // -------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobApplicationReadDto>>> GetMyApplications()
        {
            var userId = GetUserId();

            var apps = await _context.JobApplications
                .Where(a => a.UserId == userId)
                .Include(a => a.Company)
                .OrderByDescending(a => a.AppliedDate)
                .ProjectTo<JobApplicationReadDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(apps);
        }

        // -------------------------------------------------
        // GET: api/JobApplications/5   (single application)
        // -------------------------------------------------
        [HttpGet("{id:int}")]
        public async Task<ActionResult<JobApplicationReadDto>> GetApplication(int id)
        {
            var userId = GetUserId();

            var app = await _context.JobApplications
                .Include(a => a.Company)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (app == null)
                return NotFound();

            return Ok(_mapper.Map<JobApplicationReadDto>(app));
        }

        // -------------------------------------------------
        // POST: api/JobApplications

        // -------------------------------------------------
        [HttpPost]
        public async Task<ActionResult<JobApplicationReadDto>> CreateApplication(JobApplicationCreateDto dto)
        {
            var userId = GetUserId();
            JobApplication entity;

            if (dto.JobId.HasValue)
            {
                var job = await _context.Jobs
                    .Include(j => j.Company)
                    .FirstOrDefaultAsync(j => j.Id == dto.JobId.Value);

                if (job == null)
                    return BadRequest($"Job with id {dto.JobId.Value} was not found.");

                entity = new JobApplication
                {
                    UserId = userId,
                    JobId = job.Id,
                    CompanyId = job.CompanyId,
                    Title = job.Title,
                    Location = job.Location ?? job.Company?.Location,
                    Status = dto.Status,
                    AppliedDate = dto.AppliedDate ?? DateTime.UtcNow,
                    SourceUrl = !string.IsNullOrWhiteSpace(dto.SourceUrl)
                        ? dto.SourceUrl
                        : job.SourceId,
                    Notes = dto.Notes
                };
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.Title))
                    return BadRequest("Title is required when no JobId is supplied.");

                entity = new JobApplication
                {
                    UserId = userId,
                    JobId = null,
                    CompanyId = dto.CompanyId,
                    Title = dto.Title,
                    Location = dto.Location,
                    Status = dto.Status,
                    AppliedDate = dto.AppliedDate ?? DateTime.UtcNow,
                    SourceUrl = dto.SourceUrl,
                    Notes = dto.Notes
                };
            }

            _context.JobApplications.Add(entity);
            await _context.SaveChangesAsync();

            var readDto = _mapper.Map<JobApplicationReadDto>(entity);
            return CreatedAtAction(nameof(GetApplication), new { id = entity.Id }, readDto);
        }

        // -------------------------------------------------
        // PUT: api/JobApplications/5
        //
        // Allows updating status, dates, notes, and optionally
        // changing the linked Job (which auto-updates title/location).
        // -------------------------------------------------
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateApplication(int id, JobApplicationCreateDto dto)
        {
            var userId = GetUserId();

            var app = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (app == null)
                return NotFound();

            // If JobId changed, re-bind to that job and auto-fill fields
            if (dto.JobId.HasValue && dto.JobId != app.JobId)
            {
                var job = await _context.Jobs
                    .Include(j => j.Company)
                    .FirstOrDefaultAsync(j => j.Id == dto.JobId.Value);

                if (job == null)
                    return BadRequest($"Job with id {dto.JobId.Value} was not found.");

                app.JobId = job.Id;
                app.CompanyId = job.CompanyId;
                app.Title = job.Title;
                app.Location = job.Location ?? job.Company?.Location;
            }
            else if (!dto.JobId.HasValue)
            {
                // Manual editing path
                if (!string.IsNullOrWhiteSpace(dto.Title))
                    app.Title = dto.Title;

                if (dto.Location != null)
                    app.Location = dto.Location;

                if (dto.CompanyId.HasValue)
                    app.CompanyId = dto.CompanyId;
            }

            // Common fields
            app.Status = dto.Status;
            app.AppliedDate = dto.AppliedDate ?? app.AppliedDate;
            app.SourceUrl = dto.SourceUrl ?? app.SourceUrl;
            app.Notes = dto.Notes ?? app.Notes;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // -------------------------------------------------
        // DELETE: api/JobApplications/5
        // -------------------------------------------------
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            var userId = GetUserId();

            var app = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (app == null)
                return NotFound();

            _context.JobApplications.Remove(app);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
