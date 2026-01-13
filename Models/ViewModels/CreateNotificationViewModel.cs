using System.ComponentModel.DataAnnotations;

namespace SchoolNotificationSystem.Models.ViewModels
{
    public class CreateNotificationViewModel
    {
        [Required(ErrorMessage = "Please select a student")]
        [Display(Name = "Student")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select notification type")]
        [Display(Name = "Notification Type")]
        public string Type { get; set; } = string.Empty;

        // NEW: Priority field
        [Required(ErrorMessage = "Please select priority")]
        [Display(Name = "Priority")]
        public string Priority { get; set; } = "Normal";
    }
}