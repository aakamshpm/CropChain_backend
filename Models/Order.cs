namespace CropChainBackend.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderData { get; set; }
        public string Status { get; set; } // Pending, Completed, Cancelled
    }
}
