using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;

namespace SchoolSystem.Controllers
{
    public class InstructorController : Controller
    {
        private readonly SchoolDbContext _context;

        public InstructorController(SchoolDbContext context)
        {
            _context = context;
        }

        // Endpoint: /Instructor/Index (Authorized for Authenticated Users only)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var instructors = await _context.Teachers
                .Include(t => t.Department)
                .Include(t => t.Course)
                .AsNoTracking()
                .ToListAsync();

            return View(instructors);
        }
    }
}
