namespace WebApplication1.Data.Options;

public class FileUploadOptions
{
    /// <summary>
    /// Used to limit the maximum length of the boundary string in multipart requests
    /// </summary>
    public int MultipartBoundaryLengthLimit { get; set; }

    /// <summary>
    /// Maximum number of key-value pairs allowed in the request
    /// </summary>
    public int ValueCountLimit { get; set; }

    /// <summary>
    /// Maximum allowed length of multipart request body (file size limit)
    /// </summary>
    public long MultipartBodyLengthLimit { get; set; }

    /// <summary>
    /// Allowed file extensions for upload
    /// </summary>
    public string[] AllowedExtensions { get; set; } = [];
}