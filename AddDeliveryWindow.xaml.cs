using System;
using System.Windows;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class AddDeliveryWindow : Window
    {
        public Delivery Delivery { get; private set; }

        public AddDeliveryWindow()
        {
            InitializeComponent();
            DeliveryDateTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd"); // Default to today
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ProductIdTextBox.Text, out int productId) ||
                !int.TryParse(QuantityTextBox.Text, out int quantity) ||
                !DateTime.TryParse(DeliveryDateTextBox.Text, out DateTime deliveryDate))
            {
                MessageBox.Show("Wprowadź poprawne wartości dla ID produktu, ilości i daty dostawy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check for future dates
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

            Delivery = new Delivery
            {
                ProductId = productId,
                Quantity = quantity,
                DeliveryDate = deliveryDate
            };
            DialogResult = true;
            Close();
        }
    }
}