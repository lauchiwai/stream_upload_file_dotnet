using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Globalization;
using System.Text;
using WebApplication1.Data.Options;

namespace WebApplication1.Helpers;

public interface IFileStreamingHelper
{
    /// <summary>
    /// Stream upload files
    /// </summary>
    /// <param name="request"></param>
    /// <param name="createStream"></param>
    /// <returns></returns>
    public Task<FormValueProvider> StreamFile(HttpRequest request, Func<FileMultipartSection, Stream> createStream);
}

public class FileStreamingHelper : IFileStreamingHelper
{
    // Inject required services and configuration
    private readonly IMultipartRequestHelper _multipartRequestHelper;
    private readonly FileUploadOptions _fileUploadOptions;

    public FileStreamingHelper(IMultipartRequestHelper multipartRequestHelper, IOptions<FileUploadOptions> fileUploadOptions)
    {
        _multipartRequestHelper = multipartRequestHelper;
        _fileUploadOptions = fileUploadOptions.Value;
    }

    public async Task<FormValueProvider> StreamFile(HttpRequest request, Func<FileMultipartSection, Stream> createStream)
    {
        if (!_multipartRequestHelper.IsMultipartContentType(request.ContentType))
        {
            throw new Exception($"Expected a multipart request, but got {request.ContentType}");
        }

        // Store Form data from request as Key and Value in this object
        var formAccumulator = new KeyValueAccumulator();

        var boundary = _multipartRequestHelper.GetBoundary(
            MediaTypeHeaderValue.Parse(request.ContentType),
            _fileUploadOptions.MultipartBoundaryLengthLimit);
        var reader = new MultipartReader(boundary, request.Body);

        var section = await reader.ReadNextSectionAsync();
        while (section != null)
        {
            // Extract each field content from the Form one by one
            ContentDispositionHeaderValue contentDisposition;
            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

            if (hasContentDispositionHeader)
            {
                // Process files
                if (_multipartRequestHelper.HasFileContentDisposition(contentDisposition))
                {
                    // File name and extension check
                    var fileName = Path.GetFileName(section.AsFileSection().FileName);
                    var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
                    var allowedExtensions = _fileUploadOptions.AllowedExtensions;

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        throw new InvalidDataException($"File extension not allowed: {fileExtension}.");
                    }

                    // Virus scanning can be added here

                    // If this part is a file, write it to the Stream
                    using (var targetStream = createStream(section.AsFileSection()))
                    {
                        await section.Body.CopyToAsync(targetStream);
                    }
                }
                // Process form data (non-file type data) rather than file upload itself
                else if (_multipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                {
                    // Get the field key name from Content-Disposition and remove quotes if they exist
                    var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;

                    // Get the corresponding encoding format based on the request part content
                    var encoding = GetEncoding(section);

                    // Use StreamReader to read the form field value character by character
                    using (var streamReader = new StreamReader(
                        section.Body,          // Form field stream
                        encoding,              // Encoding, usually UTF-8
                        detectEncodingFromByteOrderMarks: true, // Automatically detect Byte Order Mark (BOM)
                        bufferSize: 4096,      // Set buffer size for performance optimization
                        leaveOpen: true))      // Keep the stream open for subsequent reading
                    {
                        // Read the complete value of the form field
                        var value = await streamReader.ReadToEndAsync();

                        // If the field value is "undefined" string, convert it to empty string (might be a special marker from client upload)
                        if (String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                        {
                            value = String.Empty;
                        }

                        // Add the field key and value to the form accumulator for subsequent model binding
                        formAccumulator.Append(key, value);

                        // Check if the number of form fields exceeds the configured maximum limit
                        if (formAccumulator.ValueCount > _fileUploadOptions.ValueCountLimit)
                        {
                            // If there are too many form fields, throw an exception to prevent form field abuse
                            throw new InvalidDataException($"Form key count limit {_fileUploadOptions.ValueCountLimit} has been exceeded.");
                        }
                    }
                }
            }

            // Get the next field of the Form
            section = await reader.ReadNextSectionAsync();
        }

        // Virus scanning can be added here

        // Bind form data to model
        var formValueProvider = new FormValueProvider(
            BindingSource.Form,
            new FormCollection(formAccumulator.GetResults()),
            CultureInfo.CurrentCulture);

        return formValueProvider;
    }

    private static Encoding GetEncoding(MultipartSection section)
    {
        MediaTypeHeaderValue mediaType;
        var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
        // UTF-7 is insecure and should not be used. UTF-8 is used in most cases.
        if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
        {
            return Encoding.UTF8;
        }
        return mediaType.Encoding;
    }
}