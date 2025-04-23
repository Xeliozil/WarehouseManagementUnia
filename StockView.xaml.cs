using System.Windows;
using System.Windows.Controls;
using WarehouseManagementUnia.Data;

namespace WarehouseManagementUnia
{
    public partial class StockView : UserControl
    {
        private readonly WarehouseDataAccess _dataAccess;
        private readonly string _userRole;

        public StockView(string userRole)
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            _userRole = userRole;
            LoadProducts();
        }

        private void LoadProducts()
        {
            ProductsDataGrid.ItemsSource = _dataAccess.GetProducts();
        }

        private void AllProducts_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.MainContent.Content = new AllProductsView(_userRole);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.ShowMainMenu();
            }
        }
    }
}