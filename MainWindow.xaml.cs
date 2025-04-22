using System;
using System.Windows;
using WarehouseManagementUnia.Data;

namespace WarehouseManagementUnia
{
    public partial class MainWindow : Window
    {
        private readonly WarehouseDataAccess _dataAccess;
        private readonly StockView _stockView;
        private readonly DeliveriesIssuesView _deliveriesIssuesView;
        private readonly AllProductsView _allProductsView;

        public MainWindow()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            _stockView = new StockView();
            _deliveriesIssuesView = new DeliveriesIssuesView();
            _allProductsView = new AllProductsView();
            _deliveriesIssuesView.TransactionAdded += RefreshAllViews;
            _allProductsView.ProductStatusChanged += RefreshAllViews;
            MainContent.Content = _stockView;
        }

        private void ShowStock_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _stockView;
            RefreshAllViews();
        }

        private void ShowDeliveriesIssues_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _deliveriesIssuesView;
            RefreshAllViews();
        }

        private void ShowAllProducts_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _allProductsView;
            RefreshAllViews();
        }

        private void RefreshAllViews()
        {
            _stockView.LoadProducts();
            _allProductsView.LoadProducts();
            _deliveriesIssuesView.LoadTransactions();
        }
    }
}