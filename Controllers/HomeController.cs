// Controllers/HomeController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolNotificationSystem.Data;
using SchoolNotificationSystem.Models;

namespace SchoolNotificationSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Home/Index (Landing page)
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Teacher"))
                    return RedirectToAction("TeacherDashboard");
                if (User.IsInRole("Office"))
                    return RedirectToAction("OfficeDashboard");
                if (User.IsInRole("Parent"))
                    return RedirectToAction("ParentDashboard");
            }

            return RedirectToAction("Login", "Account");
        }

        // GET: /Home/TeacherDashboard
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> TeacherDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserName = user?.FullName;

            // Get recent notifications created by this teacher
            var recentNotifications = await _context.Notifications
                .Include(n => n.Student)
                .Where(n => n.CreatedBy == user!.FullName)
                .OrderByDescending(n => n.CreatedDate)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalNotifications = await _context.Notifications
                .Where(n => n.CreatedBy == user!.FullName)
                .CountAsync();

            return View(recentNotifications);
        }

        // GET: /Home/OfficeDashboard
        [Authorize(Roles = "Office")]
        public async Task<IActionResult> OfficeDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserName = user?.FullName;

            // Get all notifications
            var recentNotifications = await _context.Notifications
                .Include(n => n.Student)
                .OrderByDescending(n => n.CreatedDate)
                .Take(10)
                .ToListAsync();

            ViewBag.TotalNotifications = await _context.Notifications.CountAsync();
            ViewBag.TotalStudents = await _context.Students.CountAsync();

            return View(recentNotifications);
        }

        // GET: /Home/ParentDashboard
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> ParentDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserName = user?.FullName;

            // Get parent's children
            var students = await _context.Students
                .Where(s => s.ParentId == user!.Id)
                .ToListAsync();

            if (!students.Any())
            {
                ViewBag.Message = "No student records found for your account.";
                return View(new List<Notification>());
            }

            var studentIds = students.Select(s => s.Id).ToList();

            // Get notifications for parent's children
            var notifications = await _context.Notifications
                .Include(n => n.Student)
                .Where(n => studentIds.Contains(n.StudentId))
                .OrderByDescending(n => n.CreatedDate)
                .Take(10)
                .ToListAsync();

            ViewBag.StudentName = students.First().Name;
            ViewBag.TotalNotifications = await _context.Notifications
                .Where(n => studentIds.Contains(n.StudentId))
                .CountAsync();

            return View(notifications);
        }
    }
}
