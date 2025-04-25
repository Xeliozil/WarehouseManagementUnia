using System.Windows;
using WarehouseManagementUnia.ViewModels;

namespace WarehouseManagementUnia
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        public MainWindow(string userRole)
        {
            InitializeComponent();
            DataContext = new MainViewModel(userRole);
        }
    }
}