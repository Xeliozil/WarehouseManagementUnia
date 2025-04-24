using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using WarehouseManagementUnia.Models;
using WarehouseManagementUnia.Views;

namespace WarehouseManagementUnia.ViewModels
{
    public class ContractorsViewModel : ViewModelBase
    {
        private ObservableCollection<Contractor> _contractors;
        private readonly string _userRole;

        public ObservableCollection<Contractor> Contractors
        {
            get => _contractors;
            set { _contractors = value; OnPropertyChanged(); }
        }

        public ICommand AddContractorCommand { get; }

        public ContractorsViewModel(string userRole)
        {
            _userRole = userRole;
            Contractors = new ObservableCollection<Contractor>();
            AddContractorCommand = new RelayCommand<object>(ExecuteAddContractor);
            LoadContractors();
        }

        private void LoadContractors()
        {
            Contractors.Clear();
            using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ContractorId, Name, Address FROM Contractors", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Contractors.Add(new Contractor
                        {
                            ContractorId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Address = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }
        }

        private void ExecuteAddContractor(object parameter)
        {
            if (_userRole != "Admin")
            {
                MessageBox.Show("Only admins can add contractors.");
                return;
            }
            var addContractorWindow = new AddContractorView { DataContext = new AddContractorViewModel() };
            addContractorWindow.ShowDialog();
            LoadContractors();
        }
    }
}