using System;

namespace WarehouseManagementUnia.Models
{
    public class Transaction
    {
        public string Type { get; set; }
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }
}