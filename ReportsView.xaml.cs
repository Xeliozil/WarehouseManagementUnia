using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
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
            StartDatePicker.SelectedDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
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
                var titleFont = new XFont("Verdana", 16, XFontStyle.Bold);
                var regularFont = new XFont("Verdana", 12);
                var footerFont = new XFont("Verdana", 10);
                var textFormatter = new XTextFormatter(gfx);

                // Tytuł z zakresem dat
                string dateRange = $"{StartDatePicker.SelectedDate?.ToString("dd.MM.yyyy") ?? "Brak"} - {EndDatePicker.SelectedDate?.ToString("dd.MM.yyyy") ?? "Brak"}";
                string title = $"Raport transakcji ({dateRange})";
                gfx.DrawString(title, titleFont, XBrushes.Black,
                    new XRect(XUnit.FromPoint(0), XUnit.FromPoint(30), XUnit.FromPoint(page.Width.Point), XUnit.FromPoint(20)),
                    XStringFormats.TopCenter);

                // Pozioma kreska
                var pen = new XPen(XColors.Black, 1);
                gfx.DrawLine(pen, XUnit.FromPoint(50), XUnit.FromPoint(60), XUnit.FromPoint(page.Width.Point - 50), XUnit.FromPoint(60));

                // Oblicz szerokości kolumn
                double[] columnWidths = new double[7]; // Typ, ID, Produkt, NIP, Ilość, Data, Opis
                string[] headers = { "Typ", "ID", "Produkt", "NIP kontrahenta", "Ilość", "Data", "Opis" };

                // Początkowe szerokości na podstawie nagłówków
                for (int i = 0; i < headers.Length; i++)
                {
                    columnWidths[i] = gfx.MeasureString(headers[i], regularFont).Width;
                }

                // Sprawdź szerokości na podstawie danych
                foreach (var transaction in transactions)
                {
                    string[] rowData = new string[]
                    {
                        transaction.Type,
                        transaction.Id.ToString(),
                        transaction.ProductName,
                        transaction.ContractorNIP,
                        transaction.Quantity.ToString(),
                        transaction.Date.ToString("yyyy-MM-dd"),
                        transaction.Description
                    };

                    for (int i = 0; i < rowData.Length; i++)
                    {
                        double width = gfx.MeasureString(rowData[i], regularFont).Width;
                        if (i == 6) // Kolumna Opis - ograniczamy maksymalną szerokość, bo będzie zawijana
                        {
                            width = Math.Min(width, 150); // Maksymalnie 150 pt dla Opisu
                        }
                        columnWidths[i] = Math.Max(columnWidths[i], width);
                    }
                }

                // Dodaj odstęp między kolumnami (10 pt)
                for (int i = 0; i < columnWidths.Length; i++)
                {
                    columnWidths[i] += 10;
                }

                // Oblicz pozycje kolumn
                double[] columnPositions = new double[7];
                double currentX = 50; // Margines lewy
                for (int i = 0; i < columnWidths.Length; i++)
                {
                    columnPositions[i] = currentX;
                    currentX += columnWidths[i];
                }

                // Nagłówki kolumn
                var y = XUnit.FromPoint(80);
                for (int i = 0; i < headers.Length; i++)
                {
                    gfx.DrawString(headers[i], regularFont, XBrushes.Black, XUnit.FromPoint(columnPositions[i]), y);
                }
                y += XUnit.FromPoint(20);

                // Wiersze danych
                foreach (var transaction in transactions)
                {
                    double rowHeight = 20; // Domyślna wysokość wiersza

                    // Oblicz wysokość dla kolumny Opis z zawijaniem
                    double descriptionWidth = page.Width.Point - columnPositions[6] - 50; // Dostępna przestrzeń do prawego marginesu
                    string descriptionText = transaction.Description;
                    var descriptionRect = new XRect(columnPositions[6], y, descriptionWidth, 100); // Duża wysokość na zawijanie
                    textFormatter.Alignment = XParagraphAlignment.Left;
                    var descriptionSize = gfx.MeasureString(descriptionText, regularFont);
                    if (descriptionSize.Width > descriptionWidth)
                    {
                        // Oblicz potrzebną wysokość po zawinięciu
                        int approxLines = (int)Math.Ceiling(descriptionSize.Width / descriptionWidth);
                        rowHeight = Math.Max(rowHeight, approxLines * 20);
                    }

                    // Rysuj dane
                    gfx.DrawString(transaction.Type, regularFont, XBrushes.Black, XUnit.FromPoint(columnPositions[0]), y);
                    gfx.DrawString(transaction.Id.ToString(), regularFont, XBrushes.Black, XUnit.FromPoint(columnPositions[1]), y);
                    gfx.DrawString(transaction.ProductName, regularFont, XBrushes.Black, XUnit.FromPoint(columnPositions[2]), y);
                    gfx.DrawString(transaction.ContractorNIP, regularFont, XBrushes.Black, XUnit.FromPoint(columnPositions[3]), y);
                    gfx.DrawString(transaction.Quantity.ToString(), regularFont, XBrushes.Black, XUnit.FromPoint(columnPositions[4]), y);
                    gfx.DrawString(transaction.Date.ToString("yyyy-MM-dd"), regularFont, XBrushes.Black, XUnit.FromPoint(columnPositions[5]), y);

                    // Zawijanie tekstu w kolumnie Opis
                    descriptionRect = new XRect(columnPositions[6], y, descriptionWidth, rowHeight);
                    textFormatter.DrawString(descriptionText, regularFont, XBrushes.Black, descriptionRect);

                    y += XUnit.FromPoint(rowHeight);
                }

                // Stopka firmy
                string footer = "Firma Unia, ul. Zwycięstwa 123, 75-900 Koszalin, NIP: 1234567890";
                gfx.DrawString(footer, footerFont, XBrushes.Black,
                    new XRect(XUnit.FromPoint(0), XUnit.FromPoint(page.Height.Point - 30), XUnit.FromPoint(page.Width.Point), XUnit.FromPoint(20)),
                    XStringFormats.BottomCenter);

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF Files|*.pdf",
                    DefaultExt = ".pdf",
                    FileName = $"Raport_{DateTime.Now:yyyyMMdd}.pdf"
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
                    FileName = $"Raport_{DateTime.Now:yyyyMMdd}.csv"
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