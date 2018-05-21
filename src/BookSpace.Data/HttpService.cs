using System.Net.Http;
using System.Threading.Tasks;
using BookSpace.BlobStorage;
using BookSpace.Data.Contracts;
using BookSpace.Data.DTO;

namespace BookSpace.Data
{
    public class HttpService : IHttpService
    {
        private readonly HttpClient httpClient;

        public HttpService()
        {
            this.httpClient = new HttpClient();
        }

        public async Task<HttpResponseBodyDto> GetAsync(BlobObjectInfo blobObjectInfo)
        {
            HttpResponseMessage responseMessage = await this.httpClient.GetAsync(blobObjectInfo.Url);

            return new HttpResponseBodyDto
            {
                Body = await responseMessage.Content.ReadAsStringAsync()
            };
        }
    }
}
