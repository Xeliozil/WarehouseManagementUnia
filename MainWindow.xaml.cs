using System;
using System.Windows;
using System.Windows.Controls;
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
            ShowMainMenu();
        }

        public void ShowMainMenu()
        {
            var menu = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var stockButton = new Button
            {
                Content = "Stany magazynowe",
                Width = 150,
                Margin = new Thickness(0, 10, 0, 10)
            };
            stockButton.Click += (s, e) => MainContent.Content = new StockView();

            var deliveriesIssuesButton = new Button
            {
                Content = "Dostawy i wydania",
                Width = 150,
                Margin = new Thickness(0, 10, 0, 10)
            };
            deliveriesIssuesButton.Click += (s, e) =>
            {
                var deliveriesIssuesView = new DeliveriesIssuesView();
                deliveriesIssuesView.TransactionAdded += RefreshDeliveriesIssuesView;
                MainContent.Content = deliveriesIssuesView;
            };

            var contractorsButton = new Button
            {
                Content = "Kontrahenci",
                Width = 150,
                Margin = new Thickness(0, 10, 0, 10)
            };
            contractorsButton.Click += (s, e) => MainContent.Content = new ContractorsView();

            var reportsButton = new Button
            {
                Content = "Raporty",
                Width = 150,
                Margin = new Thickness(0, 10, 0, 10)
            };
            reportsButton.Click += (s, e) => MainContent.Content = new ReportsView();

            menu.Children.Add(stockButton);
            menu.Children.Add(deliveriesIssuesButton);
            menu.Children.Add(contractorsButton);
            menu.Children.Add(reportsButton);

            MainContent.Content = menu;
        }

        private void RefreshDeliveriesIssuesView(object sender, EventArgs e)
        {
            if (MainContent.Content is DeliveriesIssuesView deliveriesIssuesView)
            {
                deliveriesIssuesView.LoadTransactions();
            }
        }
    }
}