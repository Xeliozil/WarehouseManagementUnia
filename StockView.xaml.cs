using System.Windows;
using System.Windows.Controls;
using WarehouseManagementUnia.Data;

namespace WarehouseManagementUnia
{
    public partial class StockView : UserControl
    {
        private readonly WarehouseDataAccess _dataAccess;

        public StockView()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            LoadProducts();
        }

        public void LoadProducts()
        {
            ProductsGrid.ItemsSource = _dataAccess.GetProducts();
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funkcja generowania raportu zostanie zaimplementowana później (zakres dat: od początku miesiąca do teraz, format PDF).", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}