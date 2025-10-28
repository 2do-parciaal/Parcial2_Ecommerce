namespace Parcial2_Ecommerce.Models.Orders
{
    public class CartItemRequest
    {
        public int UserId { get; set; }      // id del cliente
        public int ProductId { get; set; }   // id del producto
        public int Quantity { get; set; }    // cantidad a agregar
    }
}