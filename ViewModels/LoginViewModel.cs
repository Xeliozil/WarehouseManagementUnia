using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WarehouseManagementUnia.Views;

namespace WarehouseManagementUnia.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand<PasswordBox>(ExecuteLogin, CanExecuteLogin);
        }

        private bool CanExecuteLogin(PasswordBox parameter)
        {
            // Allow execution unless specific conditions are added
            return true;
        }

        private void ExecuteLogin(PasswordBox passwordBox)
        {
            try
            {
                Debug.WriteLine($"Login attempted with username: {Username}");
                if (passwordBox == null)
                {
                    Debug.WriteLine("PasswordBox is null");
                    MessageBox.Show("Password field is missing.");
                    return;
                }

                string password = passwordBox.Password;
                Debug.WriteLine($"Password entered: {password}");

                if (Username == "admin" && password == "admin123")
                {
                    Debug.WriteLine("Admin login successful");
                    OpenMainWindow("Admin");
                }
                else if (Username == "user" && password == "user123")
                {
                    Debug.WriteLine("User login successful");
                    OpenMainWindow("User");
                }
                else
                {
                    Debug.WriteLine("Invalid credentials");
                    MessageBox.Show("Invalid credentials");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login error: {ex.Message}");
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void OpenMainWindow(string role)
        {
            Debug.WriteLine($"Opening MainWindow with role: {role}");
            var mainWindow = new MainView { DataContext = new MainViewModel(role) };
            mainWindow.Show();
            Application.Current.Windows[0].Close();
        }
    }
}