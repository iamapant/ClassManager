using Microsoft.AspNetCore.Identity;

namespace DBfirst.Models
{
    public class User : IdentityUser
    {
        public string ActiveCode { get; set; }
        public bool? IsActive { get; set; }
        public ICollection<Teacher> Teachers { get; set; }
        public ICollection<Student> Students { get; set; }
    }
}
