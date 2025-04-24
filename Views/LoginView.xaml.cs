using System.Windows;
using System.Windows.Controls;

namespace WarehouseManagementUnia.ViewModels
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = passwordBox.Password;
            }
        }
    }
}