using System;
using System.Text.RegularExpressions;
using System.Windows;
using WarehouseManagementUnia.Data;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class AddContractorWindow : Window
    {
        private readonly WarehouseDataAccess _dataAccess;

        public AddContractorWindow()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
        }

        private void FetchData_Click(object sender, RoutedEventArgs e)
        {
            string nip = NIPTextBox.Text.Replace("-", "").Trim();
            if (!IsValidNIP(nip))
            {
                MessageBox.Show("Podaj poprawny NIP (10 cyfr).", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Symulacja pobierania danych z API GUS
                var contractorData = FetchContractorDataByNIP(nip);
                NameTextBox.Text = contractorData.Name;
                AddressTextBox.Text = contractorData.Address;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas pobierania danych: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsValidNIP(string nip)
        {
            if (!Regex.IsMatch(nip, @"^\d{10}$"))
                return false;

            int[] weights = { 6, 5, 7, 2, 3, 4, 5, 6, 7 };
            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += int.Parse(nip[i].ToString()) * weights[i];
            int checksum = sum % 11;
            return checksum == int.Parse(nip[9].ToString());
        }

        private Contractor FetchContractorDataByNIP(string nip)
        {
            // Symulacja API GUS (do zastąpienia prawdziwym API)
            switch (nip)
            {
                case "1234567890":
                    return new Contractor { Name = "Sklep Budowlany ABC", Address = "Koszalin, ul. Budowlana 10", NIP = "1234567890" };
                case "0987654321":
                    return new Contractor { Name = "Hurtownia XYZ", Address = "Koszalin, ul. Magazynowa 5", NIP = "0987654321" };
                default:
                    throw new Exception("Nie znaleziono kontrahenta dla podanego NIP.");
            }
        }

        private void AddContractor_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Nazwa kontrahenta jest wymagana.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(NIPTextBox.Text) || !IsValidNIP(NIPTextBox.Text.Replace("-", "").Trim()))
            {
                MessageBox.Show("Podaj poprawny NIP.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var contractor = new Contractor
                {
                    Name = NameTextBox.Text,
                    Address = AddressTextBox.Text,
                    NIP = NIPTextBox.Text.Replace("-", "").Trim()
                };

                _dataAccess.AddContractor(contractor);
                MessageBox.Show("Kontrahent dodany pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas dodawania kontrahenta: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}