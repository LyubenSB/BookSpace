using BookSpace.Web.Logic.Interfaces;
using Neleus.DependencyInjection.Extensions;

namespace BookSpace.Web.Logic.Core.Strategy
{
    public class SearchStrategyFactory : ISearchStrategyFactory
    {
        private readonly IServiceByNameFactory<ISearchStrategy> _serviceByNameFactory;

        public SearchStrategyFactory(IServiceByNameFactory<ISearchStrategy> serviceByNameFactory)
        {
            _serviceByNameFactory = serviceByNameFactory;
        }

        public ISearchStrategy GetSearchStrategy(string radioFilter)
        {
            if (_serviceByNameFactory != null)
            {
                return _serviceByNameFactory.GetByName(radioFilter);
            }
            
            return null;
        }
    }
}
