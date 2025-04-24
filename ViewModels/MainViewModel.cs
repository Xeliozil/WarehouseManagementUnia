using System.Windows.Input;
using WarehouseManagementUnia.Models;
using WarehouseManagementUnia.Views;

namespace WarehouseManagementUnia.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private object _currentView;
        private StockViewModel _stockViewModel;
        private ContractorsViewModel _contractorsViewModel;

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand ShowStockViewCommand { get; }
        public ICommand ShowContractorsViewCommand { get; }

        public MainViewModel()
        {
            _stockViewModel = new StockViewModel();
            _contractorsViewModel = new ContractorsViewModel();
            CurrentView = new StockView { DataContext = _stockViewModel };

            ShowStockViewCommand = new RelayCommand<object>(ExecuteShowStockView);
            ShowContractorsViewCommand = new RelayCommand<object>(ExecuteShowContractorsView);
        }

        private void ExecuteShowStockView(object parameter)
        {
            CurrentView = new StockView { DataContext = _stockViewModel };
        }

        private void ExecuteShowContractorsView(object parameter)
        {
            CurrentView = new ContractorsView { DataContext = _contractorsViewModel };
        }
    }
}