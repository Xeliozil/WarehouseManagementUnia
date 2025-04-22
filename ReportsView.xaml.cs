using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using WarehouseManagementUnia.Data;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class ReportsView : UserControl
    {
        private readonly WarehouseDataAccess _dataAccess;

        public ReportsView()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            ContractorComboBox.ItemsSource = _dataAccess.GetContractors();
            StartDatePicker.SelectedDate = DateTime.Today.AddMonths(-1);
            EndDatePicker.SelectedDate = DateTime.Today;
        }

        private void GeneratePdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var transactions = GetFilteredTransactions();
                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var font = new XFont("Verdana", 12);
                var titleFont = new XFont("Verdana", 16, XFontStyle.Bold);
                var y = XUnit.FromPoint(50);

                gfx.DrawString("Raport transakcji", titleFont, XBrushes.Black,
                    new XRect(XUnit.FromPoint(0), y, XUnit.FromPoint(page.Width.Point), XUnit.FromPoint(20)),
                    XStringFormats.TopCenter);
                y += XUnit.FromPoint(40);

                foreach (var transaction in transactions)
                {
                    gfx.DrawString($"{transaction.Type} | ID: {transaction.Id} | Produkt: {transaction.ProductName} | NIP: {transaction.ContractorNIP} | Ilość: {transaction.Quantity} | Data: {transaction.Date:yyyy-MM-dd} | {transaction.Description}",
                        font, XBrushes.Black,
                        new XRect(XUnit.FromPoint(50), y, XUnit.FromPoint(page.Width.Point - 100), XUnit.FromPoint(20)),
                        XStringFormats.TopLeft);
                    y += XUnit.FromPoint(20);
                }

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF Files|*.pdf",
                    DefaultExt = ".pdf",
                    FileName = "Raport.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    document.Save(saveFileDialog.FileName);
                    MessageBox.Show("Raport PDF wygenerowany pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas generowania PDF: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateCsv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var transactions = GetFilteredTransactions();
                var csvLines = new List<string> { "Typ,ID,Produkt,NIP kontrahenta,Ilość,Data,Opis" };
                csvLines.AddRange(transactions.Select(t => $"{t.Type},{t.Id},{t.ProductName},{t.ContractorNIP},{t.Quantity},{t.Date:yyyy-MM-dd},{t.Description.Replace(",", "")}"));

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV Files|*.csv",
                    DefaultExt = ".csv",
                    FileName = "Raport.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllLines(saveFileDialog.FileName, csvLines);
                    MessageBox.Show("Raport CSV wygenerowany pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas generowania CSV: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<Transaction> GetFilteredTransactions()
        {
            var transactions = new List<Transaction>();
            var deliveries = _dataAccess.GetDeliveries();
            var issues = _dataAccess.GetIssues();

            transactions.AddRange(deliveries.Select(d => new Transaction
            {
                Type = "Dostawa",
                Id = d.Id,
                ProductId = d.ProductId,
                ProductName = d.ProductName,
                ContractorNIP = d.ContractorNIP ?? "",
                Quantity = d.Quantity,
                Date = d.DeliveryDate,
                Description = d.Description ?? ""
            }));

            transactions.AddRange(issues.Select(i => new Transaction
            {
                Type = "Wydanie",
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ContractorNIP = i.ContractorNIP ?? "",
                Quantity = i.Quantity,
                Date = i.IssueDate,
                Description = i.Description ?? ""
            }));

            var filteredTransactions = transactions.AsQueryable();

            if (ContractorComboBox.SelectedItem is Contractor selectedContractor)
            {
                filteredTransactions = filteredTransactions.Where(t => t.ContractorNIP == selectedContractor.NIP);
            }

            if (StartDatePicker.SelectedDate.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t => t.Date >= StartDatePicker.SelectedDate.Value);
            }

            if (EndDatePicker.SelectedDate.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t => t.Date <= EndDatePicker.SelectedDate.Value);
            }

            return filteredTransactions.OrderBy(t => t.Date).ToList();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.ShowMainMenu();
            }
        }
    }
}