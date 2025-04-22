using System;
using System.Windows;
using WarehouseManagementUnia.Data;

namespace WarehouseManagementUnia
{
    public partial class MainWindow : Window
    {
        private readonly WarehouseDataAccess _dataAccess;

        public MainWindow()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
        }

        private void ShowStock_Click(object sender, RoutedEventArgs e)
        {
            var stockView = new StockView();
            stockView.ShowDialog();
        }

        private void ShowDeliveriesIssues_Click(object sender, RoutedEventArgs e)
        {
            var deliveriesIssuesView = new DeliveriesIssuesView();
            deliveriesIssuesView.TransactionAdded += RefreshAllViews;
            deliveriesIssuesView.ShowDialog();
        }

        private void ShowContractors_Click(object sender, RoutedEventArgs e)
        {
            var contractorsWindow = new ContractorsWindow();
            contractorsWindow.ShowDialog();
        }

        private void ShowReports_Click(object sender, RoutedEventArgs e)
        {
            var reportWindow = new ReportWindow();
            reportWindow.ShowDialog();
        }

        private void RefreshAllViews(object sender, EventArgs e)
        {
            // Odśwież dane w MainWindow, jeśli istnieją (np. DataGrid dla produktów)
            // LoadMainWindowData();
            // Odśwież otwarte okno DeliveriesIssuesView, jeśli istnieje
            if (Application.Current.Windows.OfType<DeliveriesIssuesView>().FirstOrDefault() is DeliveriesIssuesView deliveriesIssuesView)
            {
                deliveriesIssuesView.LoadTransactions();
            }
        }
    }
}