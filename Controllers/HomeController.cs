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
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

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

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> TeacherDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.UserName = user.FullName;
            var recentNotifications = await _context.Notifications.Include(n => n.Student).Where(n => n.CreatedBy == user.FullName).OrderByDescending(n => n.CreatedDate).Take(5).ToListAsync();
            ViewBag.TotalNotifications = await _context.Notifications.Where(n => n.CreatedBy == user.FullName).CountAsync();
            return View(recentNotifications);
        }

        [Authorize(Roles = "Office")]
        public async Task<IActionResult> OfficeDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.UserName = user.FullName;
            var recentNotifications = await _context.Notifications.Include(n => n.Student).OrderByDescending(n => n.CreatedDate).Take(10).ToListAsync();
            ViewBag.TotalNotifications = await _context.Notifications.CountAsync();
            ViewBag.TotalStudents = await _context.Students.CountAsync();
            return View(recentNotifications);
        }

        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> ParentDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.UserName = user.FullName;
            var students = await _context.Students.Where(s => s.ParentId == user.Id).ToListAsync();
            if (!students.Any())
            {
                ViewBag.Message = "No student records found for your account.";
                return View(new List<Notification>());
            }
            var studentIds = students.Select(s => s.Id).ToList();
            var notifications = await _context.Notifications.Include(n => n.Student).Where(n => studentIds.Contains(n.StudentId)).OrderByDescending(n => n.CreatedDate).Take(10).ToListAsync();
            ViewBag.StudentName = students.First().Name;
            ViewBag.TotalNotifications = await _context.Notifications.Where(n => studentIds.Contains(n.StudentId)).CountAsync();
            return View(notifications);
        }

        [Authorize(Roles = "Office")]
        public async Task<IActionResult> ManageStudents()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserName = user?.FullName;
            var students = await _context.Students.Include(s => s.Parent).OrderBy(s => s.Grade).ThenBy(s => s.Name).ToListAsync();
            var studentsByGrade = students.GroupBy(s => s.Grade).ToDictionary(g => g.Key, g => g.ToList());
            var viewModel = new ManageStudentsViewModel
            {
                StudentsByGrade = studentsByGrade,
                TotalStudents = students.Count,
                TotalGrades = studentsByGrade.Keys.Count
            };
            return View(viewModel);
        }

        [Authorize(Roles = "Office")]
        [HttpGet]
        public IActionResult RegisterParent()
        {
            var grades = new List<string> { "Grade 1", "Grade 2", "Grade 3", "Grade 4", "Grade 5", "Grade 6", "Grade 7", "Grade 8" };
            ViewBag.Grades = new SelectList(grades);
            return View();
        }

        [Authorize(Roles = "Office")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterParent(RegisterParentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var parent = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.ParentFullName,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(parent, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(parent, "Parent");
                    var student = new Student
                    {
                        Name = model.StudentName,
                        Grade = model.Grade,
                        ParentId = parent.Id
                    };
                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Parent {model.ParentFullName} and student {model.StudentName} registered successfully!";
                    return RedirectToAction("ManageStudents");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            var grades = new List<string> { "Grade 1", "Grade 2", "Grade 3", "Grade 4", "Grade 5", "Grade 6", "Grade 7", "Grade 8" };
            ViewBag.Grades = new SelectList(grades);
            return View(model);
        }

        [Authorize(Roles = "Office")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Student deleted successfully!";
            }
            return RedirectToAction("ManageStudents");
        }

        [Authorize(Roles = "Office")]
        [HttpGet]
        public async Task<IActionResult> EditStudent(int id)
        {
            var student = await _context.Students.Include(s => s.Parent).FirstOrDefaultAsync(s => s.Id == id);
            if (student == null)
            {
                return NotFound();
            }
            var viewModel = new EditStudentViewModel
            {
                Id = student.Id,
                Name = student.Name,
                Grade = student.Grade,
                ParentName = student.Parent.FullName,
                ParentEmail = student.Parent.Email
            };
            var grades = new List<string> { "Grade 1", "Grade 2", "Grade 3", "Grade 4", "Grade 5", "Grade 6", "Grade 7", "Grade 8" };
            ViewBag.Grades = new SelectList(grades);
            return View(viewModel);
        }

        [Authorize(Roles = "Office")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(EditStudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var student = await _context.Students.FindAsync(model.Id);
                if (student == null)
                {
                    return NotFound();
                }
                student.Name = model.Name;
                student.Grade = model.Grade;
                _context.Students.Update(student);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Student {model.Name} updated successfully!";
                return RedirectToAction("ManageStudents");
            }
            var grades = new List<string> { "Grade 1", "Grade 2", "Grade 3", "Grade 4", "Grade 5", "Grade 6", "Grade 7", "Grade 8" };
            ViewBag.Grades = new SelectList(grades);
            return View(model);
        }

        [Authorize(Roles = "Office")]
        [HttpGet]
        public IActionResult AddChild()
        {
            var grades = new List<string> { "Grade 1", "Grade 2", "Grade 3", "Grade 4", "Grade 5", "Grade 6", "Grade 7", "Grade 8" };
            ViewBag.Grades = new SelectList(grades);
            return View();
        }

        [Authorize(Roles = "Office")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChild(AddChildViewModel model)
        {
            if (ModelState.IsValid)
            {
                var parent = await _userManager.FindByEmailAsync(model.ParentEmail);
                if (parent == null)
                {
                    ModelState.AddModelError("ParentEmail", "Parent with this email not found.");
                }
                else
                {
                    var isParent = await _userManager.IsInRoleAsync(parent, "Parent");
                    if (!isParent)
                    {
                        ModelState.AddModelError("ParentEmail", "This user is not a parent.");
                    }
                    else
                    {
                        var student = new Student
                        {
                            Name = model.StudentName,
                            Grade = model.Grade,
                            ParentId = parent.Id
                        };
                        _context.Students.Add(student);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = $"Student {model.StudentName} added to parent {parent.FullName} successfully!";
                        return RedirectToAction("ManageStudents");
                    }
                }
            }
            var grades = new List<string> { "Grade 1", "Grade 2", "Grade 3", "Grade 4", "Grade 5", "Grade 6", "Grade 7", "Grade 8" };
            ViewBag.Grades = new SelectList(grades);
            return View(model);
        }

        [Authorize(Roles = "Office")]
        public async Task<IActionResult> SearchStudents(string searchTerm, string selectedGrade)
        {
            var query = _context.Students.Include(s => s.Parent).AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s => s.Name.Contains(searchTerm) || s.Parent.FullName.Contains(searchTerm) || s.Parent.Email.Contains(searchTerm));
            }
            if (!string.IsNullOrWhiteSpace(selectedGrade))
            {
                query = query.Where(s => s.Grade == selectedGrade);
            }
            var results = await query.OrderBy(s => s.Name).ToListAsync();
            var viewModel = new SearchStudentsViewModel
            {
                SearchTerm = searchTerm ?? string.Empty,
                SelectedGrade = selectedGrade ?? string.Empty,
                Results = results,
                TotalResults = results.Count
            };
            var grades = new List<string> { "Grade 1", "Grade 2", "Grade 3", "Grade 4", "Grade 5", "Grade 6", "Grade 7", "Grade 8" };
            ViewBag.Grades = new SelectList(grades);
            return View(viewModel);
        }
    }
}