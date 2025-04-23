using System.Windows;
using System.Windows.Input;

namespace WarehouseManagementUnia
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            this.Closed += (s, e) =>
            {
                if (Application.Current.MainWindow == this || Application.Current.MainWindow == null)
                {
                    Application.Current.Shutdown();
                }
            };
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            PerformLogin();
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformLogin();
            }
        }

        private void PerformLogin()
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (username == "admin" && password == "admin123")
            {
                MainWindow mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.InitializeWithRole("Admin");
                mainWindow.Show();
                this.Close();
            }
            else if (username == "user" && password == "user123")
            {
                MainWindow mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.InitializeWithRole("User");
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Nieprawidłowa nazwa użytkownika lub hasło.", "Błąd logowania", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}