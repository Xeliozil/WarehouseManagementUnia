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

        private void LoadProducts()
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
            }
        }

        private void ToggleActive_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is Product selectedProduct)
            {
                _dataAccess.SetProductActiveStatus(selectedProduct.Id, !selectedProduct.IsActive);
                LoadProducts();
                ProductStatusChanged?.Invoke();
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is Product selectedProduct)
            {
                bool deleted = _dataAccess.DeleteProduct(selectedProduct.Id);
                if (deleted)
                {
                    LoadProducts();
                    ProductStatusChanged?.Invoke();
                }
                else
                {
                    MessageBox.Show("Nie można usunąć produktu, ponieważ jest powiązany z dostawami.", "Błąd usuwania", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}   