using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia.ViewModels
{
    public class DocumentViewModel : ViewModelBase
    {
        private ObservableCollection<DocumentType> _documentTypes;
        private ObservableCollection<Product> _availableProducts;
        private ObservableCollection<SelectedProductItem> _selectedProducts;
        private ObservableCollection<Contractor> _contractors;
        private ObservableCollection<Warehouse> _warehouses;
        private ObservableCollection<Warehouse> _availableTargetWarehouses;
        private DocumentType _selectedDocumentType;
        private Product _selectedProductForAdd;
        private Contractor _selectedContractor;
        private Warehouse _sourceWarehouse;
        private Warehouse _targetWarehouse;
        private bool _includeName = true;
        private bool _includeQuantity = true;
        private bool _includePrice = true;
        private bool _includeDocumentType = true;
        private bool _includeContractor = true;
        private bool _includeWarehouse = true;
        private bool _isContractorVisible;
        private bool _isTargetWarehouseVisible;
        private bool _isSourceWarehouseEditable;
        private bool _isNewProductButtonVisible;
        private readonly Warehouse _defaultSourceWarehouse;
        public Action OnDocumentGenerated { get; set; }

        public ObservableCollection<DocumentType> DocumentTypes
        {
            get => _documentTypes;
            set { _documentTypes = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Product> AvailableProducts
        {
            get => _availableProducts;
            set { _availableProducts = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SelectedProductItem> SelectedProducts
        {
            get => _selectedProducts;
            set { _selectedProducts = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Contractor> Contractors
        {
            get => _contractors;
            set { _contractors = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Warehouse> Warehouses
        {
            get => _warehouses;
            set { _warehouses = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Warehouse> AvailableTargetWarehouses
        {
            get => _availableTargetWarehouses;
            set { _availableTargetWarehouses = value; OnPropertyChanged(); }
        }

        public DocumentType SelectedDocumentType
        {
            get => _selectedDocumentType;
            set
            {
                _selectedDocumentType = value;
                OnPropertyChanged();
                UpdateVisibility();
            }
        }

        public Product SelectedProductForAdd
        {
            get => _selectedProductForAdd;
            set { _selectedProductForAdd = value; OnPropertyChanged(); }
        }

        public Contractor SelectedContractor
        {
            get => _selectedContractor;
            set { _selectedContractor = value; OnPropertyChanged(); }
        }

        public Warehouse SourceWarehouse
        {
            get => _sourceWarehouse;
            set
            {
                _sourceWarehouse = value;
                OnPropertyChanged();
                LoadProducts();
                UpdateAvailableTargetWarehouses();
            }
        }

        public Warehouse TargetWarehouse
        {
            get => _targetWarehouse;
            set { _targetWarehouse = value; OnPropertyChanged(); }
        }

        public bool IncludeName
        {
            get => _includeName;
            set { _includeName = value; OnPropertyChanged(); }
        }

        public bool IncludeQuantity
        {
            get => _includeQuantity;
            set { _includeQuantity = value; OnPropertyChanged(); }
        }

        public bool IncludePrice
        {
            get => _includePrice;
            set { _includePrice = value; OnPropertyChanged(); }
        }

        public bool IncludeDocumentType
        {
            get => _includeDocumentType;
            set { _includeDocumentType = value; OnPropertyChanged(); }
        }

        public bool IncludeContractor
        {
            get => _includeContractor;
            set { _includeContractor = value; OnPropertyChanged(); }
        }

        public bool IncludeWarehouse
        {
            get => _includeWarehouse;
            set { _includeWarehouse = value; OnPropertyChanged(); }
        }

        public bool IsContractorVisible
        {
            get => _isContractorVisible;
            set { _isContractorVisible = value; OnPropertyChanged(); }
        }

        public bool IsTargetWarehouseVisible
        {
            get => _isTargetWarehouseVisible;
            set { _isTargetWarehouseVisible = value; OnPropertyChanged(); }
        }

        public bool IsSourceWarehouseEditable
        {
            get => _isSourceWarehouseEditable;
            set { _isSourceWarehouseEditable = value; OnPropertyChanged(); }
        }

        public bool IsNewProductButtonVisible
        {
            get => _isNewProductButtonVisible;
            set { _isNewProductButtonVisible = value; OnPropertyChanged(); }
        }

        public ICommand GeneratePdfCommand { get; }
        public ICommand GenerateCsvCommand { get; }
        public ICommand DocumentTypeChangedCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand RemoveProductCommand { get; }
        public ICommand AddNewProductCommand { get; }

        public DocumentViewModel(Warehouse defaultSourceWarehouse)
        {
            _defaultSourceWarehouse = defaultSourceWarehouse;
            DocumentTypes = new ObservableCollection<DocumentType>();
            AvailableProducts = new ObservableCollection<Product>();
            SelectedProducts = new ObservableCollection<SelectedProductItem>();
            Contractors = new ObservableCollection<Contractor>();
            Warehouses = new ObservableCollection<Warehouse>();
            AvailableTargetWarehouses = new ObservableCollection<Warehouse>();
            GeneratePdfCommand = new RelayCommand<object>(ExecuteGeneratePdf, CanGenerate);
            GenerateCsvCommand = new RelayCommand<object>(ExecuteGenerateCsv, CanGenerate);
            DocumentTypeChangedCommand = new RelayCommand<object>(ExecuteDocumentTypeChanged);
            AddProductCommand = new RelayCommand<object>(ExecuteAddProduct, CanAddProduct);
            RemoveProductCommand = new RelayCommand<object>(ExecuteRemoveProduct, CanRemoveProduct);
            AddNewProductCommand = new RelayCommand<object>(ExecuteAddNewProduct, CanAddNewProduct);
            if (defaultSourceWarehouse != null)
            {
                LoadData();
            }
        }

        private void LoadData()
        {
            using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
            {
                conn.Open();

                // Load Document Types
                var cmd = new SqlCommand("SELECT DocumentTypeId, TypeName FROM DocumentTypes", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DocumentTypes.Add(new DocumentType
                        {
                            DocumentTypeId = reader.GetInt32(0),
                            TypeName = reader.GetString(1)
                        });
                    }
                }

                // Load Warehouses
                cmd = new SqlCommand("SELECT WarehouseId, WarehouseCode FROM Warehouses", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Warehouses.Add(new Warehouse
                        {
                            WarehouseId = reader.GetInt32(0),
                            WarehouseCode = reader.GetString(1)
                        });
                    }
                }

                // Set default SourceWarehouse
                SourceWarehouse = Warehouses.FirstOrDefault(w => w.WarehouseId == _defaultSourceWarehouse.WarehouseId) ?? Warehouses.FirstOrDefault();
                IsSourceWarehouseEditable = false;

                // Load Contractors
                cmd = new SqlCommand("SELECT ContractorId, Name, NIP FROM Contractors", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Contractors.Add(new Contractor
                        {
                            ContractorId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            NIP = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }
            UpdateVisibility();
        }

        private void LoadProducts()
        {
            if (SourceWarehouse == null) return;
            AvailableProducts.Clear();
            using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ProductId, Name, Quantity, Price FROM Products WHERE WarehouseId = @WarehouseId AND Quantity > 0", conn);
                cmd.Parameters.AddWithValue("@WarehouseId", SourceWarehouse.WarehouseId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        AvailableProducts.Add(new Product
                        {
                            ProductId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Quantity = reader.GetInt32(2),
                            Price = reader.GetDecimal(3)
                        });
                    }
                }
            }
        }

        private void UpdateAvailableTargetWarehouses()
        {
            AvailableTargetWarehouses.Clear();
            if (SourceWarehouse != null)
            {
                foreach (var warehouse in Warehouses.Where(w => w.WarehouseId != SourceWarehouse.WarehouseId))
                {
                    AvailableTargetWarehouses.Add(warehouse);
                }
            }
            TargetWarehouse = AvailableTargetWarehouses.FirstOrDefault();
        }

        private void ExecuteDocumentTypeChanged(object parameter)
        {
            UpdateVisibility();
            SelectedContractor = null;
            TargetWarehouse = null;
            SelectedProducts.Clear();
        }

        private void UpdateVisibility()
        {
            IsContractorVisible = SelectedDocumentType?.TypeName != "Transfer";
            IsTargetWarehouseVisible = SelectedDocumentType?.TypeName == "Transfer";
            IsNewProductButtonVisible = SelectedDocumentType?.TypeName == "Delivery";
            IncludeContractor = IsContractorVisible;
        }

        private bool CanAddProduct(object parameter)
        {
            return SelectedProductForAdd != null && !SelectedProducts.Any(sp => sp.Product.ProductId == SelectedProductForAdd.ProductId);
        }

        private void ExecuteAddProduct(object parameter)
        {
            if (SelectedProductForAdd != null)
            {
                SelectedProducts.Add(new SelectedProductItem { Product = SelectedProductForAdd, Quantity = 1 });
                SelectedProductForAdd = null;
            }
        }

        private bool CanRemoveProduct(object parameter)
        {
            return SelectedProducts.Any();
        }

        private void ExecuteRemoveProduct(object parameter)
        {
            if (SelectedProducts.Any())
            {
                SelectedProducts.Remove(SelectedProducts.Last());
            }
        }

        private bool CanAddNewProduct(object parameter)
        {
            return SelectedDocumentType?.TypeName == "Delivery" && SourceWarehouse != null;
        }

        private void ExecuteAddNewProduct(object parameter)
        {
            try
            {
                // Simple input dialog for new product (replace with custom dialog if needed)
                string name = Microsoft.VisualBasic.Interaction.InputBox("Enter product name:", "New Product", "");
                if (string.IsNullOrWhiteSpace(name)) return;

                string quantityStr = Microsoft.VisualBasic.Interaction.InputBox("Enter quantity:", "New Product", "1");
                if (!int.TryParse(quantityStr, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Invalid quantity. Please enter a positive number.");
                    return;
                }

                string priceStr = Microsoft.VisualBasic.Interaction.InputBox("Enter price (e.g., 123.45):", "New Product", "0.00");
                if (!decimal.TryParse(priceStr, out decimal price) || price < 0)
                {
                    MessageBox.Show("Invalid price. Please enter a non-negative number.");
                    return;
                }

                using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "INSERT INTO Products (Name, Quantity, Price, WarehouseId) OUTPUT INSERTED.ProductId " +
                        "VALUES (@Name, @Quantity, @Price, @WarehouseId)", conn);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@Price", price);
                    cmd.Parameters.AddWithValue("@WarehouseId", SourceWarehouse.WarehouseId);
                    int productId = (int)cmd.ExecuteScalar();

                    var newProduct = new Product
                    {
                        ProductId = productId,
                        Name = name,
                        Quantity = quantity,
                        Price = price
                    };
                    AvailableProducts.Add(newProduct);
                    SelectedProducts.Add(new SelectedProductItem { Product = newProduct, Quantity = quantity });
                }
                MessageBox.Show("New product added successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding new product: {ex.Message}");
            }
        }

        private bool CanGenerate(object parameter)
        {
            if (SelectedDocumentType == null || !SelectedProducts.Any() || SourceWarehouse == null)
                return false;

            foreach (var item in SelectedProducts)
            {
                if (item.Quantity <= 0)
                    return false;
                if (SelectedDocumentType.TypeName == "Issue" || SelectedDocumentType.TypeName == "Transfer")
                {
                    if (item.Quantity > item.Product.Quantity)
                        return false;
                }
            }

            if (SelectedDocumentType.TypeName == "Transfer")
            {
                return TargetWarehouse != null && TargetWarehouse.WarehouseId != SourceWarehouse.WarehouseId;
            }

            return true;
        }

        private void ExecuteGeneratePdf(object parameter)
        {
            int documentId = SaveDocument();
            if (documentId == 0) return;

            GeneratePdfReport(documentId, false, saveFileDialog =>
            {
                saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
                saveFileDialog.FileName = $"Document_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            });
        }

        public void GeneratePdfReport(int documentId, bool isCopy, Action<SaveFileDialog> configureSaveFileDialog)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog();
                configureSaveFileDialog(saveFileDialog);

                if (saveFileDialog.ShowDialog() != true) return;

                // Fetch document data
                DocumentReportData reportData = GetDocumentReportData(documentId);

                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var titleFont = new XFont("Verdana", 14);
                var font = new XFont("Verdana", 10);
                double x = 50, y = 50;

                // Generate title
                var docType = reportData.DocumentType;
                var docTypeCode = docType switch
                {
                    "Delivery" => "in",
                    "Issue" => "out",
                    "Transfer" => "mm",
                    _ => "unk"
                };
                var docNumber = documentId.ToString("D3");
                var docName = $"{reportData.DocumentDate:yyyy}/{docTypeCode}/{reportData.DocumentDate:MM-dd}/{docNumber}";
                var title = isCopy ? $"COPY {docType} {docName}" : $"{docType} {docName}";
                gfx.DrawString(title, titleFont, XBrushes.Black, new XPoint(x, y));
                y += 20;

                // Draw separator
                gfx.DrawLine(XPens.Black, x, y, x + 500, y);
                y += 20;

                // Calculate dynamic column widths
                var headers = new List<string>();
                if (reportData.IncludeDocumentType) headers.Add("Document Type");
                if (reportData.IncludeName) headers.Add("Product");
                if (reportData.IncludeQuantity) headers.Add("Quantity");
                if (reportData.IncludePrice) headers.Add("Price");
                if (reportData.IncludeContractor && reportData.IsContractorVisible) headers.Add("Contractor");
                if (reportData.IncludeWarehouse) headers.Add("Warehouse");

                var columnWidths = new double[headers.Count];
                for (int i = 0; i < headers.Count; i++)
                {
                    columnWidths[i] = headers[i].Length * 8;
                    foreach (var item in reportData.Items)
                    {
                        var value = GetColumnValue(i, item, reportData);
                        columnWidths[i] = Math.Max(columnWidths[i], (value?.Length ?? 0) * 8);
                    }
                }

                // Draw headers
                double currentX = x;
                for (int i = 0; i < headers.Count; i++)
                {
                    gfx.DrawString(headers[i], font, XBrushes.Black, new XPoint(currentX, y));
                    currentX += columnWidths[i] + 5;
                }
                y += 20;

                // Draw rows
                foreach (var item in reportData.Items)
                {
                    currentX = x;
                    for (int i = 0; i < headers.Count; i++)
                    {
                        var value = GetColumnValue(i, item, reportData);
                        gfx.DrawString(value, font, XBrushes.Black, new XPoint(currentX, y));
                        currentX += columnWidths[i] + 5;
                    }
                    y += 20;
                }

                document.Save(saveFileDialog.FileName);
                document.Close();
                MessageBox.Show($"PDF generated successfully at {saveFileDialog.FileName}");
                if (!isCopy) OnDocumentGenerated?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}");
            }
        }

        private string GetColumnValue(int columnIndex, SelectedProductItem item, DocumentReportData reportData)
        {
            int currentIndex = 0;
            if (reportData.IncludeDocumentType)
            {
                if (columnIndex == currentIndex) return reportData.DocumentType;
                currentIndex++;
            }
            if (reportData.IncludeName)
            {
                if (columnIndex == currentIndex) return item.Product.Name;
                currentIndex++;
            }
            if (reportData.IncludeQuantity)
            {
                if (columnIndex == currentIndex) return item.Quantity.ToString();
                currentIndex++;
            }
            if (reportData.IncludePrice)
            {
                if (columnIndex == currentIndex) return item.Product.Price.ToString("C");
                currentIndex++;
            }
            if (reportData.IncludeContractor && reportData.IsContractorVisible)
            {
                if (columnIndex == currentIndex) return $"{reportData.ContractorName} (NIP: {reportData.ContractorNIP ?? "N/A"})";
                currentIndex++;
            }
            if (reportData.IncludeWarehouse)
            {
                if (columnIndex == currentIndex) return reportData.WarehouseCode;
            }
            return string.Empty;
        }

        private DocumentReportData GetDocumentReportData(int documentId)
        {
            using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
            {
                conn.Open();
                var reportData = new DocumentReportData();

                // Fetch document metadata
                var cmd = new SqlCommand(
                    @"SELECT dt.TypeName, d.DocumentDate, w.WarehouseCode, c.Name AS ContractorName, c.NIP, 
                             d.IncludeDocumentType, d.IncludeName, d.IncludeQuantity, d.IncludePrice, d.IncludeContractor, d.IncludeWarehouse
                      FROM Documents d
                      JOIN DocumentTypes dt ON d.DocumentTypeId = dt.DocumentTypeId
                      JOIN Warehouses w ON d.WarehouseId = w.WarehouseId
                      LEFT JOIN Contractors c ON d.ContractorId = c.ContractorId
                      WHERE d.DocumentId = @DocumentId", conn);
                cmd.Parameters.AddWithValue("@DocumentId", documentId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new Exception($"Document with ID {documentId} not found.");
                    }
                    reportData.DocumentType = reader.GetString(0);
                    reportData.DocumentDate = reader.GetDateTime(1);
                    reportData.WarehouseCode = reader.GetString(2);
                    reportData.ContractorName = reader.IsDBNull(3) ? null : reader.GetString(3);
                    reportData.ContractorNIP = reader.IsDBNull(4) ? null : reader.GetString(4);
                    reportData.IncludeDocumentType = reader.GetBoolean(5);
                    reportData.IncludeName = reader.GetBoolean(6);
                    reportData.IncludeQuantity = reader.GetBoolean(7);
                    reportData.IncludePrice = reader.GetBoolean(8);
                    reportData.IncludeContractor = reader.GetBoolean(9);
                    reportData.IncludeWarehouse = reader.GetBoolean(10);
                    reportData.IsContractorVisible = reportData.DocumentType != "Transfer";
                }

                // Fetch document items
                cmd = new SqlCommand(
                    @"SELECT p.ProductId, p.Name, p.Quantity, di.Quantity AS DocumentQuantity, p.Price
                      FROM DocumentItems di
                      JOIN Products p ON di.ProductId = p.ProductId
                      WHERE di.DocumentId = @DocumentId", conn);
                cmd.Parameters.AddWithValue("@DocumentId", documentId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reportData.Items.Add(new SelectedProductItem
                        {
                            Product = new Product
                            {
                                ProductId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Quantity = reader.GetInt32(2),
                                Price = reader.GetDecimal(4)
                            },
                            Quantity = reader.GetInt32(3)
                        });
                    }
                }

                if (!reportData.Items.Any() && reportData.IncludeName)
                {
                    MessageBox.Show($"Warning: No products found for document ID {documentId}.");
                }

                return reportData;
            }
        }

        private void ExecuteGenerateCsv(object parameter)
        {
            int documentId = SaveDocument();
            if (documentId == 0) return;

            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FileName = $"Document_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveFileDialog.ShowDialog() != true) return;

                using (var writer = new StreamWriter(saveFileDialog.FileName))
                {
                    // Write headers
                    var headers = new List<string>();
                    if (IncludeName) headers.Add("Product");
                    if (IncludeQuantity) headers.Add("Quantity");
                    if (IncludePrice) headers.Add("Price");
                    if (IncludeDocumentType) headers.Add("Document Type");
                    if (IncludeContractor && IsContractorVisible) headers.Add("Contractor");
                    if (IncludeWarehouse) headers.Add("Warehouse");
                    writer.WriteLine(string.Join(",", headers));

                    // Write data
                    foreach (var item in SelectedProducts)
                    {
                        var data = new List<string>();
                        if (IncludeName) data.Add(item.Product.Name);
                        if (IncludeQuantity) data.Add(item.Quantity.ToString());
                        if (IncludePrice) data.Add(item.Product.Price.ToString("C"));
                        if (IncludeDocumentType) data.Add(SelectedDocumentType.TypeName);
                        if (IncludeContractor && IsContractorVisible) data.Add($"{SelectedContractor?.Name} (NIP: {SelectedContractor?.NIP ?? "N/A"})");
                        if (IncludeWarehouse) data.Add(SourceWarehouse.WarehouseCode);
                        writer.WriteLine(string.Join(",", data));
                    }
                }
                MessageBox.Show($"CSV generated successfully at {saveFileDialog.FileName}");
                OnDocumentGenerated?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating CSV: {ex.Message}");
            }
        }

        private int SaveDocument()
        {
            try
            {
                using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
                {
                    conn.Open();
                    // Begin transaction
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Insert single document
                            var cmd = new SqlCommand(
                                @"INSERT INTO Documents (DocumentTypeId, WarehouseId, ContractorId, DocumentDate, 
                                                        IncludeDocumentType, IncludeName, IncludeQuantity, IncludePrice, IncludeContractor, IncludeWarehouse)
                                  OUTPUT INSERTED.DocumentId
                                  VALUES (@DocumentTypeId, @WarehouseId, @ContractorId, @DocumentDate, 
                                          @IncludeDocumentType, @IncludeName, @IncludeQuantity, @IncludePrice, @IncludeContractor, @IncludeWarehouse)", conn, transaction);
                            cmd.Parameters.AddWithValue("@DocumentTypeId", SelectedDocumentType.DocumentTypeId);
                            cmd.Parameters.AddWithValue("@WarehouseId", SourceWarehouse.WarehouseId);
                            cmd.Parameters.AddWithValue("@ContractorId", SelectedContractor != null && IsContractorVisible ? SelectedContractor.ContractorId : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DocumentDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@IncludeDocumentType", IncludeDocumentType);
                            cmd.Parameters.AddWithValue("@IncludeName", IncludeName);
                            cmd.Parameters.AddWithValue("@IncludeQuantity", IncludeQuantity);
                            cmd.Parameters.AddWithValue("@IncludePrice", IncludePrice);
                            cmd.Parameters.AddWithValue("@IncludeContractor", IncludeContractor && IsContractorVisible);
                            cmd.Parameters.AddWithValue("@IncludeWarehouse", IncludeWarehouse);
                            int documentId = (int)cmd.ExecuteScalar();

                            // Insert document items
                            foreach (var item in SelectedProducts)
                            {
                                cmd = new SqlCommand(
                                    "INSERT INTO DocumentItems (DocumentId, ProductId, Quantity) " +
                                    "VALUES (@DocumentId, @ProductId, @Quantity)", conn, transaction);
                                cmd.Parameters.AddWithValue("@DocumentId", documentId);
                                cmd.Parameters.AddWithValue("@ProductId", item.Product.ProductId);
                                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmd.ExecuteNonQuery();

                                if (SelectedDocumentType.TypeName == "Delivery")
                                {
                                    cmd = new SqlCommand(
                                        "UPDATE Products SET Quantity = Quantity + @Quantity WHERE ProductId = @ProductId", conn, transaction);
                                    cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    cmd.Parameters.AddWithValue("@ProductId", item.Product.ProductId);
                                    cmd.ExecuteNonQuery();
                                }
                                else if (SelectedDocumentType.TypeName == "Issue")
                                {
                                    if (item.Product.Quantity < item.Quantity)
                                    {
                                        throw new Exception($"Cannot issue more than available stock for {item.Product.Name}.");
                                    }
                                    cmd = new SqlCommand(
                                        "UPDATE Products SET Quantity = Quantity - @Quantity WHERE ProductId = @ProductId", conn, transaction);
                                    cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    cmd.Parameters.AddWithValue("@ProductId", item.Product.ProductId);
                                    cmd.ExecuteNonQuery();
                                }
                                else if (SelectedDocumentType.TypeName == "Transfer")
                                {
                                    if (item.Product.Quantity < item.Quantity)
                                    {
                                        throw new Exception($"Cannot transfer more than available stock for {item.Product.Name}.");
                                    }
                                    // Reduce quantity in source warehouse
                                    cmd = new SqlCommand(
                                        "UPDATE Products SET Quantity = Quantity - @Quantity WHERE ProductId = @ProductId AND WarehouseId = @SourceWarehouseId", conn, transaction);
                                    cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    cmd.Parameters.AddWithValue("@ProductId", item.Product.ProductId);
                                    cmd.Parameters.AddWithValue("@SourceWarehouseId", SourceWarehouse.WarehouseId);
                                    cmd.ExecuteNonQuery();

                                    // Add or update product in target warehouse
                                    cmd = new SqlCommand(
                                        @"IF EXISTS (SELECT 1 FROM Products WHERE Name = @Name AND WarehouseId = @TargetWarehouseId)
                                          UPDATE Products SET Quantity = Quantity + @Quantity, Price = @Price WHERE Name = @Name AND WarehouseId = @TargetWarehouseId
                                          ELSE INSERT INTO Products (Name, Quantity, Price, WarehouseId) VALUES (@Name, @Quantity, @Price, @TargetWarehouseId)", conn, transaction);
                                    cmd.Parameters.AddWithValue("@Name", item.Product.Name);
                                    cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                    cmd.Parameters.AddWithValue("@Price", item.Product.Price);
                                    cmd.Parameters.AddWithValue("@TargetWarehouseId", TargetWarehouse.WarehouseId);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            return documentId;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Error saving document: {ex.Message}");
                            return 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving document: {ex.Message}");
                return 0;
            }
        }
    }

    public class SelectedProductItem : ViewModelBase
    {
        private Product _product;
        private int _quantity;

        public Product Product
        {
            get => _product;
            set { _product = value; OnPropertyChanged(); }
        }

        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }
    }

    public class DocumentReportData
    {
        public string DocumentType { get; set; }
        public DateTime DocumentDate { get; set; }
        public string WarehouseCode { get; set; }
        public string ContractorName { get; set; }
        public string ContractorNIP { get; set; }
        public bool IncludeDocumentType { get; set; }
        public bool IncludeName { get; set; }
        public bool IncludeQuantity { get; set; }
        public bool IncludePrice { get; set; }
        public bool IncludeContractor { get; set; }
        public bool IncludeWarehouse { get; set; }
        public bool IsContractorVisible { get; set; }
        public List<SelectedProductItem> Items { get; set; } = new List<SelectedProductItem>();
    }
}  