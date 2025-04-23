using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WarehouseManagementUnia.Data;

namespace WarehouseManagementUnia
{
    public partial class DeliveriesIssuesView : UserControl
    {
        private readonly WarehouseDataAccess _dataAccess;

        public DeliveriesIssuesView()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            DataContext = SettingsHelper.Instance;
            Loaded += (s, e) =>
            {
                LoadDeliveriesIssues();
                ApplySettings();
            };
        }

        private void LoadDeliveriesIssues()
        {
            var items = _dataAccess.GetDeliveriesIssues();
            DeliveriesIssuesDataGrid.ItemsSource = null;
            DeliveriesIssuesDataGrid.ItemsSource = items;
        }

        private void AddDelivery_Click(object sender, RoutedEventArgs e)
        {
            var addDeliveryWindow = new AddDeliveryWindow();
            addDeliveryWindow.DeliveryAdded += (s, args) => LoadDeliveriesIssues();
            addDeliveryWindow.ShowDialog();
        }

        private void AddIssue_Click(object sender, RoutedEventArgs e)
        {
            var addIssueWindow = new AddIssueWindow();
            addIssueWindow.IssueAdded += (s, args) => LoadDeliveriesIssues();
            addIssueWindow.ShowDialog();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.ShowMainMenu();
            }
        }

        public void ApplySettings()
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                string username = mainWindow.GetUsername();
                string fontColor = Properties.Settings.Default[$"FontColor_{username}"].ToString() ?? "Black";
                double fontSize = Properties.Settings.Default[$"FontSize_{username}"] != null ? (double)Properties.Settings.Default[$"FontSize_{username}"] : 14;

                ApplySettingsToElement(this, fontColor, fontSize);
            }
        }

        private void ApplySettingsToElement(DependencyObject element, string fontColor, double fontSize)
        {
            if (element == null) return;

            if (element is FrameworkElement fe && !(fe is DataGridCell))
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
    }
}