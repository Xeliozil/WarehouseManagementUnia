using System;
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
                var command = new SqlCommand("SELECT Id, Name, Quantity, Price, IsActive FROM Products WHERE IsActive = 1", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Quantity = reader.GetInt32(2),
                            Price = reader.GetDecimal(3),
                            IsActive = reader.GetBoolean(4)
                        });
                    }
                }
            }
            return products;
        }

        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Name, Quantity, Price, IsActive FROM Products", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Quantity = reader.GetInt32(2),
                            Price = reader.GetDecimal(3),
                            IsActive = reader.GetBoolean(4)
                        });
                    }
                }
            }
            return products;
        }

        public List<Product> GetProductsForSelection()
        {
            var products = new List<Product>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Name FROM Products", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
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
                    "INSERT INTO Products (Name, Quantity, Price, IsActive) VALUES (@Name, @Quantity, @Price, @IsActive); SELECT SCOPE_IDENTITY();",
                    connection);
                command.Parameters.AddWithValue("@Name", product.Name);
                command.Parameters.AddWithValue("@Quantity", product.Quantity);
                command.Parameters.AddWithValue("@Price", product.Price);
                command.Parameters.AddWithValue("@IsActive", product.Quantity > 0);
                product.Id = Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void SoftDeleteProduct(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("UPDATE Products SET Quantity = 0, IsActive = 0 WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }
        }

        public bool HardDeleteProduct(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM Products WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
                return true;
            }
        }

        public void AddDelivery(Delivery delivery)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "INSERT INTO Deliveries (ProductId, Quantity, DeliveryDate, Description) VALUES (@ProductId, @Quantity, @DeliveryDate, @Description); SELECT SCOPE_IDENTITY();",
                    connection);
                command.Parameters.AddWithValue("@ProductId", delivery.ProductId);
                command.Parameters.AddWithValue("@Quantity", delivery.Quantity);
                command.Parameters.AddWithValue("@DeliveryDate", delivery.DeliveryDate);
                command.Parameters.AddWithValue("@Description", (object)delivery.Description ?? DBNull.Value);
                delivery.Id = Convert.ToInt32(command.ExecuteScalar());

                var updateCommand = new SqlCommand(
                    "UPDATE Products SET Quantity = Quantity + @Quantity, IsActive = 1 WHERE Id = @ProductId",
                    connection);
                updateCommand.Parameters.AddWithValue("@Quantity", delivery.Quantity);
                updateCommand.Parameters.AddWithValue("@ProductId", delivery.ProductId);
                updateCommand.ExecuteNonQuery();
            }
        }

        public void AddIssue(Issue issue)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var checkCommand = new SqlCommand("SELECT Quantity FROM Products WHERE Id = @ProductId", connection);
                checkCommand.Parameters.AddWithValue("@ProductId", issue.ProductId);
                int currentQuantity = (int)checkCommand.ExecuteScalar();

                if (currentQuantity < issue.Quantity)
                {
                    throw new InvalidOperationException("Nie można wydać więcej niż dostępna ilość.");
                }

                var command = new SqlCommand(
                    "INSERT INTO Issues (ProductId, Quantity, IssueDate, Description) VALUES (@ProductId, @Quantity, @IssueDate, @Description); SELECT SCOPE_IDENTITY();",
                    connection);
                command.Parameters.AddWithValue("@ProductId", issue.ProductId);
                command.Parameters.AddWithValue("@Quantity", issue.Quantity);
                command.Parameters.AddWithValue("@IssueDate", issue.IssueDate);
                command.Parameters.AddWithValue("@Description", (object)issue.Description ?? DBNull.Value);
                issue.Id = Convert.ToInt32(command.ExecuteScalar());

                var updateCommand = new SqlCommand(
                    "UPDATE Products SET Quantity = Quantity - @Quantity, IsActive = CASE WHEN Quantity - @Quantity > 0 THEN 1 ELSE 0 END WHERE Id = @ProductId",
                    connection);
                updateCommand.Parameters.AddWithValue("@Quantity", issue.Quantity);
                updateCommand.Parameters.AddWithValue("@ProductId", issue.ProductId);
                updateCommand.ExecuteNonQuery();
            }
        }

        public List<Delivery> GetDeliveries()
        {
            var deliveries = new List<Delivery>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, ProductId, Quantity, DeliveryDate, Description FROM Deliveries", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        deliveries.Add(new Delivery
                        {
                            Id = reader.GetInt32(0),
                            ProductId = reader.GetInt32(1),
                            Quantity = reader.GetInt32(2),
                            DeliveryDate = reader.GetDateTime(3),
                            Description = reader.IsDBNull(4) ? null : reader.GetString(4)
                        });
                    }
                }
            }
            return deliveries;
        }

        public List<Issue> GetIssues()
        {
            var issues = new List<Issue>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, ProductId, Quantity, IssueDate, Description FROM Issues", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        issues.Add(new Issue
                        {
                            Id = reader.GetInt32(0),
                            ProductId = reader.GetInt32(1),
                            Quantity = reader.GetInt32(2),
                            IssueDate = reader.GetDateTime(3),
                            Description = reader.IsDBNull(4) ? null : reader.GetString(4)
                        });
                    }
                }
            }
            return issues;
        }
    }
}