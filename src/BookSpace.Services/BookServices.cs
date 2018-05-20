using BookSpace.Data;
using BookSpace.Data.Contracts;
using BookSpace.Factories;
using BookSpace.Factories.ResponseModels;
using BookSpace.Models;
using BookSpace.Repositories;
using BookSpace.Repositories.Contracts;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BookSpace.Services
{
    public class BookServices
    {
        private const string regexPatern = @"[^\w-]+";
        private readonly BookSpaceContext dbCtx;
        private readonly IBookRepository bookRepository;
        private readonly IGenreRepository genreRepository;
        private readonly ITagRepository tagRepository;
        private readonly IBookGenreRepository bookGenreRepository;
        private readonly IBookTagRepository bookTagRepository;
        private readonly ICommentRepository commentRepository;
        private readonly UserManager<ApplicationUser> userManager;

        public BookServices(BookSpaceContext dbCtx, IBookRepository bookRepository, IGenreRepository genreRepository, ITagRepository tagRepository,
            IBookGenreRepository bookGenreRepository, IBookTagRepository bookTagRepository, ICommentRepository commentRepository, UserManager<ApplicationUser> userManager)
        {
            this.dbCtx = dbCtx ?? throw new ArgumentNullException(nameof(dbCtx));
            this.bookRepository = bookRepository;
            this.genreRepository = genreRepository;
            this.tagRepository = tagRepository;
            this.bookGenreRepository = bookGenreRepository;
            this.bookTagRepository = bookTagRepository;
            this.commentRepository = commentRepository;
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
                var genre = await this.genreRepository.GetGenreByNameAsync(genreName);
                if (genre == null)
                {
                    var genreNew = new Genre()
                    {
                        GenreId = Guid.NewGuid().ToString(),
                        Name = genreName
                    };
                    genre = genreNew;

                    await this.genreRepository.AddAsync(genre);
                }

                var genreId = genre.GenreId;
                var bookGenreRecord = new BookGenre()
                {
                    BookId = bookId,
                    GenreId = genreId
                };

                await this.bookGenreRepository.AddAsync(bookGenreRecord);
            }
        }

        public async Task MatchTagToBookAsync(IEnumerable<string> tags, string bookId)
        {
            foreach (var tagName in tags)
            {
                var tag = await this.tagRepository.GetTagByNameAsync(tagName);
                if (tag == null)
                {
                    var tagNew = new Tag()
                    {
                        TagId = Guid.NewGuid().ToString(),
                        Value = tagName
                    };

                    tag = tagNew;
                    await this.tagRepository.AddAsync(tag);
                }

                var tagId = tag.TagId;
                var bookTagRecord = new BookTag()
                {
                    BookId = bookId,
                    TagId = tagId
                };

                await this.bookTagRepository.AddAsync(bookTagRecord);
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

            await this.bookRepository.UpdateAsync(book);
        }

        public async Task<IEnumerable<Book>> SearchBook(string filterRadio, string filter)
        {

            List<Book> foundBooks = new List<Book>();
            if (filterRadio == "default")
            {
                foundBooks = new List<Book>(await bookRepository.Search(x => x.Title.Contains(filter) || x.Author.Contains(filter)));
            }
            else if (filterRadio == "title")
            {
                foundBooks = new List<Book>(await bookRepository.Search(x => x.Title.Contains(filter)));
            }
            else if (filterRadio == "author")
            {
                foundBooks = new List<Book>(await bookRepository.Search(x => x.Author.Contains(filter)));
            }
            else if (filterRadio == "genre")
            {
                foundBooks = new List<Book>(await this.genreRepository.GetBooksByGenreNameAsync(filter));
            }
            else if (filterRadio == "tag")
            {
                foundBooks = new List<Book>(await this.tagRepository.GetBooksByTagAsync(filter));
            }

            return foundBooks;
        }
    }
}
