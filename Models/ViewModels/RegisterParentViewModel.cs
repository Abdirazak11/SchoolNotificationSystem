using System.ComponentModel.DataAnnotations;

namespace SchoolNotificationSystem.Models.ViewModels
{
    public class RegisterParentViewModel
    {
        // Parent Information
        [Required(ErrorMessage = "Parent full name is required")]
        [Display(Name = "Parent Full Name")]
        [StringLength(100)]
        public string ParentFullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Student Information
        [Required(ErrorMessage = "Student name is required")]
        [Display(Name = "Student Name")]
        [StringLength(100)]
        public string StudentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Grade is required")]
        [Display(Name = "Grade")]
        public string Grade { get; set; } = string.Empty;
    }
}
