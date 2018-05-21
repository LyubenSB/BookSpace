using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookSpace.BlobStorage.Contracts;
using BookSpace.Data.Contracts;
using BookSpace.Factories;
using BookSpace.Factories.ResponseModels;
using BookSpace.Models;
using BookSpace.Services;
using BookSpace.Web.Areas.Admin.Models;
using BookSpace.Web.Areas.Admin.Models.ApplicationUserViewModels;
using BookSpace.Web.Models.BookViewModels;
using BookSpace.Web.Models.GenreViewModels;
using BookSpace.Web.Services.SmtpService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookSpace.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IRepository<ApplicationUser> userRepository;
        private readonly IRepository<Book> bookRepository;
        private readonly IRepository<Tag> tagRepository;
        private readonly IRepository<Genre> genreRepository;
        private readonly IMapper objectMapper;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly BookServices bookServices;
        private readonly IBlobStorageService blobStorageService;
        private readonly IFactory<Book, BookResponseModel> bookFactory;
        private readonly IFactory<Genre, GenreResponseModel> genreFactory;
        private readonly IFactory<Tag, TagResponseModel> tagFactory;
        const int recordsOnPageIndex = 30;

        public AdminController(IRepository<ApplicationUser> userRepository, 
                               IRepository<Book> bookRepository, 
                               IRepository<Tag> tagRepository,
                               IRepository<Genre> genreRepository,
                               IFactory<Book, BookResponseModel> bookFactory, 
                               IFactory<Genre, GenreResponseModel> genreFactory, 
                               IFactory<Tag, TagResponseModel> tagFactory,
                               IMapper objectMapper, UserManager<ApplicationUser> userManager, 
                               BookServices bookServices, 
                               IBlobStorageService blobStorageService)
        {
            this.userRepository = userRepository;
            this.bookRepository = bookRepository;
            this.tagRepository = tagRepository;
            this.genreRepository = genreRepository;
            this.objectMapper = objectMapper;
            this.userManager = userManager;
            this.bookServices = bookServices;
            this.blobStorageService = blobStorageService;
            this.bookFactory = bookFactory;
            this.genreFactory = genreFactory;
            this.tagFactory = tagFactory;
        }

        public IActionResult AllUsers()
        {
            var users = this.userRepository.GetAllAsync().Result.ToList();

            var userViewModels = this.objectMapper.Map<IEnumerable<ApplicationUser>, IEnumerable<ApplicationUserViewModel>>(users);

            return View(userViewModels);
        }

        public async Task<IActionResult> AllBooks(int page = 1)
        {
            var allBooksViewModel = new AllBooksAdminViewModel()
            {
                Books = await this.GetBooksPage(page),
                BooksCount = await this.bookRepository.GetCount()
            };

            return View(allBooksViewModel);
        }

        public async Task<IActionResult> BooksPage(int page)
        {
            var allBooksViewModel = new AllBooksAdminViewModel()
            {
                Books = await this.GetBooksPage(page),
                BooksCount = await this.bookRepository.GetCount()
            };

            return PartialView("_BooksPagePartial", allBooksViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(ApplicationUserViewModel userViewModel)
        {
            //TODO:Getting user by anything that can be changed is impossible!So I must use Id and therefore ID cannot be changed which is not good!
            var dbUser = await this.userManager.FindByIdAsync(userViewModel.Id);

            var user = this.objectMapper.Map(userViewModel, dbUser);

            if (userViewModel.isAdmin)
            {
                await this.userManager.AddToRoleAsync(user, "Admin");
            }
            else
            {
                await this.userManager.RemoveFromRoleAsync(user, "Admin");
            }

            await this.userManager.UpdateAsync(user);

            return this.RedirectToAction("AllUsers");
        }

        public async Task<IActionResult> EditUserAsync(string id)
        {
            var dbModel = await this.userRepository.GetByIdAsync(id);
            var userViewModel = objectMapper.Map<ApplicationUserViewModel>(dbModel);

            return View(userViewModel);
        }

        [HttpPost("/EditBook/{bookid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBook(DetailedBookViewModel bookViewModel)
        {
            var dbModel = await this.bookRepository.GetByIdAsync(bookViewModel.BookId);

            var book = this.objectMapper.Map(bookViewModel, dbModel);

            await this.bookRepository.UpdateAsync(book);

            return this.RedirectToAction("AllBooks");
        }

        [HttpGet("/EditBook/{bookid}")]
        public async Task<IActionResult> EditBookAsync(string bookId)
        {
            var dbModel = await this.bookRepository.GetByIdAsync(bookId);

            var bookViewModel = objectMapper.Map<DetailedBookViewModel>(dbModel);

            return View(bookViewModel);
        }

        public async Task<IActionResult> DeleteBook(ListBookViewModel bookViewModel)
        {
            var dbModel = await this.bookRepository.GetByIdAsync(bookViewModel.BookId);

            await this.bookRepository.DeleteAsync(dbModel);

            return this.RedirectToAction("AllBooks");
        }


        [HttpGet("/CreateBook")]
        public IActionResult CreateBook()
        {
            return View();
        }

        [HttpPost("/CreateBook")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBookAsync(CreateBookViewModel bookViewModel)
        {
            if (ModelState.IsValid)
            {

            var bookResponse = this.objectMapper.Map<BookResponseModel>(bookViewModel);
            var book = this.bookFactory.Create(bookResponse);

            await this.bookRepository.AddAsync(book);

            var genres = this.bookServices.FormatStringResponse(bookViewModel.Genres);
            var tags = this.bookServices.FormatStringResponse(bookViewModel.Tags);

            await this.bookServices.MatchGenresToBookAsync(genres, book.BookId);
            await this.bookServices.MatchTagToBookAsync(tags, book.BookId);
            }

            return RedirectToAction("CreateBook");

        }

        [HttpGet("/CreateTag")]
        public IActionResult CreateTag()
        {
            return View();
        }

        [HttpPost("/CreateTag")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTagAsync(TagViewModel tagViewModel)
        {
            var tagResponse = this.objectMapper.Map<TagResponseModel>(tagViewModel);

            var tag = this.tagFactory.Create(tagResponse);

            await this.tagRepository.AddAsync(tag);

            return RedirectToAction("CreateTag");
        }

        [HttpGet("/CreateGenre")]
        public IActionResult CreateGenre()
        {
            return View();
        }

        [HttpPost("/CreateGenre")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGenreAsync(GenreViewModel genreViewModel)
        {
            var genreResponse = this.objectMapper.Map<GenreResponseModel>(genreViewModel);

            var genre = this.genreFactory.Create(genreResponse);

            await this.genreRepository.AddAsync(genre);

            return RedirectToAction("CreateGenre");
        }

        private async Task<IEnumerable<ListBookViewModel>> GetBooksPage(int page)
        {
            var books = await this.bookRepository.GetPaged(page, recordsOnPageIndex);
            var booksViewModels = this.objectMapper.Map<IEnumerable<Book>, IEnumerable<ListBookViewModel>>(books.Results);
            return booksViewModels;
        }
    }
}