using System.Data.SqlClient;
using System.Windows.Input;
using System.Windows;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia.ViewModels
{
    public class AddProductViewModel : ViewModelBase
    {
        private string _productName;
        private int _quantity;
        private readonly Warehouse _warehouse;

        public string ProductName
        {
            get => _productName;
            set { _productName = value; OnPropertyChanged(); }
        }

        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }

        public AddProductViewModel(Warehouse warehouse)
        {
            _warehouse = warehouse;
            AddCommand = new RelayCommand<object>(ExecuteAdd);
        }

        private void ExecuteAdd(object parameter)
        {
            using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
            {
                conn.Open();
                var cmd = new SqlCommand("INSERT INTO Products (Name, Quantity, WarehouseId) VALUES (@Name, @Quantity, @WarehouseId)", conn);
                cmd.Parameters.AddWithValue("@Name", ProductName);
                cmd.Parameters.AddWithValue("@Quantity", Quantity);
                cmd.Parameters.AddWithValue("@WarehouseId", _warehouse.WarehouseId);
                cmd.ExecuteNonQuery();
            }
            Application.Current.Windows[Application.Current.Windows.Count - 1].Close();
        }
    }
}