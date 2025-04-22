using WebApplication1.Dtos.Common;

namespace WebApplication1.Services.Interfaces;

public interface ILocalUploadService
{
    /// <summary>
    /// Stream upload files to local storage
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<ResultDTO<object>> UploadFileAsync(HttpRequest request);
}