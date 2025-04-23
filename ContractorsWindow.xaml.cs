using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WarehouseManagementUnia.Data;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class ContractorsWindow : Window
    {
        private readonly WarehouseDataAccess _dataAccess;

        public ContractorsWindow()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            RefreshContractors();
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

        private void RefreshContractors()
        {
            ContractorsDataGrid.ItemsSource = _dataAccess.GetContractors();
        }

        private void AddContractor_Click(object sender, RoutedEventArgs e)
        {
            var addContractorWindow = new AddContractorWindow();
            addContractorWindow.ShowDialog();
            RefreshContractors();
        }

        private void DeleteContractor_Click(object sender, RoutedEventArgs e)
        {
            if (ContractorsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Wybierz kontrahenta do usunięcia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var contractor = (Contractor)ContractorsDataGrid.SelectedItem;
            try
            {
                _dataAccess.DeleteContractor(contractor.Id);
                MessageBox.Show("Kontrahent usunięty pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshContractors();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas usuwania kontrahenta: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}