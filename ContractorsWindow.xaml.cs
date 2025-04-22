using System;
using System.Windows;
using System.Windows.Controls;
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