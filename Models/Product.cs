namespace WarehouseManagementUnia.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public int? ContractorId { get; set; }
        public Contractor Contractor { get; set; }
    }
}