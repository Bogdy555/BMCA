using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BMCA.Models
{

    public class Channel
    {

        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "A name is required!")]
        [MinLength(3, ErrorMessage = "Name must contain at least 3 characters!")]
        [StringLength(20, ErrorMessage = "Name must contain at most 20 characters!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "A description is required!")]
        [MinLength(3, ErrorMessage = "Description must contain at least 3 characters!")]
        [StringLength(200, ErrorMessage = "Description must contain at most 200 characters!")]
        public string Description { get; set; }

        public string OwnerId { get; set; }

        public virtual ApplicationUser Owner { get; set; }

    }

}
