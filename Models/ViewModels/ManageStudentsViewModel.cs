namespace SchoolNotificationSystem.Models.ViewModels
{
    public class ManageStudentsViewModel
    {
        public Dictionary<string, List<Student>> StudentsByGrade { get; set; } = new();
        public int TotalStudents { get; set; }
        public int TotalGrades { get; set; }
    }
}
