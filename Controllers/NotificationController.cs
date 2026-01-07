using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolNotificationSystem.Data;
using SchoolNotificationSystem.Models;
using SchoolNotificationSystem.Models.ViewModels;

namespace SchoolNotificationSystem.Controllers
{
    [Authorize] // All actions require authentication
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Notification/Create (For Teachers and Office only)
        [Authorize(Roles = "Teacher,Office")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Get all students for dropdown
            var students = await _context.Students
                .OrderBy(s => s.Name)
                .ToListAsync();

            ViewBag.Students = new SelectList(students, "Id", "Name");

            // Set notification types based on role
            if (User.IsInRole("Teacher"))
            {
                ViewBag.NotificationTypes = new SelectList(new[] { "Attendance", "Academic" });
            }
            else // Office
            {
                ViewBag.NotificationTypes = new SelectList(new[] { "Administrative", "Health" });
            }

            return View();
        }

        // POST: /Notification/Create
        [Authorize(Roles = "Teacher,Office")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNotificationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Create new notification
                var notification = new Notification
                {
                    StudentId = model.StudentId,
                    Title = model.Title,
                    Message = model.Message,
                    Type = model.Type,
                    CreatedBy = user.FullName,
                    CreatedDate = DateTime.Now
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Notification sent successfully!";

                // Redirect based on role
                if (User.IsInRole("Teacher"))
                    return RedirectToAction("TeacherDashboard", "Home");
                else
                    return RedirectToAction("OfficeDashboard", "Home");
            }

            // If validation fails, reload dropdowns
            var students = await _context.Students.OrderBy(s => s.Name).ToListAsync();
            ViewBag.Students = new SelectList(students, "Id", "Name");

            if (User.IsInRole("Teacher"))
                ViewBag.NotificationTypes = new SelectList(new[] { "Attendance", "Academic" });
            else
                ViewBag.NotificationTypes = new SelectList(new[] { "Administrative", "Health" });

            return View(model);
        }

        // GET: /Notification/List (For Parents only)
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> List()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get parent's children
            var students = await _context.Students
                .Where(s => s.ParentId == user.Id)
                .ToListAsync();

            if (!students.Any())
            {
                ViewBag.Message = "No student records found.";
                return View(new List<Notification>());
            }

            var studentIds = students.Select(s => s.Id).ToList();

            // Get all notifications for parent's children
            var notifications = await _context.Notifications
                .Include(n => n.Student)
                .Where(n => studentIds.Contains(n.StudentId))
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            return View(notifications);
        }
    }
}