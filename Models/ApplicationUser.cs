// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace SchoolNotificationSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}


