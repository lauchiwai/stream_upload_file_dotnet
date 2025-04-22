using System.ComponentModel;

namespace WebApplication1.Enums;

public enum MessageEnum
{
    [Description("Failed")] Fail,
    [Description("Login failed")] LoginFail,
    [Description("Add failed")] AddFail,
    [Description("Edit failed")] EditFail,
    [Description("Delete failed")] DeleteFail,
    [Description("Review submission failed")] ReviewFail,
    [Description("Import failed")] ImportFail,
    [Description("Disabled status")] Disable,
    [Description("Retrieval failed")] GetFail,
    [Description("Data already exists")] DataExist,
    [Description("Approval failed")] AgreeFail,
    [Description("Return failed")] ReturnFail,
    [Description("Calculation failed")] CalculateFail,
    [Description("Conversion failed")] ConversionFailed,
    [Description("Upload failed")] UploadFail,
    [Description("Download failed")] DownloadFail,
    [Description("Not found")] NotFound,
    [Description("Azure upload failed")] AzureUploadFail,
    [Description("Required file or parameter not provided")] invalidInput,
}