using System.Data.SqlClient;
using System.Collections.Generic;
using WarehouseManagementUnia.Models;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace WarehouseManagementUnia.Data
{
    public class WarehouseDataAccess
    {
        private readonly string _connectionString;

        public WarehouseDataAccess()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<Product> GetProducts()
        {
            var products = new List<Product>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Name, Quantity, Price FROM Products", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Quantity = reader.GetInt32(2),
                            Price = reader.GetDecimal(3)
                        });
                    }
                }
            }
            return products;
        }

        public void AddProduct(Product product)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "INSERT INTO Products (Name, Quantity, Price) VALUES (@Name, @Quantity, @Price); SELECT SCOPE_IDENTITY();",
                    connection);
                command.Parameters.AddWithValue("@Name", product.Name);
                command.Parameters.AddWithValue("@Quantity", product.Quantity);
                command.Parameters.AddWithValue("@Price", product.Price);
                product.Id = Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void DeleteProduct(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM Products WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }
        }
        public void AddDelivery(Delivery delivery)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "INSERT INTO Deliveries (ProductId, Quantity, DeliveryDate) VALUES (@ProductId, @Quantity, @DeliveryDate); SELECT SCOPE_IDENTITY();",
                    connection);
                command.Parameters.AddWithValue("@ProductId", delivery.ProductId);
                command.Parameters.AddWithValue("@Quantity", delivery.Quantity);
                command.Parameters.AddWithValue("@DeliveryDate", delivery.DeliveryDate);
                delivery.Id = Convert.ToInt32(command.ExecuteScalar());

                // Update product quantity
                var updateCommand = new SqlCommand(
                    "UPDATE Products SET Quantity = Quantity + @Quantity WHERE Id = @ProductId",
                    connection);
                updateCommand.Parameters.AddWithValue("@Quantity", delivery.Quantity);
                updateCommand.Parameters.AddWithValue("@ProductId", delivery.ProductId);
                updateCommand.ExecuteNonQuery();
            }
        }

        public List<Delivery> GetDeliveries()
        {
            var deliveries = new List<Delivery>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, ProductId, Quantity, DeliveryDate FROM Deliveries", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        deliveries.Add(new Delivery
                        {
                            Id = reader.GetInt32(0),
                            ProductId = reader.GetInt32(1),
                            Quantity = reader.GetInt32(2),
                            DeliveryDate = reader.GetDateTime(3)
                        });
                    }
                }
            }
            return deliveries;
        }
    }
}