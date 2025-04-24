using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WarehouseManagementUnia.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            DataContext = new ViewModels.LoginViewModel();
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is ViewModels.LoginViewModel viewModel && sender is PasswordBox passwordBox)
                {
                    viewModel.LoginCommand.Execute(passwordBox);
                }
            }
        }
    }
}