using Microsoft.AspNetCore.Identity;

namespace BMCA.Models
{
    public class ApplicationUser : IdentityUser
    {

        public virtual ICollection<BindChannelUser>? BindChannelUser { get; set; }

    }
}
