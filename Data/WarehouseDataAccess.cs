using System.Collections.Generic;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia.Data
{
    public class WarehouseDataAccess
    {
        public List<Warehouse> GetWarehouses() =>
            new() { new Warehouse { Id = 301, Name = "Magazyn 301" }, new Warehouse { Id = 302, Name = "Magazyn 302" } };

        public List<Product> GetProductsByWarehouse(int warehouseId) =>
            new() {
                new Product { Id = 1, Name = "Młotek", Category = "Narzędzia", Quantity = 50, WarehouseId = warehouseId },
                new Product { Id = 2, Name = "Śrubokręt", Category = "Narzędzia", Quantity = 30, WarehouseId = warehouseId }
            };

        public List<Contractor> GetContractors() =>
            new() { new Contractor { Id = 1, Name = "Firma ABC", Address = "ul. Długa 10" } };

        public void AddProduct(Product product) { /* Zapis do bazy danych */ }

        public void AddContractor(Contractor contractor) { /* Zapis do bazy danych */ }

        public void CreateDocument(string type, int warehouseId, int? contractorId) { /* logika + raport */ }
    }
}
