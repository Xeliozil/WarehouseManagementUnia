using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WarehouseManagementUnia.Models;
using WarehouseManagementUnia.Views;

namespace WarehouseManagementUnia.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private UserControl _currentView;
        private Warehouse _defaultWarehouse;
        private string _userRole;

        public UserControl CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand NavigateStockCommand { get; }
        public ICommand NavigateContractorsCommand { get; }
        public ICommand NavigateDocumentsCommand { get; }

        public MainViewModel()
            : this(null)
        {
        }

        public MainViewModel(string userRole)
        {
            _userRole = userRole;
            NavigateStockCommand = new RelayCommand<object>(ExecuteNavigateStock);
            NavigateContractorsCommand = new RelayCommand<object>(ExecuteNavigateContractors);
            NavigateDocumentsCommand = new RelayCommand<object>(ExecuteNavigateDocuments);

            // Load default warehouse
            try
            {
                using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
                {
                    conn.Open();
                    var cmd = new SqlCommand("SELECT TOP 1 WarehouseId, WarehouseCode FROM Warehouses", conn);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            _defaultWarehouse = new Warehouse
                            {
                                WarehouseId = reader.GetInt32(0),
                                WarehouseCode = reader.GetString(1)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading default warehouse: {ex.Message}");
            }

            // Set initial view
            ExecuteNavigateStock(null);
        }

        private void ExecuteNavigateStock(object parameter)
        {
            CurrentView = new StockView { DataContext = new StockViewModel(_defaultWarehouse, this) };
        }

        private void ExecuteNavigateContractors(object parameter)
        {
            CurrentView = new ContractorsView { DataContext = new ContractorsViewModel(_userRole) };
        }

        private void ExecuteNavigateDocuments(object parameter)
        {
            CurrentView = new DocumentsView { DataContext = new DocumentsViewModel(_userRole) };
        }
    }
}