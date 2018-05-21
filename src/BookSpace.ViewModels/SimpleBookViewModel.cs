using System;
using System.Collections.Generic;

namespace BookSpace.ViewModels
{
    public class SimpleBookViewModel
    {
        public string BookId { get; set; }
        public string Title { get; set; }
        public string Isbn { get; set; }
        public DateTime PublicationYear { get; set; }
        public decimal Rating { get; set; }
        public string CoverUrl { get; set; }
        public string Author { get; set; }
    }
}
