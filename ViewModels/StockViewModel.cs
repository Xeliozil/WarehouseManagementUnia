using System;
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
        private readonly MainViewModel _mainViewModel;

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

        public ICommand GenerateDocumentCommand { get; }
        public ICommand AddWarehouseCommand { get; }

        public StockViewModel()
            : this(null, null)
        {
        }

        public StockViewModel(Warehouse defaultWarehouse, MainViewModel mainViewModel = null)
        {
            _mainViewModel = mainViewModel;
            Warehouses = new ObservableCollection<Warehouse>();
            Products = new ObservableCollection<Product>();
            GenerateDocumentCommand = new RelayCommand<object>(ExecuteGenerateDocument);
            AddWarehouseCommand = new RelayCommand<object>(ExecuteAddWarehouse);
            LoadWarehouses();
            SelectedWarehouse = defaultWarehouse ?? Warehouses.FirstOrDefault();
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
        }

        private void LoadProducts()
        {
            Products.Clear();
            if (SelectedWarehouse == null) return;
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

        private void ExecuteGenerateDocument(object parameter)
        {
            if (SelectedWarehouse != null)
            {
                if (_mainViewModel != null)
                {
                    _mainViewModel.CurrentView = new DocumentView(SelectedWarehouse)
                    {
                        DataContext = new DocumentViewModel(SelectedWarehouse)
                    };
                }
                else
                {
                    // Fallback: Open in a new window
                    var window = new Window
                    {
                        Title = "Generate Document",
                        Content = new DocumentView(SelectedWarehouse),
                        Width = 800,
                        Height = 600,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };
                    window.Show();
                }
            }
        }

        private void ExecuteAddWarehouse(object parameter)
        {
            var inputDialog = new InputDialog("Enter Warehouse Code:");
            if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputDialog.InputText))
            {
                try
                {
                    using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
                    {
                        conn.Open();
                        var cmd = new SqlCommand("INSERT INTO Warehouses (WarehouseCode) VALUES (@WarehouseCode); SELECT SCOPE_IDENTITY();", conn);
                        cmd.Parameters.AddWithValue("@WarehouseCode", inputDialog.InputText);
                        var warehouseId = Convert.ToInt32(cmd.ExecuteScalar());

                        Warehouses.Add(new Warehouse
                        {
                            WarehouseId = warehouseId,
                            WarehouseCode = inputDialog.InputText
                        });
                        MessageBox.Show($"Warehouse '{inputDialog.InputText}' added successfully.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding warehouse: {ex.Message}");
                }
            }
        }
    }
}