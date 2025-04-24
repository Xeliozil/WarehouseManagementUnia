using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Input;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia.ViewModels
{
    public class DocumentsViewModel : ViewModelBase
    {
        private ObservableCollection<Document> _documents;
        private ObservableCollection<DocumentType> _documentTypes;
        private ObservableCollection<Contractor> _contractors;
        private DocumentType _selectedDocumentType;
        private Contractor _selectedContractor;
        private readonly string _userRole;

        public ObservableCollection<Document> Documents
        {
            get => _documents;
            set { _documents = value; OnPropertyChanged(); }
        }

        public ObservableCollection<DocumentType> DocumentTypes
        {
            get => _documentTypes;
            set { _documentTypes = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Contractor> Contractors
        {
            get => _contractors;
            set { _contractors = value; OnPropertyChanged(); }
        }

        public DocumentType SelectedDocumentType
        {
            get => _selectedDocumentType;
            set
            {
                _selectedDocumentType = value;
                OnPropertyChanged();
                LoadDocuments();
            }
        }

        public Contractor SelectedContractor
        {
            get => _selectedContractor;
            set
            {
                _selectedContractor = value;
                OnPropertyChanged();
                LoadDocuments();
            }
        }

        public ICommand ClearFiltersCommand { get; }

        public DocumentsViewModel(string userRole)
        {
            _userRole = userRole;
            Documents = new ObservableCollection<Document>();
            DocumentTypes = new ObservableCollection<DocumentType>();
            Contractors = new ObservableCollection<Contractor>();
            ClearFiltersCommand = new RelayCommand<object>(ExecuteClearFilters);
            LoadDocumentTypes();
            LoadContractors();
            LoadDocuments();
        }

        private void LoadDocumentTypes()
        {
            DocumentTypes.Clear();
            DocumentTypes.Add(new DocumentType { DocumentTypeId = 0, TypeName = "All" }); // Add "All" option
            using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
            {
                conn.Open();
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
            }
        }

        private void LoadContractors()
        {
            Contractors.Clear();
            Contractors.Add(new Contractor { ContractorId = 0, Name = "All" }); // Add "All" option
            using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ContractorId, Name, NIP FROM Contractors", conn);
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
        }

        private void LoadDocuments()
        {
            Documents.Clear();
            using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
            {
                conn.Open();
                var query = "SELECT d.DocumentId, dt.TypeName, p.Name, d.Quantity, w.WarehouseCode, c.Name, c.NIP, d.DocumentDate " +
                            "FROM Documents d " +
                            "JOIN DocumentTypes dt ON d.DocumentTypeId = dt.DocumentTypeId " +
                            "JOIN Products p ON d.ProductId = p.ProductId " +
                            "JOIN Warehouses w ON d.WarehouseId = w.WarehouseId " +
                            "LEFT JOIN Contractors c ON d.ContractorId = c.ContractorId " +
                            "WHERE 1=1";
                if (SelectedDocumentType != null && SelectedDocumentType.DocumentTypeId != 0)
                    query += " AND d.DocumentTypeId = @DocumentTypeId";
                if (SelectedContractor != null && SelectedContractor.ContractorId != 0)
                    query += " AND d.ContractorId = @ContractorId";

                var cmd = new SqlCommand(query, conn);
                if (SelectedDocumentType != null && SelectedDocumentType.DocumentTypeId != 0)
                    cmd.Parameters.AddWithValue("@DocumentTypeId", SelectedDocumentType.DocumentTypeId);
                if (SelectedContractor != null && SelectedContractor.ContractorId != 0)
                    cmd.Parameters.AddWithValue("@ContractorId", SelectedContractor.ContractorId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Documents.Add(new Document
                        {
                            DocumentId = reader.GetInt32(0),
                            DocumentTypeName = reader.GetString(1),
                            ProductName = reader.GetString(2),
                            Quantity = reader.GetInt32(3),
                            WarehouseCode = reader.GetString(4),
                            ContractorName = reader.IsDBNull(5) ? null : reader.GetString(5),
                            ContractorNIP = reader.IsDBNull(6) ? null : reader.GetString(6),
                            DocumentDate = reader.GetDateTime(7)
                        });
                    }
                }
            }
        }

        private void ExecuteClearFilters(object parameter)
        {
            SelectedDocumentType = DocumentTypes.FirstOrDefault(dt => dt.DocumentTypeId == 0); // Select "All"
            SelectedContractor = Contractors.FirstOrDefault(c => c.ContractorId == 0); // Select "All"
            LoadDocuments();
        }
    }
}