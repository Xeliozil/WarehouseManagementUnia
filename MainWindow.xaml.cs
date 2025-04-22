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
            LoadMainWindowData();
        }

        private void OpenDeliveriesIssuesView_Click(object sender, RoutedEventArgs e)
        {
            var view = new DeliveriesIssuesView();
            view.TransactionAdded += RefreshAllViews; // Poprawna subskrypcja
            view.ShowDialog();
        }

        private void RefreshAllViews(object sender, EventArgs e)
        {
            LoadMainWindowData();
            // Jeśli DeliveriesIssuesView jest otwarte, odśwież jego DataGrid
            if (Application.Current.Windows.OfType<DeliveriesIssuesView>().FirstOrDefault() is DeliveriesIssuesView deliveriesIssuesView)
            {
                deliveriesIssuesView.LoadTransactions();
            }
        }

        private void LoadMainWindowData()
        {
            // Przykład: Ładowanie danych do DataGrid w MainWindow
            // Zakładam, że MainWindow ma DataGrid dla produktów lub transakcji
            // Dostosuj do swojego kodu
            // ProductsDataGrid.ItemsSource = _dataAccess.GetProducts();
        }

        // Dodaj inne metody, np. dla ContractorsWindow, ReportWindow
        private void OpenContractorsWindow_Click(object sender, RoutedEventArgs e)
        {
            new ContractorsWindow().ShowDialog();
        }

        private void OpenReportWindow_Click(object sender, RoutedEventArgs e)
        {
            new ReportWindow().ShowDialog();
        }
    }
}