using Azure.Storage.Blobs;
using WebApplication1.Dtos.Common;
using WebApplication1.Helpers;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementations;

public class AzureUploadService : IAzureUploadService
{
    private readonly IFileStreamingHelper _fileStreamingHelper;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureUploadService> _logger;

    public AzureUploadService(
        IFileStreamingHelper fileStreamingHelper,
        IConfiguration configuration,
        ILogger<AzureUploadService> logger)
    {
        _fileStreamingHelper = fileStreamingHelper;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ResultDTO<object>> UploadFileAsync(HttpRequest request)
    {
        var result = new ResultDTO<object>() { Success = true };
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "temp_uploads", Guid.NewGuid().ToString());
        var fullFilePathList = new List<string>();
        var uploadedBlobs = new List<string>();

        try
        {
            _logger.LogInformation("Starting Azure file upload process, temporary directory: {FolderPath}", folderPath);

            // 1. First save files to local temporary storage
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

            _logger.LogInformation("File temporary storage completed, total {FileCount} files", fullFilePathList.Count);

            // 2. Execute database operations (don't SaveChanges yet)
            // Example: Create file records but don't save yet
            // var fileRecords = await CreateFileRecordsAsync(formValueProvider, fullFilePathList);
            // await _dbContext.FileRecords.AddRangeAsync(fileRecords);

            // 3. Upload to Azure Blob Storage
            var containerClient = await GetBlobContainerClientAsync();
            await UploadLocalFileToBlobAsync(containerClient, fullFilePathList, uploadedBlobs);

            _logger.LogInformation("Azure Blob upload completed, total {BlobCount} files uploaded", uploadedBlobs.Count);

            // 4. If Azure upload successful, commit database changes
            // await _dbContext.SaveChangesAsync();

            // 5. Delete local temporary files
            DeleteLocalFiles(folderPath, fullFilePathList);

            result.Message = "File upload to Azure successful.";
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure file upload process failed");

            // Error handling: rollback all operations
            await RollbackOperationsAsync(folderPath, fullFilePathList, uploadedBlobs);

            result.Success = false;
            result.Message = $"Error uploading file to Azure: {ex.Message}";
            return result;
        }
    }

    public async Task<ResultDTO<object>> UploadLocalFilesAsync(List<string> localFilePaths)
    {
        var result = new ResultDTO<object>() { Success = true };
        var uploadedBlobs = new List<string>();

        try
        {
            _logger.LogInformation("Starting upload of {FileCount} local files to Azure", localFilePaths.Count);

            // Validate that all files exist
            foreach (var filePath in localFilePaths)
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Local file not found: {filePath}");
                }
            }

            // Upload to Azure Blob Storage
            var containerClient = await GetBlobContainerClientAsync();
            await UploadLocalFileToBlobAsync(containerClient, localFilePaths, uploadedBlobs);

            _logger.LogInformation("Azure Blob upload completed, total {BlobCount} files uploaded", uploadedBlobs.Count);

            result.Message = "Local files uploaded to Azure successful.";
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Local files upload to Azure failed");

            // Error handling: rollback Azure uploads
            if (uploadedBlobs.Any())
            {
                var containerClient = await GetBlobContainerClientAsync();
                await RollbackUploadsAsync(containerClient, uploadedBlobs);
            }

            result.Success = false;
            result.Message = $"Error uploading local files to Azure: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// Upload local files to Azure Blob Storage
    /// </summary>
    private async Task UploadLocalFileToBlobAsync(
        BlobContainerClient containerClient,
        List<string> fullFilePathList,
        List<string> uploadedBlobs)
    {
        foreach (var fullFilePath in fullFilePathList)
        {
            try
            {
                string fileName = Path.GetFileName(fullFilePath);
                string blobName = $"{Guid.NewGuid()}_{fileName}";
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                await using (var fileStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read))
                {
                    var response = await blobClient.UploadAsync(fileStream, overwrite: true);
                    uploadedBlobs.Add(blobName);
                    _logger.LogInformation("File {FileName} uploaded to Azure Blob: {BlobName}", fileName, blobName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file {FilePath} to Azure", fullFilePath);
                throw new Exception($"Failed to upload file {Path.GetFileName(fullFilePath)} to Azure: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Rollback operations: Delete uploaded Azure Blobs and local files
    /// </summary>
    private async Task RollbackOperationsAsync(
        string folderPath,
        List<string> fullFilePathList,
        List<string> uploadedBlobs)
    {
        _logger.LogWarning("Starting rollback operation, {BlobCount} blobs uploaded, {FileCount} local files",
            uploadedBlobs.Count, fullFilePathList.Count);

        try
        {
            // 1. Delete uploaded Azure Blobs
            if (uploadedBlobs.Any())
            {
                var containerClient = await GetBlobContainerClientAsync();
                await RollbackUploadsAsync(containerClient, uploadedBlobs);
            }

            // 2. Delete local temporary files
            DeleteLocalFiles(folderPath, fullFilePathList);

            // 3. Database rollback (no special handling needed since no SaveChanges)

            _logger.LogInformation("Rollback operation completed");
        }
        catch (Exception rollbackEx)
        {
            _logger.LogError(rollbackEx, "Rollback operation failed");
        }
    }

    /// <summary>
    /// Delete local files and directories
    /// </summary>
    private static void DeleteLocalFiles(string folderPath, List<string> fullFilePathList)
    {
        // Delete all files
        foreach (var filePath in fullFilePathList)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete local file {filePath}: {ex.Message}");
            }
        }

        // Delete directory if empty
        try
        {
            if (Directory.Exists(folderPath) && !Directory.EnumerateFileSystemEntries(folderPath).Any())
            {
                Directory.Delete(folderPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete directory {folderPath}: {ex.Message}");
        }
    }

    /// <summary>
    /// Get Azure Blob Container Client
    /// </summary>
    private async Task<BlobContainerClient> GetBlobContainerClientAsync()
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
            var containerName = _configuration["AzureBlobStorage:ContainerName"];

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Azure Blob Storage connection string not configured");
            if (string.IsNullOrEmpty(containerName))
                throw new InvalidOperationException("Azure Blob Storage container name not configured");

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure container exists
            await containerClient.CreateIfNotExistsAsync();
            return containerClient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Azure Blob Container Client");
            throw new Exception("Unable to connect to Azure Blob Storage: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Rollback Azure Blob uploads
    /// </summary>
    private static async Task RollbackUploadsAsync(BlobContainerClient containerClient, List<string> uploadedBlobs)
    {
        foreach (var blobName in uploadedBlobs)
        {
            try
            {
                BlobClient blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync();
                Console.WriteLine($"Rolled back Azure Blob: {blobName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to rollback Azure Blob {blobName}: {ex.Message}");
            }
        }
    }
}