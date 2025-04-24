using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
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
        private bool _includeDocumentType = true;
        private bool _includeContractor = true;
        private bool _includeWarehouse = true;
        private bool _isContractorVisible;
        private bool _isTargetWarehouseVisible;
        private bool _isSourceWarehouseEditable;
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

        public ICommand GeneratePdfCommand { get; }
        public ICommand GenerateCsvCommand { get; }
        public ICommand DocumentTypeChangedCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand RemoveProductCommand { get; }

        public DocumentViewModel(Warehouse defaultSourceWarehouse)
        {
            _defaultSourceWarehouse = defaultSourceWarehouse ?? throw new ArgumentNullException(nameof(defaultSourceWarehouse));
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
            LoadData();
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
                var cmd = new SqlCommand("SELECT ProductId, Name, Quantity FROM Products WHERE WarehouseId = @WarehouseId AND Quantity > 0", conn);
                cmd.Parameters.AddWithValue("@WarehouseId", SourceWarehouse.WarehouseId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        AvailableProducts.Add(new Product
                        {
                            ProductId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Quantity = reader.GetInt32(2)
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
            var documentIds = SaveDocuments();
            if (!documentIds.Any()) return;

            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"Document_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveFileDialog.ShowDialog() != true) return;

                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var titleFont = new XFont("Verdana", 14);
                var font = new XFont("Verdana", 10);
                double x = 50, y = 50;

                // Generate title
                var docType = SelectedDocumentType.TypeName;
                var docTypeCode = docType switch
                {
                    "Delivery" => "in",
                    "Issue" => "out",
                    "Transfer" => "mm",
                    _ => "unk"
                };
                var docNumber = documentIds.First().ToString("D3"); // First document ID, padded to 3 digits
                var docName = $"{DateTime.Now:yyyy}/{docTypeCode}/{DateTime.Now:MM-dd}/{docNumber}";
                var title = $"{docType} {docName}";
                gfx.DrawString(title, titleFont, XBrushes.Black, new XPoint(x, y));
                y += 20;

                // Draw separator
                gfx.DrawLine(XPens.Black, x, y, x + 500, y);
                y += 20;

                // Calculate dynamic column widths
                var headers = new System.Collections.Generic.List<string>();
                if (IncludeDocumentType) headers.Add("Document Type");
                if (IncludeName) headers.Add("Product");
                if (IncludeQuantity) headers.Add("Quantity");
                if (IncludeContractor && IsContractorVisible) headers.Add("Contractor");
                if (IncludeWarehouse) headers.Add("Warehouse");

                var columnWidths = new double[headers.Count];
                for (int i = 0; i < headers.Count; i++)
                {
                    columnWidths[i] = headers[i].Length * 8; // Base width on header length
                    foreach (var item in SelectedProducts)
                    {
                        var value = GetColumnValue(i, item.Product, docType);
                        columnWidths[i] = Math.Max(columnWidths[i], (value?.Length ?? 0) * 8);
                    }
                }

                // Draw headers
                double currentX = x;
                for (int i = 0; i < headers.Count; i++)
                {
                    gfx.DrawString(headers[i], font, XBrushes.Black, new XPoint(currentX, y));
                    currentX += columnWidths[i] + 10; // Spacing between columns
                }
                y += 20;

                // Draw rows
                foreach (var item in SelectedProducts)
                {
                    currentX = x;
                    for (int i = 0; i < headers.Count; i++)
                    {
                        var value = GetColumnValue(i, item.Product, docType);
                        gfx.DrawString(value, font, XBrushes.Black, new XPoint(currentX, y));
                        currentX += columnWidths[i] + 10;
                    }
                    y += 20;
                }

                document.Save(saveFileDialog.FileName);
                document.Close();
                MessageBox.Show($"PDF generated successfully at {saveFileDialog.FileName}");
                OnDocumentGenerated?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}");
            }
        }

        private string GetColumnValue(int columnIndex, Product product, string docType)
        {
            int currentIndex = 0;
            if (IncludeDocumentType)
            {
                if (columnIndex == currentIndex) return docType;
                currentIndex++;
            }
            if (IncludeName)
            {
                if (columnIndex == currentIndex) return product.Name;
                currentIndex++;
            }
            if (IncludeQuantity)
            {
                if (columnIndex == currentIndex) return SelectedProducts.First(sp => sp.Product.ProductId == product.ProductId).Quantity.ToString();
                currentIndex++;
            }
            if (IncludeContractor && IsContractorVisible)
            {
                if (columnIndex == currentIndex) return $"{SelectedContractor?.Name} (NIP: {SelectedContractor?.NIP ?? "N/A"})";
                currentIndex++;
            }
            if (IncludeWarehouse)
            {
                if (columnIndex == currentIndex) return SourceWarehouse.WarehouseCode;
            }
            return string.Empty;
        }

        private void ExecuteGenerateCsv(object parameter)
        {
            var documentIds = SaveDocuments();
            if (!documentIds.Any()) return;

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
                    var headers = new System.Collections.Generic.List<string>();
                    if (IncludeName) headers.Add("Product");
                    if (IncludeQuantity) headers.Add("Quantity");
                    if (IncludeDocumentType) headers.Add("Document Type");
                    if (IncludeContractor && IsContractorVisible) headers.Add("Contractor");
                    if (IncludeWarehouse) headers.Add("Warehouse");
                    writer.WriteLine(string.Join(",", headers));

                    // Write data
                    foreach (var item in SelectedProducts)
                    {
                        var data = new System.Collections.Generic.List<string>();
                        if (IncludeName) data.Add(item.Product.Name);
                        if (IncludeQuantity) data.Add(item.Quantity.ToString());
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

        private System.Collections.Generic.List<int> SaveDocuments()
        {
            var documentIds = new System.Collections.Generic.List<int>();
            try
            {
                using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
                {
                    conn.Open();
                    foreach (var item in SelectedProducts)
                    {
                        var cmd = new SqlCommand(
                            "INSERT INTO Documents (DocumentTypeId, ProductId, WarehouseId, ContractorId, Quantity, DocumentDate) " +
                            "OUTPUT INSERTED.DocumentId " +
                            "VALUES (@DocumentTypeId, @ProductId, @WarehouseId, @ContractorId, @Quantity, @DocumentDate)", conn);
                        cmd.Parameters.AddWithValue("@DocumentTypeId", SelectedDocumentType.DocumentTypeId);
                        cmd.Parameters.AddWithValue("@ProductId", item.Product.ProductId);
                        cmd.Parameters.AddWithValue("@WarehouseId", SourceWarehouse.WarehouseId);
                        cmd.Parameters.AddWithValue("@ContractorId", SelectedContractor != null && IsContractorVisible ? SelectedContractor.ContractorId : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmd.Parameters.AddWithValue("@DocumentDate", DateTime.Now);
                        var documentId = (int)cmd.ExecuteScalar();
                        documentIds.Add(documentId);

                        if (SelectedDocumentType.TypeName == "Delivery")
                        {
                            cmd = new SqlCommand("UPDATE Products SET Quantity = Quantity + @Quantity WHERE ProductId = @ProductId", conn);
                            cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmd.Parameters.AddWithValue("@ProductId", item.Product.ProductId);
                            cmd.ExecuteNonQuery();
                        }
                        else if (SelectedDocumentType.TypeName == "Issue")
                        {
                            if (item.Product.Quantity < item.Quantity)
                            {
                                MessageBox.Show($"Cannot issue more than available stock for {item.Product.Name}.");
                                return new System.Collections.Generic.List<int>();
                            }
                            cmd = new SqlCommand("UPDATE Products SET Quantity = Quantity - @Quantity WHERE ProductId = @ProductId", conn);
                            cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmd.Parameters.AddWithValue("@ProductId", item.Product.ProductId);
                            cmd.ExecuteNonQuery();
                        }
                        else if (SelectedDocumentType.TypeName == "Transfer")
                        {
                            if (item.Product.Quantity < item.Quantity)
                            {
                                MessageBox.Show($"Cannot transfer more than available stock for {item.Product.Name}.");
                                return new System.Collections.Generic.List<int>();
                            }
                            // Reduce quantity in source warehouse
                            cmd = new SqlCommand(
                                "UPDATE Products SET Quantity = Quantity - @Quantity WHERE ProductId = @ProductId AND WarehouseId = @SourceWarehouseId", conn);
                            cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmd.Parameters.AddWithValue("@ProductId", item.Product.ProductId);
                            cmd.Parameters.AddWithValue("@SourceWarehouseId", SourceWarehouse.WarehouseId);
                            cmd.ExecuteNonQuery();

                            // Add or update product in target warehouse
                            cmd = new SqlCommand(
                                "IF EXISTS (SELECT 1 FROM Products WHERE Name = @Name AND WarehouseId = @TargetWarehouseId) " +
                                "UPDATE Products SET Quantity = Quantity + @Quantity WHERE Name = @Name AND WarehouseId = @TargetWarehouseId " +
                                "ELSE INSERT INTO Products (Name, Quantity, WarehouseId) VALUES (@Name, @Quantity, @TargetWarehouseId)", conn);
                            cmd.Parameters.AddWithValue("@Name", item.Product.Name);
                            cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmd.Parameters.AddWithValue("@TargetWarehouseId", TargetWarehouse.WarehouseId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return documentIds;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving documents: {ex.Message}");
                return new System.Collections.Generic.List<int>();
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
}