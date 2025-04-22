using WebApplication1.Enums;

namespace WebApplication1.Dtos.Common;
public interface IResultDto
{
    bool Success { get; set; }

    string Message { get; set; }

    MessageEnum MessageNo { get; set; }

    string Description { get; set; }

    IEnumerable<string> Remark { get; set; }
}

public class ResultDTO<T> : IResultDto
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Message type/category
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// Message type number/code
    /// </summary>
    public MessageEnum MessageNo { get; set; }

    /// <summary>
    /// Detailed message
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Remarks/notes
    /// </summary>
    public IEnumerable<string> Remark { get; set; } = new List<string>();

    /// <summary>
    /// Universal field for any data
    /// </summary>
    public T? Object { get; set; }
}