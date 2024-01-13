using Microsoft.AspNetCore.Identity;

namespace BMCA.Models
{
    public class ApplicationUser : IdentityUser
    {

        public virtual ICollection<BindChannelUser>? BindsChannelUser { get; set; }

        public virtual ICollection<Message>? Messages { get; set; }

    }
}
