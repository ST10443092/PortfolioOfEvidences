using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CLDVP1.Interfaces
{
    public interface IBlobService
    {
        Task<string> UploadFileAsync(IFormFile file);
        Task DeleteFileAsync(string fileUrl);
    }
}
