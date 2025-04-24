using System;

namespace WarehouseManagementUnia.Models
{
    public class Document
    {
        public int DocumentId { get; set; }
        public string DocumentTypeName { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string WarehouseCode { get; set; }
        public string ContractorName { get; set; }
        public string ContractorNIP { get; set; }
        public DateTime DocumentDate { get; set; }
    }
}