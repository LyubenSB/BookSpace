using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BookSpace.Models;
using Microsoft.AspNetCore.Mvc;
using BookSpace.Web.Models;
using BookSpace.Web.Models.BookViewModels;
using AutoMapper;
using BookSpace.Web.Models.GenreViewModels;
using BookSpace.Data.Contracts;

namespace BookSpace.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<Book> bookRepository;
        private readonly IRepository<Genre> genreRepository;
        private readonly IRepository<Tag> tagRepository;
        private readonly IMapper objectMapper;
        
        public HomeController(IRepository<Book> bookRepository,
                              IRepository<Genre> genreRepository,
                              IRepository<Tag> tagRepository,
                              IMapper mapper)
        {
            this.bookRepository = bookRepository;
            this.genreRepository = genreRepository;
            this.tagRepository = tagRepository;
            this.objectMapper = mapper;
        }
        public async Task<IActionResult> Index()
        {
            var popularBooks = await this.bookRepository.FindByExpressionOrdered(book => book.Rating, 12);
            var popularBooksViewModels = this.objectMapper.Map<IEnumerable<Book>, IEnumerable<PopularBookViewModel>>(popularBooks);

            var newBooks = await this.bookRepository.FindByExpressionOrdered(book => book.PublicationYear, 6);
            var newBooksViewModels = this.objectMapper.Map<IEnumerable<Book>, IEnumerable<NewBookViewModel>>(newBooks);

            var bookOfTheDay = await this.bookRepository.FindByExpressionOrdered(x => new Guid(), 1);
            var bookOfTheDayViewModel = this.objectMapper.Map<Book, BookOfTheDayViewModel>(bookOfTheDay.FirstOrDefault());

            var tags = await this.tagRepository.GetAllAsync();
            var tagsViewModel = this.objectMapper.Map<IEnumerable<Tag>, IEnumerable<TagViewModel>>(tags);

            var homePageViewModel = new HomePageViewModel()
            {
                BookOfTheDay = bookOfTheDayViewModel,
                PopularBooks = popularBooksViewModels,
                NewBooks = newBooksViewModels,
                Tags = tagsViewModel
            };

            return View(homePageViewModel);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Book()
        {
          
            return View();
        }

        public IActionResult Category()
        {

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Helpers
        public async Task<IActionResult> GenresNav()
        {
            var genres = await this.genreRepository.GetAllAsync();
            var genresViewModels = this.objectMapper.Map<IEnumerable<Genre>, IEnumerable<GenreViewModel>>(genres);

            return PartialView("_BookGenresNavPartial", genresViewModels);
        }

        public async Task<IActionResult> TagsNav()
        {
            var tags = await this.tagRepository.GetAllAsync();
            var tagsViewModels = this.objectMapper.Map<IEnumerable<Tag>, IEnumerable<TagViewModel>>(tags);

            return PartialView("_BookGenresNavPartial", tagsViewModels);
        }

        #endregion
    }
}
