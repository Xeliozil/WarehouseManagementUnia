using System;

namespace WarehouseManagementUnia.Models
{
    public class Issue
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime IssueDate { get; set; }
        public string Description { get; set; }
    }
}