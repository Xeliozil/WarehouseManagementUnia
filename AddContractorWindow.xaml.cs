using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WarehouseManagementUnia.Data;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class AddContractorWindow : Window
    {
        private readonly WarehouseDataAccess _dataAccess;
        public event EventHandler ContractorAdded;

        public AddContractorWindow()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
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

        private void AddContractor_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Podaj nazwę kontrahenta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(NIPTextBox.Text) || NIPTextBox.Text.Length != 10 || !long.TryParse(NIPTextBox.Text, out _))
            {
                MessageBox.Show("Podaj poprawny NIP (10 cyfr).", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var contractor = new Contractor
                {
                    Name = NameTextBox.Text,
                    Address = AddressTextBox.Text,
                    NIP = NIPTextBox.Text
                };

                _dataAccess.AddContractor(contractor);
                MessageBox.Show("Kontrahent dodany pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                ContractorAdded?.Invoke(this, EventArgs.Empty);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas dodawania kontrahenta: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}