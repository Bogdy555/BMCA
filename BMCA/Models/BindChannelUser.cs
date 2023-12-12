using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BMCA.Models
{

    public class BindChannelUser
    {

        [Key]
        public int Id { get; set; }

        public int ChannelId { get; set; }

        public string UserId { get; set; }

        public virtual Channel Channel { get; set; }

        public virtual ApplicationUser User { get; set; }

    }

}
