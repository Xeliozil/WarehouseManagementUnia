using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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