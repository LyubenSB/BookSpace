using BookSpace.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookSpace.Web.Logic.Interfaces
{
    public interface ISearchFactory
    {
        Task<List<Book>> GetSearchedBooks(string filter, string radioFilter);
    }
}
