using AutoMapper;
using BookSpace.Data.Contracts;
using BookSpace.Factories;
using BookSpace.Factories.ResponseModels;
using BookSpace.Models;
using BookSpace.Services;
using BookSpace.Web.Logic.Interfaces;
using BookSpace.Web.Models.BookViewModels;
using BookSpace.Web.Models.CommentViewModels;
using BookSpace.Web.Models.GenreViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookSpace.Web.Controllers
{
    public class BookController : Controller
    {
        //Repositories 
        private readonly IRepository<ApplicationUser> applicationUserRepository;
        private readonly IRepository<Book> bookRepository;
        private readonly IRepository<Genre> genreRepository;
        private readonly IRepository<Tag> tagRepository;
        private readonly IRepository<Comment> commentRepository;
        private readonly IRepository<BookUser> bookUserRepository;

        private readonly IUpdateService<Book> bookUpdateService;
        private readonly IUpdateService<Comment> commentUpdateService;

        private readonly IMapper objectMapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFactory<Comment, CommentResponseModel> commentFactory;
        private readonly BookDataServices dataService;
        private readonly ISearchFactory searchFactory;
        private const int recordsOnPageIndex = 30;
        private const int recordsOnPageCategory = 10;

        public BookController(IRepository<ApplicationUser> applicationUserRepository,
                              IRepository<Book> bookRepository,
                              IRepository<Genre> genreRepository,
                              IRepository<Tag> tagRepository,
                              IRepository<Comment> commentRepository,
                              IRepository<BookUser> bookUserRepository,
                              IUpdateService<Book> bookUpdateService,
                              IUpdateService<Comment> commentUpdateService,
                              UserManager<ApplicationUser> userManager,
                              IFactory<Comment, CommentResponseModel> commentFactory,
                              BookDataServices dataService,
                              IMapper objectMapper,              
                              ISearchFactory searchFactory)
        {
            this.applicationUserRepository = applicationUserRepository;
            this.bookRepository = bookRepository;
            this.genreRepository = genreRepository;
            this.tagRepository = tagRepository;
            this.bookUserRepository = bookUserRepository;
            this.commentRepository = commentRepository;
            this.bookUpdateService = bookUpdateService;
            this.commentUpdateService = commentUpdateService;
            this._userManager = userManager;
            this.commentFactory = commentFactory;
            this.dataService = dataService;
            this.objectMapper = objectMapper;
            this.searchFactory = searchFactory;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var indexViewModel = new AllBooksViewModel()
            {
                Books = await this.GetBooksPage(page),
                BooksCount = await this.bookRepository.GetCount()
            };

            return View(indexViewModel);
        }

        public async Task<IActionResult> Category([FromRoute] string id, int page = 1)
        {
            var genre = await this.genreRepository.GetByIdAsync(id);
            var genreViewModel = this.objectMapper.Map<Genre, GenreViewModel>(genre);
            var books = await this.GetBooksByCategoryPage(id, page);
            var categoryViewModel = new CategoryPageViewModel { Genre = genreViewModel, Books = books };

            return View(categoryViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> BooksList([FromQuery] int page)
        {
            return PartialView("Book/_BooksPagePartial", await this.GetBooksPage(page));
        }

        [HttpGet]
        public async Task<IActionResult> BooksByCategoryList([FromRoute] string id, [FromQuery] int page)
        {
            var genre = await this.genreRepository.GetByIdAsync(id);
            var genreViewModel = this.objectMapper.Map<Genre, GenreViewModel>(genre);
            var books = await this.GetBooksByCategoryPage(id, page);
            var categoryViewModel = new CategoryPageViewModel { Genre = genreViewModel, Books = books };

            return PartialView("Book/_BookByCategoryPagePartial", categoryViewModel);
        }

        [ResponseCache(NoStore = true, Duration = 0)]
        public async Task<IActionResult> BookDetails([FromRoute] string id)
        {
            var book = await this.bookRepository.GetByIdAsync(id);
            var comments = await this.bookRepository.GetOneToManyAsync(b => b.BookId == id,
                                                       bg => bg.Comments);

            foreach (var comment in comments)
            {
                var user = await this._userManager.FindByIdAsync(comment.UserId);

                comment.User = user;
            }

            var genres = await this.bookRepository.GetManyToManyAsync(b => b.BookId == id,
                                                       bg => bg.BookGenres,
                                                       g => g.Genre);

            var tags = await this.bookRepository.GetManyToManyAsync(b => b.BookId == id,
                                                       bg => bg.BookTags,
                                                       g => g.Tag);

            var bookUser = await this.bookUserRepository.GetByExpressionAsync(bu => bu.BookId == id);

            var bookViewModel = this.objectMapper.Map<Book, BookViewModel>(book);
            var commentsViewModel = this.objectMapper.Map<IEnumerable<Comment>, IEnumerable<CommentViewModel>>(comments);

            foreach (var comment in commentsViewModel)
            {
                var user = await this._userManager.FindByNameAsync(comment.Author);

                comment.AuthorPicUrl = user.ProfilePictureUrl;
            }
            

            if (this.User.Identity.IsAuthenticated)
            {
                foreach (var comment in commentsViewModel)
                {
                    var isAdmin = this.User.IsInRole("Admin");

                    var commentCreatorId = comment.UserId;
                    var currentUser = await this.applicationUserRepository.GetByExpressionAsync(u => u.UserName == this.User.Identity.Name);
                    var isCreator = commentCreatorId == currentUser.Id;

                    comment.CanEdit = isAdmin || isCreator;
                }
            }

            var propertiesViewModel = new BookPropertiesViewModel
            {
                Comments = commentsViewModel,
                Tags = tags.ToList().Select(t => t.Value),
                Genres = genres.ToList().Select(g => g.Name)
            };

            bool isRated = bookUser == null || bookUser.HasRatedBook == false ? false : true;
            int userRating = bookUser == null || bookUser.HasRatedBook == false ? 0 : bookUser.Rate;

            var singleBookViewModel = new SingleBookViewModel
            {
                Book = bookViewModel,
                Properties = propertiesViewModel,
                IsRated = isRated,
                UserRating = userRating
            };
            return View(singleBookViewModel);
        }

        public async Task<IActionResult> UpdateBookRating(string id, string rate, bool isNewUser)
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
            return RedirectToAction("BookDetails", "Book", new { id });
        }

        public IActionResult GetBookGenres(string bookId)
        {
            //TODO:Not finished
            var dbModel = this.bookRepository.GetManyToManyAsync(b => b.BookId == bookId,
                                                       bg => bg.BookGenres,
                                                       g => g.Genre);

            var mappedGenreViewModel = this.objectMapper.Map<GenreViewModel>(dbModel);

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(string id, string comment, string userId)
        {
            //creating response object from input
            var commentResponse = this.commentFactory.Create(new CommentResponseModel()
            {
                //TODO: NOT WORKING WITH USERID
                UserId = userId,
                BookId = id,
                Content = comment,
                Date = DateTime.Now
            });

            await this.commentUpdateService.AddAsync(commentResponse);

            return Ok();
        }

        public async Task<IActionResult> Search(string filter, string filterRadio = "Default")
        {
            var foundBooks = await this.searchFactory.GetSearchedBooks(filter, filterRadio);

            var foundBooksViewModel = this.objectMapper.Map<IEnumerable<Book>, IEnumerable<SearchedBookViewModel>>(foundBooks);

            return View("Search", foundBooksViewModel);
        }

        public async Task<IEnumerable<Book>> SerachByGenre(string filter)
        {

            var foundBooks = await bookRepository.SearchByNavigationProperty
                                    ("BookGenres", "Genre", b => CheckBookGenres(b, filter));
            return foundBooks;
        }

        [HttpGet]
        public async Task<IEnumerable<Book>> SerachByTag(string filter)
        {

            var foundBooks = await bookRepository.SearchByNavigationProperty
                                   ("BookTags", "Tag", b => CheckBookTags(b, filter));

            return foundBooks;
        }

        #region Helpers

        private async Task<IEnumerable<BooksIndexViewModel>> GetBooksPage(int page)
        {
            var books = await this.bookRepository.GetPaged(page, recordsOnPageIndex);
            var booksViewModels = this.objectMapper.Map<IEnumerable<Book>, IEnumerable<BooksIndexViewModel>>(books.Results);
            return booksViewModels;
        }

        private async Task<IEnumerable<BookByCategoryViewModel>> GetBooksByCategoryPage(string genreId, int page)
        {
            var books = await this.genreRepository.GetPagedManyToMany(g => g.GenreId == genreId,
                                                      b => b.GenreBooks,
                                                      x => x.Book,
                                                      page, recordsOnPageCategory);

            var booksViewModel = this.objectMapper.Map<IEnumerable<Book>, IEnumerable<BookByCategoryViewModel>>(books.Results);
            return booksViewModel;
        }

        private bool CheckBookGenres(Book book, string filter)
        {
            var enumerator = book.BookGenres.GetEnumerator();
            while (enumerator.Current != null)
            {
                if (enumerator.Current.Genre.Name.Contains(filter))
                    return true;
            };

            return false;
        }

        private bool CheckBookTags(Book book, string filter)
        {
            var enumerator = book.BookGenres.GetEnumerator();
            while (enumerator.Current != null)
            {
                if (enumerator.Current.Genre.Name.Contains(filter))
                    return true;
            };

            return false;
        }
        #endregion
    }
}