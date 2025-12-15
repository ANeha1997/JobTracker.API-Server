using AutoMapper;
using JobTracker.API;
using JobTracker.API.Dtos;
using JobTracker.API.Models;
using JobTracker.API.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobTracker.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationsHub> _hubContext;

        public InterviewsController(
            ApplicationDbContext context,
            IMapper mapper,
            IHubContext<NotificationsHub> hubContext)
        {
            _context = context;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private static InterviewType MapTypeFromString(string? type)
        {
            if (!string.IsNullOrWhiteSpace(type))
            {
                var cleaned = type.Replace(" ", "");
                if (Enum.TryParse<InterviewType>(cleaned, ignoreCase: true, out var parsed))
                {
                    return parsed;
                }
            }
            return default;
        }

        private static string MapTypeToString(InterviewType type)
        {
            return type.ToString();
        }

        [HttpGet("by-application/{applicationId:int}")]
        public async Task<ActionResult<IEnumerable<InterviewReadDto>>> GetForApplication(int applicationId)
        {
            var userId = GetUserId();

            var owned = await _context.JobApplications
                .AnyAsync(a => a.Id == applicationId && a.UserId == userId);

            if (!owned)
                return Forbid();

            var interviews = await _context.Interviews
                .Where(i => i.JobApplicationId == applicationId)
                .OrderBy(i => i.ScheduledAt)
                .ToListAsync();

            var dtoList = interviews.Select(i => new InterviewReadDto
            {
                Id = i.Id,
                JobApplicationId = i.JobApplicationId,
                ScheduledAt = i.ScheduledAt,
                Type = MapTypeToString(i.Type),
                LocationOrLink = i.LocationOrLink,
                Notes = i.Notes,
                Result = i.Result
            });

            return Ok(dtoList);
        }

        [HttpPost]
        public async Task<ActionResult<InterviewReadDto>> Create(InterviewCreateDto dto)
        {
            var userId = GetUserId();

            // verify ownership
            var app = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.Id == dto.JobApplicationId && a.UserId == userId);

            if (app == null)
                return Forbid();

            var entity = new Interview
            {
                JobApplicationId = dto.JobApplicationId,
                ScheduledAt = dto.ScheduledAt,
                Type = MapTypeFromString(dto.Type),
                LocationOrLink = dto.LocationOrLink,
                Notes = dto.Notes,
                Result = dto.Result
            };

            _context.Interviews.Add(entity);
            await _context.SaveChangesAsync();

            var readDto = new InterviewReadDto
            {
                Id = entity.Id,
                JobApplicationId = entity.JobApplicationId,
                ScheduledAt = entity.ScheduledAt,
                Type = MapTypeToString(entity.Type),
                LocationOrLink = entity.LocationOrLink,
                Notes = entity.Notes,
                Result = entity.Result
            };

            // load job + company info (Company can be null)
            var appWithCompany = await _context.JobApplications
                .Include(a => a.Company)
                .FirstAsync(a => a.Id == entity.JobApplicationId);

            //  build notification payload
            var notification = new InterviewNotificationDto
            {
                InterviewId = entity.Id,
                ScheduledAt = entity.ScheduledAt,
                JobApplicationId = entity.JobApplicationId,
                JobTitle = appWithCompany.Title,
                CompanyName = appWithCompany.Company?.Name ?? "Unknown Company"
            };

            // send enriched notification
            await _hubContext.Clients
                .User(userId)
                .SendAsync("InterviewCreated", notification);

            return CreatedAtAction(
                nameof(GetForApplication),
                new { applicationId = dto.JobApplicationId },
                readDto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, InterviewCreateDto dto)
        {
            var userId = GetUserId();

            var interview = await _context.Interviews
                .Include(i => i.JobApplication)
                .FirstOrDefaultAsync(i => i.Id == id && i.JobApplication.UserId == userId);

            if (interview == null)
                return NotFound();

            var appOwned = await _context.JobApplications
                .AnyAsync(a => a.Id == dto.JobApplicationId && a.UserId == userId);

            if (!appOwned)
                return Forbid();

            interview.JobApplicationId = dto.JobApplicationId;
            interview.ScheduledAt = dto.ScheduledAt;
            interview.Type = MapTypeFromString(dto.Type);
            interview.LocationOrLink = dto.LocationOrLink;
            interview.Notes = dto.Notes;
            interview.Result = dto.Result;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();

            var interview = await _context.Interviews
                .Include(i => i.JobApplication)
                .FirstOrDefaultAsync(i => i.Id == id && i.JobApplication.UserId == userId);

            if (interview == null)
                return NotFound();

            _context.Interviews.Remove(interview);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
