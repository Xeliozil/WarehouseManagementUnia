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
        private ObservableCollection<Product> _products;
        private ObservableCollection<Contractor> _contractors;
        private ObservableCollection<Warehouse> _warehouses;
        private ObservableCollection<Warehouse> _availableTargetWarehouses;
        private DocumentType _selectedDocumentType;
        private Product _selectedProduct;
        private Contractor _selectedContractor;
        private Warehouse _sourceWarehouse;
        private Warehouse _targetWarehouse;
        private int _quantity;
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

        public ObservableCollection<Product> Products
        {
            get => _products;
            set { _products = value; OnPropertyChanged(); }
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

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set { _selectedProduct = value; OnPropertyChanged(); }
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

        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
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

        public DocumentViewModel(Warehouse defaultSourceWarehouse)
        {
            _defaultSourceWarehouse = defaultSourceWarehouse ?? throw new ArgumentNullException(nameof(defaultSourceWarehouse));
            DocumentTypes = new ObservableCollection<DocumentType>();
            Products = new ObservableCollection<Product>();
            Contractors = new ObservableCollection<Contractor>();
            Warehouses = new ObservableCollection<Warehouse>();
            AvailableTargetWarehouses = new ObservableCollection<Warehouse>();
            GeneratePdfCommand = new RelayCommand<object>(ExecuteGeneratePdf, CanGenerate);
            GenerateCsvCommand = new RelayCommand<object>(ExecuteGenerateCsv, CanGenerate);
            DocumentTypeChangedCommand = new RelayCommand<object>(ExecuteDocumentTypeChanged);
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
                IsSourceWarehouseEditable = false; // Lock source warehouse to the default

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
            Products.Clear();
            using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ProductId, Name, Quantity FROM Products WHERE WarehouseId = @WarehouseId AND Quantity > 0", conn);
                cmd.Parameters.AddWithValue("@WarehouseId", SourceWarehouse.WarehouseId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Products.Add(new Product
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
        }

        private void UpdateVisibility()
        {
            IsContractorVisible = SelectedDocumentType?.TypeName != "Transfer";
            IsTargetWarehouseVisible = SelectedDocumentType?.TypeName == "Transfer";
            IncludeContractor = IsContractorVisible; // Ensure contractor is not included in reports for transfers
        }

        private bool CanGenerate(object parameter)
        {
            if (SelectedDocumentType == null || SelectedProduct == null || Quantity <= 0 || SourceWarehouse == null)
                return false;

            if (SelectedDocumentType.TypeName == "Transfer")
            {
                if (TargetWarehouse == null || TargetWarehouse.WarehouseId == SourceWarehouse.WarehouseId)
                    return false;
            }

            // Validate stock for Issue or Transfer
            if (SelectedDocumentType.TypeName == "Issue" || SelectedDocumentType.TypeName == "Transfer")
            {
                return SelectedProduct.Quantity >= Quantity;
            }

            return true;
        }

        private void ExecuteGeneratePdf(object parameter)
        {
            if (!SaveDocument()) return;

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
                var font = new XFont("Verdana", 10);
                // For PdfSharp 1.50, use XFontStyle.Bold; for PdfSharpCore, use XFontStyle.Bold or adjust as needed
                var headerFont = new XFont("Verdana", 10);
                double x = 50, y = 50;
                double cellWidth = 120, cellHeight = 20;
                double[] columnWidths = { 120, 80, 80, 120, 100 }; // Adjust for each column

                // Draw table headers
                var headers = new System.Collections.Generic.List<string>();
                if (IncludeDocumentType) headers.Add("Document Type");
                if (IncludeName) headers.Add("Product");
                if (IncludeQuantity) headers.Add("Quantity");
                if (IncludeContractor && IsContractorVisible) headers.Add("Contractor");
                if (IncludeWarehouse) headers.Add("Warehouse");

                for (int i = 0; i < headers.Count; i++)
                {
                    gfx.DrawRectangle(XPens.Black, x + i * columnWidths[i], y, columnWidths[i], cellHeight);
                    gfx.DrawString(headers[i], headerFont, XBrushes.Black, new XRect(x + i * columnWidths[i] + 5, y + 5, columnWidths[i], cellHeight), XStringFormats.TopLeft);
                }
                y += cellHeight;

                // Draw table row
                int colIndex = 0;
                if (IncludeDocumentType)
                {
                    gfx.DrawRectangle(XPens.Black, x + colIndex * columnWidths[colIndex], y, columnWidths[colIndex], cellHeight);
                    gfx.DrawString(SelectedDocumentType.TypeName, font, XBrushes.Black, new XRect(x + colIndex * columnWidths[colIndex] + 5, y + 5, columnWidths[colIndex], cellHeight), XStringFormats.TopLeft);
                    colIndex++;
                }
                if (IncludeName)
                {
                    gfx.DrawRectangle(XPens.Black, x + colIndex * columnWidths[colIndex], y, columnWidths[colIndex], cellHeight);
                    gfx.DrawString(SelectedProduct.Name, font, XBrushes.Black, new XRect(x + colIndex * columnWidths[colIndex] + 5, y + 5, columnWidths[colIndex], cellHeight), XStringFormats.TopLeft);
                    colIndex++;
                }
                if (IncludeQuantity)
                {
                    gfx.DrawRectangle(XPens.Black, x + colIndex * columnWidths[colIndex], y, columnWidths[colIndex], cellHeight);
                    gfx.DrawString(Quantity.ToString(), font, XBrushes.Black, new XRect(x + colIndex * columnWidths[colIndex] + 5, y + 5, columnWidths[colIndex], cellHeight), XStringFormats.TopLeft);
                    colIndex++;
                }
                if (IncludeContractor && SelectedContractor != null && IsContractorVisible)
                {
                    gfx.DrawRectangle(XPens.Black, x + colIndex * columnWidths[colIndex], y, columnWidths[colIndex], cellHeight);
                    gfx.DrawString($"{SelectedContractor.Name} (NIP: {SelectedContractor.NIP ?? "N/A"})", font, XBrushes.Black, new XRect(x + colIndex * columnWidths[colIndex] + 5, y + 5, columnWidths[colIndex], cellHeight), XStringFormats.TopLeft);
                    colIndex++;
                }
                if (IncludeWarehouse)
                {
                    gfx.DrawRectangle(XPens.Black, x + colIndex * columnWidths[colIndex], y, columnWidths[colIndex], cellHeight);
                    gfx.DrawString(SourceWarehouse.WarehouseCode, font, XBrushes.Black, new XRect(x + colIndex * columnWidths[colIndex] + 5, y + 5, columnWidths[colIndex], cellHeight), XStringFormats.TopLeft);
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

        private void ExecuteGenerateCsv(object parameter)
        {
            if (!SaveDocument()) return;

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
                    var data = new System.Collections.Generic.List<string>();
                    if (IncludeName) data.Add(SelectedProduct.Name);
                    if (IncludeQuantity) data.Add(Quantity.ToString());
                    if (IncludeDocumentType) data.Add(SelectedDocumentType.TypeName);
                    if (IncludeContractor && IsContractorVisible) data.Add($"{SelectedContractor?.Name} (NIP: {SelectedContractor?.NIP ?? "N/A"})");
                    if (IncludeWarehouse) data.Add(SourceWarehouse.WarehouseCode);
                    writer.WriteLine(string.Join(",", data));
                }
                MessageBox.Show($"CSV generated successfully at {saveFileDialog.FileName}");
                OnDocumentGenerated?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating CSV: {ex.Message}");
            }
        }

        private bool SaveDocument()
        {
            try
            {
                using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "INSERT INTO Documents (DocumentTypeId, ProductId, WarehouseId, ContractorId, Quantity, DocumentDate) " +
                        "VALUES (@DocumentTypeId, @ProductId, @WarehouseId, @ContractorId, @Quantity, @DocumentDate)", conn);
                    cmd.Parameters.AddWithValue("@DocumentTypeId", SelectedDocumentType.DocumentTypeId);
                    cmd.Parameters.AddWithValue("@ProductId", SelectedProduct.ProductId);
                    cmd.Parameters.AddWithValue("@WarehouseId", SourceWarehouse.WarehouseId);
                    cmd.Parameters.AddWithValue("@ContractorId", SelectedContractor != null && IsContractorVisible ? SelectedContractor.ContractorId : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Quantity", Quantity);
                    cmd.Parameters.AddWithValue("@DocumentDate", DateTime.Now);
                    cmd.ExecuteNonQuery();

                    if (SelectedDocumentType.TypeName == "Delivery")
                    {
                        cmd = new SqlCommand("UPDATE Products SET Quantity = Quantity + @Quantity WHERE ProductId = @ProductId", conn);
                        cmd.Parameters.AddWithValue("@Quantity", Quantity);
                        cmd.Parameters.AddWithValue("@ProductId", SelectedProduct.ProductId);
                        cmd.ExecuteNonQuery();
                    }
                    else if (SelectedDocumentType.TypeName == "Issue")
                    {
                        if (SelectedProduct.Quantity < Quantity)
                        {
                            MessageBox.Show("Cannot issue more than available stock.");
                            return false;
                        }
                        cmd = new SqlCommand("UPDATE Products SET Quantity = Quantity - @Quantity WHERE ProductId = @ProductId", conn);
                        cmd.Parameters.AddWithValue("@Quantity", Quantity);
                        cmd.Parameters.AddWithValue("@ProductId", SelectedProduct.ProductId);
                        cmd.ExecuteNonQuery();
                    }
                    else if (SelectedDocumentType.TypeName == "Transfer")
                    {
                        if (SelectedProduct.Quantity < Quantity)
                        {
                            MessageBox.Show("Cannot transfer more than available stock.");
                            return false;
                        }
                        // Reduce quantity in source warehouse
                        cmd = new SqlCommand(
                            "UPDATE Products SET Quantity = Quantity - @Quantity WHERE ProductId = @ProductId AND WarehouseId = @SourceWarehouseId", conn);
                        cmd.Parameters.AddWithValue("@Quantity", Quantity);
                        cmd.Parameters.AddWithValue("@ProductId", SelectedProduct.ProductId);
                        cmd.Parameters.AddWithValue("@SourceWarehouseId", SourceWarehouse.WarehouseId);
                        cmd.ExecuteNonQuery();

                        // Add or update product in target warehouse
                        cmd = new SqlCommand(
                            "IF EXISTS (SELECT 1 FROM Products WHERE Name = @Name AND WarehouseId = @TargetWarehouseId) " +
                            "UPDATE Products SET Quantity = Quantity + @Quantity WHERE Name = @Name AND WarehouseId = @TargetWarehouseId " +
                            "ELSE INSERT INTO Products (Name, Quantity, WarehouseId) VALUES (@Name, @Quantity, @TargetWarehouseId)", conn);
                        cmd.Parameters.AddWithValue("@Name", SelectedProduct.Name);
                        cmd.Parameters.AddWithValue("@Quantity", Quantity);
                        cmd.Parameters.AddWithValue("@TargetWarehouseId", TargetWarehouse.WarehouseId);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving document: {ex.Message}");
                return false;
            }
        }
    }
}