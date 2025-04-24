using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.IO;
using System.Windows.Input;
using System.Xml.Linq;
using WarehouseManagementUnia.Models;


namespace WarehouseManagementUnia.ViewModels
{
    public class DocumentViewModel : ViewModelBase
    {
        private ObservableCollection<DocumentType> _documentTypes;
        private ObservableCollection<Product> _products;
        private ObservableCollection<Contractor> _contractors;
        private ObservableCollection<Warehouse> _warehouses;
        private DocumentType _selectedDocumentType;
        private Product _selectedProduct;
        private Contractor _selectedContractor;
        private Warehouse _targetWarehouse;
        private int _quantity;
        private bool _includeName = true;
        private bool _includeQuantity = true;
        private readonly Warehouse _currentWarehouse;

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

        public DocumentType SelectedDocumentType
        {
            get => _selectedDocumentType;
            set { _selectedDocumentType = value; OnPropertyChanged(); }
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

        public ICommand GeneratePdfCommand { get; }
        public ICommand GenerateCsvCommand { get; }

        public DocumentViewModel(Warehouse warehouse)
        {
            _currentWarehouse = warehouse;
            DocumentTypes = new ObservableCollection<DocumentType>();
            Products = new ObservableCollection<Product>();
            Contractors = new ObservableCollection<Contractor>();
            Warehouses = new ObservableCollection<Warehouse>();
            GeneratePdfCommand = new RelayCommand<object>(ExecuteGeneratePdf);
            GenerateCsvCommand = new RelayCommand<object>(ExecuteGenerateCsv);
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

                // Load Products
                cmd = new SqlCommand("SELECT ProductId, Name FROM Products WHERE WarehouseId = @WarehouseId", conn);
                cmd.Parameters.AddWithValue("@WarehouseId", _currentWarehouse.WarehouseId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Products.Add(new Product
                        {
                            ProductId = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }

                // Load Contractors
                cmd = new SqlCommand("SELECT ContractorId, Name FROM Contractors", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Contractors.Add(new Contractor
                        {
                            ContractorId = reader.GetInt32(0),
                            Name = reader.GetString(1)
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
            }
        }

        private void ExecuteGeneratePdf(object parameter)
        {
            SaveDocument();
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Verdana", 12);
            int y = 50;

            gfx.DrawString($"Document: {SelectedDocumentType.TypeName}", font, XBrushes.Black, new XPoint(50, y));
            y += 20;
            if (IncludeName)
            {
                gfx.DrawString($"Product: {SelectedProduct.Name}", font, XBrushes.Black, new XPoint(50, y));
                y += 20;
            }
            if (IncludeQuantity)
            {
                gfx.DrawString($"Quantity: {Quantity}", font, XBrushes.Black, new XPoint(50, y));
                y += 20;
            }
            if (SelectedContractor != null)
            {
                gfx.DrawString($"Contractor: {SelectedContractor.Name}", font, XBrushes.Black, new XPoint(50, y));
            }

            document.Save("document.pdf");
            document.Close();
        }

        private void ExecuteGenerateCsv(object parameter)
        {
            SaveDocument();
            using (var writer = new StreamWriter("document.csv"))
            {
                if (IncludeName) writer.Write("Product,");
                if (IncludeQuantity) writer.Write("Quantity,");
                writer.WriteLine("Document Type,Contractor");

                if (IncludeName) writer.Write($"{SelectedProduct.Name},");
                if (IncludeQuantity) writer.Write($"{Quantity},");
                writer.WriteLine($"{SelectedDocumentType.TypeName},{SelectedContractor?.Name ?? ""}");
            }
        }

        private void SaveDocument()
        {
            using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
            {
                conn.Open();
                var cmd = new SqlCommand(
                    "INSERT INTO Documents (DocumentTypeId, ProductId, WarehouseId, ContractorId, Quantity, DocumentDate) " +
                    "VALUES (@DocumentTypeId, @ProductId, @WarehouseId, @ContractorId, @Quantity, @DocumentDate)", conn);
                cmd.Parameters.AddWithValue("@DocumentTypeId", SelectedDocumentType.DocumentTypeId);
                cmd.Parameters.AddWithValue("@ProductId", SelectedProduct.ProductId);
                cmd.Parameters.AddWithValue("@WarehouseId", _currentWarehouse.WarehouseId);
                cmd.Parameters.AddWithValue("@ContractorId", SelectedContractor != null ? SelectedContractor.ContractorId : (object)DBNull.Value);
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
                    cmd = new SqlCommand("UPDATE Products SET Quantity = Quantity - @Quantity WHERE ProductId = @ProductId", conn);
                    cmd.Parameters.AddWithValue("@Quantity", Quantity);
                    cmd.Parameters.AddWithValue("@ProductId", SelectedProduct.ProductId);
                    cmd.ExecuteNonQuery();
                }
                else if (SelectedDocumentType.TypeName == "Transfer" && TargetWarehouse != null)
                {
                    cmd = new SqlCommand("UPDATE Products SET WarehouseId = @TargetWarehouseId WHERE ProductId = @ProductId", conn);
                    cmd.Parameters.AddWithValue("@TargetWarehouseId", TargetWarehouse.WarehouseId);
                    cmd.Parameters.AddWithValue("@ProductId", SelectedProduct.ProductId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}