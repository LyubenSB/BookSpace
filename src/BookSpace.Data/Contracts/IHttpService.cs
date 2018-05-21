using System.Threading.Tasks;
using BookSpace.BlobStorage;
using BookSpace.Data.DTO;

namespace BookSpace.Data.Contracts
{
    public interface IHttpService
    {
        Task<HttpResponseBodyDto> GetAsync(BlobObjectInfo blobObjectInfo);
    }
}
