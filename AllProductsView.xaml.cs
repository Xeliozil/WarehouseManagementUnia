using System;
using System.Windows;
using System.Windows.Controls;
using WarehouseManagementUnia.Data;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class AllProductsView : UserControl
    {
        private readonly WarehouseDataAccess _dataAccess;
        public event Action ProductStatusChanged;

        public AllProductsView()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            LoadProducts();
        }

        public void LoadProducts()
        {
            ProductsGrid.ItemsSource = _dataAccess.GetAllProducts();
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddProductWindow();
            if (addWindow.ShowDialog() == true)
            {
                _dataAccess.AddProduct(addWindow.Product);
                LoadProducts();
                ProductStatusChanged?.Invoke();
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is Product selectedProduct)
            {
                var result = MessageBox.Show(
                    "Czy chcesz usunąć historię dostaw związanych z tym przedmiotem?",
                    "Potwierdzenie usunięcia",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _dataAccess.HardDeleteProduct(selectedProduct.Id);
                    LoadProducts();
                    ProductStatusChanged?.Invoke();
                }
                else if (result == MessageBoxResult.No)
                {
                    _dataAccess.SoftDeleteProduct(selectedProduct.Id);
                    LoadProducts();
                    ProductStatusChanged?.Invoke();
                }
                // Cancel nic nie robi
            }
        }
    }
}