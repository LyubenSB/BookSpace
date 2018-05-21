using System.ComponentModel.DataAnnotations;

namespace BookSpace.Web.Models.CommentViewModels
{
    public class CommentEditGetViewModel
    {
        [Required]
        public string CommentId { get; set; }
    }
}
