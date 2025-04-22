using System;
using System.Windows;
using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using WarehouseManagementUnia.Data;

namespace WarehouseManagementUnia
{
    public partial class ReportWindow : Window
    {
        private readonly WarehouseDataAccess _dataAccess;

        public ReportWindow()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            InitializeReport();
        }

        private void InitializeReport()
        {
            int year = DateTime.Today.Year;
            int reportCount = _dataAccess.GetReportCount(year) + 1;
            ReportTitleTextBox.Text = $"{year}/{reportCount:D3}";
            StartDatePicker.SelectedDate = new DateTime(year, DateTime.Today.Month, 1);
            EndDatePicker.SelectedDate = DateTime.Today;
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Wybierz daty początkową i końcową.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime startDate = StartDatePicker.SelectedDate.Value;
            DateTime endDate = EndDatePicker.SelectedDate.Value;
            if (startDate > endDate)
            {
                MessageBox.Show("Data początkowa musi być wcześniejsza niż końcowa.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Pliki PDF (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                FileName = $"Raport_{ReportTitleTextBox.Text.Replace('/', '-')}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _dataAccess.GeneratePdfReport(ReportTitleTextBox.Text, startDate, endDate, saveFileDialog.FileName);
                    _dataAccess.IncrementReportCount(DateTime.Today.Year);
                    MessageBox.Show("Raport został wygenerowany.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas generowania raportu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}