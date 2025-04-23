using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WarehouseManagementUnia
{
    public partial class MainWindow : Window
    {
        private string _userRole;
        private string _username;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void InitializeWithRole(string userRole)
        {
            _userRole = userRole;
            _username = userRole?.ToLower() ?? "user"; // Domyślnie "user", jeśli userRole jest null
            SettingsHelper.Instance.SetUsername(_username);
            ApplySettings();
            ShowMainMenu();
        }

        public string GetUsername()
        {
            return _username;
        }

        public void ApplySettings()
        {
            string backgroundColor = Properties.Settings.Default[$"BackgroundColor_{_username}"].ToString() ?? "White";
            string fontColor = Properties.Settings.Default[$"FontColor_{_username}"].ToString() ?? "Black";
            double fontSize = Properties.Settings.Default[$"FontSize_{_username}"] != null ? (double)Properties.Settings.Default[$"FontSize_{_username}"] : 14;

            MainGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
            ApplySettingsToElement(MainGrid, fontColor, fontSize);

            if (MainContent.Content is StackPanel)
            {
                ShowMainMenu();
            }
            else if (MainContent.Content is UserControl view)
            {
                var method = view.GetType().GetMethod("ApplySettings");
                method?.Invoke(view, null);
            }

            SettingsHelper.Instance.UpdateSettings();
        }

        private void ApplySettingsToElement(DependencyObject element, string fontColor, double fontSize)
        {
            if (element == null) return;

            if (element is FrameworkElement fe)
            {
                fe.SetValue(Control.ForegroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString(fontColor)));
                fe.SetValue(Control.FontSizeProperty, fontSize);
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                ApplySettingsToElement(child, fontColor, fontSize);
            }
        }

        public void ShowMainMenu()
        {
            var menu = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            string fontColor = Properties.Settings.Default[$"FontColor_{_username}"].ToString() ?? "Black";
            double fontSize = Properties.Settings.Default[$"FontSize_{_username}"] != null ? (double)Properties.Settings.Default[$"FontSize_{_username}"] : 14;

            var stockButton = new Button
            {
                Content = "Stany magazynowe",
                Width = 150,
                Margin = new Thickness(0, 10, 0, 10),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fontColor)),
                FontSize = fontSize
            };
            stockButton.Click += (s, e) => MainContent.Content = new StockView(_userRole);

            var deliveriesIssuesButton = new Button
            {
                Content = "Dostawy i wydania",
                Width = 150,
                Margin = new Thickness(0, 10, 0, 10),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fontColor)),
                FontSize = fontSize
            };
            deliveriesIssuesButton.Click += (s, e) => MainContent.Content = new DeliveriesIssuesView();

            var contractorsButton = new Button
            {
                Content = "Kontrahenci",
                Width = 150,
                Margin = new Thickness(0, 10, 0, 10),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fontColor)),
                FontSize = fontSize
            };
            contractorsButton.Click += (s, e) => MainContent.Content = new ContractorsView();

            var reportsButton = new Button
            {
                Content = "Raporty",
                Width = 150,
                Margin = new Thickness(0, 10, 0, 10),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fontColor)),
                FontSize = fontSize
            };
            reportsButton.Click += (s, e) => MainContent.Content = new ReportsView();

            menu.Children.Add(stockButton);
            menu.Children.Add(deliveriesIssuesButton);
            menu.Children.Add(contractorsButton);
            menu.Children.Add(reportsButton);

            MainContent.Content = menu;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_username);
            settingsWindow.ShowDialog();
        }
    }
}