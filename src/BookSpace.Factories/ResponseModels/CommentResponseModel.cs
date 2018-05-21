using System;

namespace BookSpace.Factories.ResponseModels
{
    public class CommentResponseModel
    {
        public string UserId { get; set; }

        public string CommentId { get; set; }

        public string BookId { get; set; }

        public string Content { get; set; }

        public DateTime Date { get; set; }
    }
}