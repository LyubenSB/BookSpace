using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookSpace.Data.Contracts;
using BookSpace.Models;
using BookSpace.Models.Enums;
using BookSpace.Web.Models.BookViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BookSpace.Web.Controllers
{
    public class UserBooksController : Controller
    {
        private readonly IRepository<ApplicationUser> applicationUserRepository;
        private readonly IRepository<BookUser> bookUserRepository;
        private readonly IRepository<Book> bookRepository;
        private readonly IMapper objectMapper;

        public UserBooksController(IRepository<ApplicationUser> applicationUserRepository,
                                    IRepository<BookUser> bookUserRepository,
                                    IRepository<Book> bookRepository,
                                    IMapper objectMapper)
        {
            this.applicationUserRepository = applicationUserRepository;
            this.bookUserRepository = bookUserRepository;

            this.bookRepository = bookRepository;

            this.objectMapper = objectMapper;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> _UserBooksPartialListAsync(string statusEnum)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            BookState parsedEnum = BookState.Read;
            if (!Enum.TryParse<BookState>(statusEnum, out parsedEnum))
            {
                throw new ArgumentException("Cannot parse enum");
            }

            var user = await this.applicationUserRepository.GetAsync(u => u.UserName == User.Identity.Name);

            var userReadBooks = await this.applicationUserRepository.GetManyToManyAsync(u => u.Id == user.Id,
                                                                    bu => bu.BookUsers.Where(s => s.State == parsedEnum),
                                                                     b => b.Book);

            var mappedBooksToViewModel = Mapper.Map<IEnumerable<Book>, IEnumerable<UserBookViewModel>>(userReadBooks);

            return PartialView("_AllUserBooksPartial", mappedBooksToViewModel);
        }

        public async Task<IActionResult> RemoveBook([FromRoute] string id)
        {
            var user = await this.applicationUserRepository.GetAsync(u => u.UserName == User.Identity.Name);
            var bookUser = await this.bookUserRepository.GetAsync(bu => bu.BookId == id && bu.UserId == user.Id);
            await this.bookUserRepository.DeleteAsync(bookUser);
            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddBook(string id, string collection)
        {
            var bookState = BookState.Default;
            Enum.TryParse<BookState>(collection, out bookState);
            var book = await this.bookRepository.GetByIdAsync(id);
            var user = await this.applicationUserRepository.GetAsync(u => u.UserName == User.Identity.Name);
            var bookUser = await this.bookUserRepository.GetAsync(bu => bu.BookId == id && bu.UserId == user.Id);

            if (bookUser == null)
            {
                var bookUserEntity = new BookUser
                {
                    BookId = book.BookId,
                    UserId = user.Id,
                    State = bookState
                };

                await this.bookUserRepository.AddAsync(bookUserEntity);
            }
            else
            {
                if(bookUser.State == bookState)
                {
                    throw new ArgumentException("The book is already present in this collection");
                }
                bookUser.State = bookState;
                await this.bookUserRepository.UpdateAsync(bookUser);
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RateBook(string id, string rate)
        {
            int userRate = int.Parse(rate);
            var book = await this.bookRepository.GetByIdAsync(id);
            var user = await this.applicationUserRepository.GetAsync(u => u.UserName == User.Identity.Name);
            var bookUser = await this.bookUserRepository.GetAsync(bu => bu.BookId == id && bu.UserId == user.Id);
            bool isNewUser = false;

            if (bookUser == null)
            {
                isNewUser = true;
                var bookUserEntity = new BookUser
                {
                    BookId = book.BookId,
                    UserId = user.Id,
                    Rate = userRate,
                    HasRatedBook = true,
                    State = BookState.Default
                };
                
                await this.bookUserRepository.AddAsync(bookUserEntity);
            }
            else
            {
                bookUser.Rate = userRate;

                bookUser.HasRatedBook = true;

                await this.bookUserRepository.UpdateAsync(bookUser);
            }
            return RedirectToAction("UpdateBookRating", "Book", new { id, rate, isNewUser });
        }

        public async Task<IActionResult> Leaderboard()
        {
            var allUsers =  this.applicationUserRepository.GetAllAsync().Result.ToList();

            var mappedUsers = new List<LeaderboardViewModel>();

            foreach (var user in allUsers)
            {
                var userReadBooks = await this.applicationUserRepository.GetManyToManyAsync(u => u.Id == user.Id,
                                                                    bu => bu.BookUsers.Where(s => s.State == BookState.Read),
                                                                    b => b.Book);
                var booksRed = userReadBooks.Count();

                var mappedUser = new LeaderboardViewModel
                {
                    Id = user?.Id,
                    Email = user?.Email,
                    ProfilePictureUrl = user?.ProfilePictureUrl,
                    Username = user?.UserName,
                   BooksRed = booksRed
                };
                mappedUsers.Add(mappedUser);
            }
            var leaderboardMappedUsers = mappedUsers.OrderByDescending(x => x.BooksRed).ToList();

            return View(leaderboardMappedUsers);
        }
    }
}