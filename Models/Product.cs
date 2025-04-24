using WarehouseManagementUnia.ViewModels;

namespace WarehouseManagementUnia.Models
{
    public class Product : ViewModelBase
    {
        private int _productId;
        private string _name;
        private int _quantity;
        private decimal _price;

        public int ProductId
        {
            get => _productId;
            set { _productId = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }

        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }
    }
}