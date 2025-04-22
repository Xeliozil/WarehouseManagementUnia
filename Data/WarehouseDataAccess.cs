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
                var command = new SqlCommand("SELECT Id, Name, Quantity, Price, IsActive, ContractorId FROM Products WHERE IsActive = 1", connection);
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
                            IsActive = reader.GetBoolean(4),
                            ContractorId = reader.IsDBNull(5) ? null : reader.GetInt32(5)
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
                var command = new SqlCommand("SELECT Id, Name, Quantity, Price, IsActive, ContractorId FROM Products", connection);
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
                            IsActive = reader.GetBoolean(4),
                            ContractorId = reader.IsDBNull(5) ? null : reader.GetInt32(5)
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
                    "INSERT INTO Products (Name, Quantity, Price, IsActive, ContractorId) VALUES (@Name, @Quantity, @Price, @IsActive, @ContractorId); SELECT SCOPE_IDENTITY();",
                    connection);
                command.Parameters.AddWithValue("@Name", product.Name);
                command.Parameters.AddWithValue("@Quantity", product.Quantity);
                command.Parameters.AddWithValue("@Price", product.Price);
                command.Parameters.AddWithValue("@IsActive", product.Quantity > 0);
                command.Parameters.AddWithValue("@ContractorId", (object)product.ContractorId ?? DBNull.Value);
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

        public List<Delivery> GetDeliveries()
        {
            var deliveries = new List<Delivery>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    @"SELECT d.Id, d.ProductId, p.Name, c.Name, d.Quantity, d.DeliveryDate, d.Description
                      FROM Deliveries d
                      INNER JOIN Products p ON d.ProductId = p.Id
                      LEFT JOIN Contractors c ON p.ContractorId = c.Id", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        deliveries.Add(new Delivery
                        {
                            Id = reader.GetInt32(0),
                            ProductId = reader.GetInt32(1),
                            ProductName = reader.GetString(2),
                            ContractorName = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Quantity = reader.GetInt32(4),
                            DeliveryDate = reader.GetDateTime(5),
                            Description = reader.IsDBNull(6) ? null : reader.GetString(6)
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
                    @"SELECT i.Id, i.ProductId, p.Name, c.Name, i.Quantity, i.IssueDate, i.Description
                      FROM Issues i
                      INNER JOIN Products p ON i.ProductId = p.Id
                      LEFT JOIN Contractors c ON p.ContractorId = c.Id", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        issues.Add(new Issue
                        {
                            Id = reader.GetInt32(0),
                            ProductId = reader.GetInt32(1),
                            ProductName = reader.GetString(2),
                            ContractorName = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Quantity = reader.GetInt32(4),
                            IssueDate = reader.GetDateTime(5),
                            Description = reader.IsDBNull(6) ? null : reader.GetString(6)
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

        public void GenerateReport(string title, DateTime startDate, DateTime endDate, string filePath, string format, int? contractorId = null, int? productId = null, string transactionType = null)
        {
            var transactions = new List<Transaction>();
            var deliveries = GetDeliveries().Where(d => d.DeliveryDate >= startDate && d.DeliveryDate <= endDate);
            var issues = GetIssues().Where(i => i.IssueDate >= startDate && i.IssueDate <= endDate);

            if (contractorId.HasValue)
            {
                deliveries = deliveries.Where(d => d.ContractorName == GetContractors().FirstOrDefault(c => c.Id == contractorId)?.Name);
                issues = issues.Where(i => i.ContractorName == GetContractors().FirstOrDefault(c => c.Id == contractorId)?.Name);
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
                    ContractorName = delivery.ContractorName ?? "",
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
                    ContractorName = issue.ContractorName ?? "",
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
                        filterText.Add($"Kontrahent: {GetContractors().FirstOrDefault(c => c.Id == contractorId)?.Name}");
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
                        { "Kontrahent", 100 },
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
                    textFormatter.DrawString("Kontrahent", fontNormal, XBrushes.Black, new XRect(290, y, columnWidths["Kontrahent"], 40), XStringFormats.TopLeft);
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
                        var contractorRect = new XRect(290, y, columnWidths["Kontrahent"], 100);
                        var descriptionRect = new XRect(530, y, columnWidths["Opis"], 100);
                        double productNameHeight = MeasureWrappedTextHeight(gfx, transaction.ProductName ?? "", fontNormal, columnWidths["Nazwa produktu"]);
                        double contractorHeight = MeasureWrappedTextHeight(gfx, transaction.ContractorName ?? "", fontNormal, columnWidths["Kontrahent"]);
                        double descriptionHeight = MeasureWrappedTextHeight(gfx, transaction.Description ?? "", fontNormal, columnWidths["Opis"]);
                        double rowHeight = Math.Max(20, Math.Max(productNameHeight, Math.Max(contractorHeight, descriptionHeight)));

                        textFormatter.DrawString(transaction.Type, fontNormal, XBrushes.Black, new XRect(40, y, columnWidths["Typ"], rowHeight), XStringFormats.TopLeft);
                        textFormatter.DrawString(transaction.Id.ToString(), fontNormal, XBrushes.Black, new XRect(100, y, columnWidths["ID"], rowHeight), XStringFormats.TopLeft);
                        textFormatter.DrawString(transaction.ProductId.ToString(), fontNormal, XBrushes.Black, new XRect(140, y, columnWidths["ID produktu"], rowHeight), XStringFormats.TopLeft);
                        textFormatter.DrawString(transaction.ProductName ?? "", fontNormal, XBrushes.Black, productNameRect, XStringFormats.TopLeft);
                        textFormatter.DrawString(transaction.ContractorName ?? "", fontNormal, XBrushes.Black, contractorRect, XStringFormats.TopLeft);
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
                csv.AppendLine("Typ,ID,ID produktu,Nazwa produktu,Kontrahent,Ilość,Data,Opis");
                foreach (var transaction in transactions.OrderBy(t => t.Date))
                {
                    var fields = new[]
                    {
                        $"\"{transaction.Type}\"",
                        transaction.Id.ToString(),
                        transaction.ProductId.ToString(),
                        $"\"{transaction.ProductName?.Replace("\"", "\"\"") ?? ""}\"",
                        $"\"{transaction.ContractorName?.Replace("\"", "\"\"") ?? ""}\"",
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