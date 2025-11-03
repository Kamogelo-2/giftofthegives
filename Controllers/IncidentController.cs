#nullable disable
using GiftOfTheGivers2.Data;
using GiftOfTheGivers2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers2.Controllers
{
    public class IncidentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IncidentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Incident/Report
        public IActionResult Report()
        {
            return View();
        }

        // POST: Incident/Report
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(DisasterIncident incident)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                incident.UserId = Guid.Parse(userId);
                incident.ReportedAt = DateTime.Now;
                incident.UpdatedAt = DateTime.Now;

                _context.Add(incident);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Incident reported successfully!";
                return RedirectToAction("Index", "Home");
            }
            return View(incident);
        }

        // GET: Incident/List
        public async Task<IActionResult> List()
        {
            var incidents = await _context.DisasterIncidents
                .Include(i => i.User)
                .OrderByDescending(i => i.ReportedAt)
                .ToListAsync();
            return View(incidents);
        }

        // GET: Incident/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var incident = await _context.DisasterIncidents
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.IncidentId == id);

            if (incident == null)
            {
                return NotFound();
            }

            return View(incident);
        }
    }
}

