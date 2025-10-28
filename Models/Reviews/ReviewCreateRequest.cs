namespace Parcial2_Ecommerce.Models.Reviews
{
    public class ReviewCreateRequest
    {
        public int UserId { get; set; }     // <- necesario (lo usa el controller)
        public int ProductId { get; set; }  // <- necesario
        public int Rating { get; set; }     // 1..5
        public string? Comment { get; set; }
    }
}