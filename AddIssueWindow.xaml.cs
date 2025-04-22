using System;
using System.Windows;
using System.Linq;
using WarehouseManagementUnia.Data;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class AddIssueWindow : Window
    {
        public Issue Issue { get; private set; }
        private readonly WarehouseDataAccess _dataAccess;

        public AddIssueWindow()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            IssueDateTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
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
                !DateTime.TryParse(IssueDateTextBox.Text, out DateTime issueDate))
            {
                MessageBox.Show("Wybierz produkt, wprowadź poprawną ilość (większą od 0) i datę wydania.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (issueDate > DateTime.Today)
            {
                var result = MessageBox.Show(
                    "Data wydania jest w przyszłości. Czy na pewno chcesz kontynuować?",
                    "Ostrzeżenie",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            var selectedProduct = (Product)ProductComboBox.SelectedItem;
            Issue = new Issue
            {
                ProductId = selectedProduct.Id,
                Quantity = quantity,
                IssueDate = issueDate,
                Description = string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ? null : DescriptionTextBox.Text
            };
            DialogResult = true;
            Close();
        }
    }
}