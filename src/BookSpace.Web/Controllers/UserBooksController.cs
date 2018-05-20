using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookSpace.Models;
using BookSpace.Models.Enums;
using BookSpace.Repositories.Contracts;
using BookSpace.Web.Models.BookViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BookSpace.Web.Controllers
{
    public class UserBooksController : Controller
    {
        private readonly IApplicationUserRepository applicationUserRepository;
        private readonly IBookUserRepository bookUserRepository;
        private readonly IBookRepository bookRepository;
        private readonly IMapper objectMapper;

        public UserBooksController(IApplicationUserRepository applicationUserRepository,
                                    IBookUserRepository bookUserRepository,
                                    IBookRepository bookRepository,
                                    IMapper objectMapper)
        {
            this.applicationUserRepository = applicationUserRepository;
            this.bookUserRepository = bookUserRepository;

            this.bookRepository = bookRepository;

            this.objectMapper = objectMapper;
        }

        public IApplicationUserRepository ApplicationUserRepository { get; }

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

            var user = await this.applicationUserRepository.GetUserByUsernameAsync(User.Identity.Name);
            var userReadBooks = await this.applicationUserRepository.GetUserBooksAsync(user.Id, parsedEnum);
            var mappedBooksToViewModel = Mapper.Map<IEnumerable<Book>, IEnumerable<UserBookViewModel>>(userReadBooks);

            return PartialView("_AllUserBooksPartial", mappedBooksToViewModel);
        }

        public async Task<IActionResult> RemoveBook([FromRoute] string id)
        {
            var user = await this.applicationUserRepository.GetUserByUsernameAsync(User.Identity.Name);
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
            var user = await this.applicationUserRepository.GetUserByUsernameAsync(User.Identity.Name);
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
            var user = await this.applicationUserRepository.GetUserByUsernameAsync(User.Identity.Name);
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
    }
}