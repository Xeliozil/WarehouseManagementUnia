using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;

namespace WarehouseManagementUnia.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _username;
        private string _password;
        private string _errorMessage;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand<object>(ExecuteLogin, CanExecuteLogin);
        }

        private bool CanExecuteLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        private void ExecuteLogin(object parameter)
        {
            try
            {
                using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=UniaWarehouse;Trusted_Connection=True;"))
                {
                    conn.Open();
                    var cmd = new SqlCommand("SELECT Role FROM Users WHERE Username = @Username AND Password = @Password", conn);
                    cmd.Parameters.AddWithValue("@Username", Username);
                    cmd.Parameters.AddWithValue("@Password", Password); // Note: In production, use hashed passwords
                    var result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        string userRole = result.ToString();
                        var mainWindow = new MainWindow(userRole);
                        mainWindow.Show();

                        // Close the login window
                        if (Application.Current.Windows[0] is Window loginWindow)
                        {
                            loginWindow.Close();
                        }
                    }
                    else
                    {
                        ErrorMessage = "Invalid username or password.";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
        }
    }
}