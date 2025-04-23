using System.Windows;
using System.Windows.Controls;
using WarehouseManagementUnia.Data;

namespace WarehouseManagementUnia
{
    public partial class AllProductsView : UserControl
    {
        private readonly WarehouseDataAccess _dataAccess;

        public AllProductsView()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            LoadProducts();
        }

        private void LoadProducts()
        {
            AllProductsDataGrid.ItemsSource = _dataAccess.GetAllProducts();
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var addProductWindow = new AddProductWindow();
            addProductWindow.ProductAdded += (s, args) => LoadProducts();
            addProductWindow.ShowDialog();
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            var deleteProductWindow = new DeleteProductWindow();
            deleteProductWindow.ProductDeleted += (s, args) => LoadProducts();
            deleteProductWindow.ShowDialog();
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