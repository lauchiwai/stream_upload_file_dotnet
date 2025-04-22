using WebApplication1.Dtos.Common;
using WebApplication1.Helpers;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementations;

public class LocalUploadService : ILocalUploadService
{
    private readonly IFileStreamingHelper _fileStreamingHelper;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LocalUploadService> _logger;
    private readonly IWebHostEnvironment _environment; 

    public LocalUploadService(
        IFileStreamingHelper fileStreamingHelper,
        IConfiguration configuration,
        ILogger<LocalUploadService> logger,
        IWebHostEnvironment environment) // Inject environment variable
    {
        _fileStreamingHelper = fileStreamingHelper;
        _configuration = configuration;
        _logger = logger;
        _environment = environment;
    }

    public async Task<ResultDTO<object>> UploadFileAsync(HttpRequest request)
    {
        var result = new ResultDTO<object>() { Success = true };

        // Set the UploadedFiles folder under the project root directory
        var projectRootPath = _environment.ContentRootPath;
        var uploadedFilesPath = Path.Combine(projectRootPath, "UploadedFiles");
        var folderPath = Path.Combine(uploadedFilesPath, Guid.NewGuid().ToString());

        var fullFilePathList = new List<string>();

        try
        {
            // Ensure the UploadedFiles directory exists
            if (!Directory.Exists(uploadedFilesPath))
            {
                Directory.CreateDirectory(uploadedFilesPath);
                _logger.LogInformation("Created UploadedFiles directory: {UploadedFilesPath}", uploadedFilesPath);
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var formValueProvider = await _fileStreamingHelper.StreamFile(request, section =>
            {
                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(section.FileName)}";
                var fullFilePath = Path.Combine(folderPath, uniqueFileName);
                fullFilePathList.Add(fullFilePath);
                return new FileStream(fullFilePath, FileMode.Create);
            });

            _logger.LogInformation("File upload successful, total {FileCount} files uploaded to {FolderPath}",
                fullFilePathList.Count, folderPath);

            // Return file path information to the client
            result.Message = "File upload successful.";
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File upload failed");
            result.Success = false;
            result.Message = $"Error uploading file: {ex.Message}";
            return result;
        }
        // Note: Removed deletion in finally block so files will be permanently saved
    }
}