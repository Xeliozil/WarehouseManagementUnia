using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Reflection.Metadata;
using System.Windows.Input;
using System.Xml.Linq;
using WarehouseManagementUnia.Models;


namespace WarehouseManagementUnia.ViewModels
{
    public class DocumentsViewModel : ViewModelBase
    {
        private ObservableCollection<Models.Document> _documents;
        private ObservableCollection<Contractor> _contractors;
        private ObservableCollection<Warehouse> _warehouses;
        private ObservableCollection<DocumentType> _documentTypes;
        private Contractor _selectedContractor;
        private Warehouse _selectedWarehouse;
        private DocumentType _selectedDocumentType;
        private readonly string _userRole;

        public ObservableCollection<Models.Document> Documents
        {
            get => _documents;
            set { _documents = value; OnPropertyChanged(); }
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

        public ObservableCollection<DocumentType> DocumentTypes
        {
            get => _documentTypes;
            set { _documentTypes = value; OnPropertyChanged(); }
        }

        public Contractor SelectedContractor
        {
            get => _selectedContractor;
            set { _selectedContractor = value; OnPropertyChanged(); }
        }

        public Warehouse SelectedWarehouse
        {
            get => _selectedWarehouse;
            set { _selectedWarehouse = value; OnPropertyChanged(); }
        }

        public DocumentType SelectedDocumentType
        {
            get => _selectedDocumentType;
            set { _selectedDocumentType = value; OnPropertyChanged(); }
        }

        public ICommand FilterCommand { get; }

        public DocumentsViewModel(string userRole)
        {
            _userRole = userRole;
            Documents = new ObservableCollection<Models.Document>();
            Contractors = new ObservableCollection<Contractor>();
            Warehouses = new ObservableCollection<Warehouse>();
            DocumentTypes = new ObservableCollection<DocumentType>();
            FilterCommand = new RelayCommand<object>(ExecuteFilter);
            LoadFilters();
            LoadDocuments();
        }

        private void LoadFilters()
        {
            using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
            {
                conn.Open();

                // Load Contractors
                var cmd = new SqlCommand("SELECT ContractorId, Name FROM Contractors", conn);
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

                // Load Document Types
                cmd = new SqlCommand("SELECT DocumentTypeId, TypeName FROM DocumentTypes", conn);
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
            }
        }

        private void LoadDocuments()
        {
            Documents.Clear();
            using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
            {
                conn.Open();
                var query = "SELECT d.DocumentId, dt.TypeName, p.Name, w.WarehouseCode, c.Name, d.Quantity, d.DocumentDate " +
                            "FROM Documents d " +
                            "JOIN DocumentTypes dt ON d.DocumentTypeId = dt.DocumentTypeId " +
                            "JOIN Products p ON d.ProductId = p.ProductId " +
                            "JOIN Warehouses w ON d.WarehouseId = w.WarehouseId " +
                            "LEFT JOIN Contractors c ON d.ContractorId = c.ContractorId " +
                            "WHERE 1=1";
                if (SelectedContractor != null)
                    query += " AND d.ContractorId = @ContractorId";
                if (SelectedWarehouse != null)
                    query += " AND d.WarehouseId = @WarehouseId";
                if (SelectedDocumentType != null)
                    query += " AND d.DocumentTypeId = @DocumentTypeId";

                var cmd = new SqlCommand(query, conn);
                if (SelectedContractor != null)
                    cmd.Parameters.AddWithValue("@ContractorId", SelectedContractor.ContractorId);
                if (SelectedWarehouse != null)
                    cmd.Parameters.AddWithValue("@WarehouseId", SelectedWarehouse.WarehouseId);
                if (SelectedDocumentType != null)
                    cmd.Parameters.AddWithValue("@DocumentTypeId", SelectedDocumentType.DocumentTypeId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Documents.Add(new Models.Document
                        {
                            DocumentId = reader.GetInt32(0),
                            DocumentType = reader.GetString(1),
                            ProductName = reader.GetString(2),
                            WarehouseCode = reader.GetString(3),
                            ContractorName = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Quantity = reader.GetInt32(5),
                            DocumentDate = reader.GetDateTime(6)
                        });
                    }
                }
            }
        }

        private void ExecuteFilter(object parameter)
        {
            LoadDocuments();
        }
    }
}