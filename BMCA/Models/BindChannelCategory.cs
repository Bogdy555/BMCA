using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BMCA.Models
{

    public class BindChannelCategory
    {

        public int ChannelId { get; set; }

        public int CategoryId { get; set; }

        public virtual Channel? Channel { get; set; }

        public virtual Category? Category { get; set; }

    }

}
