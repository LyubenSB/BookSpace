using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookSpace.Web.Logic.Interfaces
{
    public interface ISearchStrategyFactory
    {
        ISearchStrategy GetSearchStrategy(string radioFilter);
    }
}
