namespace Parcial2_Ecommerce.Models.Reviews
{
    public class ReviewCreateRequest
    {
        public int ProductId { get; set; }
        public int Rating    { get; set; }  // 1..5
        public string? Comment { get; set; }
    }
}