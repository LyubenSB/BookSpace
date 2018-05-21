using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BookSpace.Web.Models.CommentViewModels
{
    public class CommentEditViewModel
    {
        [Required]
        public string CommentId { get; set; }

        [Required]
        public string BookId { get; set; }

        [Required]
        [MinLength(5)]
        public string Content { get; set; }
    }
}
