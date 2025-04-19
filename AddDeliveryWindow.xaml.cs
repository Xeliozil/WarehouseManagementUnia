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
using System;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class AddDeliveryWindow : Window
    {
        public Delivery Delivery { get; private set; }

        public AddDeliveryWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(ProductIdTextBox.Text, out int productId) &&
                int.TryParse(QuantityTextBox.Text, out int quantity) &&
                DateTime.TryParse(DeliveryDateTextBox.Text, out DateTime deliveryDate))
            {
                Delivery = new Delivery
                {
                    ProductId = productId,
                    Quantity = quantity,
                    DeliveryDate = deliveryDate
                };
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Wprowadź poprawne wartości dla ID produktu, ilości i daty dostawy.");
            }
        }
    }
}
