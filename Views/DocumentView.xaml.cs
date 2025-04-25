using System.Windows.Controls;
using WarehouseManagementUnia.ViewModels;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia.Views
{
    public partial class DocumentView : UserControl
    {
        public DocumentView()
        {
            InitializeComponent();
            DataContext = new DocumentViewModel(null);
        }

        public DocumentView(Warehouse defaultSourceWarehouse)
        {
            InitializeComponent();
            DataContext = new DocumentViewModel(defaultSourceWarehouse);
        }
    }
}