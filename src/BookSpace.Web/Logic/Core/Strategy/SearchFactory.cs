using BookSpace.Models;
using BookSpace.Web.Logic.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookSpace.Web.Logic.Core.Strategy
{
    public class SearchFactory : ISearchFactory
    {
        private readonly ISearchStrategyFactory _searchStrategyFactory;

        public SearchFactory(ISearchStrategyFactory searchStrategyFactory)
        {
            _searchStrategyFactory = searchStrategyFactory;
        }

        public async Task<List<Book>> GetSearchedBooks(string filter, string radioFilter)
        {
            ISearchStrategy searchStrategy = _searchStrategyFactory.GetSearchStrategy(radioFilter);

            if (searchStrategy != null)
            {
                return await searchStrategy.GetSearchedBook(filter);
            }

            return null;
        }
    }
}
