// Data/DbInitializer.cs
using Microsoft.AspNetCore.Identity;
using SchoolNotificationSystem.Models;

namespace SchoolNotificationSystem.Data
{
    // Initializes database with sample data for testing
    public static class DbInitializer
    {
        public static async Task Initialize(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Create roles if they don't exist
            string[] roleNames = { "Teacher", "Office", "Parent" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            // Create Teacher user
            if (await userManager.FindByEmailAsync("teacher@school.com") == null)
            {
                var teacher = new ApplicationUser
                {
                    UserName = "teacher@school.com",
                    Email = "teacher@school.com",
                    FullName = "Ahmed Hassan (Teacher)",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(teacher, "Teacher@123");
                await userManager.AddToRoleAsync(teacher, "Teacher");
            }

            // Create Office user
            if (await userManager.FindByEmailAsync("office@school.com") == null)
            {
                var office = new ApplicationUser
                {
                    UserName = "office@school.com",
                    Email = "office@school.com",
                    FullName = "Fatima Ali (Office Admin)",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(office, "Office@123");
                await userManager.AddToRoleAsync(office, "Office");
            }

            // Create Parent users with students
            var parentData = new[]
            {
                new { Email = "parent1@gmail.com", Name = "Mohammed Ahmed", StudentName = "Ali Ahmed", Grade = "Grade 1" },
                new { Email = "parent2@gmail.com", Name = "Aisha Mohammed", StudentName = "Sara Mohammed", Grade = "Grade 2" },
                new { Email = "parent3@gmail.com", Name = "Hassan Ibrahim", StudentName = "Omar Hassan", Grade = "Grade 3" }
            };

            foreach (var data in parentData)
            {
                if (await userManager.FindByEmailAsync(data.Email) == null)
                {
                    var parent = new ApplicationUser
                    {
                        UserName = data.Email,
                        Email = data.Email,
                        FullName = data.Name,
                        EmailConfirmed = true
                    };

                    await userManager.CreateAsync(parent, "Parent@123");
                    await userManager.AddToRoleAsync(parent, "Parent");

                    // Create student for this parent
                    var student = new Student
                    {
                        Name = data.StudentName,
                        Grade = data.Grade,
                        ParentId = parent.Id
                    };

                    context.Students.Add(student);
                }
            }

            await context.SaveChangesAsync();

            // Create sample notifications
            if (!context.Notifications.Any())
            {
                var students = context.Students.ToList();
                var teacher = await userManager.FindByEmailAsync("teacher@school.com");

                if (students.Any() && teacher != null)
                {
                    var notifications = new[]
                    {
                        new Notification
                        {
                            StudentId = students[0].Id,
                            Title = "Attendance - Present Today",
                            Message = "Your child Ali Ahmed was present and participated well in class activities today.",
                            Type = "Attendance",
                            CreatedBy = teacher.FullName,
                            CreatedDate = DateTime.Now.AddDays(-2)
                        },
                        new Notification
                        {
                            StudentId = students[0].Id,
                            Title = "Monthly Exam Results",
                            Message = "Total Marks: 450/500. Average: 90%. Excellent performance!",
                            Type = "Academic",
                            CreatedBy = teacher.FullName,
                            CreatedDate = DateTime.Now.AddDays(-1)
                        },
                        new Notification
                        {
                            StudentId = students[1].Id,
                            Title = "School Holiday - Eid Break",
                            Message = "School will be closed from 15th to 20th for Eid holidays. Classes resume on 21st.",
                            Type = "Administrative",
                            CreatedBy = "School Office",
                            CreatedDate = DateTime.Now
                        }
                    };

                    context.Notifications.AddRange(notifications);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
