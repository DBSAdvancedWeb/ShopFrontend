namespace ShopWeb.Models;

public class ErrorViewModel
{
    public sting? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
