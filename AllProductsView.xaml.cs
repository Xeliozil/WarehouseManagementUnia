using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WarehouseManagementUnia.Data;

namespace WarehouseManagementUnia
{
    public partial class AllProductsView : UserControl
    {
        private readonly WarehouseDataAccess _dataAccess;
        private readonly string _userRole;

        public AllProductsView(string userRole)
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            _userRole = userRole;
            DataContext = SettingsHelper.Instance;
            Loaded += (s, e) =>
            {
                LoadProducts();
                ConfigurePermissions();
                ApplySettings();
            };
        }

        private void LoadProducts()
        {
            var items = _dataAccess.GetAllProducts();
            AllProductsDataGrid.ItemsSource = null; // Wyczyść, aby wymusić odświeżenie
            AllProductsDataGrid.ItemsSource = items;
        }

        private void ConfigurePermissions()
        {
            if (_userRole != "Admin")
            {
                DeleteButton.IsEnabled = false;
                DeleteButton.ToolTip = "Za małe uprawnienia, skontaktuj się z administratorem";
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

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var addProductWindow = new AddProductWindow();
            addProductWindow.ProductAdded += (s, args) => LoadProducts();
            addProductWindow.ShowDialog();
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            var deleteProductWindow = new DeleteProductWindow();
            deleteProductWindow.ProductDeleted += (s, args) => LoadProducts();
            deleteProductWindow.ShowDialog();
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