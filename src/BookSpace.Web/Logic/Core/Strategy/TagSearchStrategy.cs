using BookSpace.Data.Contracts;
using BookSpace.Models;
using BookSpace.Web.Logic.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookSpace.Web.Logic.Core.Strategy
{
    public class TagSearchStrategy : ISearchStrategy
    {
        private readonly IRepository<Tag> _tagRepository;

        public TagSearchStrategy(IRepository<Tag> tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<List<Book>> GetSearchedBook(string filter)
        {
            var result = await _tagRepository.GetManyToManyAsync(g => g.Value.Contains(filter),
                                                      b => b.TagBooks,
                                                      x => x.Book); 
            return result.ToList();
        }
    }
}
