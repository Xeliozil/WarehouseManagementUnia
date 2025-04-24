using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WarehouseManagementUnia.Utilities;
using WarehouseManagementUnia.Views;

namespace WarehouseManagementUnia.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private object _currentView;
        private string _currentViewTitle;

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

        public ICommand ShowInventoryViewCommand { get; }
        public ICommand ShowContractorsViewCommand { get; }
        public ICommand ShowDocumentsViewCommand { get; }
        public ICommand ShowEmptyViewCommand { get; }

        public MainViewModel()
        {
            ShowInventoryViewCommand = new RelayCommand(o => ShowInventoryView());
            ShowContractorsViewCommand = new RelayCommand(o => ShowContractorsView());
            ShowDocumentsViewCommand = new RelayCommand(o => ShowDocumentsView());
            ShowEmptyViewCommand = new RelayCommand(o => ShowEmptyView());

            // Domyślny widok
            ShowInventoryView();
        }

        private void ShowInventoryView()
        {
            CurrentView = new InventoryView();
            CurrentViewTitle = "Stan Magazynowy";
        }

        private void ShowContractorsView()
        {
            CurrentView = new ContractorsView();
            CurrentViewTitle = "Kontrahenci";
        }

        private void ShowDocumentsView()
        {
            CurrentView = new DocumentsView();
            CurrentViewTitle = "Dokumenty";
        }

        private void ShowEmptyView()
        {
            CurrentView = new EmptyView();
            CurrentViewTitle = "Pusty Widok";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
