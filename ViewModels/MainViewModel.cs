using System;
using System.Windows;
using System.Windows.Input;
using WarehouseManagementUnia.Views;

namespace WarehouseManagementUnia.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly string _userRole;
        private object _currentView;

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand ShowStockViewCommand { get; }
        public ICommand ShowContractorsViewCommand { get; }
        public ICommand ShowDocumentsViewCommand { get; }

        public MainViewModel(string userRole)
        {
            _userRole = userRole;
            ShowStockViewCommand = new RelayCommand<object>(ShowStockView);
            ShowContractorsViewCommand = new RelayCommand<object>(ShowContractorsView);
            ShowDocumentsViewCommand = new RelayCommand<object>(ShowDocumentsView);
            // Set default view
            ShowStockView(null);
        }

        private void ShowStockView(object parameter)
        {
            try
            {
                CurrentView = new StockView { DataContext = new StockViewModel(_userRole) };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Stock view: {ex.Message}");
            }
        }

        private void ShowContractorsView(object parameter)
        {
            try
            {
                CurrentView = new ContractorsView { DataContext = new ContractorsViewModel(_userRole) };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Contractors view: {ex.Message}");
            }
        }

        private void ShowDocumentsView(object parameter)
        {
            try
            {
                CurrentView = new DocumentsView { DataContext = new DocumentsViewModel(_userRole) };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Documents view: {ex.Message}");
            }
        }
    }
}