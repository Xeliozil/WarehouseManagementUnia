using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using WarehouseManagementUnia.Data;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class MainWindow : Window
    {
        private readonly WarehouseDataAccess _dataAccess;

        public MainWindow()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            LoadProducts();
        }

        private void LoadProducts()
        {
            ProductsGrid.ItemsSource = _dataAccess.GetProducts();
        }

        private void AddDelivery_Click(object sender, RoutedEventArgs e)
        {
            var addDeliveryWindow = new AddDeliveryWindow();
            if (addDeliveryWindow.ShowDialog() == true)
            {
                _dataAccess.AddDelivery(addDeliveryWindow.Delivery);
                LoadProducts();
            }
        }

        private void ShowDeliveries_Click(object sender, RoutedEventArgs e)
        {
            var deliveriesWindow = new DeliveriesWindow();
            deliveriesWindow.Show();
        }

        private void ShowAllProducts_Click(object sender, RoutedEventArgs e)
        {
            var allProductsWindow = new AllProductsWindow();
            allProductsWindow.Show();
        }
    }
}