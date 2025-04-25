using System;
using System.Windows;

namespace WarehouseManagementUnia.Views
{
    public partial class AddProductDialog : Window
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public AddProductDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProductName))
            {
                MessageBox.Show("Product name is required.");
                return;
            }
            if (Quantity <= 0)
            {
                MessageBox.Show("Quantity must be greater than 0.");
                return;
            }
            if (Price < 0)
            {
                MessageBox.Show("Price cannot be negative.");
                return;
            }
            DialogResult = true;
        }
    }
}