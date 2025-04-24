using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;

namespace WarehouseManagementUnia.ViewModels
{
    public class AddContractorViewModel : ViewModelBase
    {
        private string _name;
        private string _address;
        private string _nip;

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

        public string NIP
        {
            get => _nip;
            set { _nip = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }

        public AddContractorViewModel()
        {
            AddCommand = new RelayCommand<object>(ExecuteAdd);
        }

        private void ExecuteAdd(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Name is required.");
                return;
            }

            try
            {
                using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "INSERT INTO Contractors (Name, Address, NIP) VALUES (@Name, @Address, @NIP)", conn);
                    cmd.Parameters.AddWithValue("@Name", Name);
                    cmd.Parameters.AddWithValue("@Address", (object)Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NIP", (object)NIP ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Contractor added successfully.");
                if (parameter is Window window)
                {
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding contractor: {ex.Message}");
            }
        }
    }
}