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
        private readonly ActiveProductsView _activeProductsView;
        private readonly DeliveriesView _deliveriesView;
        private readonly AllProductsView _allProductsView;

        public MainWindow()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            _activeProductsView = new ActiveProductsView();
            _deliveriesView = new DeliveriesView();
            _allProductsView = new AllProductsView();
            _allProductsView.ProductStatusChanged += RefreshActiveProducts;
            MainContent.Content = _activeProductsView;
        }

        private void ShowActiveProducts_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _activeProductsView;
        }

        private void ShowDeliveries_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _deliveriesView;
        }

        private void ShowAllProducts_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _allProductsView;
        }

        private void AddDelivery_Click(object sender, RoutedEventArgs e)
        {
            var addDeliveryWindow = new AddDeliveryWindow();
            if (addDeliveryWindow.ShowDialog() == true)
            {
                _dataAccess.AddDelivery(addDeliveryWindow.Delivery);
                _activeProductsView.LoadProducts();
            }
        }

        private void RefreshActiveProducts()
        {
            _activeProductsView.LoadProducts();
        }
    }
}