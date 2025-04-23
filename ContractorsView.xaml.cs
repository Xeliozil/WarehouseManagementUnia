using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WarehouseManagementUnia.Data;

namespace WarehouseManagementUnia
{
    public partial class ContractorsView : UserControl
    {
        private readonly WarehouseDataAccess _dataAccess;

        public ContractorsView()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            DataContext = SettingsHelper.Instance;
            Loaded += (s, e) =>
            {
                LoadContractors();
                ApplySettings();
            };
        }

        private void LoadContractors()
        {
            var items = _dataAccess.GetContractors();
            ContractorsDataGrid.ItemsSource = null; // Wyczyść, aby wymusić odświeżenie
            ContractorsDataGrid.ItemsSource = items;
        }

        private void AddContractor_Click(object sender, RoutedEventArgs e)
        {
            var addContractorWindow = new AddContractorWindow();
            addContractorWindow.ContractorAdded += (s, args) => LoadContractors();
            addContractorWindow.ShowDialog();
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