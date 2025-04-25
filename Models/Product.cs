namespace WarehouseManagementUnia.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string WarehouseCode { get; set; }
    }
}