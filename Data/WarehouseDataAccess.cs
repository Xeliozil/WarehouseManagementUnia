using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using WarehouseManagementUnia.Models;
using Microsoft.Extensions.Configuration;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Linq;
using System.Text;
using PdfSharp.Drawing.Layout;

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
                var checkProduct = new SqlCommand("SELECT COUNT(*) FROM Products WHERE Id = @ProductId", connection);
                checkProduct.Parameters.AddWithValue("@ProductId", delivery.ProductId);
                if ((int)checkProduct.ExecuteScalar() == 0)
                {
                    throw new InvalidOperationException("Wybrany produkt nie istnieje.");
                }

                var command = new SqlCommand(
                    "INSERT INTO Deliveries (ProductId, ContractorId, Quantity, DeliveryDate, Description) VALUES (@ProductId, @ContractorId, @Quantity, @DeliveryDate, @Description); SELECT SCOPE_IDENTITY();",
                    connection);
                command.Parameters.AddWithValue("@ProductId", delivery.ProductId);
                command.Parameters.AddWithValue("@ContractorId", (object)delivery.ContractorId ?? DBNull.Value);
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
                var checkProduct = new SqlCommand("SELECT COUNT(*) FROM Products WHERE Id = @ProductId", connection);
                checkProduct.Parameters.AddWithValue("@ProductId", issue.ProductId);
                if ((int)checkProduct.ExecuteScalar() == 0)
                {
                    throw new InvalidOperationException("Wybrany produkt nie istnieje.");
                }

                var checkCommand = new SqlCommand("SELECT Quantity FROM Products WHERE Id = @ProductId", connection);
                checkCommand.Parameters.AddWithValue("@ProductId", issue.ProductId);
                int currentQuantity = (int)checkCommand.ExecuteScalar();

                if (currentQuantity < issue.Quantity)
                {
                    throw new InvalidOperationException("Nie można wydać więcej niż dostępna ilość.");
                }

                var command = new SqlCommand(
                    "INSERT INTO Issues (ProductId, ContractorId, Quantity, IssueDate, Description) VALUES (@ProductId, @ContractorId, @Quantity, @IssueDate, @Description); SELECT SCOPE_IDENTITY();",
                    connection);
                command.Parameters.AddWithValue("@ProductId", issue.ProductId);
                command.Parameters.AddWithValue("@ContractorId", (object)issue.ContractorId ?? DBNull.Value);
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

        public List<Contractor> GetContractors()
        {
            var contractors = new List<Contractor>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Name, Address, NIP FROM Contractors", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        contractors.Add(new Contractor
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Address = reader.IsDBNull(2) ? null : reader.GetString(2),
                            NIP = reader.IsDBNull(3) ? null : reader.GetString(3)
                        });
                    }
                }
            }
            return contractors;
        }

        public void AddContractor(Contractor contractor)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "INSERT INTO Contractors (Name, Address, NIP) VALUES (@Name, @Address, @NIP); SELECT SCOPE_IDENTITY();",
                    connection);
                command.Parameters.AddWithValue("@Name", contractor.Name);
                command.Parameters.AddWithValue("@Address", (object)contractor.Address ?? DBNull.Value);
                command.Parameters.AddWithValue("@NIP", (object)contractor.NIP ?? DBNull.Value);
                contractor.Id = Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void DeleteContractor(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var checkCommand = new SqlCommand(
                    "SELECT COUNT(*) FROM Deliveries WHERE ContractorId = @Id UNION ALL SELECT COUNT(*) FROM Issues WHERE ContractorId = @Id",
                    connection);
                checkCommand.Parameters.AddWithValue("@Id", id);
                using (var reader = checkCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.GetInt32(0) > 0)
                            throw new InvalidOperationException("Nie można usunąć kontrahenta, który jest powiązany z dostawami lub wydaniami.");
                    }
                }

                var command = new SqlCommand("DELETE FROM Contractors WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }
        }

        public List<Delivery> GetDeliveries()
        {
            var deliveries = new List<Delivery>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    @"SELECT d.Id, d.ProductId, p.Name, d.ContractorId, c.NIP, d.Quantity, d.DeliveryDate, d.Description
                      FROM Deliveries d
                      INNER JOIN Products p ON d.ProductId = p.Id
                      LEFT JOIN Contractors c ON d.ContractorId = c.Id", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        deliveries.Add(new Delivery
                        {
                            Id = reader.GetInt32(0),
                            ProductId = reader.GetInt32(1),
                            ProductName = reader.GetString(2),
                            ContractorId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            ContractorNIP = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Quantity = reader.GetInt32(5),
                            DeliveryDate = reader.GetDateTime(6),
                            Description = reader.IsDBNull(7) ? null : reader.GetString(7)
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
                var command = new SqlCommand(
                    @"SELECT i.Id, i.ProductId, p.Name, i.ContractorId, c.NIP, i.Quantity, i.IssueDate, i.Description
                      FROM Issues i
                      INNER JOIN Products p ON i.ProductId = p.Id
                      LEFT JOIN Contractors c ON i.ContractorId = c.Id", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        issues.Add(new Issue
                        {
                            Id = reader.GetInt32(0),
                            ProductId = reader.GetInt32(1),
                            ProductName = reader.GetString(2),
                            ContractorId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            ContractorNIP = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Quantity = reader.GetInt32(5),
                            IssueDate = reader.GetDateTime(6),
                            Description = reader.IsDBNull(7) ? null : reader.GetString(7)
                        });
                    }
                }
            }
            return issues;
        }

        public int GetReportCount(int year)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT ReportCount FROM Reports WHERE Year = @Year", connection);
                command.Parameters.AddWithValue("@Year", year);
                var result = command.ExecuteScalar();
                return result == null ? 0 : (int)result;
            }
        }

        public void IncrementReportCount(int year)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "IF EXISTS (SELECT 1 FROM Reports WHERE Year = @Year) " +
                    "UPDATE Reports SET ReportCount = ReportCount + 1 WHERE Year = @Year " +
                    "ELSE INSERT INTO Reports (Year, ReportCount) VALUES (@Year, 1)",
                    connection);
                command.Parameters.AddWithValue("@Year", year);
                command.ExecuteNonQuery();
            }
        }

        private double MeasureWrappedTextHeight(XGraphics gfx, string text, XFont font, double width)
        {
            if (string.IsNullOrEmpty(text))
                return font.Height;

            var words = text.Split(' ');
            double currentWidth = 0;
            int lineCount = 1;
            foreach (var word in words)
            {
                var wordSize = gfx.MeasureString(word + " ", font);
                if (currentWidth + wordSize.Width > width)
                {
                    lineCount++;
                    currentWidth = wordSize.Width;
                }
                else
                {
                    currentWidth += wordSize.Width;
                }
            }
            return lineCount * font.Height;
        }
        public int GetNextProductId()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT ISNULL(MAX(Id), 0) + 1 FROM Products", connection);
                return (int)command.ExecuteScalar();
            }
        }


        public void DeleteProductWithHistory(int productId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Usuń dostawy
                        var deleteDeliveries = new SqlCommand("DELETE FROM Deliveries WHERE ProductId = @ProductId", connection, transaction);
                        deleteDeliveries.Parameters.AddWithValue("@ProductId", productId);
                        deleteDeliveries.ExecuteNonQuery();

                        // Usuń wydania
                        var deleteIssues = new SqlCommand("DELETE FROM Issues WHERE ProductId = @ProductId", connection, transaction);
                        deleteIssues.Parameters.AddWithValue("@ProductId", productId);
                        deleteIssues.ExecuteNonQuery();

                        // Usuń produkt
                        var deleteProduct = new SqlCommand("DELETE FROM Products WHERE Id = @Id", connection, transaction);
                        deleteProduct.Parameters.AddWithValue("@Id", productId);
                        deleteProduct.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public void GenerateReport(string title, DateTime startDate, DateTime endDate, string filePath, string format, int? contractorId = null, int? productId = null, string transactionType = null)
        {
            var transactions = new List<Transaction>();
            var deliveries = GetDeliveries().Where(d => d.DeliveryDate >= startDate && d.DeliveryDate <= endDate);
            var issues = GetIssues().Where(i => i.IssueDate >= startDate && i.IssueDate <= endDate);

            if (contractorId.HasValue)
            {
                deliveries = deliveries.Where(d => d.ContractorId == contractorId);
                issues = issues.Where(i => i.ContractorId == contractorId);
            }

            if (productId.HasValue)
            {
                deliveries = deliveries.Where(d => d.ProductId == productId);
                issues = issues.Where(i => i.ProductId == productId);
            }

            if (!string.IsNullOrEmpty(transactionType))
            {
                if (transactionType == "Dostawa")
                    issues = issues.Where(i => false);
                else if (transactionType == "Wydanie")
                    deliveries = deliveries.Where(d => false);
            }

            foreach (var delivery in deliveries)
            {
                transactions.Add(new Transaction
                {
                    Type = "Dostawa",
                    Id = delivery.Id,
                    ProductId = delivery.ProductId,
                    ProductName = delivery.ProductName,
                    ContractorNIP = delivery.ContractorNIP ?? "",
                    Quantity = delivery.Quantity,
                    Date = delivery.DeliveryDate,
                    Description = delivery.Description ?? ""
                });
            }

            foreach (var issue in issues)
            {
                transactions.Add(new Transaction
                {
                    Type = "Wydanie",
                    Id = issue.Id,
                    ProductId = issue.ProductId,
                    ProductName = issue.ProductName,
                    ContractorNIP = issue.ContractorNIP ?? "",
                    Quantity = issue.Quantity,
                    Date = issue.IssueDate,
                    Description = issue.Description ?? ""
                });
            }

            if (format == "PDF")
            {
                using (var document = new PdfDocument())
                {
                    var page = document.AddPage();
                    var gfx = XGraphics.FromPdfPage(page);
                    var fontTitle = new XFont("Arial", 16, XFontStyleEx.Bold);
                    var fontNormal = new XFont("Arial", 12, XFontStyleEx.Regular);
                    var fontFooter = new XFont("Arial", 10, XFontStyleEx.Regular);
                    var textFormatter = new XTextFormatter(gfx) { Alignment = XParagraphAlignment.Left };

                    // Header
                    var titleText = $"Raport {title}";
                    var titleSize = gfx.MeasureString(titleText, fontTitle);
                    gfx.DrawString(titleText, fontTitle, XBrushes.Black, (page.Width - titleSize.Width) / 2, 20);

                    var dateRangeText = $"Zakres dat: {startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd}";
                    var dateRangeSize = gfx.MeasureString(dateRangeText, fontNormal);
                    gfx.DrawString(dateRangeText, fontNormal, XBrushes.Black, (page.Width - dateRangeSize.Width) / 2, 40);

                    // Filters
                    var filterText = new List<string>();
                    if (contractorId.HasValue)
                        filterText.Add($"Kontrahent: {GetContractors().FirstOrDefault(c => c.Id == contractorId)?.NIP}");
                    if (productId.HasValue)
                        filterText.Add($"Produkt: {GetProducts().FirstOrDefault(p => p.Id == productId)?.Name}");
                    if (!string.IsNullOrEmpty(transactionType))
                        filterText.Add($"Typ: {transactionType}");
                    var filterString = string.Join(", ", filterText);
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        var filterSize = gfx.MeasureString(filterString, fontNormal);
                        gfx.DrawString(filterString, fontNormal, XBrushes.Black, (page.Width - filterSize.Width) / 2, 60);
                    }

                    // Column Widths
                    var columnWidths = new Dictionary<string, double>
                    {
                        { "Typ", 60 },
                        { "ID", 40 },
                        { "ID produktu", 50 },
                        { "Nazwa produktu", 100 },
                        { "NIP kontrahenta", 100 },
                        { "Ilość", 60 },
                        { "Data", 80 },
                        { "Opis", 100 }
                    };

                    // Table Header
                    double y = 90;
                    textFormatter.DrawString("Typ", fontNormal, XBrushes.Black, new XRect(40, y, columnWidths["Typ"], 40), XStringFormats.TopLeft);
                    textFormatter.DrawString("ID", fontNormal, XBrushes.Black, new XRect(100, y, columnWidths["ID"], 40), XStringFormats.TopLeft);
                    textFormatter.DrawString("ID produktu", fontNormal, XBrushes.Black, new XRect(140, y, columnWidths["ID produktu"], 40), XStringFormats.TopLeft);
                    textFormatter.DrawString("Nazwa produktu", fontNormal, XBrushes.Black, new XRect(190, y, columnWidths["Nazwa produktu"], 40), XStringFormats.TopLeft);
                    textFormatter.DrawString("NIP kontrahenta", fontNormal, XBrushes.Black, new XRect(290, y, columnWidths["NIP kontrahenta"], 40), XStringFormats.TopLeft);
                    textFormatter.DrawString("Ilość", fontNormal, XBrushes.Black, new XRect(390, y, columnWidths["Ilość"], 40), XStringFormats.TopLeft);
                    textFormatter.DrawString("Data", fontNormal, XBrushes.Black, new XRect(450, y, columnWidths["Data"], 40), XStringFormats.TopLeft);
                    textFormatter.DrawString("Opis", fontNormal, XBrushes.Black, new XRect(530, y, columnWidths["Opis"], 40), XStringFormats.TopLeft);
                    y += 40;
                    gfx.DrawLine(XPens.Black, 40, y, page.Width - 40, y);
                    y += 10;

                    // Table Rows
                    foreach (var transaction in transactions.OrderBy(t => t.Date))
                    {
                        if (y > page.Height - 60)
                        {
                            page = document.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            y = 20;
                            textFormatter = new XTextFormatter(gfx) { Alignment = XParagraphAlignment.Left };
                        }

                        var productNameRect = new XRect(190, y, columnWidths["Nazwa produktu"], 100);
                        var contractorRect = new XRect(290, y, columnWidths["NIP kontrahenta"], 100);
                        var descriptionRect = new XRect(530, y, columnWidths["Opis"], 100);
                        double productNameHeight = MeasureWrappedTextHeight(gfx, transaction.ProductName ?? "", fontNormal, columnWidths["Nazwa produktu"]);
                        double contractorHeight = MeasureWrappedTextHeight(gfx, transaction.ContractorNIP ?? "", fontNormal, columnWidths["NIP kontrahenta"]);
                        double descriptionHeight = MeasureWrappedTextHeight(gfx, transaction.Description ?? "", fontNormal, columnWidths["Opis"]);
                        double rowHeight = Math.Max(20, Math.Max(productNameHeight, Math.Max(contractorHeight, descriptionHeight)));

                        textFormatter.DrawString(transaction.Type, fontNormal, XBrushes.Black, new XRect(40, y, columnWidths["Typ"], rowHeight), XStringFormats.TopLeft);
                        textFormatter.DrawString(transaction.Id.ToString(), fontNormal, XBrushes.Black, new XRect(100, y, columnWidths["ID"], rowHeight), XStringFormats.TopLeft);
                        textFormatter.DrawString(transaction.ProductId.ToString(), fontNormal, XBrushes.Black, new XRect(140, y, columnWidths["ID produktu"], rowHeight), XStringFormats.TopLeft);
                        textFormatter.DrawString(transaction.ProductName ?? "", fontNormal, XBrushes.Black, productNameRect, XStringFormats.TopLeft);
                        textFormatter.DrawString(transaction.ContractorNIP ?? "", fontNormal, XBrushes.Black, contractorRect, XStringFormats.TopLeft);
                        textFormatter.DrawString(transaction.Quantity.ToString(), fontNormal, XBrushes.Black, new XRect(390, y, columnWidths["Ilość"], rowHeight), XStringFormats.TopLeft);
                        textFormatter.DrawString(transaction.Date.ToString("yyyy-MM-dd"), fontNormal, XBrushes.Black, new XRect(450, y, columnWidths["Data"], rowHeight), XStringFormats.TopLeft);
                        textFormatter.DrawString(transaction.Description ?? "", fontNormal, XBrushes.Black, descriptionRect, XStringFormats.TopLeft);

                        y += rowHeight + 5;
                    }

                    // Footer
                    var footerText = "Unia, Koszalin, ul. Przykładowa 123";
                    var footerSize = gfx.MeasureString(footerText, fontFooter);
                    gfx.DrawString(footerText, fontFooter, XBrushes.Black, (page.Width - footerSize.Width) / 2, page.Height - 20);

                    document.Save(filePath);
                }
            }
            else if (format == "CSV")
            {
                var csv = new StringBuilder();
                csv.AppendLine("Typ,ID,ID produktu,Nazwa produktu,NIP kontrahenta,Ilość,Data,Opis");
                foreach (var transaction in transactions.OrderBy(t => t.Date))
                {
                    var fields = new[]
                    {
                        $"\"{transaction.Type}\"",
                        transaction.Id.ToString(),
                        transaction.ProductId.ToString(),
                        $"\"{transaction.ProductName?.Replace("\"", "\"\"") ?? ""}\"",
                        $"\"{transaction.ContractorNIP?.Replace("\"", "\"\"") ?? ""}\"",
                        transaction.Quantity.ToString(),
                        $"\"{transaction.Date:yyyy-MM-dd}\"",
                        $"\"{transaction.Description?.Replace("\"", "\"\"") ?? ""}\""
                    };
                    csv.AppendLine(string.Join(",", fields));
                }
                File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
            }
        }
    }
}