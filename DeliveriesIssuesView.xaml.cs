using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WarehouseManagementUnia.Data;

namespace WarehouseManagementUnia
{
    public partial class DeliveriesIssuesView : UserControl
    {
        private readonly WarehouseDataAccess _dataAccess;
        public event Action TransactionAdded;

        public DeliveriesIssuesView()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            LoadTransactions();
        }

        public void LoadTransactions()
        {
            var transactions = new List<object>();
            var deliveries = _dataAccess.GetDeliveries();
            var issues = _dataAccess.GetIssues();

            foreach (var delivery in deliveries)
            {
                transactions.Add(new
                {
                    Type = "Dostawa",
                    Id = delivery.Id,
                    ProductId = delivery.ProductId,
                    Quantity = delivery.Quantity,
                    Date = delivery.DeliveryDate,
                    Description = delivery.Description
                });
            }

            foreach (var issue in issues)
            {
                transactions.Add(new
                {
                    Type = "Wydanie",
                    Id = issue.Id,
                    ProductId = issue.ProductId,
                    Quantity = issue.Quantity,
                    Date = issue.IssueDate,
                    Description = issue.Description
                });
            }

            TransactionsGrid.ItemsSource = transactions;
        }

        private void AddDelivery_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var addDeliveryWindow = new AddDeliveryWindow();
            if (addDeliveryWindow.ShowDialog() == true)
            {
                try
                {
                    _dataAccess.AddDelivery(addDeliveryWindow.Delivery);
                    LoadTransactions();
                    TransactionAdded?.Invoke();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddIssue_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var addIssueWindow = new AddIssueWindow();
            if (addIssueWindow.ShowDialog() == true)
            {
                try
                {
                    _dataAccess.AddIssue(addIssueWindow.Issue);
                    LoadTransactions();
                    TransactionAdded?.Invoke();
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}