using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BMCA.Models
{

    public class BindChannelUser
    {

        [Required]
        public bool IsOwner { get; set; }

        public int ChannelId { get; set; }

        public string UserId { get; set; }

        public virtual Channel Channel { get; set; }

        public virtual ApplicationUser User { get; set; }

    }

}
