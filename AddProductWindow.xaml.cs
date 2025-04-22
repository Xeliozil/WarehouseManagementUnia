using System;
using System.Windows;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class AddProductWindow : Window
    {
        public Product Product { get; private set; }

        public AddProductWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                !int.TryParse(QuantityTextBox.Text, out int quantity) ||
                quantity < 0 ||
                !decimal.TryParse(PriceTextBox.Text, out decimal price) ||
                price < 0)
            {
                MessageBox.Show("Wprowadź poprawne wartości: nazwa, ilość (nieujemna), cena (nieujemna).", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Product = new Product
            {
                Name = NameTextBox.Text,
                Quantity = quantity,
                Price = price,
                IsActive = quantity > 0
            };
            DialogResult = true;
            Close();
        }
    }
}