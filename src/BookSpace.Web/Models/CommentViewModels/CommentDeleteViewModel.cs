using System.ComponentModel.DataAnnotations;

namespace BookSpace.Web.Models.CommentViewModels
{
    public class CommentDeleteViewModel
    {
        [Required]
        public string CommentId { get; set; }
    }
}
