using System.Windows;
using System.Windows.Input;
using WarehouseManagementUnia.Views;

namespace WarehouseManagementUnia.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _username;
        private string _password;
        private string _errorMessage;
        private readonly RelayCommand<object> _loginCommand;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                _loginCommand.RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                _loginCommand.RaiseCanExecuteChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand => _loginCommand;

        public LoginViewModel()
        {
            _loginCommand = new RelayCommand<object>(ExecuteLogin, CanLogin);
        }

        private bool CanLogin(object parameter)
        {
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        }

        private void ExecuteLogin(object parameter)
        {
            // Simple hardcoded login for demo (replace with DB check in production)
            if (Username == "admin" && Password == "admin123")
            {
                var mainView = new MainView
                {
                    DataContext = new MainViewModel()
                };
                Application.Current.MainWindow.Close();
                Application.Current.MainWindow = mainView;
                mainView.Show();
            }
            else
            {
                ErrorMessage = "Invalid username or password.";
            }
        }
    }
}