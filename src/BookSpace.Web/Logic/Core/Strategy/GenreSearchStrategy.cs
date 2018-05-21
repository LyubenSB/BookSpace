using BookSpace.Data.Contracts;
using BookSpace.Models;
using BookSpace.Web.Logic.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookSpace.Web.Logic.Core.Strategy
{
    public class GenreSearchStrategy : ISearchStrategy
    {
        private readonly IRepository<Genre> _genreRepository;

        public GenreSearchStrategy(IRepository<Genre> genreRepository)
        {
            _genreRepository = genreRepository;
        }

        public async Task<List<Book>> GetSearchedBook(string filter)
        {
            var result = await _genreRepository.GetManyToManyAsync(g => g.Name.Contains(filter),
                                                 bu => bu.GenreBooks,
                                                 b => b.Book);
            return result.ToList();
        }
    }
}
