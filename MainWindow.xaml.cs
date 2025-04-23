using System.Windows;
using System.Windows.Controls;

namespace WarehouseManagementUnia
{
    public partial class MainWindow : Window
    {
        private string _userRole;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void InitializeWithRole(string userRole)
        {
            _userRole = userRole;
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
            stockButton.Click += (s, e) => MainContent.Content = new StockView(_userRole);

            var deliveriesIssuesButton = new Button
            {
                Content = "Dostawy i wydania",
                Width = 150,
                Margin = new Thickness(0, 10, 0, 10)
            };
            deliveriesIssuesButton.Click += (s, e) => MainContent.Content = new DeliveriesIssuesView();

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
    }
}