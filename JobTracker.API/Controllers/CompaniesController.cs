using AutoMapper;
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
    public class CompaniesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CompaniesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/companies
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanies()
        {
            var companies = await _context.Companies
                .OrderBy(c => c.Name)
                .ToListAsync();

            var dtos = _mapper.Map<List<CompanyDto>>(companies);
            return Ok(dtos);
        }

        // GET: api/companies/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CompanyDto>> GetCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
                return NotFound();

            var dto = _mapper.Map<CompanyDto>(company);
            return Ok(dto);
        }

        // POST: api/companies
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CompanyDto>> CreateCompany(CompanyDto dto)
        {
            var company = _mapper.Map<Company>(dto);

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var readDto = _mapper.Map<CompanyDto>(company);
            return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, readDto);
        }

        // PUT: api/companies/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCompany(int id, CompanyDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Id mismatch");

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
                return NotFound();

            _mapper.Map(dto, company);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/companies/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
                return NotFound();

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}