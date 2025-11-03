#nullable disable
using GiftOfTheGivers2.Data;
using GiftOfTheGivers2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers2.Controllers
{
    public class DonationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DonationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Donation/Donate
        public async Task<IActionResult> Donate()
        {
            ViewBag.Categories = await _context.ResourceCategories.ToListAsync();
            return View();
        }

        // POST: Donation/Donate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Donate(Donation donation)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                donation.UserId = Guid.Parse(userId);
                donation.DonatedAt = DateTime.Now;
                donation.Status = "Pending";

                _context.Add(donation);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thank you for your donation!";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Categories = await _context.ResourceCategories.ToListAsync();
            return View(donation);
        }

        // GET: Donation/MyDonations
        public async Task<IActionResult> MyDonations()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var donations = await _context.Donations
                .Include(d => d.Category)
                .Where(d => d.UserId == Guid.Parse(userId))
                .OrderByDescending(d => d.DonatedAt)
                .ToListAsync();

            return View(donations);
        }

        // GET: Donation/All
        public async Task<IActionResult> All()
        {
            var donations = await _context.Donations
                .Include(d => d.User)
                .Include(d => d.Category)
                .OrderByDescending(d => d.DonatedAt)
                .ToListAsync();
            return View(donations);
        }
    }
}

