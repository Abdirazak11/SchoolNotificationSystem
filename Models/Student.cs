using System.ComponentModel.DataAnnotations;

namespace SchoolNotificationSystem.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Grade { get; set; } = string.Empty;

        [Required]
        public string ParentId { get; set; } = string.Empty;

        // NEW: Track when student was added
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public ApplicationUser Parent { get; set; } = null!;
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}