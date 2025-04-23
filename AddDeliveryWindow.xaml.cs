using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WarehouseManagementUnia.Data;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class AddDeliveryWindow : Window
    {
        private readonly WarehouseDataAccess _dataAccess;
        public event EventHandler TransactionAdded;

        public AddDeliveryWindow()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            ProductComboBox.ItemsSource = _dataAccess.GetProductsForSelection();
            ContractorComboBox.ItemsSource = _dataAccess.GetContractors();
            DeliveryDatePicker.SelectedDate = DateTime.Today;
        }

        public void ApplySettings()
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                string username = mainWindow.GetUsername();
                string fontColor = Properties.Settings.Default[$"FontColor_{username}"].ToString() ?? "Black";
                double fontSize = Properties.Settings.Default[$"FontSize_{username}"] != null ? (double)Properties.Settings.Default[$"FontSize_{username}"] : 14;

                // Ustaw rekurencyjnie dla wszystkich elementów w widoku
                ApplySettingsToElement(this, fontColor, fontSize);
            }
        }

        private void ApplySettingsToElement(DependencyObject element, string fontColor, double fontSize)
        {
            if (element == null) return;

            if (element is FrameworkElement fe)
            {
                fe.SetValue(Control.ForegroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString(fontColor)));
                fe.SetValue(Control.FontSizeProperty, fontSize);
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                ApplySettingsToElement(child, fontColor, fontSize);
            }
        }

        private void AddDelivery_Click(object sender, RoutedEventArgs e)
        {
            if (ProductComboBox.SelectedItem == null)
            {
                MessageBox.Show("Wybierz produkt.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ContractorComboBox.SelectedItem == null)
            {
                MessageBox.Show("Wybierz kontrahenta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Podaj poprawną ilość (liczba większa od 0).", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!DeliveryDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Wybierz datę dostawy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var delivery = new Delivery
                {
                    ProductId = ((Product)ProductComboBox.SelectedItem).Id,
                    ContractorId = ((Contractor)ContractorComboBox.SelectedItem).Id,
                    Quantity = quantity,
                    DeliveryDate = DeliveryDatePicker.SelectedDate.Value,
                    Description = DescriptionTextBox.Text
                };

                _dataAccess.AddDelivery(delivery);
                MessageBox.Show("Dostawa dodana pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                TransactionAdded?.Invoke(this, EventArgs.Empty);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas dodawania dostawy: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}