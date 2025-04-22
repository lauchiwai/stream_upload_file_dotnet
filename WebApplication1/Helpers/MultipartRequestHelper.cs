using Microsoft.Net.Http.Headers;
namespace WebApplication1.Helpers;
public interface IMultipartRequestHelper
{
    /// <summary>
    /// Check if content type is multipart form data
    /// </summary>
    /// <param name="contentType"></param>
    /// <returns></returns>
    public bool IsMultipartContentType(string contentType);

    /// <summary>
    /// Get the boundary of multipart content
    /// </summary>
    /// <param name="contentType"></param>
    /// <param name="lengthLimit"></param>
    /// <returns></returns>
    public string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit);

    /// <summary>
    /// Determine if Content-Disposition is a file
    /// </summary>
    /// <param name="contentDisposition"></param>
    /// <returns></returns>
    public bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition);

    /// <summary>
    /// Determine if Content-Disposition is form data
    /// </summary>
    /// <param name="contentDisposition"></param>
    /// <returns></returns>
    public bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition);
}


public class MultipartRequestHelper : IMultipartRequestHelper
{
    public bool IsMultipartContentType(string contentType)
    {
        return !string.IsNullOrEmpty(contentType)
               && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
    {
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;
        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new InvalidDataException("Missing content-type boundary.");
        }

        if (boundary.Length > lengthLimit)
        {
            throw new InvalidDataException($"Multipart boundary length limit {lengthLimit} exceeded.");
        }

        return boundary;
    }

    public bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
    {
        // Return true if the content-disposition header indicates this is a file
        return contentDisposition != null
               && contentDisposition.DispositionType.Equals("form-data")
               && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                   || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
    }

    public bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
    {
        // Return true if the content-disposition header indicates this is form data
        return contentDisposition != null
               && contentDisposition.DispositionType.Equals("form-data")
               && string.IsNullOrEmpty(contentDisposition.FileName.Value)
               && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
    }
}