using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(16, 65)]
        public int Age { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public ICollection<StuCrsRes> StuCrsRes { get; set; } = new List<StuCrsRes>();
    }
}
