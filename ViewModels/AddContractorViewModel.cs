using System.Data.SqlClient;
using System.Windows.Input;
using System.Windows;


namespace WarehouseManagementUnia.ViewModels
{
    public class AddContractorViewModel : ViewModelBase
    {
        private string _name;
        private string _address;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }

        public AddContractorViewModel()
        {
            AddCommand = new RelayCommand<object>(ExecuteAdd);
        }

        private void ExecuteAdd(object parameter)
        {
            using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
            {
                conn.Open();
                var cmd = new SqlCommand("INSERT INTO Contractors (Name, Address) VALUES (@Name, @Address)", conn);
                cmd.Parameters.AddWithValue("@Name", Name);
                cmd.Parameters.AddWithValue("@Address", Address ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            Application.Current.Windows[Application.Current.Windows.Count - 1].Close();
        }
    }
}