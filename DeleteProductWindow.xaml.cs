using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WarehouseManagementUnia.Data;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class DeleteProductWindow : Window
    {
        private readonly WarehouseDataAccess _dataAccess;
        private Product _selectedProduct;
        public event EventHandler ProductDeleted;

        public DeleteProductWindow()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            LoadProducts();
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

        private void LoadProducts()
        {
            var products = _dataAccess.GetProducts();
            ProductComboBox.ItemsSource = products.Select(p => new { Display = $"{p.Name} (ID: {p.Id})", Product = p }).ToList();
            ProductComboBox.DisplayMemberPath = "Display";
        }

        private void DeleteWithHistory_Click(object sender, RoutedEventArgs e)
        {
            if (ProductComboBox.SelectedItem == null)
            {
                MessageBox.Show("Wybierz produkt do usunięcia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _selectedProduct = ((dynamic)ProductComboBox.SelectedItem).Product;
            ConfirmationLabel.Visibility = Visibility.Visible;
            ConfirmationTextBox.Visibility = Visibility.Visible;
            ConfirmDeleteButton.Visibility = Visibility.Visible;
            ProductComboBox.IsEnabled = false;
        }

        private void CancelDelete_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmationTextBox.Text != _selectedProduct.Name)
            {
                MessageBox.Show("Nazwa produktu nie zgadza się. Usuwanie anulowane.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                _dataAccess.DeleteProductWithHistory(_selectedProduct.Id);
                MessageBox.Show("Produkt i jego historia zostały usunięte.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                ProductDeleted?.Invoke(this, EventArgs.Empty);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas usuwania produktu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}