using System.ComponentModel.DataAnnotations;

namespace SchoolNotificationSystem.Models.ViewModels
{
    public class AddChildViewModel
    {
        [Required(ErrorMessage = "Parent email is required")]
        [EmailAddress]
        [Display(Name = "Parent Email")]
        public string ParentEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Student name is required")]
        [Display(Name = "Student Name")]
        [StringLength(100)]
        public string StudentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Grade is required")]
        [Display(Name = "Grade")]
        public string Grade { get; set; } = string.Empty;
    }
}
