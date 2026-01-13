using System.ComponentModel.DataAnnotations;

namespace SchoolNotificationSystem.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        // NEW: Priority/Category (Urgent, Normal, Info)
        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = "Normal";

        // NEW: Read/Unread status
        public bool IsRead { get; set; } = false;

        // NEW: When was it read?
        public DateTime? ReadDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public Student Student { get; set; } = null!;
    }
}