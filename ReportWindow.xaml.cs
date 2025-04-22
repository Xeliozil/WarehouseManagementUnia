using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using WarehouseManagementUnia.Data;

namespace WarehouseManagementUnia
{
    public partial class ReportWindow : Window
    {
        private readonly WarehouseDataAccess _dataAccess;
        private readonly int _currentYear = DateTime.Now.Year; // Dynamicznie pobierany rok, np. 2025

        public ReportWindow()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();

            ReportTitleTextBox.Text = _dataAccess.GetReportCount(_currentYear).ToString("2025/000");
            StartDatePicker.SelectedDate = DateTime.Today.AddMonths(-1);
            EndDatePicker.SelectedDate = DateTime.Today;

            ContractorComboBox.ItemsSource = _dataAccess.GetContractors();
            ContractorComboBox.Items.Insert(0, new { Id = 0, Name = "Wszystkie" });
            ContractorComboBox.SelectedIndex = 0;

            ProductComboBox.ItemsSource = _dataAccess.GetProducts();
            ProductComboBox.Items.Insert(0, new { Id = 0, Name = "Wszystkie" });
            ProductComboBox.SelectedIndex = 0;

            TransactionTypeComboBox.SelectedIndex = 0;
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Proszę wybrać daty.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (StartDatePicker.SelectedDate > EndDatePicker.SelectedDate)
            {
                MessageBox.Show("Data początkowa nie może być późniejsza niż data końcowa.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = FormatComboBox.SelectedIndex == 0 ? "PDF files (*.pdf)|*.pdf" : "CSV files (*.csv)|*.csv",
                FileName = $"Raport_{ReportTitleTextBox.Text.Replace("/", "_")}"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    int? contractorId = ContractorComboBox.SelectedIndex == 0 ? null : (ContractorComboBox.SelectedItem as dynamic).Id;
                    int? productId = ProductComboBox.SelectedIndex == 0 ? null : (ProductComboBox.SelectedItem as dynamic).Id;
                    string transactionType = TransactionTypeComboBox.SelectedIndex == 0 ? null : (TransactionTypeComboBox.SelectedItem as ComboBoxItem).Content.ToString();

                    _dataAccess.GenerateReport(
                        ReportTitleTextBox.Text,
                        StartDatePicker.SelectedDate.Value,
                        EndDatePicker.SelectedDate.Value,
                        saveFileDialog.FileName,
                        FormatComboBox.SelectedIndex == 0 ? "PDF" : "CSV",
                        contractorId,
                        productId,
                        transactionType
                    );

                    _dataAccess.IncrementReportCount(_currentYear);
                    ReportTitleTextBox.Text = _dataAccess.GetReportCount(_currentYear).ToString("2025/000");
                    MessageBox.Show("Raport został wygenerowany.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas generowania raportu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}