using System.Windows;
using System.Windows.Controls;
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
            LoadContractors();
        }

        private void LoadContractors()
        {
            ContractorsDataGrid.ItemsSource = _dataAccess.GetContractors();
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
    }
}