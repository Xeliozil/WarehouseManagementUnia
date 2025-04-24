using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
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
        public ICommand WarehouseChangedCommand { get; }
        public ICommand OpenDocumentViewCommand { get; }

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

        public StockViewModel()
        {
            Warehouses = new ObservableCollection<Warehouse>();
            Products = new ObservableCollection<Product>();
            WarehouseChangedCommand = new RelayCommand<object>(ExecuteWarehouseChanged);
            OpenDocumentViewCommand = new RelayCommand<object>(ExecuteOpenDocumentView);
            LoadWarehouses();
        }

        private void LoadWarehouses()
        {
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
            SelectedWarehouse = Warehouses.FirstOrDefault();
        }

        private void LoadProducts()
        {
            if (SelectedWarehouse == null) return;
            Products.Clear();
            using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ProductId, Name, Quantity, Price FROM Products WHERE WarehouseId = @WarehouseId AND Quantity > 0", conn);
                cmd.Parameters.AddWithValue("@WarehouseId", SelectedWarehouse.WarehouseId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Products.Add(new Product
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

        private void ExecuteWarehouseChanged(object parameter)
        {
            LoadProducts();
        }

        private void ExecuteOpenDocumentView(object parameter)
        {
            if (SelectedWarehouse != null)
            {
                var documentViewModel = new DocumentViewModel(SelectedWarehouse);
                var documentView = new DocumentView { DataContext = documentViewModel };
                var window = new Window
                {
                    Title = "Document Management",
                    Content = documentView,
                    Width = 800,
                    Height = 450,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                documentViewModel.OnDocumentGenerated = () => LoadProducts();
                window.ShowDialog();
            }
        }
    }
}