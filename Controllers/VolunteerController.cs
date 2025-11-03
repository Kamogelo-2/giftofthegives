#nullable disable
using GiftOfTheGivers2.Data;
using GiftOfTheGivers2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers2.Controllers
{
    public class VolunteerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VolunteerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Volunteer/Tasks
        public async Task<IActionResult> Tasks()
        {
            var tasks = await _context.VolunteerTasks
                .Include(t => t.Incident)
                .Where(t => t.Status == "Open" && t.StartDate > DateTime.Now)
                .OrderBy(t => t.StartDate)
                .ToListAsync();
            return View(tasks);
        }

        // GET: Volunteer/TaskDetails/5
        public async Task<IActionResult> TaskDetails(Guid id)
        {
            var task = await _context.VolunteerTasks
                .Include(t => t.Incident)
                .FirstOrDefaultAsync(t => t.TaskId == id);

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: Volunteer/JoinTask/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinTask(Guid id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var task = await _context.VolunteerTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            // Check if user is already assigned
            var existingAssignment = await _context.VolunteerAssignments
                .FirstOrDefaultAsync(va => va.TaskId == id && va.UserId == Guid.Parse(userId));

            if (existingAssignment != null)
            {
                TempData["ErrorMessage"] = "You are already assigned to this task.";
                return RedirectToAction("TaskDetails", new { id });
            }

            // Check if task has available slots
            if (task.CurrentVolunteers >= task.RequiredVolunteers)
            {
                TempData["ErrorMessage"] = "This task is already full.";
                return RedirectToAction("TaskDetails", new { id });
            }

            // Create assignment
            var assignment = new VolunteerAssignment
            {
                TaskId = id,
                UserId = Guid.Parse(userId),
                AssignmentDate = DateTime.Now,
                Status = "Assigned"
            };

            // Update task volunteer count
            task.CurrentVolunteers++;
            if (task.CurrentVolunteers >= task.RequiredVolunteers)
            {
                task.Status = "InProgress";
            }

            _context.Add(assignment);
            _context.Update(task);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Successfully joined the task!";
            return RedirectToAction("MyAssignments");
        }

        // GET: Volunteer/MyAssignments
        public async Task<IActionResult> MyAssignments()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var assignments = await _context.VolunteerAssignments
                .Include(a => a.Task)
                .ThenInclude(t => t.Incident)
                .Where(a => a.UserId == Guid.Parse(userId))
                .OrderByDescending(a => a.AssignmentDate)
                .ToListAsync();

            return View(assignments);
        }

        // POST: Volunteer/UpdateHours/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateHours(Guid id, decimal hoursWorked, string notes)
        {
            var assignment = await _context.VolunteerAssignments
                .Include(a => a.Task)
                .FirstOrDefaultAsync(a => a.AssignmentId == id);

            if (assignment == null)
            {
                return NotFound();
            }

            assignment.HoursWorked = hoursWorked;
            assignment.Notes = notes;
            assignment.Status = "Completed";

            _context.Update(assignment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Hours updated successfully!";
            return RedirectToAction("MyAssignments");
        }
    }
}

