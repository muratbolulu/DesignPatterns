using Microsoft.AspNetCore.Identity;

namespace BaseProject.Models
{
    public class AppUser: IdentityUser
    {
        public int Age { get; set; }
        //ihtiyaca göre içerisini ekleyeceğim.
    }
}
