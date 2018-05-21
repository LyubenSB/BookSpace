using BookSpace.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookSpace.Web.Logic.Interfaces
{
    public interface ISearchStrategy
    {
        Task<List<Book>> GetSearchedBook(string filter);
    }
}
