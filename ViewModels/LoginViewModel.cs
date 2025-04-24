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
            LoginCommand = new RelayCommand<PasswordBox>(ExecuteLogin);
        }

        private void ExecuteLogin(PasswordBox passwordBox)
        {
            string password = passwordBox.Password;
            if (Username == "admin" && password == "admin123")
            {
                OpenMainWindow("Admin");
            }
            else if (Username == "user" && password == "user123")
            {
                OpenMainWindow("User");
            }
            else
            {
                MessageBox.Show("Invalid credentials");
            }
        }

        private void OpenMainWindow(string role)
        {
            var mainWindow = new MainView { DataContext = new MainViewModel(role) };
            mainWindow.Show();
            Application.Current.Windows[0].Close();
        }
    }
}