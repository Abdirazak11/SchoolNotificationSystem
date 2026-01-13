using System.ComponentModel.DataAnnotations;

namespace SchoolNotificationSystem.Models.ViewModels
{
    public class EditStudentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Student name is required")]
        [Display(Name = "Student Name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Grade is required")]
        [Display(Name = "Grade")]
        public string Grade { get; set; } = string.Empty;

        [Display(Name = "Parent Name")]
        public string ParentName { get; set; } = string.Empty;

        [Display(Name = "Parent Email")]
        public string ParentEmail { get; set; } = string.Empty;
    }
}
