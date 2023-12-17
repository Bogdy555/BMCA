using System.ComponentModel.DataAnnotations;

namespace BMCA.Models
{

    public class Category
    {

        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "A name is required!")]
        [MinLength(3, ErrorMessage = "Name must contain at least 3 characters!")]
        [StringLength(50, ErrorMessage = "Name must contain at most 50 characters!")]
        public string Name { get; set; }

        public virtual ICollection<BindChannelCategory>? BindChannelCategory { get; set; }

    }

}
