using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia.Data
{
    public static class DatabaseService
    {
        public static List<Product> Products = new();
        public static List<Warehouse> Warehouses = new()
        {
            new Warehouse { Id = 301, Name = "Magazyn 301" },
            new Warehouse { Id = 302, Name = "Magazyn 302" },
            new Warehouse { Id = 303, Name = "Magazyn 303" }
        };

        public static List<Contractor> Contractors = new();
        public static List<Document> Documents = new();

        public static void AddProduct(Product product) => Products.Add(product);
        public static List<Product> GetProductsByWarehouse(int warehouseId) =>
            Products.Where(p => p.WarehouseId == warehouseId).ToList();

        public static void AddContractor(Contractor contractor) => Contractors.Add(contractor);
        public static void AddDocument(Document doc) => Documents.Add(doc);
    }
}
