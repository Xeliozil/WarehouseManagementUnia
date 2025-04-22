using System;
using System.Windows;
using System.Linq;
using WarehouseManagementUnia.Data;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class AddDeliveryWindow : Window
    {
        public Delivery Delivery { get; private set; }
        private readonly WarehouseDataAccess _dataAccess;

        public AddDeliveryWindow()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            DeliveryDateTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
            var products = _dataAccess.GetProductsForSelection();
            if (!products.Any())
            {
                MessageBox.Show("Brak produktów w bazie. Dodaj produkt w widoku 'Wszystkie produkty'.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
                return;
            }
            ProductComboBox.ItemsSource = products;
            ProductComboBox.SelectedIndex = 0;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (ProductComboBox.SelectedItem == null ||
                !int.TryParse(QuantityTextBox.Text, out int quantity) ||
                quantity <= 0 ||
                !DateTime.TryParse(DeliveryDateTextBox.Text, out DateTime deliveryDate))
            {
                MessageBox.Show("Wybierz produkt, wprowadź poprawną ilość (większą od 0) i datę dostawy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (deliveryDate > DateTime.Today)
            {
                var result = MessageBox.Show(
                    "Data dostawy jest w przyszłości. Czy na pewno chcesz kontynuować?",
                    "Ostrzeżenie",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            var selectedProduct = (Product)ProductComboBox.SelectedItem;
            Delivery = new Delivery
            {
                ProductId = selectedProduct.Id,
                Quantity = quantity,
                DeliveryDate = deliveryDate,
                Description = string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ? null : DescriptionTextBox.Text
            };
            DialogResult = true;
            Close();
        }
    }
}