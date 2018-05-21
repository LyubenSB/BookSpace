using BookSpace.Data;
using BookSpace.Data.Contracts;
using BookSpace.Factories;
using BookSpace.Factories.ResponseModels;
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
        private readonly IRepository<Book> bookRepository;
        private readonly IRepository<Genre> genreRepository;
        private readonly IRepository<Tag> tagRepository;
        private readonly IUpdateService<Book> bookUpdateService;
        private readonly IUpdateService<Genre> genreUpdateService;
        private readonly IUpdateService<Tag> tagUpdateService;
        private readonly IUpdateService<BookGenre> bookGenreUpdateService;
        private readonly IUpdateService<BookTag> bookTagUpdateService;
        private readonly UserManager<ApplicationUser> userManager;

        public BookDataServices(
                                IRepository<Book> bookRepository,
                                IRepository<Genre> genreRepository,
                                IRepository<Tag> tagRepository,
                                IUpdateService<Book> bookUpdateService,
                                IUpdateService<Genre> genreUpdateService,
                                IUpdateService<Tag> tagUpdateService,
                                IUpdateService<BookGenre> bookGenreUpdateService,
                                IUpdateService<BookTag> bookTagUpdateService,
                                UserManager<ApplicationUser> userManager)
        {
            this.bookRepository = bookRepository;
            this.genreRepository = genreRepository;
            this.tagRepository = tagRepository;
            this.bookUpdateService = bookUpdateService;
            this.genreUpdateService = genreUpdateService;
            this.tagUpdateService = tagUpdateService;
            this.bookGenreUpdateService = bookGenreUpdateService;
            this.bookTagUpdateService = bookTagUpdateService;
            this.userManager = userManager;
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

        public async Task MatchCommentToUser(IEnumerable<Comment> comments)
        {
            foreach (var comment in comments)
            {
                var user = await this.userManager.FindByIdAsync(comment.UserId);
                comment.User = user;
            }
        }

        public async Task MatchUserToPicture(IEnumerable<CommentResponseModel> comments)
        {
            foreach (var comment in comments)
            {
                var user = await this.userManager.FindByNameAsync(comment.Author);
                comment.AuthorPicUrl = user.ProfilePictureUrl;
            }
        }

        public async Task CheckUserCommentRights(IEnumerable<CommentResponseModel> comments, string username)
        {
            foreach (var comment in comments)
            {
                var user = await this.userManager.FindByNameAsync(username);
                var isAdmin = user.isAdmin;
                var commentCreator = comment.Author;
                var isCreator = commentCreator == username;
                comment.CanEdit = isAdmin || isCreator;
            }
        }

        public async Task UpdateBookRating(string id, string rate, bool isNewUser)
        {
            var book = await this.bookRepository.GetByIdAsync(id);
            int ratesCount = book.RatesCount;
            if (isNewUser)
            {
                book.RatesCount++;
                book.Rating = ((book.Rating * (ratesCount)) + int.Parse(rate)) / (ratesCount + 1);
            }
            else
            {
                book.Rating = ((book.Rating * (ratesCount - 1)) + int.Parse(rate)) / ratesCount;
            }

            await this.bookUpdateService.UpdateAsync(book);
        }
    }
}
