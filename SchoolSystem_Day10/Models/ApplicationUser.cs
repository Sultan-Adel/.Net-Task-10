using Microsoft.AspNetCore.Identity;

namespace SchoolSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
