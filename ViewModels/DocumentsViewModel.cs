using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
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
        private Document _selectedDocument;
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

        public Document SelectedDocument
        {
            get => _selectedDocument;
            set { _selectedDocument = value; OnPropertyChanged(); }
        }

        public ICommand ClearFiltersCommand { get; }
        public ICommand PrintDocumentCommand { get; }

        public DocumentsViewModel(string userRole)
        {
            _userRole = userRole;
            Documents = new ObservableCollection<Document>();
            DocumentTypes = new ObservableCollection<DocumentType>();
            Contractors = new ObservableCollection<Contractor>();
            ClearFiltersCommand = new RelayCommand<object>(ClearFilters);
            PrintDocumentCommand = new RelayCommand<Document>(PrintDocument, CanPrintDocument);
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
            LoadDocuments();
        }

        private void LoadDocuments()
        {
            Documents.Clear();
            using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
            {
                conn.Open();
                var query = @"SELECT d.DocumentId, dt.TypeName AS DocumentTypeName, w.WarehouseCode, 
                                    c.Name AS ContractorName, c.NIP AS ContractorNIP, d.DocumentDate
                              FROM Documents d
                              JOIN DocumentTypes dt ON d.DocumentTypeId = dt.DocumentTypeId
                              JOIN Warehouses w ON d.WarehouseId = w.WarehouseId
                              LEFT JOIN Contractors c ON d.ContractorId = c.ContractorId
                              WHERE (@DocumentTypeId IS NULL OR d.DocumentTypeId = @DocumentTypeId)
                                AND (@ContractorId IS NULL OR d.ContractorId = @ContractorId)";
                var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@DocumentTypeId", SelectedDocumentType?.DocumentTypeId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ContractorId", SelectedContractor?.ContractorId ?? (object)DBNull.Value);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Documents.Add(new Document
                        {
                            DocumentId = reader.GetInt32(0),
                            DocumentTypeName = reader.GetString(1),
                            WarehouseCode = reader.GetString(2),
                            ContractorName = reader.IsDBNull(3) ? null : reader.GetString(3),
                            ContractorNIP = reader.IsDBNull(4) ? null : reader.GetString(4),
                            DocumentDate = reader.GetDateTime(5)
                        });
                    }
                }
            }
        }

        private void ClearFilters(object parameter)
        {
            SelectedDocumentType = null;
            SelectedContractor = null;
            LoadDocuments();
        }

        private bool CanPrintDocument(Document document)
        {
            return document != null;
        }

        private void PrintDocument(Document document)
        {
            if (document == null) return;

            var documentViewModel = new DocumentViewModel(defaultSourceWarehouse: null); // Fixed parameter name
            documentViewModel.GeneratePdfReport(document.DocumentId, true, saveFileDialog =>
            {
                saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
                saveFileDialog.FileName = $"COPY_Document_{document.DocumentId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            });
        }
    }
}