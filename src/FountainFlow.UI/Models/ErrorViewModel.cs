namespace FountainFlow.UI.Models;

public class ErrorViewModel
{
    public string Message { get; set; } = "An error occurred.  Be grateful that the error has been handled gracefully.";
    public string RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
