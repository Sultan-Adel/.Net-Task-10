using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.ViewModels
{
    public class AddRoleViewModel
    {
        [Required(ErrorMessage = "Role Name is required.")]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;
    }
}
