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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using WarehouseManagementUnia.Data;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class AllProductsView : UserControl
    {
        private readonly WarehouseDataAccess _dataAccess;
        public event Action ProductStatusChanged; // Event to notify main window

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
                ProductStatusChanged?.Invoke(); // Notify main window
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
                    ProductStatusChanged?.Invoke(); // Notify main window
                }
                else
                {
                    MessageBox.Show("Nie można usunąć produktu, ponieważ jest powiązany z dostawami.", "Błąd usuwania", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}
