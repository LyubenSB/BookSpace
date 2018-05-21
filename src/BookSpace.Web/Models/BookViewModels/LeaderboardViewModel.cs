using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookSpace.Web.Models.BookViewModels
{
    public class LeaderboardViewModel
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string Username { get; set; }

        public string ProfilePictureUrl { get; set; }

        public int BooksRed { get; set; }
    }
}
