using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class AddProductWindow : Window
    {
        public Product Product { get; private set; }

        public AddProductWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(QuantityTextBox.Text, out int quantity) && decimal.TryParse(PriceTextBox.Text, out decimal price))
            {
                Product = new Product
                {
                    Name = NameTextBox.Text,
                    Quantity = quantity,
                    Price = price
                };
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Wprowadź poprawne wartości dla ilości i ceny.");
            }
        }
    }
}