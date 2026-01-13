
namespace SchoolNotificationSystem.Models.ViewModels
{
    public class SearchStudentsViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public string SelectedGrade { get; set; } = string.Empty;
        public List<Student> Results { get; set; } = new List<Student>();
        public int TotalResults { get; set; }
    }
}