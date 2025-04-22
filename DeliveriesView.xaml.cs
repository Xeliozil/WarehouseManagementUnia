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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WarehouseManagementUnia.Data;

namespace WarehouseManagementUnia
{
    public partial class DeliveriesView : UserControl
    {
        private readonly WarehouseDataAccess _dataAccess;

        public DeliveriesView()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
            LoadDeliveries();
        }

        private void LoadDeliveries()
        {
            DeliveriesGrid.ItemsSource = _dataAccess.GetDeliveries();
        }
    }
}
