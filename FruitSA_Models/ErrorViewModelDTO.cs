namespace FruitSA_Assessment.Models
{
    public class ErrorViewModelDTO
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
