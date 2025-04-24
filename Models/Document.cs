using System.Collections.Generic;

namespace WarehouseManagementUnia.Data
{
    public enum DocumentType { Dostawa, MM, Wydanie }

    public class Document
    {
        public int Id { get; set; }
        public DocumentType Type { get; set; }
        public string ContractorName { get; set; }
        public int FromWarehouseId { get; set; }
        public int? ToWarehouseId { get; set; }
        public List<Product> Products { get; set; } = new();
    }
}
