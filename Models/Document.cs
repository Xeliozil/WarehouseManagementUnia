using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagementUnia.Models
{
    public class Document
    {
        public int DocumentId { get; set; }
        public string DocumentType { get; set; }
        public string ProductName { get; set; }
        public string WarehouseCode { get; set; }
        public string ContractorName { get; set; }
        public int Quantity { get; set; }
        public DateTime DocumentDate { get; set; }
    }
}
