namespace WarehouseManagementUnia.Models
{
    public class Delivery
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? ContractorId { get; set; }
        public string ContractorNIP { get; set; }
        public int Quantity { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string Description { get; set; }
    }
}