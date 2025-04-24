using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WarehouseManagementUnia.Models;
using WarehouseManagementUnia.Views;

namespace WarehouseManagementUnia.ViewModels
{
    public class StockViewModel : ViewModelBase
    {
        private ObservableCollection<Warehouse> _warehouses;
        private ObservableCollection<Product> _products;
        private Warehouse _selectedWarehouse;
        private string _filterName;
        private readonly string _userRole;

        public ObservableCollection<Warehouse> Warehouses
        {
            get => _warehouses;
            set { _warehouses = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Product> Products
        {
            get => _products;
            set { _products = value; OnPropertyChanged(); }
        }

        public Warehouse SelectedWarehouse
        {
            get => _selectedWarehouse;
            set
            {
                _selectedWarehouse = value;
                OnPropertyChanged();
                LoadProducts();
            }
        }

        public string FilterName
        {
            get => _filterName;
            set { _filterName = value; OnPropertyChanged(); }
        }

        public ICommand FilterCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand OpenDocumentCommand { get; }

        public StockViewModel(string userRole)
        {
            _userRole = userRole;
            Warehouses = new ObservableCollection<Warehouse>();
            Products = new ObservableCollection<Product>();
            FilterCommand = new RelayCommand<object>(ExecuteFilter);
            AddProductCommand = new RelayCommand<object>(ExecuteAddProduct);
            OpenDocumentCommand = new RelayCommand<object>(ExecuteOpenDocument);
            LoadWarehouses();
        }

        private void LoadWarehouses()
        {
            Warehouses.Clear();
            using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT WarehouseId, WarehouseCode FROM Warehouses", conn);
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
            if (Warehouses.Any())
            {
                SelectedWarehouse = Warehouses.First();
            }
        }

        public void LoadProducts()
        {
            if (SelectedWarehouse == null) return;
            Products.Clear();
            using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
            {
                conn.Open();
                var query = "SELECT p.ProductId, p.Name, p.Quantity, w.WarehouseCode " +
                            "FROM Products p JOIN Warehouses w ON p.WarehouseId = w.WarehouseId " +
                            "WHERE p.WarehouseId = @WarehouseId AND p.Quantity > 0";
                if (!string.IsNullOrEmpty(FilterName))
                    query += " AND p.Name LIKE @FilterName";

                var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@WarehouseId", SelectedWarehouse.WarehouseId);
                if (!string.IsNullOrEmpty(FilterName))
                    cmd.Parameters.AddWithValue("@FilterName", $"%{FilterName}%");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Products.Add(new Product
                        {
                            ProductId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Quantity = reader.GetInt32(2),
                            WarehouseCode = reader.GetString(3)
                        });
                    }
                }
            }
        }

        private void ExecuteFilter(object parameter)
        {
            LoadProducts();
        }

        private void ExecuteAddProduct(object parameter)
        {
            if (_userRole != "Admin")
            {
                MessageBox.Show("Only admins can add products.");
                return;
            }
            if (SelectedWarehouse == null)
            {
                MessageBox.Show("Please select a warehouse before adding a product.");
                return;
            }
            var addProductWindow = new AddProductView { DataContext = new AddProductViewModel(SelectedWarehouse) };
            addProductWindow.ShowDialog();
            LoadProducts();
        }

        private void ExecuteOpenDocument(object parameter)
        {
            if (_userRole != "Admin")
            {
                MessageBox.Show("Only admins can generate documents.");
                return;
            }
            if (SelectedWarehouse == null)
            {
                MessageBox.Show("Please select a warehouse before generating a document.");
                return;
            }
            var documentViewModel = new DocumentViewModel(SelectedWarehouse);
            documentViewModel.OnDocumentGenerated = LoadProducts; // Refresh products after document generation
            var documentWindow = new DocumentView { DataContext = documentViewModel };
            documentWindow.ShowDialog();
        }
    }
}