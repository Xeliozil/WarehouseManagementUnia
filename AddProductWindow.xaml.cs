// AddProductWindow.xaml.cs
using System.Windows;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia.Views
{
    public partial class AddProductWindow : Window
    {
        private int warehouseId;
        public AddProductWindow(int warehouseId)
        {
            InitializeComponent();
            this.warehouseId = warehouseId;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string name = NameBox.Text;
            string category = CategoryBox.Text;
            if (int.TryParse(QuantityBox.Text, out int quantity))
            {
                // Tutaj dodanie do bazy danych
                DatabaseService.AddProduct(new Product
                {
                    Name = name,
                    Category = category,
                    Quantity = quantity,
                    WarehouseId = warehouseId
                });
                this.Close();
            }
            else
            {
                MessageBox.Show("Niepoprawna ilość.");
            }
        }
    }
}
