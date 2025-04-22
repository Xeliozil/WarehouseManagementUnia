using System.Windows;
using WarehouseManagementUnia.Data;

namespace WarehouseManagementUnia
{
    public partial class StockView : Window
    {
        private readonly WarehouseDataAccess _dataAccess;

        public StockView()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            LoadProducts();
        }

        private void LoadProducts()
        {
            ProductsDataGrid.ItemsSource = _dataAccess.GetProducts();
        }
    }
}