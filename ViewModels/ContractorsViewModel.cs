using System.Collections.ObjectModel;
using System.Data.SqlClient;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia.ViewModels
{
    public class ContractorsViewModel : ViewModelBase
    {
        private ObservableCollection<Contractor> _contractors;

        public ObservableCollection<Contractor> Contractors
        {
            get => _contractors;
            set { _contractors = value; OnPropertyChanged(); }
        }

        public ContractorsViewModel()
        {
            Contractors = new ObservableCollection<Contractor>();
            LoadContractors();
        }

        private void LoadContractors()
        {
            using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ContractorId, Name, Address, NIP FROM Contractors", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Contractors.Add(new Contractor
                        {
                            ContractorId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Address = reader.IsDBNull(2) ? null : reader.GetString(2),
                            NIP = reader.IsDBNull(3) ? null : reader.GetString(3)
                        });
                    }
                }
            }
        }
    }
}