using System.Windows.Input;
using WarehouseManagementUnia.Views;


namespace WarehouseManagementUnia.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private object _currentView;
        private string _currentViewTitle;
        private readonly string _userRole;

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public string CurrentViewTitle
        {
            get => _currentViewTitle;
            set { _currentViewTitle = value; OnPropertyChanged(); }
        }

        public ICommand NavigateCommand { get; }

        public MainViewModel(string userRole)
        {
            _userRole = userRole;
            NavigateCommand = new RelayCommand<string>(Navigate);
            Navigate("Stock"); // Default view
        }

        private void Navigate(string viewName)
        {
            switch (viewName)
            {
                case "Stock":
                    CurrentView = new StockView { DataContext = new StockViewModel(_userRole) };
                    CurrentViewTitle = "Stock Status";
                    break;
                case "Contractors":
                    CurrentView = new ContractorsView { DataContext = new ContractorsViewModel(_userRole) };
                    CurrentViewTitle = "Contractors";
                    break;
                case "Documents":
                    CurrentView = new DocumentsView { DataContext = new DocumentsViewModel(_userRole) };
                    CurrentViewTitle = "Documents";
                    break;
                case "Empty":
                    CurrentView = new EmptyView();
                    CurrentViewTitle = "Empty View";
                    break;
            }
        }
    }
}