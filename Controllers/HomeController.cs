// ADD THESE METHODS TO YOUR EXISTING HomeController.cs

// GET: /Home/ManageStudents (Office only)
[Authorize(Roles = "Office")]
public async Task<IActionResult> ManageStudents()
{
    var user = await _userManager.GetUserAsync(User);
    ViewBag.UserName = user?.FullName;

    // Get all students grouped by grade
    var students = await _context.Students
        .Include(s => s.Parent)
        .OrderBy(s => s.Grade)
        .ThenBy(s => s.Name)
        .ToListAsync();

    // Group students by grade
    var studentsByGrade = students
        .GroupBy(s => s.Grade)
        .ToDictionary(g => g.Key, g => g.ToList());

    var viewModel = new ManageStudentsViewModel
    {
        StudentsByGrade = studentsByGrade,
        TotalStudents = students.Count,
        TotalGrades = studentsByGrade.Keys.Count
    };

    return View(viewModel);
}

// GET: /Home/RegisterParent (Office only)
[Authorize(Roles = "Office")]
[HttpGet]
public IActionResult RegisterParent()
{
    // List of grades for dropdown
    var grades = new List<string>
    {
        "Grade 1", "Grade 2", "Grade 3", "Grade 4",
        "Grade 5", "Grade 6", "Grade 7", "Grade 8"
    };

    ViewBag.Grades = new SelectList(grades);
    return View();
}

// POST: /Home/RegisterParent
[Authorize(Roles = "Office")]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> RegisterParent(RegisterParentViewModel model)
{
    if (ModelState.IsValid)
    {
        // Create parent user
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
            // Add to Parent role
            await _userManager.AddToRoleAsync(parent, "Parent");

            // Create student
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

        // If failed, add errors
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    // Reload grades dropdown
    var grades = new List<string>
    {
        "Grade 1", "Grade 2", "Grade 3", "Grade 4",
        "Grade 5", "Grade 6", "Grade 7", "Grade 8"
    };
    ViewBag.Grades = new SelectList(grades);

    return View(model);
}

// GET: /Home/DeleteStudent (Office only)
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