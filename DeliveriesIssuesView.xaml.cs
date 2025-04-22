using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WarehouseManagementUnia.Data;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class DeliveriesIssuesView : UserControl
    {
        private readonly WarehouseDataAccess _dataAccess;
        public event EventHandler TransactionAdded;

        public DeliveriesIssuesView()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            LoadTransactions();
        }

        private void AddDelivery_Click(object sender, RoutedEventArgs e)
        {
            var addDeliveryWindow = new AddDeliveryWindow();
            addDeliveryWindow.TransactionAdded += (s, args) =>
            {
                LoadTransactions();
                TransactionAdded?.Invoke(this, EventArgs.Empty);
            };
            addDeliveryWindow.ShowDialog();
        }

        private void AddIssue_Click(object sender, RoutedEventArgs e)
        {
            var addIssueWindow = new AddIssueWindow();
            addIssueWindow.TransactionAdded += (s, args) =>
            {
                LoadTransactions();
                TransactionAdded?.Invoke(this, EventArgs.Empty);
            };
            addIssueWindow.ShowDialog();
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

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.ShowMainMenu();
            }
        }
    }
}