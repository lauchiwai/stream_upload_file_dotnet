using WebApplication1.Dtos.Common;

namespace WebApplication1.Services.Interfaces;

public interface IAzureUploadService
{
    /// <summary>
    /// Stream upload files to Azure Blob Storage
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<ResultDTO<object>> UploadFileAsync(HttpRequest request);

    /// <summary>
    /// Upload existing local files to Azure Blob Storage
    /// </summary>
    /// <param name="localFilePaths"></param>
    /// <returns></returns>
    Task<ResultDTO<object>> UploadLocalFilesAsync(List<string> localFilePaths);
}