using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WarehouseManagementUnia.Data;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class DeliveriesIssuesView : Window
    {
        private readonly WarehouseDataAccess _dataAccess;
        public event EventHandler TransactionAdded;

        public DeliveriesIssuesView()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            DeliveryProductComboBox.ItemsSource = _dataAccess.GetProductsForSelection();
            DeliveryContractorComboBox.ItemsSource = _dataAccess.GetContractors();
            IssueProductComboBox.ItemsSource = _dataAccess.GetProductsForSelection();
            IssueContractorComboBox.ItemsSource = _dataAccess.GetContractors();
            DeliveryDatePicker.SelectedDate = DateTime.Today;
            IssueDatePicker.SelectedDate = DateTime.Today;
            LoadTransactions();
        }

        private void AddDelivery_Click(object sender, RoutedEventArgs e)
        {
            if (DeliveryProductComboBox.SelectedItem == null)
            {
                MessageBox.Show("Wybierz produkt.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (DeliveryContractorComboBox.SelectedItem == null)
            {
                MessageBox.Show("Wybierz kontrahenta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(DeliveryQuantityTextBox.Text, out int quantity) || quantity <= 0)
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
                    ProductId = ((Product)DeliveryProductComboBox.SelectedItem).Id,
                    ContractorId = ((Contractor)DeliveryContractorComboBox.SelectedItem).Id,
                    Quantity = quantity,
                    DeliveryDate = DeliveryDatePicker.SelectedDate.Value,
                    Description = DeliveryDescriptionTextBox.Text
                };

                _dataAccess.AddDelivery(delivery);
                MessageBox.Show("Dostawa dodana pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearDeliveryFields();
                LoadTransactions();
                TransactionAdded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas dodawania dostawy: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddIssue_Click(object sender, RoutedEventArgs e)
        {
            if (IssueProductComboBox.SelectedItem == null)
            {
                MessageBox.Show("Wybierz produkt.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (IssueContractorComboBox.SelectedItem == null)
            {
                MessageBox.Show("Wybierz kontrahenta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(IssueQuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Podaj poprawną ilość (liczba większa od 0).", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IssueDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Wybierz datę wydania.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var issue = new Issue
                {
                    ProductId = ((Product)IssueProductComboBox.SelectedItem).Id,
                    ContractorId = ((Contractor)IssueContractorComboBox.SelectedItem).Id,
                    Quantity = quantity,
                    IssueDate = IssueDatePicker.SelectedDate.Value,
                    Description = IssueDescriptionTextBox.Text
                };

                _dataAccess.AddIssue(issue);
                MessageBox.Show("Wydanie dodane pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearIssueFields();
                LoadTransactions();
                TransactionAdded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas dodawania wydania: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadTransactions()
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

            TransactionsDataGrid.ItemsSource = transactions.OrderBy(t => t.Date).ToList();
        }

        private void ClearDeliveryFields()
        {
            DeliveryProductComboBox.SelectedIndex = -1;
            DeliveryContractorComboBox.SelectedIndex = -1;
            DeliveryQuantityTextBox.Text = "";
            DeliveryDatePicker.SelectedDate = DateTime.Today;
            DeliveryDescriptionTextBox.Text = "";
        }

        private void ClearIssueFields()
        {
            IssueProductComboBox.SelectedIndex = -1;
            IssueContractorComboBox.SelectedIndex = -1;
            IssueQuantityTextBox.Text = "";
            IssueDatePicker.SelectedDate = DateTime.Today;
            IssueDescriptionTextBox.Text = "";
        }
    }
}