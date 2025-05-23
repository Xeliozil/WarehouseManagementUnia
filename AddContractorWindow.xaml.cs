﻿using System;
using System.Windows;
using WarehouseManagementUnia.Data;
using WarehouseManagementUnia.Models;

namespace WarehouseManagementUnia
{
    public partial class AddContractorWindow : Window
    {
        private readonly WarehouseDataAccess _dataAccess;
        public event EventHandler ContractorAdded;

        public AddContractorWindow()
        {
            InitializeComponent();
            _dataAccess = new WarehouseDataAccess();
        }

        private void AddContractor_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Podaj nazwę kontrahenta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(NIPTextBox.Text) || NIPTextBox.Text.Length != 10 || !long.TryParse(NIPTextBox.Text, out _))
            {
                MessageBox.Show("Podaj poprawny NIP (10 cyfr).", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var contractor = new Contractor
                {
                    Name = NameTextBox.Text,
                    Address = AddressTextBox.Text,
                    NIP = NIPTextBox.Text
                };

                _dataAccess.AddContractor(contractor);
                MessageBox.Show("Kontrahent dodany pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                ContractorAdded?.Invoke(this, EventArgs.Empty);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas dodawania kontrahenta: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}