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
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Teacher,Office")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var students = await _context.Students.OrderBy(s => s.Name).ToListAsync();
            ViewBag.Students = new SelectList(students, "Id", "Name");

            if (User.IsInRole("Teacher"))
            {
                ViewBag.NotificationTypes = new SelectList(new[] { "Attendance", "Academic" });
            }
            else
            {
                ViewBag.NotificationTypes = new SelectList(new[] { "Administrative", "Health" });
            }

            // NEW: Priority options
            ViewBag.Priorities = new SelectList(new[] { "Normal", "Urgent", "Info" });

            return View();
        }

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

                var notification = new Notification
                {
                    StudentId = model.StudentId,
                    Title = model.Title,
                    Message = model.Message,
                    Type = model.Type,
                    Priority = model.Priority, // NEW
                    CreatedBy = user.FullName,
                    CreatedDate = DateTime.Now,
                    IsRead = false // NEW
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Notification sent successfully!";

                if (User.IsInRole("Teacher"))
                    return RedirectToAction("TeacherDashboard", "Home");
                else
                    return RedirectToAction("OfficeDashboard", "Home");
            }

            var students = await _context.Students.OrderBy(s => s.Name).ToListAsync();
            ViewBag.Students = new SelectList(students, "Id", "Name");

            if (User.IsInRole("Teacher"))
                ViewBag.NotificationTypes = new SelectList(new[] { "Attendance", "Academic" });
            else
                ViewBag.NotificationTypes = new SelectList(new[] { "Administrative", "Health" });

            ViewBag.Priorities = new SelectList(new[] { "Normal", "Urgent", "Info" });

            return View(model);
        }

        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> List()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var students = await _context.Students.Where(s => s.ParentId == user.Id).ToListAsync();

            if (!students.Any())
            {
                ViewBag.Message = "No student records found.";
                return View(new List<Notification>());
            }

            var studentIds = students.Select(s => s.Id).ToList();

            var notifications = await _context.Notifications
                .Include(n => n.Student)
                .Where(n => studentIds.Contains(n.StudentId))
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            // NEW: Calculate statistics
            ViewBag.TotalNotifications = notifications.Count;
            ViewBag.UnreadCount = notifications.Count(n => !n.IsRead);
            ViewBag.UrgentCount = notifications.Count(n => n.Priority == "Urgent" && !n.IsRead);

            return View(notifications);
        }

        // NEW: Mark notification as read
        [Authorize(Roles = "Parent")]
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("List");
        }

        // NEW: Mark all as read
        [Authorize(Roles = "Parent")]
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var user = await _userManager.GetUserAsync(User);
            var students = await _context.Students.Where(s => s.ParentId == user.Id).ToListAsync();
            var studentIds = students.Select(s => s.Id).ToList();

            var unreadNotifications = await _context.Notifications
                .Where(n => studentIds.Contains(n.StudentId) && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "All notifications marked as read!";

            return RedirectToAction("List");
        }
    }
}