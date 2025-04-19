using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WarehouseManagementUnia.Data;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class AllProductsWindow : Window
    {
        private readonly WarehouseDataAccess _dataAccess;

        public AllProductsWindow()
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

        private void DeactivateProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is Product selectedProduct)
            {
                _dataAccess.SetProductInactive(selectedProduct.Id);
                LoadProducts();
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
                }
                else
                {
                    MessageBox.Show("Nie można usunąć produktu, ponieważ jest powiązany z dostawami.", "Błąd usuwania", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}
