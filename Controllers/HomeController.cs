#nullable disable
using GiftOfTheGivers2.Data;
using GiftOfTheGivers2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GiftOfTheGivers2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalIncidents = await _context.DisasterIncidents.CountAsync();
            ViewBag.ActiveVolunteers = await _context.Users.CountAsync(u => u.UserType == "Volunteer" && u.IsActive);
            ViewBag.TotalDonations = await _context.Donations.CountAsync();
            ViewBag.OpenTasks = await _context.VolunteerTasks.CountAsync(t => t.Status == "Open");

            var recentIncidents = await _context.DisasterIncidents
                .OrderByDescending(i => i.ReportedAt)
                .Take(3)
                .ToListAsync();

            return View(recentIncidents);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
       


        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
