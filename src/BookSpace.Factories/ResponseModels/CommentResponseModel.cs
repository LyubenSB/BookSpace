using System;

namespace BookSpace.Factories.ResponseModels
{
    public class CommentResponseModel
    {
        public string CommentId { get; set; }

        public string Content { get; set; }

        public DateTime Date { get; set; }

        public string Author { get; set; }

        public string AuthorPicUrl { get; set; }

        public string UserId { get; set; }

        public bool CanEdit { get; set; }

        public string BookId { get; set; }
    }
}