using System;
using System.Windows;
using WarehouseManagementUnia.Data;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class AddProductWindow : Window
    {
        private readonly WarehouseDataAccess _dataAccess;
        public event EventHandler ProductAdded;

        public AddProductWindow()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Podaj nazwę produktu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var product = new Product
                {
                    Id = _dataAccess.GetNextProductId(),
                    Name = NameTextBox.Text,
                    Price = 0,
                    Quantity = 0
                };

                _dataAccess.AddProduct(product);
                MessageBox.Show("Produkt dodany pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                ProductAdded?.Invoke(this, EventArgs.Empty);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas dodawania produktu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}