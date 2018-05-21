using BookSpace.Data;
using BookSpace.Data.Contracts;
using BookSpace.Factories;
using BookSpace.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BookSpace.Services
{
    public class BookDataServices
    {
        private const string regexPatern = @"[^\w-]+";


        private readonly BookSpaceContext dbCtx;
        private readonly IRepository<Genre> genreRepository;
        private readonly IRepository<Tag> tagRepository;

        private readonly IUpdateService<Genre> genreUpdateService;
        private readonly IUpdateService<Tag> tagUpdateService;
        private readonly IUpdateService<BookGenre> bookGenreUpdateService;
        private readonly IUpdateService<BookTag> bookTagUpdateService;

        public BookDataServices(BookSpaceContext dbCtx,
                                IRepository<Genre> genreRepository,
                                IRepository<Tag> tagRepository,
                                IUpdateService<Genre> genreUpdateService,
                                IUpdateService<Tag> tagUpdateService,
                                IUpdateService<BookGenre> bookGenreUpdateService,
                                IUpdateService<BookTag> bookTagUpdateService,
                                UserManager<ApplicationUser> user)
        {
            this.dbCtx = dbCtx ?? throw new ArgumentNullException(nameof(dbCtx));
            this.genreRepository = genreRepository;
            this.tagRepository = tagRepository;
            this.genreUpdateService = genreUpdateService;
            this.tagUpdateService = tagUpdateService;
            this.bookGenreUpdateService = bookGenreUpdateService;
            this.bookTagUpdateService = bookTagUpdateService;
        }

        //splitting tags genres response to seperate entities
        public IEnumerable<string> FormatStringResponse(string response)
        {
            try
            {
                var handledString = Regex.Split(response, regexPatern, RegexOptions.None);
                return handledString;
            }
            catch (ArgumentNullException)
            {

                return null;
            }
        }

        public async Task MatchGenresToBookAsync(IEnumerable<string> genres, string bookId)
        {
            foreach (var genreName in genres)
            {
                var genre = await this.genreRepository.GetByExpressionAsync(g => g.Name == genreName);

                if (genre == null)
                {
                    var genreNew = new Genre()
                    {
                        GenreId = Guid.NewGuid().ToString(),
                        Name = genreName
                    };
                    genre = genreNew;

                    await this.genreUpdateService.AddAsync(genre);
                }
                var genreId = genre.GenreId;

                var bookGenreRecord = new BookGenre()
                {
                    BookId = bookId,
                    GenreId = genreId
                };

                await this.bookGenreUpdateService.AddAsync(bookGenreRecord);
            }
        }

        public async Task MatchTagToBookAsync(IEnumerable<string> tags, string bookId)
        {
            foreach (var tagName in tags)
            {
                var tag = await this.tagRepository.GetByExpressionAsync(t => t.Value == tagName);

                if (tag == null)
                {
                    var tagNew = new Tag()
                    {
                        TagId = Guid.NewGuid().ToString(),
                        Value = tagName
                    };

                    tag = tagNew;
                    await this.tagUpdateService.AddAsync(tag);
                }
                var tagId = tag.TagId;

                var bookTagRecord = new BookTag()
                {
                    BookId = bookId,
                    TagId = tagId
                };
                await this.bookTagUpdateService.AddAsync(bookTagRecord);
            }
        }

        public void MatchCommentToUser(string commentId, string userId)
        {

            var user = dbCtx.Users.Where(u => u.Id == userId).SingleOrDefault();

            var comment = dbCtx.Comments.Where(cm => cm.CommentId == commentId).SingleOrDefault();

            user.Comments.Add(comment);

            comment.User = user;

        }
    }
}
