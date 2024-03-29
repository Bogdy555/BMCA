﻿using System.ComponentModel.DataAnnotations;

namespace BMCA.Models
{

    public class Message
    {

        [Key]
        public int ID { get; set; }

        [MinLength(1, ErrorMessage = "")]
        [StringLength(100, ErrorMessage = "What a log message you want to send...")]
        public string? Content { get; set; }

        [StringLength(100, ErrorMessage = "What a long file name...")]
        public string? FilePath { get; set; }

        [StringLength(100, ErrorMessage = "What a long file type...")]
        public string? FileType { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int ChannelId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Channel? Channel { get; set; }

    }

}
